using System.Collections;
using TMPro;
using UnityEngine;

namespace ARCeye
{
    public class UnitySignPOI : UnityModel
    {
        [SerializeField]
        private TextMeshPro m_Text;

        [SerializeField]
        private SpriteRenderer m_Label;

        [SerializeField]
        private SpriteRenderer m_IconRenderer;

        private Billboard m_Billboard;

        private float m_Opacity;


        private void Awake() {
            m_Billboard = GetComponent<Billboard>();
            m_Billboard.rotationMode = Billboard.RotationMode.AXIS_Y;

            SetOpacity(0);

            InitLayerInModel("ARItem");
        }

        public void SetType(int type) {

        }

        public void SetIcon(Sprite icon) {
            m_IconRenderer.sprite = icon;

            // Icon을 설정한 뒤에 SetActive(false)를 통해 opacity를 0으로 설정해야 한다.
            // SetIcon을 호출하기 전까지는 POIIconRenderer가 설정되어 있지 않기 때문.
            SetActive(false);
        }

        public void SetLabel(string content) {
            m_Text.text = content;         
            m_Label.size = CalculateLabelSize();
        }

        private Vector2 CalculateLabelSize()
        {
            RectTransform rectTransform = m_Text.GetComponent<RectTransform>();

            Vector2 size = m_Text.GetPreferredValues();
            Vector3 scale = rectTransform.localScale;
            Vector2 scaledSize = new Vector2(size.x * scale.x, size.y * scale.y);
            Vector2 margin     = new Vector3(0.22f, 0.07f);

            return scaledSize + margin;
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

            Color iconColor = m_IconRenderer.color;
            iconColor.a = opacity;
            m_IconRenderer.color = iconColor;

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
            m_IconRenderer.gameObject.SetActive(value);
        }
    }
}