using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityModel : MonoBehaviour
    {
        private PostEventGltfAsset m_Model;
        private string m_FilePath;

        private Animation m_Animation;
        private Animation anim
        {
            get
            {
                if (m_Animation == null)
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

        private IntPtr m_NativePtr;
        private string m_WrapMode;

        public void SetNativePtr(IntPtr nativePtr)
        {
            m_NativePtr = nativePtr;
        }

        public IntPtr GetNativePtr()
        {
            return m_NativePtr;
        }

        public virtual void Initialize()
        {
            InitLayerInModel("ARItem");
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

        public virtual void Initialize(PostEventGltfAsset model, string filePath)
        {
            m_FilePath = filePath;
            model.Load(filePath);
        }

        public virtual void SetActive(bool value)
        {
            // SetActive가 false일 경우에만 구현.
            //   본 메서드는 gameObject의 SetActive가 아닌, ARPG에서 모델이 보이는지 여부를 설정함.
            //   SetActive(true)일 경우 SetOpacity(1)를 설정하면 actionRange로 진입했을때
            //   opacity가 순간적으로 1이 되기 때문에 fade 애니메이션이 정상적으로 실행되지 않는다.
            if (!value)
            {
                SetOpacity(0);
                BoxCollider collider = GetComponentInChildren<BoxCollider>();
                if (collider)
                {
                    collider.enabled = false;
                }
            }
            else
            {
                BoxCollider collider = GetComponentInChildren<BoxCollider>();
                if (collider)
                {
                    collider.enabled = true;
                }
            }
        }

        protected virtual void FindMaterials()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            List<Material> allMaterials = new();
            Material zwriteMat = MaterialGenerator.GetZWriteMaterial();

            foreach (var renderer in renderers)
            {
                // 기존에 존재하던 Materials 추출.
                List<Material> materials = new();
                renderer.GetMaterials(materials);

                // fade 효과가 적용될 Material들만 별도로 관리.
                allMaterials.AddRange(materials);

                // 기존에 설정된 PBR 쉐이더를 2Pass 전용 쉐이더로 변경.
                foreach (var material in materials)
                {
                    if (IsGLTFMaterial(material))
                    {
                        MaterialGenerator.SetFadeModeBlend(material);
                    }
                }

                // zwrite 패스 추가.
                materials.Insert(0, zwriteMat);

                renderer.SetMaterials(materials);
            }

            m_Materials = allMaterials.ToArray();
        }

        private bool IsGLTFMaterial(Material material)
        {
            string shaderName = material.shader.name;
            return shaderName.Contains("glTF");
        }

        public virtual void PlayAnimation(string animName, string playModeStr)
        {
            if (!CanPlayAnimation())
            {
                PrintAnimErrorLog();
                return;
            }

            AnimationClip clip = anim.GetClip(animName);
            if (clip == null)
            {
                return;
            }

            switch (playModeStr.ToLower())
            {
                case "once":
                    anim.wrapMode = WrapMode.Once;
                    m_WrapMode = "once";
                    break;
                case "loop":
                    anim.wrapMode = WrapMode.Loop;
                    m_WrapMode = "loop";
                    break;
                case "clamp":
                    anim.wrapMode = WrapMode.Clamp;
                    m_WrapMode = "clamp";
                    break;
                default:
                    NativeLogger.Print(LogLevel.WARNING, $"[UnityModel] Can't find wrapMode {playModeStr}. Use WrapMode.Once instead");
                    anim.wrapMode = WrapMode.Once;
                    m_WrapMode = "once";
                    break;
            }

            anim.Play(animName);
        }

        // 모든 UnityModel은 기본적으로 ARItem 레이어로 설정. 다른 레이어를 사용하는 아이템의 경우 자식 클래스의 Initialize 메서드에서 설정한다.
        protected void InitLayerInModel(string layerName)
        {
            var children = gameObject.GetComponentsInChildren<Transform>();
            var layer = LayerMask.NameToLayer(layerName);
            foreach (var child in children)
            {
                child.gameObject.layer = layer;
            }
            gameObject.layer = layer;
        }

        public void StopAnimation()
        {
            if (CanPlayAnimation())
            {
                anim.Stop();
            }
        }

        private bool CanPlayAnimation()
        {
            return anim != null;
        }

        private void PrintAnimErrorLog()
        {
            if (m_Animation == null)
            {

            }
        }

        public void SetBillboard(bool value)
        {
            m_UseBillboard = value;

            Billboard billboard = gameObject.GetComponent<Billboard>();
            if (value)
            {
                if (!billboard)
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
                if (billboard)
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

            if (anim == null)
            {
                return 0.1f;
            }

            AnimationClip clip = anim.GetClip(animName);

            if (clip == null)
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
            if (!gameObject.activeSelf)
            {
                return;
            }

            StartCoroutine(FadeInternal(duration, fadeIn, onComplete));
        }

        private IEnumerator FadeInternal(float duration, bool fadeIn, Action onComplete)
        {
            if (fadeIn)
            {
                if (m_WrapMode != "loop")
                {
                    anim?.Rewind();
                }
            }

            if (m_Materials == null)
            {
                yield break;
            }

            Color baseColor = m_Materials[0].color;
            // TODO. baseColor.a에서 정상 동작해야함.
            // float start = baseColor.a;

            float start = fadeIn ? 0 : 1;
            float end = fadeIn ? 1 : 0;

            bool isFinished = false;
            float accumTime = 0;

            if (fadeIn)
            {
                EnableAllRenderers(true);
            }

            while (!isFinished)
            {
                float t = accumTime / duration;

                float a = Mathf.Lerp(start, end, t);
                foreach (var material in m_Materials)
                {
                    Color color = material.color;
                    color.a = a;
                    material.color = color;
                }

                yield return null;

                accumTime += Time.deltaTime;

                if (accumTime >= duration)
                {
                    isFinished = true;
                }
            }

            // 최종 투명도로 설정.
            foreach (var material in m_Materials)
            {
                Color color = material.color;
                color.a = end;
                material.color = color;
            }

            if (onComplete != null)
            {
                onComplete.Invoke();
            }

            if (!fadeIn)
            {
                EnableAllRenderers(false);
            }
        }

        public virtual void SetOpacity(float opacity)
        {
            if (m_Materials == null)
            {
                return;
            }

            foreach (var material in m_Materials)
            {
                Color baseColor = material.color;
                baseColor.a = opacity;
                material.color = baseColor;
            }

            if (opacity == 0.0f)
            {
                EnableAllRenderers(false);
            }
            else
            {
                EnableAllRenderers(true);
            }
        }

        public virtual void SetPickable(bool value)
        {
            if (!value)
            {
                Transform collider = transform.Find("Collider");
                if (collider != null)
                {
                    collider.gameObject.SetActive(false);
                }
            }
            else
            {
                Transform collider = transform.Find("Collider");
                if (collider != null)
                {
                    collider.gameObject.SetActive(true);
                }

                // Create collider for all children meshes.
                else
                {
                    Renderer[] renderers = GetComponentsInChildren<Renderer>();

                    if (renderers.Length > 0)
                    {
                        Bounds localBounds = renderers[0].localBounds;
                        Bounds bounds = renderers[0].bounds;

                        for (int i = 1; i < renderers.Length; i++)
                        {
                            localBounds.Encapsulate(renderers[i].localBounds);
                            bounds.Encapsulate(renderers[i].bounds);
                        }

                        GameObject go = new GameObject("Collider");
                        go.transform.localPosition = bounds.center;
                        go.transform.parent = transform;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = new Vector3(1, 1, 1);

                        BoxCollider boxCollider = go.AddComponent<BoxCollider>();

                        boxCollider.size = localBounds.size;
                    }
                }
            }
        }

        public void RunCoroutine(System.Action action, float delay)
        {
            StartCoroutine(RunCoroutineInternal(action, delay));
        }

        private IEnumerator RunCoroutineInternal(System.Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }

        private void EnableAllRenderers(bool value)
        {
            foreach (var elem in m_Renderers)
            {
                elem.enabled = value;
            }
        }
    }
}
