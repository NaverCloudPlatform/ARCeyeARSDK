using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityMapPOI : UnityModel
    {
        private Billboard m_Billboard;
        private MapPOIRenderer m_MapPOIRenderer;

        // Billboard 효과가 적용될 카메라를 할당.
        private Camera m_TargetCamera;
        public Camera targetCamera
        {
            get
            {
                return m_TargetCamera;
            }
            set
            {
                m_TargetCamera = value;
                m_Billboard.targetCamera = value;
            }
        }

        // 지도를 렌더링하는 MapCamera의 Translation을 담당하는 rig.
        // rig의 localPosition.z 값을 기준으로 MapPOI의 zoom 값을 설정한다.
        private TranslationRig m_TranslationRig;

        // MapPOI의 scale이 1이 되는 MapCamera의 거리. 이 값이 클수록 MapPOI가 작게 보인다.
        private float m_DefaultDist;
        // private const float k_DefaultDistFullmap = 55.0f;
        private const float k_DefaultDistFullmap = 55.0f;
        private const float k_DefaultDistMinimap = 15.0f;


        private void Awake()
        {
            m_Billboard = gameObject.AddComponent<Billboard>();
            m_Billboard.rotationMode = Billboard.RotationMode.CAMERA;

            m_MapPOIRenderer = GetComponent<MapPOIRenderer>();
            if (m_MapPOIRenderer == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[UnityMapPOI] MapPOIRenderer component is missing.");
                return;
            }

            ActivateMinimapMode();

            InitLayerInModel("MapPOI");
        }

        private void Start()
        {
            MapCameraController mapCameraController = FindObjectOfType<MapCameraController>();
            if (mapCameraController == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[UnityMapPOI] Failed to find MapCameraController in the scene.");
                return;
            }

            m_TranslationRig = mapCameraController.GetComponentInChildren<TranslationRig>();
            if (m_TranslationRig == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[UnityMapPOI] Failed to find TranslationRig under MapCameraController.");
                return;
            }
        }

        private void LateUpdate()
        {
            ScaleByCameraDistance();
        }

        private void ScaleByCameraDistance()
        {
            float dist = Mathf.Abs(m_TranslationRig.transform.localPosition.z);
            float scale = dist / m_DefaultDist;
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public void ActivateFullmapMode()
        {
            m_DefaultDist = k_DefaultDistFullmap;
        }

        public void ActivateMinimapMode()
        {
            m_DefaultDist = k_DefaultDistMinimap;
        }

        public void SetIcon(Sprite icon)
        {
            m_MapPOIRenderer.SetIcon(icon);
        }

        public void SetLabel(string content)
        {
            m_MapPOIRenderer.SetLabel(content);
        }

        public void SetFontSize(float fontSize)
        {
            m_MapPOIRenderer.SetFontSize(fontSize);
        }

        /// <summary>
        ///   ARPG의 POIDisplayType 값에 따라 display 컴포넌트 설정.
        ///   Display의 각 값들은 다음과 같다.
        ///     None - 0
        ///     Icon - 1
        ///     Text - 2
        ///     Icon and Text - 3
        /// </summary>
        public void SetDisplay(int display)
        {
            switch (display)
            {
                case 0:
                    m_MapPOIRenderer.ShowText(false);
                    m_MapPOIRenderer.ShowIcon(false);
                    break;
                case 1:
                    m_MapPOIRenderer.ShowText(false);
                    m_MapPOIRenderer.ShowIcon(true);
                    break;
                case 2:
                    m_MapPOIRenderer.ShowText(true);
                    m_MapPOIRenderer.ShowIcon(false);
                    break;
                case 3:
                    m_MapPOIRenderer.ShowText(true);
                    m_MapPOIRenderer.ShowIcon(true);
                    break;
                default:
                    NativeLogger.Print(LogLevel.ERROR, $"[UnityMapPOI] Invalid POI display value. display={display}");
                    break;
            }
        }
    }
}