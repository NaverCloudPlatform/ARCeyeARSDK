using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ARCeye
{
    public class DistanceText : MonoBehaviour
    {
        private TextMeshPro m_TextMesh;
        public TextMeshPro textMesh => m_TextMesh;

        void Awake()
        {
            m_TextMesh = gameObject.AddComponent<TextMeshPro>();
            m_TextMesh.rectTransform.pivot = new Vector2(1, 0.5f);
            m_TextMesh.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            m_TextMesh.alignment = TextAlignmentOptions.Center;
            m_TextMesh.fontSize = 200;
            m_TextMesh.fontStyle = FontStyles.Bold;
            m_TextMesh.richText = true;
            m_TextMesh.color = Color.white;

            SetOpacity(0);

            if (ItemGenerator.Instance.font != null)
            {
                m_TextMesh.font = ItemGenerator.Instance.font;
            }

            m_TextMesh.fontMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 1;
        }

        public void SetLabel(string label)
        {
            m_TextMesh.text = label;
        }

        public void SetOpacity(float opacity)
        {
            var color = m_TextMesh.color;
            color.a = opacity;
            m_TextMesh.color = color;
        }

        public void ResetTransform()
        {
            // TurnSpot 모델의 Distance_Root 위치로 인해 TextMesh가 정상적으로 렌더링 되지 않음.
            // 아래와 같이 직접 설정.
            transform.localPosition = new Vector3(0.02f, 0, 0);
            transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            transform.localScale = new Vector3(0.035f, 0.035f, 0.035f);
        }
    }

}
