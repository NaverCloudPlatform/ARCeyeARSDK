using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ARCeye
{
    /// <summary>
    /// Scene 뷰에 amproj 드로잉 데이터를 렌더링.
    /// </summary>
    internal class AMProjSceneRenderer
    {
        private List<AMProjDrawData.Layer> m_Layers = new List<AMProjDrawData.Layer>();

        // POI 텍스트 스타일 캐시
        private GUIStyle m_OutlineStyle;
        private GUIStyle m_TextStyle;

        // 시각화 표시 여부
        public bool Visible { get; set; } = true;
        public bool GeometryVisible { get; set; } = true;
        public bool POIVisible { get; set; } = true;

        // 렌더링할 레이어 설정
        public void SetLayers(List<AMProjDrawData.Layer> layers)
        {
            m_Layers = layers ?? new List<AMProjDrawData.Layer>();
        }

        // Scene 뷰 렌더링 콜백
        public void OnSceneGUI(SceneView sceneView)
        {
            if (!Visible || m_Layers.Count == 0 || EditorApplication.isPlayingOrWillChangePlaymode) return;

            EnsureStyles();

            foreach (var layer in m_Layers)
            {
                if (GeometryVisible) DrawLines(layer);
                if (POIVisible) DrawPOIs(layer, sceneView);
            }
        }

        // Geometry/GraphEdge 선분 렌더링
        private void DrawLines(AMProjDrawData.Layer layer)
        {
            Handles.color = layer.color;

            foreach (var line in layer.lines)
            {
                if (line.points.Length < 2) continue;

                for (int i = 0; i < line.points.Length - 1; i++)
                {
                    Handles.DrawLine(line.points[i], line.points[i + 1], 2f);
                }

                // loop 연결 (마지막 → 첫 번째)
                if (line.loop && line.points.Length > 2)
                {
                    Handles.DrawLine(line.points[^1], line.points[0], 2f);
                }
            }
        }

        // POI 점 + 텍스트 렌더링
        private void DrawPOIs(AMProjDrawData.Layer layer, SceneView sceneView)
        {
            Handles.color = layer.color;

            foreach (var poi in layer.pois)
            {
                // 카메라 방향 기준 원형 점
                Handles.DrawSolidDisc(poi.position, sceneView.camera.transform.forward, 0.1f);

                // 검은 outline (8방향 1px 오프셋) + 흰 글씨
                Vector3 labelPos = poi.position + Vector3.up * 0.4f;

                for (int ox = -1; ox <= 1; ox++)
                {
                    for (int oy = -1; oy <= 1; oy++)
                    {
                        if (ox == 0 && oy == 0) continue;
                        m_OutlineStyle.contentOffset = new Vector2(ox, oy);
                        Handles.Label(labelPos, poi.displayText, m_OutlineStyle);
                    }
                }

                Handles.Label(labelPos, poi.displayText, m_TextStyle);
            }
        }

        // GUIStyle 초기화 (1회만 생성)
        private void EnsureStyles()
        {
            if (m_OutlineStyle != null) return;

            m_OutlineStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 13
            };
            m_OutlineStyle.normal.textColor = Color.black;
            m_OutlineStyle.hover.textColor = Color.black;
            m_OutlineStyle.active.textColor = Color.black;
            m_OutlineStyle.focused.textColor = Color.black;

            m_TextStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 13
            };
            m_TextStyle.normal.textColor = Color.white;
            m_TextStyle.hover.textColor = Color.white;
            m_TextStyle.active.textColor = Color.white;
            m_TextStyle.focused.textColor = Color.white;
        }
    }
}
