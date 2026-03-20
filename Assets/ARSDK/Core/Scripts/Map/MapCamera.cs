using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARCeye
{
    public class MapCamera : MonoBehaviour
    {
        private Camera m_Camera;
        public Camera Camera => m_Camera;

        private Camera[] m_OverlayCameras;

        private void Awake()
        {
            m_Camera = GetComponent<Camera>();

            int mapLayer = LayerMask.NameToLayer("Map");
            int amprojVizLayer = LayerMask.NameToLayer("AMProjViz");
            int mapPOILayer = LayerMask.NameToLayer("MapPOI");
            int mapArrowLayer = LayerMask.NameToLayer("MapArrow");

            m_Camera.cullingMask = (1 << mapLayer) | (1 << amprojVizLayer);

            Camera poiCamera = CreateOverlayCamera("MapPOICamera", 1 << mapPOILayer);
            Camera arrowCamera = CreateOverlayCamera("MapArrowCamera", 1 << mapArrowLayer);
            m_OverlayCameras = new[] { poiCamera, arrowCamera };

            if (GraphicsSettings.currentRenderPipeline != null)
            {
                ConfigureURPCameraStack(poiCamera, arrowCamera);
            }
        }

        private Camera CreateOverlayCamera(string name, int cullingMask)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var cam = go.AddComponent<Camera>();
            cam.CopyFrom(m_Camera);
            cam.cullingMask = cullingMask;
            cam.clearFlags = CameraClearFlags.Depth;
            cam.depth = m_Camera.depth + 1;
            cam.targetTexture = m_Camera.targetTexture;

            return cam;
        }

        /// <summary>
        /// Base 카메라와 Overlay 카메라에 동일한 RenderTexture를 설정.
        /// MapScreen에서 RenderTexture 생성/변경 시 호출.
        /// </summary>
        public void SetTargetTexture(RenderTexture renderTexture)
        {
            bool isEnabled = renderTexture != null;

            m_Camera.targetTexture = renderTexture;
            m_Camera.enabled = isEnabled;

            if (m_OverlayCameras == null) return;

            foreach (var cam in m_OverlayCameras)
            {
                cam.targetTexture = renderTexture;
                cam.enabled = isEnabled;
            }
        }

        /// <summary>
        /// 리플렉션으로 URP Camera Stack 구성.
        /// ARSDK asmdef에 URP 참조가 없으므로 리플렉션 사용.
        /// </summary>
        private void ConfigureURPCameraStack(Camera poiCamera, Camera arrowCamera)
        {
            Type cameraDataType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");

            if (cameraDataType == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[MapCamera] UniversalAdditionalCameraData type not found.");
                return;
            }

            var renderTypeProp = cameraDataType.GetProperty("renderType");
            var cameraStackProp = cameraDataType.GetProperty("cameraStack");

            if (renderTypeProp == null || cameraStackProp == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[MapCamera] URP camera properties not found.");
                return;
            }

            Type renderTypeEnum = renderTypeProp.PropertyType;
            object baseType = Enum.ToObject(renderTypeEnum, 0);    // CameraRenderType.Base
            object overlayType = Enum.ToObject(renderTypeEnum, 1);  // CameraRenderType.Overlay

            // Base camera
            var baseData = GetOrAddComponent(m_Camera, cameraDataType);
            if (baseData == null) return;
            renderTypeProp.SetValue(baseData, baseType);

            // Overlay cameras
            SetRenderType(poiCamera, cameraDataType, renderTypeProp, overlayType);
            SetRenderType(arrowCamera, cameraDataType, renderTypeProp, overlayType);

            // Camera Stack에 Overlay 추가
            if (cameraStackProp.GetValue(baseData) is List<Camera> cameraStack)
            {
                cameraStack.Clear();
                cameraStack.Add(poiCamera);
                cameraStack.Add(arrowCamera);
            }
        }

        private void SetRenderType(Camera cam, Type cameraDataType,
            System.Reflection.PropertyInfo renderTypeProp, object renderType)
        {
            var data = GetOrAddComponent(cam, cameraDataType);
            if (data != null)
            {
                renderTypeProp.SetValue(data, renderType);
            }
        }

        private Component GetOrAddComponent(Camera cam, Type componentType)
        {
            var component = cam.GetComponent(componentType);
            if (component == null)
            {
                component = cam.gameObject.AddComponent(componentType);
            }
            return component;
        }
    }
}
