using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace ARCeye
{
    [StructLayout(LayoutKind.Sequential)]
    public class UnityFrame
    {
        public long timestamp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] viewMatrix;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] projMatrix;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] texTrans;

        IntPtr imageBuffer;

        double latitude;
        double longitude;
        double altitude;
        double relaltitude;
    }
}