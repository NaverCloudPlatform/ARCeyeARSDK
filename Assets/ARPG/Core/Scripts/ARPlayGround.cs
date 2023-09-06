using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using AOT;
using UnityEngine.UI;

namespace ARCeye
{
    public class ARPlayGround : MonoBehaviour
    {
        const string PLUGIN_VERSION = "1.1.0";

        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif


        [DllImport(dll)]
        private static extern void InitializePluginNative();

        [DllImport(dll)]
        private static extern void DestroyPluginNative();

        [DllImport(dll)]
        private static extern void PrintNativePluginBuiltTime();

        [DllImport(dll)]
        private static extern IntPtr GetVersionNative();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LoadNative(string amprojFilePath);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DownloadAndLoadResourcesNative(string downloadDirPath, string location);

        [DllImport(dll)]
        private static extern void ResetNative();

        [DllImport(dll)]
        private static extern void CheckResourceUpdatedNative();

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

        // private string m_Location;
        // public string location => m_Location;

        private Camera m_MainCamera;

        private PathFinder m_PathFinder;
        private NetworkController m_NetworkController;
        private NativeLogger m_NativeLogger;
        private NativeEventHandler m_NativeEventHandler;
        private UnityFrame m_Frame;

        private string m_CurrStage;

        private List<GameObject> m_NextStepGameObjects = new List<GameObject>();


        ///
        /// Events 
        ///
        public UnityEvent<string> m_OnStageChanged;
        public UnityEvent<List<LayerPOIItem>> m_OnPOIList;

        [Header("Navigation")]
        public UnityEvent        m_OnNavigationStarted;
        public UnityEvent<float> m_OnDistanceUpdated;
        public UnityEvent        m_OnDestinationArrived;
        public UnityEvent        m_OnNavigationEnded;
        public UnityEvent<int>   m_OnTransitMovingStarted;
        public UnityEvent        m_OnTransitMovingEnded;


        private void Awake()
        {
            InitNativeLogger();
            InitComponents();
            InitializePluginNative();
            InitMainCamera();

            m_Frame = new UnityFrame();
        }

        private void Start()
        {
            Debug.Log($"<b>ARPG version {PLUGIN_VERSION}, native {GetVersion()}</b>");

            Load();
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

            m_NativeEventHandler = GetComponent<NativeEventHandler>();
            m_NativeEventHandler.m_OnPOIList = m_OnPOIList;
            m_NativeEventHandler.m_OnDistanceUpdated = m_OnDistanceUpdated;
            m_NativeEventHandler.m_OnStageChanged = m_OnStageChanged;
            m_NativeEventHandler.m_OnNavigationStarted = m_OnNavigationStarted;
            m_NativeEventHandler.m_OnNavigationEnded = m_OnNavigationEnded;
            m_NativeEventHandler.m_OnDestinationArrived = m_OnDestinationArrived;
            m_NativeEventHandler.m_OnTransitMovingStarted = m_OnTransitMovingStarted;
            m_NativeEventHandler.m_OnTransitMovingEnded = m_OnTransitMovingEnded;
        }

        private void InitNativeLogger()
        {
            m_NativeLogger = new NativeLogger();
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
            IntPtr versionPtr = GetVersionNative();
            return Marshal.PtrToStringAnsi(versionPtr);
        }

        /// <summary>
        /// amproj 파일을 로드합니다.
        /// </summary>
        /// <param name="location"></param>
        private void Load()
        {
            string locationName = Path.GetFileName(contentsPath);
            string amprojFilePath = Application.streamingAssetsPath + $"/{contentsPath}/{locationName}.amproj";

            LoadNative(amprojFilePath);
        }

        public void Reset()
        {
            // Reset 요청은 Native 영역 내부에서 즉시 실행되어야 한다.
            ResetNative();
            SetVLPassNative(false);
        }

        public string GetStageName() {
            return m_CurrStage;
        }

        public void SetStage(string stageName)
        {
            m_CurrStage = stageName;

            SetStageNative(stageName);
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
             LoadNavigationParams param = new LoadNavigationParams();

            var stageCodes = poiItem.stageName.Split('_');
            var endCode = stageCodes.Length == 0 ? "" : stageCodes[stageCodes.Length - 1];
            var coord = poiItem.entrance[0];

            param.endFloor = endCode;
            param.endPoints = new float[]{ (float) -coord[0], (float) coord[1], (float) coord[2] };
            param.connectionType = connectionType;
            
            m_PathFinder.LoadNavigation(param);
        }

        public void UnloadNavigation()
        {
            UnloadNavigationNative();
        }

        public void CheckResourceUpdated()
        {
            CheckResourceUpdatedNative();
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
    }
}
