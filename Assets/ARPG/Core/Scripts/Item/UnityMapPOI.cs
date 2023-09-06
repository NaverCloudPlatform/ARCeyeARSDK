using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityMapPOI : UnityModel
    {
        [SerializeField]
        private TextMesh m_Text;

        [SerializeField]
        private SpriteRenderer m_IconRenderer;

        private Billboard m_Billboard;

        // Billboard 효과가 적용될 카메라를 할당.
        public Camera targetCamera {
            set {
                m_Billboard.targetCamera = value;
            }
        }


        private void Awake() {
            int layerIndex = LayerMask.NameToLayer("MapPOI");
            gameObject.layer = layerIndex;
            m_Text.gameObject.layer = layerIndex;
            m_IconRenderer.gameObject.layer = layerIndex;

            m_Billboard = GetComponent<Billboard>();
            m_Billboard.rotationMode = Billboard.RotationMode.CAMERA;

            if(ItemGenerator.Instance.font != null) {
                m_Text.font = ItemGenerator.Instance.font;
            }

            var meshRenderer = m_Text.GetComponent<MeshRenderer>();
            
            Texture fontTexture = m_Text.font.material.mainTexture;
            meshRenderer.material = ItemGenerator.Instance.poiTextMaterial;
            meshRenderer.sharedMaterial.mainTexture = fontTexture;
        }

        public void SetIcon(Sprite icon) {
            m_IconRenderer.sprite = icon;
        }

        public void SetLabel(string content) {
            m_Text.text = content;
        }

        public void SetFontSize(float fontSize) {
            m_Text.fontSize = (int) fontSize;
        }
    }
}