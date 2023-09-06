using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class MapPOICamera : MonoBehaviour
    {
        private void Awake()
        {
            Camera camera = GetComponent<Camera>();

            int layerIndex = LayerMask.NameToLayer("MapPOI");

            if (layerIndex == -1)
            {
                Debug.LogError("Layer 'MapPOI' not found!");
            }
            else
            {
                camera.cullingMask = 1 << layerIndex;
            }
        }
    }
}