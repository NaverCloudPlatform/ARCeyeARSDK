using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast.Materials;

namespace ARCeye
{
    public class UnityModel : MonoBehaviour
    {
        private PostEventGltfAsset m_Model;

        private Animation m_Animation;
        private Animation anim {
            get {
                if(m_Animation == null)
                {
                    m_Animation = GetComponentInChildren<Animation>();
                }
                return m_Animation;
            }
        }

        private Material[] m_Materials;
        private Renderer[] m_Renderers;
        private Dictionary<Material, bool> m_OpaqueMaterials;
        private bool m_UseBillboard = false;
        protected Billboard.RotationMode m_BillboardRotationMode = Billboard.RotationMode.AXIS_Y;
        
        const string k_PBRMetallicRoughness = "glTF/PbrMetallicRoughness (2Pass)";
        const string k_PBRSpecularGlossiness = "glTF/PbrSpecularGlossiness (2Pass)";
        const string k_PBRUnlit = "glTF/Unlit (2Pass)";

        public virtual void Initialize()
        {

        }

        public virtual void Initialize(PostEventGltfAsset model)
        {
            m_Model = model;
            m_OpaqueMaterials = new Dictionary<Material, bool>();

            gameObject.name = GetType().ToString();

            m_Renderers = GetComponentsInChildren<Renderer>();

            StopAnimation();
            FindMaterials();
            SetOpacity(0);

            Initialize();
        }

        public virtual void SetActive(bool value)
        {
            // SetActive가 false일 경우에만 구현.
            //   본 메서드는 gameObject의 SetActive가 아닌, ARPG에서 모델이 보이는지 여부를 설정함.
            //   SetActive(true)일 경우 SetOpacity(1)를 설정하면 actionRange로 진입했을때
            //   opacity가 순간적으로 1이 되기 때문에 fade 애니메이션이 정상적으로 실행되지 않는다.
            if(!value)
            {
                SetOpacity(0);
            }
        }

        protected virtual void FindMaterials()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            m_Materials = new Material[renderers.Length];

            for(int i=0 ; i<m_Materials.Length ; i++)
            {
                var material = renderers[i].material;

                if(material.shader.name == "glTF/PbrMetallicRoughness")
                {
                    Shader shader = Shader.Find(k_PBRMetallicRoughness);
                    material.shader = shader;
                    material.SetTexture("normalTexture", null);
                    m_OpaqueMaterials.Add(material, false);
                }
                else if(material.shader.name == "glTF/PbrSpecularGlossiness")
                {
                    Shader shader = Shader.Find(k_PBRSpecularGlossiness);
                    material.shader = shader;
                    m_OpaqueMaterials.Add(material, false);
                }
                else if(material.shader.name == "glTF/Unlit")
                {
                    Shader shader = Shader.Find(k_PBRUnlit);
                    material.shader = shader;

                    // gltf/Unlit 쉐이더의 경우 backface culling을 수행하면 frontface culling 같이 출력 된다.
                    // 성능 상의 이슈가 없을 경우 culling을 비활성화.
                    material.SetFloat("_CullMode", 0);

                    // Unlit을 사용하는 오브젝트는 항상 Opaque 상태로 로딩이 되도록.
                    m_OpaqueMaterials.Add(material, true);
                }
                
                m_Materials[i] = material;
            }
        }

        public virtual void PlayAnimation(string animName, string playModeStr)
        {
            if (!CanPlayAnimation())
            {
                PrintAnimErrorLog();
                return;
            }

            AnimationClip clip = anim.GetClip(animName);
            if(clip == null)
            {
                return;
            }

            switch(playModeStr.ToLower())
            {
                case "once" : 
                    anim.wrapMode = WrapMode.Once;
                    break;
                case "loop" : 
                    anim.wrapMode = WrapMode.Loop;
                    break;
                case "clamp" : 
                    anim.wrapMode = WrapMode.Clamp;
                    break;
                default : 
                    NativeLogger.Print(LogLevel.WARNING, $"[UnityModel] Can't find wrapMode {playModeStr}. Use WrapMode.Once instead");
                    anim.wrapMode = WrapMode.Once;
                    break;
            }
            
            anim.Play(animName);
        }

        public void StopAnimation() {
            if (CanPlayAnimation()) {
                anim.Stop();
            }
        }

        private bool CanPlayAnimation()
        {
            return anim != null;
        }

        private void PrintAnimErrorLog()
        {
            if(m_Animation == null)
            {
                
            }
        }

        public void SetBillboard(bool value)
        {
            m_UseBillboard = value;

            Billboard billboard = gameObject.GetComponent<Billboard>();
            if(value) 
            {
                if(!billboard) 
                {
                    billboard = gameObject.AddComponent<Billboard>();
                    // GLB 모델이 z+ front일 경우.
                    m_BillboardRotationMode = Billboard.RotationMode.AXIS_Y_FLIP;
                }
                
                billboard.enabled = true;
                billboard.rotationMode = m_BillboardRotationMode;
            }
            else
            {
                if(billboard)
                {
                    billboard.enabled = false;
                }
            }
        }

        public bool HasAnimation(string animName)
        {
            AnimationClip clip = anim.GetClip(animName);
            bool value = (clip == null);
            return value;
        }

        public float AnimationDuration(string animName)
        {
            float value = 0.1f;

            if(anim == null) {
                return 0.1f;
            }

            AnimationClip clip = anim.GetClip(animName);

            if(clip == null)
            {
                return value;
            }
            value = clip.length;

            return value;
        }

        /////
        // gltfast의 shader들은 fade 기능을 지원하지 않는다.
        // 이를 위해서는 shader의 수정이 필요.
        ////
        public virtual void Fade(float duration, bool fadeIn, Action onComplete = null)
        {
            if(!gameObject.activeSelf) {
                return;
            }

            StartCoroutine( FadeInternal(duration, fadeIn, onComplete) );
        }

        private IEnumerator FadeInternal(float duration, bool fadeIn, Action onComplete)
        {
            if(fadeIn)
            {
                anim?.Rewind();
            }
            
            if(m_Materials == null)
            {
                yield break;
            }

            Color baseColor = m_Materials[0].color;
            float start = baseColor.a;
            float end = fadeIn ? 1 : 0;

            bool isFinished = false;
            float accumTime = 0;

            if(fadeIn)
            {
                EnableAllRenderers(true);
            }

            foreach(var material in m_Materials)
            {
                if(material.shader.name != k_PBRUnlit)
                {
                    continue;
                }

                BuiltInMaterialGenerator.SetAlphaModeBlend(material);
            }

            while(!isFinished)
            {
                float t = accumTime / duration;

                float a = Mathf.Lerp(start, end, t);
                foreach(var material in m_Materials)
                {
                    Color color = material.color;
                    material.color = new Color(color.r, color.g, color.b, a);
                }

                yield return null;

                accumTime += Time.deltaTime;

                if(accumTime >= duration) {
                    isFinished = true;
                }
            }

            // 최종 투명도로 설정.
            foreach(var material in m_Materials)
            {
                Color color = material.color;
                material.color = new Color(color.r, color.g, color.b, end);
            }

            // fadein 시 기존의 material 속성 변경.
            if(fadeIn)
            {
                foreach(var material in m_Materials)
                {
                    if(material.shader.name != k_PBRUnlit)
                    {
                        continue;
                    }

                    if(m_OpaqueMaterials[material]) {
                        BuiltInMaterialGenerator.SetOpaqueMode(material);
                    } else {
                        BuiltInMaterialGenerator.SetAlphaModeBlend(material);
                    }
                }
            }

            if(onComplete != null)
            {
                onComplete.Invoke();
            }

            if(!fadeIn)
            {
                EnableAllRenderers(false);
            }
        }

        public virtual void SetOpacity(float opacity)
        {
            if(m_Materials == null) {
                return;
            }

            foreach(var material in m_Materials)
            {
                BuiltInMaterialGenerator.SetAlphaModeBlend(material);

                Color baseColor = material.color;
                material.color = new Color(baseColor.r, baseColor.g, baseColor.b, opacity);
            }

            if(opacity == 0.0f)
            {
                EnableAllRenderers(false);
            }
            else
            {
                EnableAllRenderers(true);

                if(opacity == 1.0f)
                {
                    foreach(var material in m_Materials)
                    {
                        if(m_OpaqueMaterials[material]) {
                            BuiltInMaterialGenerator.SetOpaqueMode(material);
                        } else {
                            BuiltInMaterialGenerator.SetAlphaModeBlend(material);
                        }
                    }   
                }
            }
        }

        public void RunCoroutine(System.Action action, float delay) {
            StartCoroutine( RunCoroutineInternal(action, delay) );
        }

        private IEnumerator RunCoroutineInternal(System.Action action, float delay) {
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }

        private void EnableAllRenderers(bool value)
        {
            foreach(var elem in m_Renderers)
            {
                elem.enabled = value;
            }
        }
    }
}
