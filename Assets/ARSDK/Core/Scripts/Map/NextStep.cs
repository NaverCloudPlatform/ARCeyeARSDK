using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye
{
    public class NextStep : MonoBehaviour
    {
        [Header("Model Paths")]
        public string NextStepArrow = "/Contents/Indicator/NextStep_Arrow.glb";
        public string NextStepDot = "/Contents/Indicator/NextStep_Dot.glb";
        public string NextStepText = "/Contents/Indicator/NextStep_Text.glb";

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