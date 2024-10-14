using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class MapArrowCamera : MonoBehaviour
    {
        private void Awake()
        {
            Camera camera = GetComponent<Camera>();

            int layerIndex = LayerMask.NameToLayer("MapArrow");

            if (layerIndex == -1)
            {
                NativeLogger.Print(LogLevel.ERROR, "Layer 'MapArrow' not found!");
            }
            else
            {
                camera.cullingMask = 1 << layerIndex;
            }
        }
    }
}