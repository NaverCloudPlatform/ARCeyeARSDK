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
                Debug.LogError("Layer 'MapArrow' not found!");
            }
            else
            {
                camera.cullingMask = 1 << layerIndex;
            }
        }
    }
}