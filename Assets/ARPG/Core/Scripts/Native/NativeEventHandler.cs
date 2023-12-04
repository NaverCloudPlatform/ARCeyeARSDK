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
        public UnityEvent<string> m_OnStageChanged;
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
        public UnityEvent m_OnNavigationReSearched;
        [HideInInspector]
        public UnityEvent<ConnectionType, string> m_OnTransitMovingStarted;
        [HideInInspector]
        public UnityEvent m_OnTransitMovingEnded;
        [HideInInspector]
        public UnityEvent<string, string> m_OnCustomRangeEntered;
        [HideInInspector]
        public UnityEvent<string, string> m_OnCustomRangeExited;


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
        public delegate void F_Func(float f);
        public delegate void S_Func(string f);
        public delegate void SS_Func(string s1, string s2);
        public delegate void III_Func(int i1, int i2, int i3);
        public delegate void FBI_Func(float f, bool b, int i);
        public delegate void SSFpFpFp_Func(string s1, string s2, IntPtr fp1, IntPtr fp2, IntPtr fp3);
        public delegate void UILayerInfo_Func(UnityUILayerInfo info);
        public delegate void LayerPOIItem_Func(IntPtr itemsPtr, int count);

        
        [DllImport(dll)] private static extern void SetOnAppConfigFuncNative(SSFpFpFp_Func func);
        [DllImport(dll)] private static extern void SetOnUIChangedFuncNative(UILayerInfo_Func func);
        [DllImport(dll)] private static extern void SetOnPOIListFuncNative(LayerPOIItem_Func func);
        [DllImport(dll)] private static extern void SetOnStageChangedFuncNative(S_Func func);
        [DllImport(dll)] private static extern void SetOnNavigationStartedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnRemainDistanceFuncNative(F_Func func);
        [DllImport(dll)] private static extern void SetOnDestinationArrivedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnNavigationEndedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnNavigationReSearchedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnTransitMovingStartedFuncNative(IS_Func func);
        [DllImport(dll)] private static extern void SetOnTransitMovingEndedFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetOnCustomRangeEnteredFuncNative(SS_Func func);
        [DllImport(dll)] private static extern void SetOnCustomRangeExitedFuncNative(SS_Func func);


        private void Awake() {
            s_Instance = this;

            SetOnAppConfigFuncNative(OnAppConfig);
            SetOnUIChangedFuncNative(OnUIChanged);
            SetOnPOIListFuncNative(OnPOIList);
            SetOnRemainDistanceFuncNative(OnRemainDistance);
            SetOnStageChangedFuncNative(OnStageChanged);
            SetOnNavigationStartedFuncNative(OnNavigationStarted);
            SetOnNavigationEndedFuncNative(OnNavigationEnded);
            SetOnNavigationReSearchedFuncNative(OnNavigationReSearched);
            SetOnDestinationArrivedFuncNative(OnDestinationArrived);
            SetOnTransitMovingStartedFuncNative(OnTransitMovingStarted);
            SetOnTransitMovingEndedFuncNative(OnTransitMovingEnded);
            SetOnCustomRangeEnteredFuncNative(OnCustomRangeEntered);
            SetOnCustomRangeExitedFuncNative(OnCustomRangeExited);
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
                item.display = nativeItem.display;
                item.category = nativeItem.category;
                item.dpcode = nativeItem.dpcode;
                item.entrance = new List<Vector3>();

                // 입구점 좌표 배열의 포인터를 입구점 좌표 배열로 변환.
                float[] entrance = new float[nativeItem.coord_count];
                Marshal.Copy(nativeItem.enterance, entrance, 0, nativeItem.coord_count);

                int entranceCount = nativeItem.coord_count/3;
                for(int j=0 ; j < entranceCount ; j++) {
                    float x = entrance[j + 0];
                    float y = entrance[j + 1];
                    float z = entrance[j + 2];
                    item.entrance.Add(new Vector3(x, y, z));
                }

                layerPOIs.Add(item);
            }

            s_Instance.m_OnPOIList.Invoke(layerPOIs);
        }

        [MonoPInvokeCallback(typeof(S_Func))]
        private static void OnStageChanged(string stage) {
            s_Instance.m_OnStageChanged.Invoke(stage);
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
        private static void OnNavigationReSearched() {
            s_Instance.m_OnNavigationReSearched.Invoke();
        }

        [MonoPInvokeCallback(typeof(IS_Func))]
        private static void OnTransitMovingStarted(int transitType, string destFloor) {
            s_Instance.m_OnTransitMovingStarted.Invoke((ConnectionType) transitType, destFloor);
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
        
        [MarshalAs(UnmanagedType.I4)]
        public int    display;
        [MarshalAs(UnmanagedType.I4)]
        public int    category;
        [MarshalAs(UnmanagedType.I4)]
        public int    dpcode;

        public IntPtr enterance;

        [MarshalAs(UnmanagedType.I4)]
        public int    coord_count;
    }

    public struct LayerPOIItem {
        public string uuid;
        public string name;
        public string fullName;
        public string stageName;
        
        public int    display;
        public int    category;
        public int    dpcode;

        public List<Vector3> entrance;

        public override string ToString()
        {
            return $"{fullName}, {stageName}, {category}, {entrance[0]}";
        }
    }
}