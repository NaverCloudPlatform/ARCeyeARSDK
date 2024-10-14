using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using GLTFast.Materials;

namespace ARCeye
{
    public class UnitySignPOI : UnityModel
    {
        [SerializeField]
        private TextMesh m_Text;

        [SerializeField]
        private SpriteRenderer m_Label;

        [SerializeField]
        private Transform m_Icon;

        private SkinnedMeshRenderer[] m_POIIconRenderers;

        private Billboard m_Billboard;

        private float m_Opacity;


        private void Awake() {
            m_Billboard = GetComponent<Billboard>();
            m_Billboard.rotationMode = Billboard.RotationMode.AXIS_Y;

            if(ItemGenerator.Instance.font != null) {
                m_Text.font = ItemGenerator.Instance.font;
            }

            var meshRenderer = m_Text.GetComponent<MeshRenderer>();
            
            Texture fontTexture = m_Text.font.material.mainTexture;
            meshRenderer.material = ItemGenerator.Instance.poiTextMaterial;
            meshRenderer.sharedMaterial.mainTexture = fontTexture;

            SetOpacity(0);
        }

        public void SetType(int type) {

        }

        public void SetIcon(GameObject icon) {
            icon.transform.SetParent(m_Icon);
            icon.transform.localPosition = Vector3.zero;
            icon.transform.localEulerAngles = new Vector3(0, 180, 0);
            icon.transform.localScale = new Vector3(2, 2, 2);

            m_POIIconRenderers = icon.GetComponentsInChildren<SkinnedMeshRenderer>();

            // Icon을 설정한 뒤에 SetActive(false)를 통해 opacity를 0으로 설정해야 한다.
            // SetIcon을 호출하기 전까지는 POIIconRenderer가 설정되어 있지 않기 때문.
            SetActive(false);
        }

        public void SetLabel(string content) {
            m_Text.text = content;

            var meshRenderer = m_Text.GetComponent<MeshRenderer>();

            Vector3 localSize  = meshRenderer.localBounds.size;
            float   localScale = meshRenderer.transform.localScale.x;
            Vector3 scaledSize = localSize * localScale;
            Vector3 margin     = new Vector3(0.2f, 0.05f, 0.0f);

            Vector3 size = scaledSize + margin;
            m_Label.size = size;
        }

        public void SetAutoRotateMode(int rotationMode) {
            m_Billboard.rotationMode = (Billboard.RotationMode) rotationMode;
        }

        public override void Fade(float duration, bool fadeIn, System.Action onComplete = null)
        {
            if(!gameObject.activeSelf) {
                return;
            }

            StartCoroutine( FadeInternal(duration, fadeIn, onComplete) );
        }

        private IEnumerator FadeInternal(float duration, bool fadeIn, System.Action onComplete)
        {
            float start = m_Opacity;
            float end = fadeIn ? 1 : 0;

            bool isFinished = false;
            float accumTime = 0;

            while(!isFinished)
            {
                float t = accumTime / duration;

                float a = Mathf.Lerp(start, end, t);
                
                SetOpacity(a);

                yield return null;

                accumTime += Time.deltaTime;

                if(accumTime >= duration) {
                    isFinished = true;
                }
            }

            SetOpacity(end);
        }

        public override void SetOpacity(float opacity)
        {
            Color textColor = m_Text.color;
            Color labelColor = m_Label.color;

            textColor.a = opacity;
            labelColor.a = opacity;

            m_Text.color = textColor;
            m_Label.color = labelColor;

            if(m_POIIconRenderers != null)
            {
                foreach(var renderer in m_POIIconRenderers)
                {
                    // Alpha 효과를 주기 위해서는 Alpha blend 모드가 되어야 한다.
                    BuiltInMaterialGenerator.SetAlphaModeBlend(renderer.material);

                    Color color = renderer.material.color;
                    renderer.material.color = new Color(color.r, color.g, color.b, opacity);
                }
            }

            m_Opacity = opacity;

            if(m_Opacity == 0)
            {
                SetActivePOI(false);
            }
            else
            {
                SetActivePOI(true);
            }
        }

        private void SetActivePOI(bool value)
        {
            m_Text.gameObject.SetActive(value);
            m_Label.gameObject.SetActive(value);

            if(m_POIIconRenderers != null)
            {
                foreach(var renderer in m_POIIconRenderers)
                {
                    renderer.gameObject.SetActive(value);
                }
            }
        }
    }
}