using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class CameraUtil
    {
        public static void AddCullingMask(Camera camera, string layerName)
        {
            int cullingMask = camera.cullingMask;
            int layer = LayerMask.NameToLayer(layerName);

            if (layer == -1)
            {
                NativeLogger.Print(LogLevel.ERROR, $"Layer '{layerName}' not found!");
            }
            else
            {
                cullingMask |= 1 << layer;
                camera.cullingMask = cullingMask;
            }
        }

        public static void RemoveCullingMask(Camera camera, string layerName)
        {
            int cullingMask = camera.cullingMask;
            int layer = LayerMask.NameToLayer(layerName);

            if (layer == -1)
            {
                NativeLogger.Print(LogLevel.ERROR, $"Layer '{layerName}' not found!");
            }
            else
            {
                cullingMask &= ~(1 << layer);
                camera.cullingMask = cullingMask;
            }
        }
    }
}