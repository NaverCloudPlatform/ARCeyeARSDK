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

            CameraUtil.AddCullingMask(camera, "Map");
            CameraUtil.AddCullingMask(camera, "AMProjViz");
        }
    }
}