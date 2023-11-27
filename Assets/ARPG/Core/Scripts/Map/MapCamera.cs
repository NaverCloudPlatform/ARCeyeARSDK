using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class MapCamera : MonoBehaviour
    {
        private void Awake()
        {
            Camera camera = GetComponent<Camera>();

            int layerIndex = LayerMask.NameToLayer("Map");

            if (layerIndex == -1)
            {
                NativeLogger.Print(LogLevel.ERROR, "Layer 'Map' not found!");
            }
            else
            {
                camera.cullingMask = 1 << layerIndex;
            }
        }
    }
}