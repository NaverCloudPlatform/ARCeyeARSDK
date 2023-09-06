using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOT;

namespace ARCeye
{
    public enum ConnectionType {
        Default, Escalator, Elevator, Stair
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class LoadNavigationParams {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] endPoints;
        public string endFloor; 
        public ConnectionType connectionType;
    }
    
    public class PathFinder : MonoBehaviour
    {
        /* -- Native plugin -- */
        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif

        [DllImport(dll)] private static extern void LoadNavigationNative(LoadNavigationParams param);

        public void LoadNavigation(LoadNavigationParams param)
        {
            LoadNavigationNative(param);
        }
    }
}