using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using AOT;

namespace ARCeye
{
    public class NativeEventHandler : MonoBehaviour
    {
        private static NativeEventHandler s_Instance;

        /* -- Events --*/
        [HideInInspector]
        public UnityEvent<float, bool, int> m_OnDownloadProgressUpdated;
        [HideInInspector]
        public UnityEvent<string, string> m_OnStageChanged;
        [HideInInspector]
        public UnityEvent<string, string, string> m_OnSceneLoaded;
        [HideInInspector]
        public UnityEvent<string> m_OnSceneUnloaded;
        [HideInInspector]
        public UnityEvent<UnityUILayerInfo> m_OnUIChanged;
        [HideInInspector]
        public UnityEvent<List<LayerPOIItem>> m_OnPOIList;
        [HideInInspector]
        public UnityEvent<bool> m_OnResourceUpdated;
        [HideInInspector]
        public UnityEvent m_OnNavigationStarted;
        [HideInInspector]
        public UnityEvent<float> m_OnDistanceUpdated;
        [HideInInspector]
        public UnityEvent m_OnDestinationArrived;
        [HideInInspector]
        public UnityEvent m_OnNavigationEnded;
        [HideInInspector]
        public UnityEvent m_OnNavigationFailed;
        [HideInInspector]
        public UnityEvent m_OnNavigationReSearched;
        [HideInInspector]
        public UnityEvent<ConnectionType, string, string> m_OnTransitMovingStarted;
        [HideInInspector]
        public UnityEvent m_OnTransitMovingEnded;
        [HideInInspector]
        public UnityEvent<string, string> m_OnCustomRangeEntered;
        [HideInInspector]
        public UnityEvent<string, string> m_OnCustomRangeExited;
        [HideInInspector]
        public UnityEvent<Vector3, Quaternion> m_OnCameraPoseUpdated;
        [HideInInspector]
        public UnityEvent<UnityMediaInfo> m_OnVideoLoaded;
        [HideInInspector]
        public UnityEvent<string, int, float> m_OnVideoPlaying;
        [HideInInspector]
        public UnityEvent<string, int, bool> m_OnVideoUnloaded;
        [HideInInspector]
        public UnityEvent<UnityMediaInfo> m_OnAudioLoaded;
        [HideInInspector]
        public UnityEvent<string, int, float> m_OnAudioPlaying;
        [HideInInspector]
        public UnityEvent<string, int, bool> m_OnAudioUnloaded;


        /* -- Plugin connections --*/

        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif

        public delegate void V_Func();
        public delegate void B_Func(bool b);
        public delegate void I_Func(int i);
        public delegate void IS_Func(int i, string s);
        public delegate void ISS_Func(int i, string s1, string s2);
        public delegate void F_Func(float f);
        public delegate void FFFFFFF_Func(float f1, float f2, float f3, float f4, float f5, float f6, float f7);
        public delegate void S_Func(string f);
        public delegate void SS_Func(string s1, string s2);
        public delegate void SSS_Func(string s1, string s2, string s3);
        public delegate void III_Func(int i1, int i2, int i3);
        public delegate void SIS_Func(string s, int i, float f);
        public delegate void SIB_Func(string s, int i, bool b);
        public delegate void FBI_Func(float f, bool b, int i);
        public delegate void SSFpFpFp_Func(string s1, string s2, IntPtr fp1, IntPtr fp2, IntPtr fp3);
        public delegate void UILayerInfo_Func(UnityUILayerInfo info);
        public delegate void LayerPOIItem_Func(IntPtr itemsPtr, int count);
        public delegate void MediaInfo_Func(UnityMediaInfo info);
        public delegate void ISIBBFFFFI_Func(int i1, string s2, int i2, bool b1, bool b2, float f1, float f2, float f3, float f4, int i3);

        [DllImport(dll)] private static extern void SetOnAppConfigFuncNative(SSFpFpFp_Func func);
        [DllImport(dll)] private static extern void SetOnUIChangedFuncNative(UILayerInfo_Func func);
        [DllImport(dll)] private static extern void SetOnPOIListFuncNative(LayerPOIItem_Func func);
        [DllImport(dll)] private static extern void SetOnStageChangedFuncNative(SS_Func func);
        [DllImport(dll)] private static extern void SetOnSceneLoadedFuncNative(SSS_Func func);
        [DllImport(dll)] private static extern void SetOnSceneUnloadedFuncNative(S_Func func);
        [DllImport(dll)] private static extern void SetOnNavigationStartedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnRemainDistanceFuncNative(F_Func func);
        [DllImport(dll)] private static extern void SetOnDestinationArrivedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnNavigationEndedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnNavigationFailedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnNavigationReSearchedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnTransitMovingStartedFuncNative(ISS_Func func);
        [DllImport(dll)] private static extern void SetOnTransitMovingEndedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnCustomRangeEnteredFuncNative(SS_Func func);
        [DllImport(dll)] private static extern void SetOnCustomRangeExitedFuncNative(SS_Func func);
        [DllImport(dll)] private static extern void SetOnUpdatePoseFuncNative(FFFFFFF_Func func);

        [DllImport(dll)] private static extern void SetOnVideoLoadedFuncNative(MediaInfo_Func func);
        [DllImport(dll)] private static extern void SetOnVideoPlayingFuncNative(SIS_Func func);
        [DllImport(dll)] private static extern void SetOnVideoUnloadedFuncNative(SIB_Func func);
        [DllImport(dll)] private static extern void SetOnAudioLoadedFuncNative(MediaInfo_Func func);
        [DllImport(dll)] private static extern void SetOnAudioPlayingFuncNative(SIS_Func func);
        [DllImport(dll)] private static extern void SetOnAudioUnloadedFuncNative(SIB_Func func);

        private void Awake() {
            s_Instance = this;

            SetOnAppConfigFuncNative(OnAppConfig);
            SetOnUIChangedFuncNative(OnUIChanged);
            SetOnPOIListFuncNative(OnPOIList);
            SetOnRemainDistanceFuncNative(OnRemainDistance);
            SetOnStageChangedFuncNative(OnStageChanged);
            SetOnSceneLoadedFuncNative(OnSceneLoaded);
            SetOnSceneUnloadedFuncNative(OnSceneUnloaded);
            SetOnNavigationStartedFuncNative(OnNavigationStarted);
            SetOnNavigationEndedFuncNative(OnNavigationEnded);
            SetOnNavigationFailedFuncNative(OnNavigationFailed);
            SetOnNavigationReSearchedFuncNative(OnNavigationReSearched);
            SetOnDestinationArrivedFuncNative(OnDestinationArrived);
            SetOnTransitMovingStartedFuncNative(OnTransitMovingStarted);
            SetOnTransitMovingEndedFuncNative(OnTransitMovingEnded);
            SetOnCustomRangeEnteredFuncNative(OnCustomRangeEntered);
            SetOnCustomRangeExitedFuncNative(OnCustomRangeExited);
            SetOnUpdatePoseFuncNative(OnUpdatePose);
            SetOnVideoLoadedFuncNative(OnVideoLoaded);
            SetOnVideoPlayingFuncNative(OnVideoPlaying);
            SetOnVideoUnloadedFuncNative(OnVideoUnloaded);
            SetOnAudioLoadedFuncNative(OnAudioLoaded);
            SetOnAudioPlayingFuncNative(OnAudioPlaying);
            SetOnAudioUnloadedFuncNative(OnAudioUnloaded);
        }

        [MonoPInvokeCallback(typeof(III_Func))]
        private static void onVersionInfo(int latestVer, int minVer, int errorCode)
        {

        }

        [MonoPInvokeCallback(typeof(FBI_Func))]
        private static void onDownloadProgress(float fPercentage, bool isComplete, int errorCode)
        {

        }

        [MonoPInvokeCallback(typeof(SSFpFpFp_Func))]
        unsafe private static void OnAppConfig(string iconName, string fontStyle, IntPtr foregroundColorPtr, IntPtr strokeColorPtr, IntPtr backgroundColorPtr) {
            float[] foregroundColor = new float[16];
            float[] strokeColor = new float[16];
            float[] backgroundColor = new float[16];

            Marshal.Copy(foregroundColorPtr, foregroundColor, 0, 4);
            Marshal.Copy(strokeColorPtr, strokeColor, 0, 4);
            Marshal.Copy(backgroundColorPtr, backgroundColor, 0, 4);
        }

        [MonoPInvokeCallback(typeof(UILayerInfo_Func))]
        private static void OnUIChanged(UnityUILayerInfo layerInfo) {
            s_Instance.m_OnUIChanged.Invoke(layerInfo);
        }

        [MonoPInvokeCallback(typeof(LayerPOIItem_Func))]
        private unsafe static void OnPOIList(IntPtr itemsPtr, int count) {
            // 구조체 포인터의 배열을 가리키는 포인터를 구조체 포인터의 배열로 변환.
            IntPtr[] layerPOIsPtr = new IntPtr[count];
            Marshal.Copy(itemsPtr, layerPOIsPtr, 0, count);

            // 구조체 포인터 배열의 각 요소들을 C# 구조체로 변경.
            // UnityLayerPOIItem[] layerPOIs = new UnityLayerPOIItem[count];

            List<LayerPOIItem> layerPOIs = new List<LayerPOIItem>();

            for(int i=0 ; i<count ; i++)
            {
                // entrace 요소가 아직 native 메모리에 배치된 상태.
                UnityLayerPOIItem nativeItem = Marshal.PtrToStructure<UnityLayerPOIItem>(layerPOIsPtr[i]);

                // C# 구조체 인스턴스로 복사.
                LayerPOIItem item = new LayerPOIItem();

                item.uuid = nativeItem.uuid;
                item.name = nativeItem.name;
                item.fullName = nativeItem.full_name;
                item.stageName = nativeItem.stage_name;
                item.stageLabel = nativeItem.stage_label;
                item.display = nativeItem.display;
                item.category = nativeItem.category;
                item.dpcode = nativeItem.dpcode;
                item.entrance = new List<Vector3>();
                item.usage = nativeItem.usage;

                // 입구점 좌표 배열의 포인터를 입구점 좌표 배열로 변환.
                float[] entrance = new float[nativeItem.coord_count];
                Marshal.Copy(nativeItem.enterance, entrance, 0, nativeItem.coord_count);

                int entranceCount = nativeItem.coord_count/3;
                for(int j=0 ; j < entranceCount ; j++) {
                    float x = entrance[j * 3 + 0];
                    float y = entrance[j * 3 + 1];
                    float z = entrance[j * 3 + 2];
                    item.entrance.Add(new Vector3(x, y, z));
                }

                layerPOIs.Add(item);
            }

            s_Instance.m_OnPOIList.Invoke(layerPOIs);
        }

        [MonoPInvokeCallback(typeof(SS_Func))]
        private static void OnStageChanged(string name, string label) {
            label = label == string.Empty ? name : label;
            s_Instance.m_OnStageChanged.Invoke(name, label);
        }

        [MonoPInvokeCallback(typeof(SSS_Func))]
        private static void OnSceneLoaded(string keyname, string crscode, string locale) {
            s_Instance.m_OnSceneLoaded.Invoke(keyname, crscode, locale);
        }

        [MonoPInvokeCallback(typeof(S_Func))]
        private static void OnSceneUnloaded(string keyname) {
            s_Instance.m_OnSceneUnloaded.Invoke(keyname);
        }

        [MonoPInvokeCallback(typeof(B_Func))]
        private static void OnResourceUpdated(bool isSuccess) {
            s_Instance.m_OnResourceUpdated.Invoke(isSuccess);   
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        private static void OnNavigationStarted() {
            s_Instance.m_OnNavigationStarted.Invoke();
        }

        [MonoPInvokeCallback(typeof(F_Func))]
        private static void OnRemainDistance(float distance) {
            s_Instance.m_OnDistanceUpdated.Invoke(distance);
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        private static void OnDestinationArrived() {
            s_Instance.m_OnDestinationArrived.Invoke();
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        private static void OnNavigationEnded() {
            s_Instance.m_OnNavigationEnded.Invoke();
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        private static void OnNavigationFailed() {
            s_Instance.m_OnNavigationFailed.Invoke();
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        private static void OnNavigationReSearched() {
            s_Instance.m_OnNavigationReSearched.Invoke();
        }

        [MonoPInvokeCallback(typeof(ISS_Func))]
        private static void OnTransitMovingStarted(int transitType, string destFloor, string destFloorLabel) {
            s_Instance.m_OnTransitMovingStarted.Invoke((ConnectionType) transitType, destFloor, destFloorLabel);
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        private static void OnTransitMovingEnded() {
            s_Instance.m_OnTransitMovingEnded.Invoke();
        }

        [MonoPInvokeCallback(typeof(IS_Func))]
        private static void OnCustomRangeEntered(string uuid, string name) {
            s_Instance.m_OnCustomRangeEntered.Invoke(uuid, name);
        }

        [MonoPInvokeCallback(typeof(IS_Func))]
        private static void OnCustomRangeExited(string uuid, string name) {
            s_Instance.m_OnCustomRangeExited.Invoke(uuid, name);
        }

        [MonoPInvokeCallback(typeof(FFFFFFF_Func))]
        private static void OnUpdatePose(float tx, float ty, float tz, float qw, float qx, float qy, float qz) {
            Vector3 glPosition = new Vector3(tx, ty, tz);
            Quaternion glRotation = new Quaternion(qx, qy, qz, qw);

            Matrix4x4 glTransformMatrix = Matrix4x4.TRS(glPosition, glRotation, Vector3.one);
            Matrix4x4 matrix = ConvertOpenGLToUnity(glTransformMatrix);

            Vector3 position = new Vector3(matrix.m03, matrix.m13, matrix.m23);
            Quaternion rotation = Quaternion.LookRotation(
                new Vector3(matrix.m02, matrix.m12, matrix.m22),
                new Vector3(matrix.m01, matrix.m11, matrix.m21)
            );
            s_Instance.m_OnCameraPoseUpdated.Invoke(position, rotation);
        }

        private static Matrix4x4 ConvertOpenGLToUnity(Matrix4x4 openGLMatrix)
        {
            Matrix4x4 flipX = Matrix4x4.identity;
            flipX.m00 = -1;

            Matrix4x4 flipZ = Matrix4x4.identity;
            flipZ.m22 = -1;

            Matrix4x4 unityMatrix = flipX * openGLMatrix * flipZ.inverse;
            return unityMatrix;
        }
        
        [MonoPInvokeCallback(typeof(MediaInfo_Func))]
        private static void OnVideoLoaded(UnityMediaInfo mediaInfo) {
            s_Instance.m_OnVideoLoaded.Invoke(mediaInfo);
        }

        [MonoPInvokeCallback(typeof(SIS_Func))]
        private static void OnVideoPlaying(string tag, int playerType, float distance) {
            s_Instance.m_OnVideoPlaying.Invoke(tag, playerType, distance);
        }

        [MonoPInvokeCallback(typeof(I_Func))]
        private static void OnVideoUnloaded(string tag, int playerType, bool ignoreFade) {
            s_Instance.m_OnVideoUnloaded.Invoke(tag, playerType, ignoreFade);
        }

        [MonoPInvokeCallback(typeof(MediaInfo_Func))]
        private static void OnAudioLoaded(UnityMediaInfo mediaInfo) {
            s_Instance.m_OnAudioLoaded.Invoke(mediaInfo);
        }

        [MonoPInvokeCallback(typeof(SIS_Func))]
        private static void OnAudioPlaying(string tag, int playerType, float distance) {
            s_Instance.m_OnAudioPlaying.Invoke(tag, playerType, distance);
        }

        [MonoPInvokeCallback(typeof(SIB_Func))]
        private static void OnAudioUnloaded(string tag, int playerType, bool ignoreFade) {
            s_Instance.m_OnAudioUnloaded.Invoke(tag, playerType, ignoreFade);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UnityUILayerInfo {
        [MarshalAs(UnmanagedType.U1)]
        public bool isSplashView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isNaviGuideView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isScanningView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isRecommandView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isTransSelView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isTourDetView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isRecmdDetView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isFocusView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isDestinationNormalView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isDestinationCompleteView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isARAroundView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isARTourView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isMapView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isMapFull;
        [MarshalAs(UnmanagedType.U1)]
        public bool isFloorMvView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isFloorArvedView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isRecommTourView;
        [MarshalAs(UnmanagedType.U1)]
        public bool isTourExitView;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct UnityLayerPOIItem {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string uuid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string full_name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string stage_name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string stage_label;
        
        [MarshalAs(UnmanagedType.I4)]
        public int    display;
        [MarshalAs(UnmanagedType.I4)]
        public int    category;
        [MarshalAs(UnmanagedType.I4)]
        public int    dpcode;

        public IntPtr enterance;

        [MarshalAs(UnmanagedType.I4)]
        public int    coord_count;
        public int    usage;
    }

    public struct LayerPOIItem {
        public string uuid;
        public string name;
        public string fullName;
        public string stageName;
        public string stageLabel;
        
        public int    display;
        public int    category;
        public int    dpcode;

        public List<Vector3> entrance;
        public int    usage;

        public override string ToString()
        {
            return $"{fullName}, {stageName}, {category}, {entrance[0]}";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct UnityMediaInfo {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string uuid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string fullpath;
        
        [MarshalAs(UnmanagedType.I4)]
        public int    playerType;

        [MarshalAs(UnmanagedType.U1)]
        public bool   isLoop;

        [MarshalAs(UnmanagedType.U1)]
        public bool   isSpatial;

        public float fadeIn;
        public float fadeOut;
        public float volume;
        public float distance;

        [MarshalAs(UnmanagedType.I4)]
        public int    spatialCurve;
    }
}