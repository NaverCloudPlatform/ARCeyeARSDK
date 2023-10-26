using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace ARCeye
{
    public class NativeScreenBridge : MonoBehaviour
    {
        enum RendererScreen
        {
            Preview = 0, Camera, Map, Indicator, Scan
        }

        private static NativeScreenBridge s_Instance;


        /* -- Native plugin -- */
        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif

        public delegate void B_Func(bool b);
        public delegate bool I_BFunc(int i);
        public delegate void IB_Func(int i, bool b);

        [DllImport(dll)] private static extern void SetActivateScreenFuncNative(IB_Func func);
        [DllImport(dll)] private static extern void SetIsScreenActivatedFuncNative(I_BFunc func);


        private void Awake()
        {
            s_Instance = this;

            SetActivateScreenFuncNative( ActivateScreen );
            SetIsScreenActivatedFuncNative( IsScreenActivated );
        }


        [MonoPInvokeCallback(typeof(IB_Func))]
        unsafe private static void ActivateScreen(int screenTypeIdx, bool active)
        {
            RendererScreen screenType = (RendererScreen) screenTypeIdx;

            switch(screenType)
            {
                case RendererScreen.Map : {
                    
                    break;
                }
                case RendererScreen.Indicator : {

                    break;
                }
                default : {
                    
                    break;
                }
            }
        }

        [MonoPInvokeCallback(typeof(I_BFunc))]
        private static bool IsScreenActivated(int screenTypeIdx)
        {
            RendererScreen screenType = (RendererScreen) screenTypeIdx;

            switch(screenType)
            {
                case RendererScreen.Map : {
                    return true;
                }
                case RendererScreen.Indicator : {
                    return false;
                }
                default : {
                    return false;
                }
            }
        }
    }
}