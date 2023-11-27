using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class MapCameraRig : MonoBehaviour
    {
        [Header("Render Target")]
        [SerializeField]
        private RenderTexture m_ShrinkRenderTexture;

        [SerializeField]
        private RenderTexture m_FullRenderTexture;

        
        [Header("Distance")]
        [SerializeField]
        private float m_ShinkCameraDistance;
        [SerializeField]
        private float m_FullCameraDistance;
        [SerializeField]
        private float m_FullCameraMinDistance = 20;
        [SerializeField]
        private float m_FullCameraMaxDistance = 90;


        private Camera m_MapCamera;
        private Camera m_MapPOICamera;
        private Camera m_MapArrowCamera;


        private Transform m_PitchRig;
        private Transform m_TranslationRig;
        private Transform m_MapArrow;

        private bool m_IsFullMode = false;
        private float m_InitFullYaw;  // 전체화면 MapView 모드에서 카메라의 회전을 고정.


        private Transform m_MainCamera;

        void Awake()
        {
            m_MainCamera = Camera.main.transform;
            FindMapCameraRigComponents();
            CheckLayersOfMapRigCameras();
        }

        void LateUpdate()
        {
            float x = m_MainCamera.transform.position.x;
            float z = m_MainCamera.transform.position.z;
            float yaw = m_MainCamera.transform.rotation.eulerAngles.y;

            var euler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, 180 + yaw, euler.z);

            if(m_IsFullMode) {
                Vector3 fullEulerAngle = m_PitchRig.rotation.eulerAngles;
                fullEulerAngle.x = 90;
                fullEulerAngle.y = m_InitFullYaw;
                fullEulerAngle.z = 0;
                m_PitchRig.rotation = Quaternion.Euler(fullEulerAngle);

                m_MapArrow.position = new Vector3(x, transform.position.y, z);
            } else {
                m_TranslationRig.localEulerAngles = new Vector3(0, 0, 0);

                transform.position = new Vector3(x, transform.position.y, z);
                m_MapArrow.localPosition = new Vector3(0, 0.15f, 0);
            }
        }

        private void FindMapCameraRigComponents() {
            m_MapCamera      = GetComponentInChildren<MapCamera>().GetComponent<Camera>();
            m_MapPOICamera   = GetComponentInChildren<MapPOICamera>().GetComponent<Camera>();
            m_MapArrowCamera = GetComponentInChildren<MapArrowCamera>().GetComponent<Camera>();

            m_PitchRig       = GetComponentInChildren<PitchRig>().transform;
            m_TranslationRig = GetComponentInChildren<TranslationRig>().transform;
            m_MapArrow       = GetComponentInChildren<MapArrow>().transform;
        }

        /// <summary>
        /// Map Rig 내 카메라들의 Culling Mask가 'Map', 'MapPOI', 'MapArrow'인지 확인.
        /// </summary>
        private void CheckLayersOfMapRigCameras() {
            CheckCameraLayer(m_MapCamera, "Map");
            CheckCameraLayer(m_MapPOICamera, "MapPOI");
            CheckCameraLayer(m_MapArrowCamera, "MapArrow");
        }

        private void CheckCameraLayer(Camera camera, string layerName) {
            int layerIndex = LayerMask.NameToLayer(layerName);
            int layerMask  = 1 << layerIndex;

            if(camera.cullingMask != layerMask) {
                NativeLogger.Print(LogLevel.WARNING, $"[MapCameraRig] {camera.name}의 culling mask != {layerName}");
                camera.cullingMask = layerMask;
            }
        }

        public void ChangeToFullMap(bool value) {
            m_IsFullMode = value;
            m_InitFullYaw = m_MainCamera.transform.rotation.eulerAngles.y;

            m_MapCamera.targetTexture = GetTargetTexture(value);
            m_MapPOICamera.targetTexture = GetTargetTexture(value);
            m_MapArrowCamera.targetTexture = GetTargetTexture(value);

            Vector3 euler = m_PitchRig.localRotation.eulerAngles;
            euler.x = value ? 90 : 130;
            euler.y = 0;
            euler.z = -180;
            m_PitchRig.localRotation = Quaternion.Euler(euler);

            // Full mode
            Vector3 cameraPosition = m_TranslationRig.localPosition;

            cameraPosition.x = 0;
            cameraPosition.y = 0;
            cameraPosition.z = value ? -m_FullCameraDistance : -m_ShinkCameraDistance;

            m_TranslationRig.localPosition = cameraPosition;
        }

        private RenderTexture GetTargetTexture(bool value) {
            return value ? m_FullRenderTexture : m_ShrinkRenderTexture;
        }

        public void Move(Vector2 delta) {
            float camToMapDist = m_MapCamera.transform.position.y;
            Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2, camToMapDist);
            Vector3 dest = new Vector3(center.x + delta.x, center.y + delta.y, camToMapDist);

            // Screen Coordinate에서의 이동 거리가 World Coordinate에서 얼마나 이동했는지 확인.
            Vector3 worldCenter = m_MapCamera.ScreenToWorldPoint(center);
            Vector3 destCenter = m_MapCamera.ScreenToWorldPoint(dest);
            Vector3 diff = -(destCenter - worldCenter);

            m_TranslationRig.Translate(diff, Space.World);
        }

        public void Zoom(float deltaRatio) {
            Vector3 cameraPosition = m_TranslationRig.localPosition;
            
            cameraPosition.z *= (1.0f + deltaRatio);
            cameraPosition.z = Mathf.Clamp(cameraPosition.z, -m_FullCameraMaxDistance, -m_FullCameraMinDistance);
            
            m_TranslationRig.localPosition = cameraPosition;
        }
    }
}