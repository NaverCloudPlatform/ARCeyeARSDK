using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ARCeye
{
    public class UnityMapPOI : UnityModel
    {
        [SerializeField]
        private TextMeshPro m_Text;

        [SerializeField]
        private SpriteRenderer m_IconRenderer;

        private Billboard m_Billboard;

        // Billboard 효과가 적용될 카메라를 할당.
        private Camera m_TargetCamera;
        public Camera targetCamera {
            get {
                return m_TargetCamera;
            }
            set {
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


        private void Awake() {
            int layerIndex = LayerMask.NameToLayer("MapPOI");
            gameObject.layer = layerIndex;
            m_Text.gameObject.layer = layerIndex;
            m_IconRenderer.gameObject.layer = layerIndex;

            m_Billboard = GetComponent<Billboard>();
            m_Billboard.rotationMode = Billboard.RotationMode.CAMERA;

            if(ItemGenerator.Instance.font != null) {
                // m_Text.font = ItemGenerator.Instance.font;
            }

            ActivateMinimapMode();
        }

        private void Start() {
            MapCameraRig mapCameraRig = FindObjectOfType<MapCameraRig>();
            if(mapCameraRig == null)
            {
                Debug.LogError("Failed to find 'MapCameraRig' component in the current scene.");
                return;
            }

            m_TranslationRig = mapCameraRig.GetComponentInChildren<TranslationRig>();
            if(m_TranslationRig == null)
            {
                Debug.LogError("Failed to find 'TranslationRig' component under the MapCameraRig");
                return;
            }
        }
        
        private void Update() {
            ScaleByCameraDistance();
        }

        private void ScaleByCameraDistance() {
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

        public void SetIcon(Sprite icon) {
            m_IconRenderer.sprite = icon;
        }

        public void SetLabel(string content) {
            m_Text.text = content;
        }

        public void SetFontSize(float fontSize) {
            m_Text.fontSize = (int) fontSize;
        }

        /// <summary>
        ///   ARPG의 POIDisplayType 값에 따라 display 컴포넌트 설정.
        ///   Display의 각 값들은 다음과 같다.
        ///     None - 0
        ///     Icon - 1
        ///     Text - 2
        ///     Icon and Text - 3
        /// </summary>
        public void SetDisplay(int display) {
            switch(display)
            {
                case 0:
                    m_Text.gameObject.SetActive(false);
                    m_IconRenderer.gameObject.SetActive(false);
                    break;
                case 1:
                    m_Text.gameObject.SetActive(false);
                    m_IconRenderer.gameObject.SetActive(true);
                    break;
                case 2:
                    m_Text.gameObject.SetActive(true);
                    m_IconRenderer.gameObject.SetActive(false);
                    break;
                case 3:
                    m_Text.gameObject.SetActive(true);
                    m_IconRenderer.gameObject.SetActive(true);
                    break;
                default:
                    Debug.LogError($"Invalid display value ({display})");
                    break;
            }
        }
    }
}