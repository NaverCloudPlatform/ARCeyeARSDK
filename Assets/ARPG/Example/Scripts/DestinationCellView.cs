using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ARCeye
{
    public class DestinationCellView : MonoBehaviour
    {
        [SerializeField]
        private Text m_NameText;
        
        [SerializeField]
        private Text m_FullNameText;
        
        [SerializeField]
        private Image m_Icon;

        [SerializeField]
        private Button m_Button;
        

        public void Initialize(LayerPOIItem item)
        {
            m_NameText.text = item.name;
            m_FullNameText.text = $"{item.stageName} - {item.fullName}";

            string iconName = POIGenerator.ConvertToName(item.dpcode);
            Texture2D iconTexture = Resources.Load<Texture2D>($"Image/Categories/UI_{iconName}");
            Sprite iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            m_Icon.sprite = iconSprite;
        }

        public void RegisterAction(UnityAction action)
        {
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(action);
        }
    }
}
