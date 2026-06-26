using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ARCeye.ARSDK.Tests.PlayMode")]

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

        // ARPG_TRANSIT_UNKNOWN                  0
        // ARPG_TRANSIT_OBSERVE_VISUAL_CUES      1
        // ARPG_TRANSIT_OBSERVE_BAROMETER        2
        // ARPG_TRANSIT_PERSIST_RANGE_CONSTRAINT 3
        [HideInInspector]
        public UInt32 transitOption;

        // ARPG_FILESYSTEM_DEFAULT           0
        // ARPG_FILESYSTEM_UNITY             1
        [HideInInspector]
        public UInt32 filesystemOption;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ExternalTurnSpot
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string assetRelativePath;
        public int type;
        [MarshalAs(UnmanagedType.LPStr)]
        public string label;
        [MarshalAs(UnmanagedType.LPStr)]
        public string unit;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ExternalNextStep
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string assetRelativePath;
        public int type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ExternalMap
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string modelRelativePath;
        [MarshalAs(UnmanagedType.LPStr)]
        public string heightFieldRelativePath;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ExternalStage
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string stage;
        [MarshalAs(UnmanagedType.LPStr)]
        public string iblRelativePath;
        public IntPtr externalMap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ResourceConfiguration
    {
        public IntPtr turnspots;
        public int turnspotsCount;
        public IntPtr nextsteps;
        public int nextstepsCount;
        public IntPtr stages;
        public int stagesCount;
    }

    [DefaultExecutionOrder(-2000)]
    public class ARPlayGround : MonoBehaviour
    {
        const string PLUGIN_VERSION = "1.9.0";

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

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LoadNativeWithConfig(string amprojFilePath, ResourceConfiguration config);

        [DllImport(dll)]
        private static extern void ResetNative();

        [DllImport(dll)]
        private static extern void UpdateSceneNative(UnityFrame unityPose);

        [DllImport(dll)]
        private static extern void RenderNative(long frameTimeNanos);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TryUpdateStageNative(string stageName);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ForceUpdateStageNative(string stageName);

        [DllImport(dll)]
        private static extern void UnloadNavigationNative();

        [DllImport(dll)]
        private static extern void SetDestinationArrivalDistanceNative(float distance);


        [SerializeField]
        private string m_ContentsFolder;
        public string ContentsFolder
        {
            get => m_ContentsFolder;
            set => m_ContentsFolder = value;
        }

        public string amprojFilePath
        {
            get
            {
                string locationName = Path.GetFileName(ContentsFolder);
                string dataRootPath = StreamingAssets ? Application.streamingAssetsPath : Application.persistentDataPath;
                return $"{dataRootPath}/{ContentsFolder}/{locationName}.amproj";
            }
        }

        [field: SerializeField]
        public bool LoadOnAwake { get; set; } = true;

        [field: SerializeField]
        public bool StreamingAssets { get; set; } = true;

        [SerializeField]
        private Locale m_Locale;
        public Locale locale
        {
            get => m_Locale;
            set => m_Locale = value;
        }

        public LogLevel m_LogLevel = LogLevel.WARNING;
        [field: SerializeField]
        public bool VisualizeAMProj { get; set; } = true;

        [HideInInspector]
        public StageChangeMethod stageChangeMethod = StageChangeMethod.LayerInfo;


        private bool m_IsLoadingRequested = false;

        private bool m_IsLoaded = false;
        public bool IsLoaded => m_IsLoaded;

        private Camera m_MainCamera;

        private LayerInfoConverter m_LayerInfoConverter;
        private PathFinder m_PathFinder;
        private NetworkController m_NetworkController;
        private NativeLogger m_NativeLogger;
        private NativeEventHandler m_NativeEventHandler;
        private NativeFileSystemHelper m_NativeFileSystemHelper;

        private UnityFrame m_Frame;
        private string m_CurrStage;
        private string m_CurrStageLabel;
        private double m_CurrRelAltitude;


        private List<GameObject> m_NextStepGameObjects = new List<GameObject>();

        private ItemGenerator m_ItemGenerator;
        private NaviItemGenerator m_NaviSpotGenerator;
        private NextStep m_NextStep;
        [SerializeField]
        private StageConfig m_StageConfig;
        private AMProjVisualizer m_Visualizer;

        public bool IsNaviMode { get; private set; } = false;


        ///
        /// Events 
        ///
        public UnityEvent<string, string, string> m_OnSceneLoaded;
        public UnityEvent<string> m_OnSceneUnloaded;
        public UnityEvent<string, string> m_OnStageChanged;
        public UnityEvent<List<LayerPOIItem>> m_OnPOIListLoaded;
        [HideInInspector]
        [Obsolete("m_OnPOIList is deprecated, use m_OnPOIListLoaded instead.")]
        public UnityEvent<List<LayerPOIItem>> m_OnPOIList;
        public UnityEvent<string, string> m_OnCustomRangeEntered;
        public UnityEvent<string, string> m_OnCustomRangeExited;

        public UnityEvent m_OnNavigationStarted;
        public UnityEvent m_OnNavigationEnded;
        public UnityEvent m_OnNavigationFailed;
        public UnityEvent m_OnNavigationRerouted;
        public UnityEvent<float> m_OnDistanceUpdated;
        public UnityEvent m_OnDestinationArrived;
        public UnityEvent<ConnectionType, string, string> m_OnTransitMovingStarted;
        public UnityEvent m_OnTransitMovingEnded;
        public UnityEvent<string, string> m_OnTransitMovingFailed;


        // Transit의 목적지 스테이지 이름. Navi 모드일 경우에만 할당된다.
        private string m_TransitDestStageName;

        private Coroutine m_ShowARItemsCoroutine;


        private void Awake()
        {
            CheckARPGCondition();

            InitNativeLogger();
            InitComponents();

            // #if !(UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN)
            //             // 실제 기기에서만 locale 자동 업데이트.
            //             // Editor에서는 테스트를 위해 자동 업데이트를 방지.
            //             DetectLocale();
            // #endif

            ARPGConfiguration config = new ARPGConfiguration();
            config.languageCode = LocaleConverter.GetLanguageCode(locale);
            config.countryCode = LocaleConverter.GetCountryCode(locale);

            if (stageChangeMethod == StageChangeMethod.LayerInfo)
            {
                config.transitOption = 1;
            }
            else if (stageChangeMethod == StageChangeMethod.Barometer)
            {
                config.transitOption = 7;
            }

#if !UNITY_EDITOR && UNITY_ANDROID
            config.filesystemOption = (UInt32) (StreamingAssets ? 1 : 0);
#else
            config.filesystemOption = 0;
#endif

            InitializePluginNative(config);
            InitMainCamera();

            m_Frame = new UnityFrame();
        }

        private void Start()
        {
            NativeLogger.Print(LogLevel.INFO, $"<b>ARSDK version {PLUGIN_VERSION}, native {GetVersion()}</b>");
            m_NativeLogger.SetLogLevel(m_LogLevel);

            if (LoadOnAwake)
            {
                Load();
            }
        }

        private void LateUpdate()
        {
            // Camera의 view matrix와 pose matrix를 계산해서 ARPlayGround를 갱신.
            Matrix4x4 poseMatrix = Matrix4x4.TRS(m_MainCamera.transform.position, m_MainCamera.transform.rotation, Vector3.one);
            Matrix4x4 viewMatrix = Matrix4x4.Inverse(poseMatrix);
            Matrix4x4 projMatrix = m_MainCamera.projectionMatrix;

            UpdateScene(viewMatrix, projMatrix);

            long frameNanoTime = (long)Time.deltaTime * 1000000000;
            RenderNative(frameNanoTime);
        }

        /// <summary>
        ///   매 프레임마다 실행 되어야 하는 업데이트 메서드.
        /// </summary>
        internal void UpdateScene(Matrix4x4 viewMatrix, Matrix4x4 projMatrix)
        {
            // convert left handed to right handed.
            var rhViewMatirx = PoseHelper.ConvertLHRHView(viewMatrix).transpose;
            projMatrix.m23 = -1;

            m_Frame.viewMatrix = rhViewMatirx.ToDataDouble();
            m_Frame.projMatrix = projMatrix.ToData();
            m_Frame.relaltitude = m_CurrRelAltitude;

            UpdateSceneNative(m_Frame);
        }

        private void OnDestroy()
        {
            m_NativeLogger.Release();
            DestroyPluginNative();
        }

        /// <summary>
        ///   ARSDK가 정상적으로 설정되었는지 확인.
        /// </summary>
        private void CheckARPGCondition()
        {
            // ItemGenerator 추가 여부 확인.
            var itemGenerator = FindFirstObjectByType<ItemGenerator>();
            if (itemGenerator == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[ARPlayGround] ItemGenerator is not added to the scene. Please add ARPG/Core/Prefabs/ItemGenerator.prefab.");
            }

            // MapCameraController 추가 여부 확인.
            var mapCameraController = FindFirstObjectByType<MapCameraController>();
            if (mapCameraController == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[ARPlayGround] MapCameraController is not added to the scene. Please add ARPG/Core/Prefabs/MapCameraController.prefab.");
            }

            // NextStep 추가 여부 확인.
            var nextStep = FindFirstObjectByType<NextStep>();
            if (nextStep == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[ARPlayGround] NextStep is not added to the scene. Please add ARPG/Core/Prefabs/NextStep.prefab.");
            }
        }


        private void InitComponents()
        {
            m_NetworkController = GetComponent<NetworkController>();
            m_PathFinder = GetComponent<PathFinder>();

            m_LayerInfoConverter = GetComponent<LayerInfoConverter>();
            m_Visualizer = GetComponent<AMProjVisualizer>();

            m_ItemGenerator = FindFirstObjectByType<ItemGenerator>();
            if (m_ItemGenerator == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[ARPlayGround] Failed to find ItemGenerator.");
            }

            m_NaviSpotGenerator = FindFirstObjectByType<NaviItemGenerator>();
            m_NextStep = FindFirstObjectByType<NextStep>();
            m_NativeEventHandler = GetComponent<NativeEventHandler>();
            m_NativeEventHandler.m_OnPOIList = m_OnPOIListLoaded;
            m_NativeEventHandler.m_OnDistanceUpdated = m_OnDistanceUpdated;
            m_NativeEventHandler.m_OnStageChanged = m_OnStageChanged;
            m_NativeEventHandler.m_OnNavigationStarted = m_OnNavigationStarted;
            m_NativeEventHandler.m_OnNavigationEnded = m_OnNavigationEnded;
            m_NativeEventHandler.m_OnNavigationFailed = m_OnNavigationFailed;
            m_NativeEventHandler.m_OnNavigationRerouted = m_OnNavigationRerouted;
            m_NativeEventHandler.m_OnDestinationArrived = m_OnDestinationArrived;
            m_NativeEventHandler.m_OnTransitMovingStarted = m_OnTransitMovingStarted;
            m_NativeEventHandler.m_OnTransitMovingEnded = m_OnTransitMovingEnded;
            m_NativeEventHandler.m_OnCustomRangeEntered = m_OnCustomRangeEntered;
            m_NativeEventHandler.m_OnCustomRangeExited = m_OnCustomRangeExited;
            m_NativeEventHandler.m_OnCameraPoseUpdated.AddListener((p, r) => OnCameraPoseUpdated(p, r));

            m_NativeEventHandler.m_OnStageChanged.AddListener((name, label) => { OnDrawAMProj(name, label); m_CurrStageLabel = label; });
            m_NativeEventHandler.m_OnNavigationStarted.AddListener(() => { IsNaviMode = true; });
            m_NativeEventHandler.m_OnNavigationEnded.AddListener(() => { IsNaviMode = false; });
            m_NativeEventHandler.m_OnNavigationFailed.AddListener(() => { IsNaviMode = false; });
            m_NativeEventHandler.m_OnTransitMovingStarted.AddListener((type, dest, label) => OnTransitMovingStarted(type, dest, label));
            m_NativeEventHandler.m_OnTransitMovingEnded.AddListener(() => OnTransitMovingEnded());

            m_NativeEventHandler.m_OnSceneLoaded.AddListener((keyname, crscode, localeStr) =>
            {
                switch (localeStr)
                {
                    case "en_US":
                        locale = Locale.en_US;
                        break;
                    case "ko_KR":
                        locale = Locale.ko_KR;
                        break;
                    case "zh_CN":
                        locale = Locale.zh_CN;
                        break;
                    case "zh_TW":
                        locale = Locale.zh_TW;
                        break;
                    case "ja_JP":
                        locale = Locale.ja_JP;
                        break;
                    case "fr_FR":
                        locale = Locale.fr_FR;
                        break;
                    case "de_DE":
                        locale = Locale.de_DE;
                        break;
                    case "it_IT":
                        locale = Locale.it_IT;
                        break;
                    case "pt_BR":
                        locale = Locale.pt_BR;
                        break;
                    case "es_ES":
                        locale = Locale.es_ES;
                        break;
                    default:
                        break;
                }

                m_OnSceneLoaded.Invoke(keyname, crscode, localeStr);
            });

            m_NativeEventHandler.m_OnSceneUnloaded = m_OnSceneUnloaded;

            // Assign default audio and video events.
            m_NativeEventHandler.m_OnVideoLoaded.AddListener(info =>
            {
                m_ItemGenerator.OnVideoLoaded(info);
            });
            m_NativeEventHandler.m_OnVideoPlaying.AddListener((uuid, playerType, distance) =>
            {
                m_ItemGenerator.OnVideoPlaying(uuid, playerType, distance);
            });
            m_NativeEventHandler.m_OnVideoUnloaded.AddListener((uuid, playerType, ignoreFade) =>
            {
                m_ItemGenerator.OnVideoUnloaded(uuid, playerType, ignoreFade);
            });
            m_NativeEventHandler.m_OnAudioLoaded.AddListener(info =>
            {
                m_ItemGenerator.OnAudioLoaded(info);
            });
            m_NativeEventHandler.m_OnAudioPlaying.AddListener((uuid, playerType, distance) =>
            {
                m_ItemGenerator.OnAudioPlaying(uuid, playerType, distance);
            });
            m_NativeEventHandler.m_OnAudioUnloaded.AddListener((uuid, playerType, ignoreFade) =>
            {
                m_ItemGenerator.OnAudioUnloaded(uuid, playerType, ignoreFade);
            });

            m_NativeFileSystemHelper = GetComponent<NativeFileSystemHelper>();
#if !UNITY_EDITOR && UNITY_ANDROID
            m_NativeFileSystemHelper.useAndroidStreamingAssets = StreamingAssets;
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

            ShowARItems();

            CameraUtil.RemoveCullingMask(m_MainCamera, "Map");
            CameraUtil.RemoveCullingMask(m_MainCamera, "MapPOI");
            CameraUtil.RemoveCullingMask(m_MainCamera, "MapArrow");
            CameraUtil.RemoveCullingMask(m_MainCamera, "UI");

            if (VisualizeAMProj)
            {
                CameraUtil.AddCullingMask(m_MainCamera, "AMProjViz");
            }
        }

        public string GetVersion()
        {
            IntPtr versionPtr = ARPG_GetVersionNative();
            return Marshal.PtrToStringAnsi(versionPtr);
        }

        private void DetectLocale()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    locale = Locale.en_US;
                    break;
                case SystemLanguage.Korean:
                    locale = Locale.ko_KR;
                    break;
                case SystemLanguage.Chinese:
                    locale = Locale.zh_CN;
                    break;
                case SystemLanguage.ChineseTraditional:
                    locale = Locale.zh_TW;
                    break;
                case SystemLanguage.Japanese:
                    locale = Locale.ja_JP;
                    break;
                case SystemLanguage.French:
                    locale = Locale.fr_FR;
                    break;
                case SystemLanguage.German:
                    locale = Locale.de_DE;
                    break;
                case SystemLanguage.Italian:
                    locale = Locale.it_IT;
                    break;
                case SystemLanguage.Portuguese:
                    locale = Locale.pt_BR;
                    break;
                case SystemLanguage.Spanish:
                    locale = Locale.es_ES;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 코루틴을 이용하여 amproj 파일을 로드합니다.
        /// </summary>
        public void Load(System.Action completeCallback = null)
        {
            if (m_IsLoadingRequested)
            {
                NativeLogger.Print(LogLevel.WARNING, "[ARPlayGround] Load has already been called.");
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
            StartCoroutine(LoadInternal(filePath, completeCallback));
        }

        private IEnumerator LoadInternal(string filePath, System.Action completeCallback)
        {
            int version = 1;
            yield return AMProjFileReader.ReadVersionCoroutine(filePath, v => version = v);

            LoadAmproj(filePath, version);

            yield return new WaitUntil(() => m_NativeFileSystemHelper.isReadingComplete);

            NativeLogger.Print(LogLevel.INFO, "[ARPlayGround] amproj file loaded successfully.");

            LoadLayerInfo(version);

            if (LoadOnAwake && VisualizeAMProj)
            {
                NativeLogger.Print(LogLevel.INFO, "[ARPlayGround] amproj visualizer enabled.");
                m_Visualizer.Load(amprojFilePath);
            }

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
            int version = await AMProjFileReader.ReadVersionAsync(filePath);
            LoadAmproj(filePath, version);

            LoadLayerInfo(version);

            if (LoadOnAwake && VisualizeAMProj)
            {
                NativeLogger.Print(LogLevel.INFO, "[ARPlayGround] amproj visualizer enabled.");
                m_Visualizer.Load(amprojFilePath);
            }

            await TaskUtil.WaitUntil(() => { return m_NativeFileSystemHelper.isReadingComplete; });

            NativeLogger.Print(LogLevel.INFO, "[ARPlayGround] amproj file loaded successfully.");

            m_IsLoaded = true;
        }

        private void LoadAmproj(string filePath, int version)
        {
            var resourceConfig = BuildResourceConfiguration();

            if (version >= 3)
            {
                LoadNativeWithConfig(filePath, resourceConfig);
            }
            else
            {
                LoadNative(filePath);
            }

            FreeResourceConfiguration(resourceConfig);
        }

        private ResourceConfiguration BuildResourceConfiguration()
        {
            var config = new ResourceConfiguration();

            if (m_NaviSpotGenerator == null || m_NaviSpotGenerator.TurnSpotPrefab == null)
                return config;

            // TurnSpot
            var prefab = m_NaviSpotGenerator.TurnSpotPrefab;
            var turnSpots = new ExternalTurnSpot[]
            {
                new ExternalTurnSpot { assetRelativePath = prefab.TurnSpotLeft,     type = 0 },
                new ExternalTurnSpot { assetRelativePath = prefab.TurnSpotRight,    type = 1 },
                new ExternalTurnSpot { assetRelativePath = prefab.TurnSpotStraight, type = 2 },
                new ExternalTurnSpot { assetRelativePath = prefab.TurnSpotUp,       type = 3 },
                new ExternalTurnSpot { assetRelativePath = prefab.TurnSpotDown,     type = 4 },
                new ExternalTurnSpot { assetRelativePath = prefab.Destination,      type = 5 },
            };

            int turnSpotSize = Marshal.SizeOf<ExternalTurnSpot>();
            config.turnspots = Marshal.AllocHGlobal(turnSpotSize * turnSpots.Length);
            config.turnspotsCount = turnSpots.Length;
            for (int i = 0; i < turnSpots.Length; i++)
            {
                Marshal.StructureToPtr(turnSpots[i], config.turnspots + turnSpotSize * i, false);
            }

            if (m_NextStep == null)
                return config;

            // NextStep
            var nextSteps = new ExternalNextStep[]
            {
                new ExternalNextStep { assetRelativePath = m_NextStep.NextStepArrow, type = 0 },
                new ExternalNextStep { assetRelativePath = m_NextStep.NextStepDot,   type = 1 },
                new ExternalNextStep { assetRelativePath = m_NextStep.NextStepText,  type = 2 },
            };

            int nextStepSize = Marshal.SizeOf<ExternalNextStep>();
            config.nextsteps = Marshal.AllocHGlobal(nextStepSize * nextSteps.Length);
            config.nextstepsCount = nextSteps.Length;
            for (int i = 0; i < nextSteps.Length; i++)
            {
                Marshal.StructureToPtr(nextSteps[i], config.nextsteps + nextStepSize * i, false);
            }

            if (m_StageConfig == null || m_StageConfig.stages.Count == 0)
                return config;

            // Stage — 외부 리소스(ibl, mapModel, mapHeightField)가 하나라도 있는 스테이지만 전달.
            var externalStages = m_StageConfig.stages.FindAll(s =>
                !string.IsNullOrEmpty(s.ibl) ||
                !string.IsNullOrEmpty(s.mapModel) ||
                !string.IsNullOrEmpty(s.mapHeightField));

            if (externalStages.Count == 0)
                return config;

            int stageSize = Marshal.SizeOf<ExternalStage>();
            config.stages = Marshal.AllocHGlobal(stageSize * externalStages.Count);
            config.stagesCount = externalStages.Count;
            for (int i = 0; i < externalStages.Count; i++)
            {
                var src = externalStages[i];
                var externalStage = new ExternalStage
                {
                    stage = src.stage,
                    iblRelativePath = src.ibl,
                    externalMap = IntPtr.Zero,
                };

                if (!string.IsNullOrEmpty(src.mapModel) ||
                    !string.IsNullOrEmpty(src.mapHeightField))
                {
                    var map = new ExternalMap
                    {
                        modelRelativePath = src.mapModel,
                        heightFieldRelativePath = src.mapHeightField,
                    };
                    externalStage.externalMap = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalMap>());
                    Marshal.StructureToPtr(map, externalStage.externalMap, false);
                }

                Marshal.StructureToPtr(externalStage, config.stages + stageSize * i, false);
            }

            return config;
        }

        private void FreeResourceConfiguration(ResourceConfiguration config)
        {
            if (config.turnspots != IntPtr.Zero)
            {
                int turnSpotSize = Marshal.SizeOf<ExternalTurnSpot>();
                for (int i = 0; i < config.turnspotsCount; i++)
                {
                    Marshal.DestroyStructure<ExternalTurnSpot>(config.turnspots + turnSpotSize * i);
                }
                Marshal.FreeHGlobal(config.turnspots);
            }

            if (config.nextsteps != IntPtr.Zero)
            {
                int nextStepSize = Marshal.SizeOf<ExternalNextStep>();
                for (int i = 0; i < config.nextstepsCount; i++)
                {
                    Marshal.DestroyStructure<ExternalNextStep>(config.nextsteps + nextStepSize * i);
                }
                Marshal.FreeHGlobal(config.nextsteps);
            }

            if (config.stages != IntPtr.Zero)
            {
                int stageSize = Marshal.SizeOf<ExternalStage>();
                for (int i = 0; i < config.stagesCount; i++)
                {
                    var stagePtr = config.stages + stageSize * i;
                    var stage = Marshal.PtrToStructure<ExternalStage>(stagePtr);
                    if (stage.externalMap != IntPtr.Zero)
                    {
                        Marshal.DestroyStructure<ExternalMap>(stage.externalMap);
                        Marshal.FreeHGlobal(stage.externalMap);
                    }
                    Marshal.DestroyStructure<ExternalStage>(stagePtr);
                }
                Marshal.FreeHGlobal(config.stages);
            }
        }

        public void Reset()
        {
            // ARSDK 레벨 요소 리셋. 
            m_TransitDestStageName = "";

            // Reset 요청은 Native 영역 내부에서 즉시 실행되어야 한다.
            ResetNative();

            if (VisualizeAMProj && m_Visualizer != null)
            {
                m_Visualizer.Reset();
            }
        }

        public string GetStageName()
        {
            CheckAMProjLoaded();

            if (m_CurrStage == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[ARPlayGround] Current stage is not assigned. Ensure SetStage has been called.");
            }

            return m_CurrStage;
        }

        public string GetStageLabel()
        {
            if (m_CurrStageLabel == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[ARPlayGround] Current stage label is not assigned. Ensure SetStage has been called.");
            }

            return m_CurrStageLabel;
        }

        public void TryUpdateStage(string stageName)
        {
            // Transit 목적지가 없는 경우 스테이지 전환 시도.
            if (string.IsNullOrEmpty(m_TransitDestStageName))
            {
                StartCoroutine(TryUpdateStageInternal(stageName));
            }
            // Transit 목적지와 전환을 시도하는 목적지가 같은 경우.
            else if (m_TransitDestStageName == stageName)
            {
                StartCoroutine(TryUpdateStageInternal(stageName));
            }
            else
            {
                // 인식된 stage 이름, 목적지 stage 이름.
                m_OnTransitMovingFailed?.Invoke(stageName, m_TransitDestStageName);
            }
        }

        public void ForceUpdateStage(string stageName)
        {
            ForceUpdateStageInternal(stageName);
        }

        public void SetRelativeAltitude(double value)
        {
            m_CurrRelAltitude = value;
        }

        public void SetDestinationArrivalDistance(float distance)
        {
            if (distance <= 0.0f)
            {
                Debug.LogError($"[ARPlayGround] SetDestinationArrivalDistance: distance must be greater than 0. (input: {distance})");
                return;
            }

            SetDestinationArrivalDistanceNative(distance);
        }

        private IEnumerator TryUpdateStageInternal(string stageName)
        {
            // Localization이 완료된 이후 SetStageNative 진행.
            if (m_MainCamera == null || m_MainCamera.transform.parent == null)
            {
                yield return null;
            }
            else
            {
                yield return new WaitWhile(() =>
                {
                    Vector3 originPosition = m_MainCamera.transform.parent.localPosition;
                    NativeLogger.Print(LogLevel.VERBOSE, "[ARPlayGround] Waiting localization");
                    return Vector3.Distance(originPosition, Vector3.zero) < 0.001f;
                });
            }

            MainThreadDispatcher.Instance().Enqueue(() =>
            {
                CheckAMProjLoaded();

                NativeLogger.Print(LogLevel.VERBOSE, $"[ARPlayGround] SetStage called. stageName={stageName}");
                m_CurrStage = stageName;

                TryUpdateStageNative(stageName);
            });
        }

        private void ForceUpdateStageInternal(string stageName)
        {
            MainThreadDispatcher.Instance().Enqueue(() =>
            {
                CheckAMProjLoaded();

                NativeLogger.Print(LogLevel.VERBOSE, $"[ARPlayGround] SetStage called. stageName={stageName}");
                m_CurrStage = stageName;

                ForceUpdateStageNative(stageName);
            });
        }

        private void LoadLayerInfo(int version)
        {
            if (version >= 3 && m_StageConfig != null)
            {
                m_LayerInfoConverter.Load(m_StageConfig);
            }
            else
            {
                m_LayerInfoConverter.Load();
            }
        }

        /// <summary>
        /// VL에서 전달 받은 LayerInfo 값과 매칭되는 Stage를 로드한다.
        /// </summary>
        /// <param name="layerInfo"></param>
        public void SetLayerInfo(string layerInfo, bool forceUpdate = false)
        {
            CheckAMProjLoaded();

            string stageName = m_LayerInfoConverter.Convert(layerInfo);

            if (forceUpdate)
                ForceUpdateStage(stageName);
            else
                TryUpdateStage(stageName);
        }

        public void OnStageChanged(string name, string label)
        {
            m_OnStageChanged.Invoke(name, label);
        }

        private void OnDrawAMProj(string name, string label)
        {
            if (VisualizeAMProj)
            {
                m_Visualizer.Visualize(name);
            }
        }

        private void OnTransitMovingStarted(ConnectionType connectionType, string destStageName, string destStageLabel)
        {
            // Transit에 진입할 때 NextStep과 관련된 요소들 제거.
            m_NextStepGameObjects.Clear();

            m_TransitDestStageName = destStageName;

            if (VisualizeAMProj)
            {
                m_Visualizer.Reset();
            }
        }

        private void OnTransitMovingEnded()
        {
            m_TransitDestStageName = "";
        }

        public void SetVLPass(bool value)
        {
            StartCoroutine(SetVLPassInternal(value));
        }

        private IEnumerator SetVLPassInternal(bool value)
        {
            // ARPG 내부 로직으로 인해 한 프레임을 건너뛴다.
            //   VL 인식 직후 ANTracker의 내부에서 OnStateChanged를 호출할때 본 메서드가 호출되며 ARPG를 리셋하고 경탐을 요청하는 로직이 수행됨.
            //   VL 인식 직후이기 때문에 실제 ANTracker의 pose는 갱신되지 않은 상태에서 경탐을 요청
            //   이로 인해 원점에서 경탐을 요청하는 문제 발생.
            //   이 문제를 해결하기 위해 한 프레임을 스킵하여 ANTracker의 pose를 갱신한 뒤에 ARPG 리셋을 진행하는 방식으로 변경.
            yield return null;

            if (!value)
            {
                Reset();
            }
        }

        public void LoadNavigation(Vector3 coord, string stageName, PathFindingType pathFindingType = PathFindingType.Default)
        {
            LayerPOIItem poiItem = new LayerPOIItem();

            poiItem.entrance = new List<Vector3>();
            poiItem.entrance.Add(coord);
            poiItem.stageName = stageName;

            LoadNavigation(poiItem, pathFindingType);
        }

        public void LoadNavigation(LayerPOIItem poiItem, PathFindingType pathFindingType = PathFindingType.Default)
        {
            // 내비게이션 로딩을 요청할 때 NextStep과 관련된 요소들 제거.
            m_NextStepGameObjects.Clear();

            CheckAMProjLoaded();

            LoadNavigationParams param = new LoadNavigationParams();

            param.endStage = poiItem.stageName;

            // entrance 좌표값을 raw pointer로 변경.
            float[] entrances = new float[poiItem.entrance.Count * 3];
            for (int i = 0; i < poiItem.entrance.Count; i++)
            {
                entrances[i * 3 + 0] = poiItem.entrance[i].x;
                entrances[i * 3 + 1] = poiItem.entrance[i].y;
                entrances[i * 3 + 2] = poiItem.entrance[i].z;
            }

            GCHandle endPointsHandle = GCHandle.Alloc(entrances, GCHandleType.Pinned);
            IntPtr endPointsPtr = endPointsHandle.AddrOfPinnedObject();

            param.endPoints = endPointsPtr;
            param.count = entrances.Length;
            param.pathFindingType = pathFindingType;

            m_PathFinder.LoadNavigation(param);

            endPointsHandle.Free();
        }

        public void UnloadNavigation()
        {
            UnloadNavigationNative();
            OnDrawAMProj(m_CurrStage, m_CurrStageLabel);
        }


        public void ActivateNextStep(bool value)
        {
            if (m_NextStepGameObjects.Count == 0 || m_NextStepGameObjects[0] == null)
            {
                m_NextStepGameObjects.Clear();

                UnityNextStepArrow[] arrows = FindObjectsByType<UnityNextStepArrow>(FindObjectsSortMode.None);
                UnityNextStepDot[] dots = FindObjectsByType<UnityNextStepDot>(FindObjectsSortMode.None);
                UnityNextStepText[] texts = FindObjectsByType<UnityNextStepText>(FindObjectsSortMode.None);

                foreach (var elem in arrows)
                    m_NextStepGameObjects.Add(elem.gameObject);
                foreach (var elem in dots)
                    m_NextStepGameObjects.Add(elem.gameObject);
                foreach (var elem in texts)
                    m_NextStepGameObjects.Add(elem.gameObject);
            }

            foreach (var elem in m_NextStepGameObjects)
            {
                if (elem)
                {
                    elem.gameObject.SetActive(value);
                }
            }
        }

        /// <summary>
        ///   증강되는 물체들이 화면에 렌더링 되도록 설정. 카메라의 culling mask에 ARItem을 추가한다.
        /// </summary>
        public void ShowARItems(float delay = 0.0f)
        {
            m_ShowARItemsCoroutine = StartCoroutine(ShowARItemsInternal(delay));
        }

        // ARPG의 내부 로직으로 인해 TurnSpot과 같은 요소들은 활성화 시 기본 상태를 먼저 보여준 뒤 각종 효과가 실행됨.
        // 기본 상태를 먼저 보여주는 것으로 인해 화면이 순간 깜빡거릴 수 있음.
        // 이를 방지하기 위해 delay를 추가하여 기본 상태를 실행 완료한 후 ARItem을 시각화.
        private IEnumerator ShowARItemsInternal(float delay)
        {
            yield return new WaitForSeconds(delay);
            CameraUtil.AddCullingMask(m_MainCamera, "ARItem");
            m_ShowARItemsCoroutine = null;
        }

        /// <summary>
        ///   증강되는 물체들이 화면에 렌더링 되지 않도록. 카메라의 culling mask에 ARItem을 제외한다.
        /// </summary>
        public void HideARItems()
        {
            if (m_ShowARItemsCoroutine != null)
            {
                StopCoroutine(m_ShowARItemsCoroutine);
                m_ShowARItemsCoroutine = null;
            }

            CameraUtil.RemoveCullingMask(m_MainCamera, "ARItem");
        }

        private void CheckAMProjLoaded()
        {
            if (!m_IsLoaded)
            {
                NativeLogger.Print(LogLevel.WARNING, "[ARPlayGround] amproj file is not loaded. Please ensure Load has been called.");
            }
        }
        private void OnCameraPoseUpdated(Vector3 position, Quaternion rotation)
        {
            // ARPG를 통해 계산된 camera position을 이용하여 scene의 바닥 높이를 계산. 
            m_ItemGenerator.UpdateSceneHeight(position.y);
        }
    }
}
