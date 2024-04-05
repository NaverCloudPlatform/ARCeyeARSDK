using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOT;

namespace ARCeye
{
    public enum ConnectionType {
        Default, Escalator = 5, Elevator = 6, Stair = 7
    }

    public enum PathFindingType {
        Default, EscalatorOnly, ElevatorOnly, StairOnly
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LoadNavigationParams {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] endPoints;
        public string endFloor; 
        public PathFindingType pathFindingType;
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