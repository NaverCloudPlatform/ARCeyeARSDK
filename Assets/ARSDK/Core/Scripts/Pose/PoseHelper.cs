using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye
{
    public class PoseHelper
    {
        static private Matrix4x4 m_FlipX = new Matrix4x4(
            new Vector4(-1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );
        static private Matrix4x4 m_FlipZ = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 0, 0, 1)
        );

        static public Matrix4x4 UnmanagedToMatrix4x4(IntPtr ptr)
        {
            float[] m = new float[16];
            Marshal.Copy(ptr, m, 0, 16);
            return new Matrix4x4(
                new Vector4(m[0], m[1], m[2], m[3]),
                new Vector4(m[4], m[5], m[6], m[7]),
                new Vector4(m[8], m[9], m[10], m[11]),
                new Vector4(m[12], m[13], m[14], m[15])
            );
        }

        static public byte[] UnmanagedToByteArray(IntPtr ptr, int length)
        {
            byte[] bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return bytes;
        }

        static public Matrix4x4 ConvertLHRH(Matrix4x4 src)
        {
            return m_FlipX * src * m_FlipZ;
        }

        static public Matrix4x4 ConvertLHRHView(Matrix4x4 src)
        {
            return m_FlipZ * src * m_FlipX;
        }

        static public bool IsValidScale(Matrix4x4 m)
        {
            var scale = m.lossyScale;
            return scale.x > 0 && scale.y > 0 && scale.z > 0;
        }

        static public float[] GetViewMatrixLH()
        {
            Matrix4x4 lhPoseMatrix = Camera.main.transform.localToWorldMatrix;
            Matrix4x4 poseMatrix = ConvertLHRH(lhPoseMatrix);
            Matrix4x4 viewMatrix = Matrix4x4.Inverse(poseMatrix).transpose;

            return viewMatrix.ToData();
        }

        static public Matrix4x4 FlipX(Matrix4x4 src)
        {
            return m_FlipX * src;
        }
    }
}