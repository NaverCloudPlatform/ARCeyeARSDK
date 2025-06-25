using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GLTFast;
using UnityEngine;

namespace ARCeye
{
    public class MainThreadLoadingHandler
    {
        private float m_FrameBudget = 0.1f;
        public float FrameBudget
        {
            get => m_FrameBudget;
            set
            {
                if (value < 0.01f)
                {
                    Debug.LogWarning("[MainThreadLoadingHandler] FrameBudget must be greater than 0.01");
                    return;
                }
                m_FrameBudget = value;
                m_DeferAgent?.SetFrameBudget(m_FrameBudget);
            }
        }

        private TimeBudgetPerFrameDeferAgent m_DeferAgent;


        public MainThreadLoadingHandler(bool useDeferAgent = true)
        {
            if (useDeferAgent)
            {
                m_DeferAgent = (new GameObject("DeferAgent")).AddComponent<TimeBudgetPerFrameDeferAgent>();
                GltfImport.SetDefaultDeferAgent(m_DeferAgent);
            }
        }

        public void Load(GameObject itemGo, string filePath, Action completeCallback)
        {
            var gltfAsset = itemGo.GetComponent<PostEventGltfAsset>();
            var unityModel = itemGo.GetComponent<UnityModel>();

            gltfAsset.PostEvent = (success) =>
            {
                if (unityModel == null)
                {
                    return;
                }

                unityModel.Initialize(gltfAsset);
                completeCallback.Invoke();
            };
            gltfAsset.Load(filePath);
        }
    }
}