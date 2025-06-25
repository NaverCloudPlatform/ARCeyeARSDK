using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye
{
    public class NextStep : MonoBehaviour
    {
        void Awake()
        {
            RawImage image = GetComponentInChildren<RawImage>();

            if (image != null)
            {
                if (image.mainTexture != null)
                {
                    if (image.mainTexture is RenderTexture)
                    {
                        (image.mainTexture as RenderTexture).Release();
                    }
                }

                RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
                rt.Create();

                image.texture = rt;

                Camera camera = GetComponentInChildren<Camera>();
                if (camera != null)
                {
                    camera.targetTexture = rt;
                }
            }
        }
    }
}