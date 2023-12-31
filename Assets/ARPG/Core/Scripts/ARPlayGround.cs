using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using AOT;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace ARCeye
{
    [StructLayout(LayoutKind.Sequential)]
    struct ARPGConfiguration
    {
        [HideInInspector]
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 64)]
        public string languageCode;

        [HideInInspector]
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 64)]
        public string countryCode;

        // ARPG_TRANSIT_UNKNOWN              0
        // ARPG_TRANSIT_USE_VISUAL_CUES      1
        // ARPG_TRANSIT_USE_AIR_PRESSURE     2
        // ARPG_TRANSIT_USE_RANGE_CONSTRAINT 3
        [HideInInspector]
        public UInt32 transitOption;

        // ARPG_FILESYSTEM_DEFAULT           0
        // ARPG_FILESYSTEM_UNITY             1
        [HideInInspector]
        public UInt32 filesystemOption;
    }

    [DefaultExecutionOrder(-2000)]
    public class ARPlayGround : MonoBehaviour
    {
        const string PLUGIN_VERSION = "1.2.5";

        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif


        [DllImport(dll)]
        private static extern void InitializePluginNative(ARPGConfiguration config);

        [DllImport(dll)]
        private static extern void DestroyPluginNative();

        [DllImport(dll)]
        private static extern void PrintNativePluginBuiltTime();

        [DllImport(dll)]
        private static extern IntPtr ARPG_GetVersionNative();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LoadNative(string amprojFilePath);

        [DllImport(dll)]
        private static extern void ResetNative();

        [DllImport(dll)]
        private static extern void UpdateSceneNative(UnityFrame unityPose);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetStageNative(string stageName);

        [DllImport(dll)]
        private static extern void UnloadNavigationNative();

        [DllImport(dll)]
        private static extern void SetVLPassNative(bool value);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoActionNative(string actionName, bool immediately);


        [SerializeField]
        private string m_ContentsPath;
        public  string contentsPath => m_ContentsPath;

        private string amprojFilePath {
            get {
                string locationName = Path.GetFileName(contentsPath);
                string dataRootPath = m_StreamingAsset ? Application.streamingAssetsPath : Application.persistentDataPath;
                return $"{dataRootPath}/{contentsPath}/{locationName}.amproj";
            }
        }

        [SerializeField]
        private bool m_StreamingAsset = true;

        [SerializeField]
        private bool m_LoadOnAwake = true;

        private bool m_IsLoadingRequested = false;
        private bool m_IsLoaded = false;

        private Camera m_MainCamera;

        private LayerInfoConverter m_LayerInfoConverter;
        private PathFinder m_PathFinder;
        private NetworkController m_NetworkController;
        private NativeLogger m_NativeLogger;
        private NativeEventHandler m_NativeEventHandler;
        private NativeFileSystemHelper m_NativeFileSystemHelper;
        private UnityFrame m_Frame;

        private string m_CurrStage;

        private List<GameObject> m_NextStepGameObjects = new List<GameObject>();


        ///
        /// Events 
        ///
        public UnityEvent<string> m_OnStageChanged;
        public UnityEvent<List<LayerPOIItem>> m_OnPOIList;

        [Header("Navigation")]
        public UnityEvent              m_OnNavigationStarted;
        public UnityEvent              m_OnNavigationEnded;
        public UnityEvent              m_OnNavigationReSearched;
        public UnityEvent<float>       m_OnDistanceUpdated;
        public UnityEvent              m_OnDestinationArrived;
        public UnityEvent<ConnectionType, string> m_OnTransitMovingStarted;
        public UnityEvent                         m_OnTransitMovingEnded;
        public UnityEvent<string, string> m_OnCustomRangeEntered;
        public UnityEvent<string, string> m_OnCustomRangeExited;

        [Header("Debug")]
        public LogLevel                m_LogLevel = LogLevel.WARNING;


        private void Awake()
        {
            InitNativeLogger();
            InitComponents();

            ARPGConfiguration config = new ARPGConfiguration();
            config.languageCode = "ko";
            config.countryCode  = "KR";
            config.transitOption = 1;
            config.filesystemOption = (UInt32) (m_StreamingAsset ? 1 : 0);

            InitializePluginNative(config);
            InitMainCamera();

            m_Frame = new UnityFrame();
        }

        private void Start()
        {
            NativeLogger.Print(LogLevel.INFO, $"<b>ARPG version {PLUGIN_VERSION}, native {GetVersion()}</b>");
            m_NativeLogger.SetLogLevel(m_LogLevel);

            if(m_LoadOnAwake)
            {
                Load();
            }
        }

        private void Update()
        {
            // Camera의 view matrix와 pose matrix를 계산해서 ARPlayGround를 갱신.
            Matrix4x4 poseMatrix = Matrix4x4.TRS(m_MainCamera.transform.position, m_MainCamera.transform.rotation, Vector3.one);
            Matrix4x4 viewMatrix = Matrix4x4.Inverse(poseMatrix);
            Matrix4x4 projMatrix = m_MainCamera.projectionMatrix;

            UpdateScene(viewMatrix, projMatrix);
        }

        /// <summary>
        ///   매 프레임마다 실행 되어야 하는 업데이트 메서드.
        /// </summary>
        private void UpdateScene(Matrix4x4 viewMatrix, Matrix4x4 projMatrix)
        {
            // convert left handed to right handed.
            var rhViewMatirx = PoseHelper.ConvertLHRHView(viewMatrix).transpose;
            projMatrix.m23 = -1;
            
            m_Frame.viewMatrix = rhViewMatirx.ToData();
            m_Frame.projMatrix = projMatrix.ToData();

            UpdateSceneNative(m_Frame);
        }

        private void OnDestroy()
        {
            DestroyPluginNative();
            m_NativeLogger.Release();
        }


        private void InitComponents()
        {
            m_NetworkController = GetComponent<NetworkController>();
            m_PathFinder = GetComponent<PathFinder>();
            m_LayerInfoConverter = GetComponent<LayerInfoConverter>();

            m_NativeEventHandler = GetComponent<NativeEventHandler>();
            m_NativeEventHandler.m_OnPOIList = m_OnPOIList;
            m_NativeEventHandler.m_OnDistanceUpdated = m_OnDistanceUpdated;
            m_NativeEventHandler.m_OnStageChanged = m_OnStageChanged;
            m_NativeEventHandler.m_OnNavigationStarted = m_OnNavigationStarted;
            m_NativeEventHandler.m_OnNavigationEnded = m_OnNavigationEnded;
            m_NativeEventHandler.m_OnNavigationReSearched = m_OnNavigationReSearched;
            m_NativeEventHandler.m_OnDestinationArrived = m_OnDestinationArrived;
            m_NativeEventHandler.m_OnTransitMovingStarted = m_OnTransitMovingStarted;
            m_NativeEventHandler.m_OnTransitMovingEnded = m_OnTransitMovingEnded;
            m_NativeEventHandler.m_OnCustomRangeEntered = m_OnCustomRangeEntered;
            m_NativeEventHandler.m_OnCustomRangeExited = m_OnCustomRangeExited;

            m_NativeFileSystemHelper = GetComponent<NativeFileSystemHelper>();
#if !UNITY_EDITOR && UNITY_ANDROID
            m_NativeFileSystemHelper.useAndroidStreamingAssets = m_StreamingAsset;
#endif
        }

        private void InitNativeLogger()
        {
            m_NativeLogger = new NativeLogger();
            m_NativeLogger.logLevel = m_LogLevel;
            m_NativeLogger.Initialize();
        }

        private void InitMainCamera()
        {
            m_MainCamera = Camera.main;

            int cullingMask = m_MainCamera.cullingMask;
            string[] layersToDisable = { "Map", "MapPOI", "MapArrow", "UI" };
            foreach (string layer in layersToDisable)
            {
                int layerIndex = LayerMask.NameToLayer(layer);
                if (layerIndex == -1)
                {
                    continue;
                }

                cullingMask &= ~(1 << layerIndex);
            }
            
            m_MainCamera.cullingMask = cullingMask;
        }

        public string GetVersion()
        {
            IntPtr versionPtr = ARPG_GetVersionNative();
            return Marshal.PtrToStringAnsi(versionPtr);
        }

        /// <summary>
        /// 코루틴을 이용하여 amproj 파일을 로드합니다.
        /// </summary>
        public void Load(System.Action completeCallback = null)
        {
            if(m_IsLoadingRequested) {
                NativeLogger.Print(LogLevel.INFO, "이미 Load 메서드가 호출 되었습니다.");
                return;
            }
            m_IsLoadingRequested = true;
            
            Load(amprojFilePath, completeCallback);
        }

        /// <summary>
        /// 코루틴을 이용하여 특정 경로의 amproj 파일을 로드합니다.
        /// </summary>
        public void Load(string filePath, System.Action completeCallback = null)
        {
            StartCoroutine( LoadInternal(filePath, completeCallback) );
        }

        private IEnumerator LoadInternal(string filePath, System.Action completeCallback)
        {
            LoadNative(filePath);

            yield return new WaitUntil(() => m_NativeFileSystemHelper.isReadingComplete );

            NativeLogger.Print(LogLevel.DEBUG, "Load amproj file finish!");

            m_IsLoaded = true;

            completeCallback?.Invoke();
        }

        /// <summary>
        /// 비동기적으로 amproj 파일을 로드합니다.
        /// </summary>
        public async Task LoadAsync()
        {
            await LoadAsync(amprojFilePath);
        }

        /// <summary>
        /// 비동기적으로 특정 경로의 amproj 파일을 로드합니다.
        /// </summary>
        public async Task LoadAsync(string filePath)
        {
            LoadNative(filePath);

            await TaskUtil.WaitUntil(() => { return m_NativeFileSystemHelper.isReadingComplete; });

            NativeLogger.Print(LogLevel.DEBUG, "Load amproj file finish!");

            m_IsLoaded = true;
        }


        public void Reset()
        {
            // Reset 요청은 Native 영역 내부에서 즉시 실행되어야 한다.
            ResetNative();
            SetVLPassNative(false);
        }

        public string GetStageName() {
            CheckAMProjLoaded();

            if(m_CurrStage == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "Current stage isn't assigned. Check 'SetStage(string)' method is called");
            }

            return m_CurrStage;
        }

        public void SetStage(string stageName)
        {
            CheckAMProjLoaded();
            
            NativeLogger.Print(LogLevel.VERBOSE, "SetStage with stageName : " + stageName);
            m_CurrStage = stageName;

            SetStageNative(stageName);
        }

        /// <summary>
        /// VL에서 전달 받은 LayerInfo 값과 매칭되는 Stage를 로드한다.
        /// </summary>
        /// <param name="layerInfo"></param>
        public void SetLayerInfo(string layerInfo)
        {
            CheckAMProjLoaded();

            string stageName = m_LayerInfoConverter.Convert(layerInfo);
            SetStage(stageName);
        }

        public void OnStageChanged(string stage)
        {
            var stageCodes = stage.Split('_');
            m_CurrStage = stageCodes.Length == 0 ? "" : stageCodes[stageCodes.Length - 1];

            m_OnStageChanged.Invoke(m_CurrStage);
        }

        public void SetVLPass(bool value)
        {
            SetVLPassNative(value);
            StartCoroutine( SetVLPassInternal(value) );
        }

        private IEnumerator SetVLPassInternal(bool value)
        {
            // ARPG 내부 로직으로 인해 한 프레임을 건너뛴다.
            //   VL 인식 직후 ANTracker의 내부에서 OnStateChanged를 호출할때 본 메서드가 호출되며 ARPG를 리셋하고 경탐을 요청하는 로직이 수행됨.
            //   VL 인식 직후이기 때문에 실제 ANTracker의 pose는 갱신되지 않은 상태에서 경탐을 요청
            //   이로 인해 원점에서 경탐을 요청하는 문제 발생.
            //   이 문제를 해결하기 위해 한 프레임을 스킵하여 ANTracker의 pose를 갱신한 뒤에 ARPG 리셋을 진행하는 방식으로 변경.
            yield return null;

            if(!value) {
                Reset();
            }
        }

        public void LoadNavigation(LayerPOIItem poiItem, ConnectionType connectionType = ConnectionType.Default)
        {
            CheckAMProjLoaded();

            LoadNavigationParams param = new LoadNavigationParams();
             
            var coord = poiItem.entrance[0];

            param.endFloor = poiItem.stageName;
            param.endPoints = new float[]{ (float) coord[0], (float) coord[1], (float) coord[2] };
            param.connectionType = connectionType;
            
            m_PathFinder.LoadNavigation(param);
        }

        public void UnloadNavigation()
        {
            UnloadNavigationNative();
        }


        public void ActivateNextStep(bool value)
        {
            if(m_NextStepGameObjects.Count == 0)
            {
                UnityNextStepArrow[] arrows = FindObjectsOfType<UnityNextStepArrow>();
                UnityNextStepDot[]   dots   = FindObjectsOfType<UnityNextStepDot>();
                UnityNextStepText[]  texts  = FindObjectsOfType<UnityNextStepText>();

                foreach(var elem in arrows)
                    m_NextStepGameObjects.Add(elem.gameObject);
                foreach(var elem in dots)
                    m_NextStepGameObjects.Add(elem.gameObject);
                foreach(var elem in texts)
                    m_NextStepGameObjects.Add(elem.gameObject);
            }

            foreach(var elem in m_NextStepGameObjects)
                if (elem) {
                    elem.gameObject.SetActive(value);
                }
        }

        private void CheckAMProjLoaded() {
            if(!m_IsLoaded) {
                NativeLogger.Print(LogLevel.WARNING, "amproj 파일이 로드되지 않았습니다. Load 메서드가 호출되었는지 확인해주세요");
            }
        }
    }
}
