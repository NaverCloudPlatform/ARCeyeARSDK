using TMPro;
using UnityEngine;


namespace ARCeye
{
    public class DefaultMapPOI : UnityMapPOI
    {
        [SerializeField]
        private TextMeshPro m_Text;

        [SerializeField]
        private SpriteRenderer m_IconRenderer;


        public override void SetIcon(Sprite icon)
        {
            m_IconRenderer.sprite = icon;
        }

        public override void SetLabel(string content)
        {
            m_Text.text = content;
        }

        public override void SetFontSize(float fontSize)
        {
            m_Text.fontSize = (int)fontSize;
        }

        public override void ShowIcon(bool value)
        {
            m_IconRenderer.gameObject.SetActive(value);
        }

        public override void ShowText(bool value)
        {
            m_Text.gameObject.SetActive(value);
        }
    }
}
