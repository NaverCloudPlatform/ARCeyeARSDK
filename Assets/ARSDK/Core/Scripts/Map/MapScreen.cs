using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARCeye
{
    public class MapScreen : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private RenderTexture m_RenderTexture;


        private void Start()
        {
            CreateMapViewTexture();

            AttachRenderTextureToCamera<MapCamera>();
            AttachRenderTextureToCamera<MapPOICamera>();
            AttachRenderTextureToCamera<MapArrowCamera>();
        }

        public void Activate(bool value)
        {
            if(value)
            {
                AttachRenderTextureToCamera<MapCamera>();
                AttachRenderTextureToCamera<MapPOICamera>();
                AttachRenderTextureToCamera<MapArrowCamera>();
            }

            gameObject.SetActive(value);
        }

        private void CreateMapViewTexture()
        {
            RawImage image = GetComponentInChildren<RawImage>();

            if(image == null) {
                Debug.LogError("[MapScreen] Failed to find RawImage under MapScreen");
                return;
            }

            if(image.mainTexture != null && image.mainTexture is RenderTexture) {
                (image.mainTexture as RenderTexture).Release();
            }

            Vector2Int rtSize = GetRenderTextureSize();

            m_RenderTexture = new RenderTexture(rtSize.x, rtSize.y, 24);
            m_RenderTexture.Create();

            image.texture = m_RenderTexture;
        }

        private void AttachRenderTextureToCamera<T>() where T : MonoBehaviour
        {
            T mapCamera = FindObjectOfType<T>();
            Camera camera = mapCamera.GetComponent<Camera>();
            if(camera != null) {
                camera.targetTexture = m_RenderTexture;
            }
        }

        private Vector2Int GetRenderTextureSize()
        {
            Canvas mainCanvas = GetComponentInParent<Canvas>();
            CanvasScaler canvasScaler = mainCanvas.GetComponent<CanvasScaler>();

            float maxWidth = canvasScaler.referenceResolution.x;
            float maxHeight = canvasScaler.referenceResolution.y;

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(transform);

            float width = bounds.size.x;
            float height = bounds.size.y;


            if(width > maxWidth) {
                float ratio = maxWidth / width;
                width *= ratio;
                height *= ratio;
            } else if(height > maxHeight) {
                float ratio = maxHeight / height;
                width *= ratio;
                height *= ratio;
            }

            return new Vector2Int((int) width, (int) height);
        }

        public void OnPointerDown(PointerEventData eventData) {
            
        }

        public void OnPointerUp(PointerEventData eventData) {
            
        }
    }
}