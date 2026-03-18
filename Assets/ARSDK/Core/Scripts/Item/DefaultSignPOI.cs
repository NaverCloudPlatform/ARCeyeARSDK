using UnityEngine;
using TMPro;

namespace ARCeye
{
    public class DefaultSignPOI : UnitySignPOI
    {
        [SerializeField]
        private TextMeshPro m_Text;

        [SerializeField]
        private SpriteRenderer m_Label;

        [SerializeField]
        private SpriteRenderer m_IconRenderer;

        public override void SetIcon(Sprite icon)
        {
            m_IconRenderer.sprite = icon;
            base.SetIcon(icon);
        }

        public override void SetLabel(string content)
        {
            m_Text.text = content;
            m_Label.size = CalculateLabelSize();
        }

        private Vector2 CalculateLabelSize()
        {
            RectTransform rectTransform = m_Text.GetComponent<RectTransform>();

            Vector2 size = m_Text.GetPreferredValues();
            Vector3 scale = rectTransform.localScale;
            Vector2 scaledSize = new Vector2(size.x * scale.x, size.y * scale.y);
            Vector2 margin = new Vector3(0.22f, 0.07f);

            return scaledSize + margin;
        }

        public override void SetOpacity(float opacity)
        {
            base.SetOpacity(opacity);

            Color textColor = m_Text.color;
            Color labelColor = m_Label.color;

            textColor.a = opacity;
            labelColor.a = opacity;

            m_Text.color = textColor;
            m_Label.color = labelColor;

            Color iconColor = m_IconRenderer.color;
            iconColor.a = opacity;
            m_IconRenderer.color = iconColor;

            if (opacity == 0)
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
