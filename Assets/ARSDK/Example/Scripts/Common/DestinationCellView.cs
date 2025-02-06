using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.U2D;
using TMPro;

namespace ARCeye
{
    public class DestinationCellView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_NameText;
        
        [SerializeField]
        private TMP_Text m_FullNameText;

        [SerializeField]
        private TMP_Text m_DistanceText;
        
        [SerializeField]
        private Image m_Icon;

        [SerializeField]
        private Button m_Button;

        private int m_Code;

        // TODO. View에서 Model을 들고 있는 구조 변경 필요.
        private LayerPOIItem m_Item;
        public LayerPOIItem poiItem => m_Item;

        public string Name => m_NameText.text;

        private float m_Distance;
        public float Distance => m_Distance;


        public void Initialize(Sprite icon, LayerPOIItem item)
        {
            string title = RemoveMultiLine(item.name);

            m_NameText.text = title;
            m_FullNameText.text = $"{item.stageName} - {item.fullName}";
            m_Code = item.dpcode;
            m_Icon.sprite = icon;
            m_DistanceText.gameObject.SetActive(false);
            m_Item = item;
        }

        private string RemoveMultiLine(string input) {
            return input.Replace("\n", " ");
        }

        public void RegisterAction(UnityAction action)
        {
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(action);
        }

        public string GetName()
        {
            return m_FullNameText.text;
        }

        public int GetCode()
        {
            return m_Code;
        }

        public void SetDistance(float value)
        {
            float distance;
            string unit = "m";
            string format = "N0";  // 기본적으로는 소수점 표기 X
            
            // 1m 이하의 거리는 0m로 표기.
            if(value < 1.0f)
            {
                distance = 0.0f;
            }
            // 1000m 이상의 값들은 km로 표기
            else if(value >= 1000.0f)
            {
                distance = value / 1000.0f;
                unit = "km";
                format = "N1";  // km 단위일 경우에만 소수점 1자리까지 표기.
            }
            else
            {
                distance = value;
            }

            m_DistanceText.text = $"{distance.ToString(format)}{unit}";

            m_Distance = value;
        }

        public void ShowDistanceText()
        {
            m_DistanceText.gameObject.SetActive(true);
        }

        public void HideDistanceText()
        {
            m_DistanceText.gameObject.SetActive(false);
        }
    }
}
