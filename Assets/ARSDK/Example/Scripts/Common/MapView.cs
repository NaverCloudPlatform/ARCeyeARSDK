using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARCeye
{
    public class MapView : View
    {        
        [SerializeField]
        private GameObject m_StageArea;

        [SerializeField]
        private TMP_Text m_StageNameText;

        [SerializeField]
        private MapScreen m_FullMapScreen;

        [SerializeField]
        private MapScreen m_ShrinkedMapScreen;

        [SerializeField]
        private GameObject m_HideMapButton;

        [SerializeField]
        private GameObject m_CloseMapButton;

        [SerializeField]
        private GameObject m_NextStep;


        private void Awake()
        {
            m_FullMapScreen.gameObject.SetActive(false);
            m_ShrinkedMapScreen.gameObject.SetActive(false);
        }

        public override void Show(bool value) {
            base.Show(value);
            m_StageArea.gameObject.SetActive(value);

            if(m_HideMapButton)
            {
                m_HideMapButton.SetActive(value);
            }

            // Hide일 경우 모든 MapScreen을 비활성화 한다.
            if(!value)
            {
                m_FullMapScreen.gameObject.SetActive(false);
                m_ShrinkedMapScreen.gameObject.SetActive(false);
            }
        }

        public void SetStageLabel(string stageName)
        {
            m_StageNameText.text = stageName;
        }

        public void ActivateFullMapScreen(bool value) {
            m_StageArea.gameObject.SetActive(!value);

            m_FullMapScreen.Activate(value);
            m_ShrinkedMapScreen.Activate(!value);

            if(m_HideMapButton)
            {
                m_HideMapButton.SetActive(!value);
            }

            if(m_CloseMapButton)
            {
                m_CloseMapButton.SetActive(value);
            }

            if(m_NextStep)
            {
                m_NextStep.SetActive(!value);
            }
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