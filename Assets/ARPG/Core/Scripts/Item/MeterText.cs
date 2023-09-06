using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class MeterText : MonoBehaviour
    {
        private MeshRenderer m_MeshRenderer;
        private TextMesh m_TextMesh;
        public TextMesh textMesh => m_TextMesh;

        void Awake()
        {
            ResetTransform();

            // TextMesh 생성.
            m_TextMesh = gameObject.AddComponent<TextMesh>();
            m_TextMesh.text = "   m";
            m_TextMesh.offsetZ = 0;
            m_TextMesh.characterSize = 1;
            m_TextMesh.lineSpacing = 1;
            m_TextMesh.anchor = TextAnchor.MiddleCenter;
            m_TextMesh.alignment = TextAlignment.Right;
            m_TextMesh.tabSize = 4;
            m_TextMesh.fontSize = 200;
            m_TextMesh.fontStyle = FontStyle.Bold;
            m_TextMesh.richText = true;
            m_TextMesh.color = Color.white;

            if(ItemGenerator.Instance.font != null) {
                m_TextMesh.font = ItemGenerator.Instance.font;
            }

            // ZTest가 비활성화 된 Text shader가 추가 된 material 생성.
            m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
            
            Texture fontTexture = m_TextMesh.font.material.mainTexture;
            m_MeshRenderer.material = ItemGenerator.Instance.poiTextMaterial;
            m_MeshRenderer.material.SetFloat("_CullMode", 0.0f);
            m_MeshRenderer.sharedMaterial.mainTexture = fontTexture;
        }

        public void ResetTransform()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        }
    }

}