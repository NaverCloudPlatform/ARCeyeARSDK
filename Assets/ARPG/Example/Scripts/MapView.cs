using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye
{
    public class MapView : View
    {        
        [SerializeField]
        private GameObject m_StageArea;

        [SerializeField]
        private Text m_StageNameText;

        [SerializeField]
        private MapScreen m_FullMapScreen;

        [SerializeField]
        private MapScreen m_ShrinkedMapScreen;

        [SerializeField]
        private GameObject m_CloseMapButton;


        public override void Show(bool value) {
            base.Show(value);
            m_StageArea.gameObject.SetActive(value);
        }

        public void Start()
        {
            ResizeFullMapScreenRect();
        }

        public void ResizeFullMapScreenRect() 
        {
            var canvas = GetComponentInParent<Canvas>();
            var canvasRect = canvas.GetComponent<RectTransform>();

            float canvasWidth = canvasRect.sizeDelta.x;
            float canvasHeight = canvasRect.sizeDelta.y;

            float fullMapRTWidth = 1440.0f;
            float fullMapRTHeight = 1920.0f;

            float newWidth = fullMapRTWidth * canvasHeight / fullMapRTHeight;

            RectTransform rectTransform = m_FullMapScreen.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(newWidth, canvasHeight);
        }

        public void SetStage(string stageName)
        {
            m_StageNameText.text = stageName;
        }

        public void ShowFullMapScreen(bool value) {
            m_FullMapScreen.gameObject.SetActive(value);
            m_ShrinkedMapScreen.gameObject.SetActive(!value);
            m_CloseMapButton.SetActive(value);
        }

        /// <summary>
        ///   MapScreen의 touch 이벤트를 수신할 오브젝트 설정.
        ///   주로 ShrinkMapScreen의 터치 이벤트를 감지한다.
        /// </summary>
        public void SetMapTouchListener(IMapTouchListener listener) {
            m_ShrinkedMapScreen.touchListener = listener;
        }
    }
}