using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace ARCeye
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UnityFrame
    {
        public long timestamp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public double[] viewMatrix;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] projMatrix;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] texTrans;

        IntPtr imageBuffer;

        double latitude;
        double longitude;
        double altitude;
        public double relaltitude;
    }
}