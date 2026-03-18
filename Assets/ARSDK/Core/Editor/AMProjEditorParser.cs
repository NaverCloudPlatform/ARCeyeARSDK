using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace ARCeye
{
    /// <summary>
    /// amproj 파일을 에디터에서 동기적으로 파싱하여 드로잉 데이터를 생성.
    /// </summary>
    internal class AMProjEditorParser
    {
        private JObject m_Root;
        private readonly Dictionary<string, float> m_Stages = new Dictionary<string, float>();
        private readonly List<string> m_StageNames = new List<string>();

        // 스테이지 이름 목록
        public List<string> StageNames => m_StageNames;

        // 현재 선택된 스테이지 이름
        public string SelectedStageName { get; private set; }

        /// <summary>
        /// amproj 파일 로드 및 스테이지 목록 파싱.
        /// </summary>
        public bool Load(string path)
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[AMProj Viewer] amproj file is empty.");
                return false;
            }

            m_Root = JObject.Parse(json);
            m_Stages.Clear();
            m_StageNames.Clear();
            SelectedStageName = null;

            ParseStages();
            return m_StageNames.Count > 0;
        }

        /// <summary>
        /// 스테이지 선택 후 해당 스테이지의 드로잉 레이어 반환.
        /// </summary>
        public List<AMProjDrawData.Layer> SelectStage(int index)
        {
            if (index < 0 || index >= m_StageNames.Count) return new List<AMProjDrawData.Layer>();

            SelectedStageName = m_StageNames[index];

            JObject stageObject = FindStage(SelectedStageName);
            if (stageObject == null) return new List<AMProjDrawData.Layer>();

            var layersArray = stageObject["layers"] as JArray;
            if (layersArray == null) return new List<AMProjDrawData.Layer>();

            var layers = new List<AMProjDrawData.Layer>();
            foreach (JObject layer in layersArray.OfType<JObject>())
            {
                var parsed = ParseLayer(layer);
                if (parsed != null) layers.Add(parsed);
            }
            return layers;
        }

        #region Stage Parsing

        // 버전별 스테이지 파싱 분기
        private void ParseStages()
        {
            int version = m_Root["version"]?.Value<int>() ?? 1;
            switch (version)
            {
                case 1:
                case 2:
                    ParseStagesFromArray(m_Root["stages"] as JArray, "height");
                    break;
                case 3:
                    ParseStagesV3();
                    break;
                default:
                    ParseStagesFromArray(m_Root["stages"] as JArray, "height");
                    break;
            }
        }

        // V1/V2 스테이지 파싱
        private void ParseStagesFromArray(JArray stageObjects, string elevationKey)
        {
            if (stageObjects == null) return;
            foreach (var stage in stageObjects)
            {
                string name = stage["name"]?.Value<string>();
                float height = stage[elevationKey]?.Value<float>() ?? 0f;
                if (name != null)
                {
                    m_Stages[name] = height;
                    m_StageNames.Add(name);
                }
            }
        }

        // V3 스테이지 파싱 (usage != 2 필터링)
        private void ParseStagesV3()
        {
            var rootChildren = m_Root["root"]?["children"] as JArray;
            if (rootChildren == null) return;

            foreach (var child in rootChildren)
            {
                if (child["type"]?.Value<string>() == "Stage" && child["usage"]?.Value<int>() != 2)
                {
                    string name = child["name"]?.Value<string>();
                    float elevation = child["elevation"]?.Value<float>() ?? 0f;
                    if (name != null)
                    {
                        m_Stages[name] = elevation;
                        m_StageNames.Add(name);
                    }
                }
            }
        }

        // 이름으로 스테이지 JObject 검색
        private JObject FindStage(string stageName)
        {
            var stageObjects = GetStageObjects();
            return stageObjects?
                .OfType<JObject>()
                .FirstOrDefault(obj => obj["name"]?.ToString() == stageName);
        }

        // 버전별 스테이지 배열 접근
        private JArray GetStageObjects()
        {
            int version = m_Root["version"]?.Value<int>() ?? 1;
            return version == 3
                ? m_Root["root"]?["children"] as JArray
                : m_Root["stages"] as JArray;
        }

        #endregion

        #region Layer Parsing

        // 레이어 내 아이템 파싱 (Geometry, GraphEdge, POI)
        private AMProjDrawData.Layer ParseLayer(JObject layer)
        {
            var items = layer["items"] as JArray;
            if (items == null || items.Count == 0) return null;

            var layerColor = layer["color"] as JArray;
            Color color = layerColor != null
                ? new Color((float)layerColor[0], (float)layerColor[1], (float)layerColor[2])
                : Color.white;

            var drawLayer = new AMProjDrawData.Layer { color = color };

            // GraphEdge 렌더링을 위한 노드 위치 캐싱
            var graphNodes = CacheGraphNodes(items);

            foreach (JObject item in items.OfType<JObject>())
            {
                string type = (string)item["type"];
                switch (type)
                {
                    case "GeometryItem":
                        ParseGeometryItem(item, drawLayer);
                        break;
                    case "GraphEdgeItem":
                        ParseGraphEdgeItem(item, drawLayer, graphNodes);
                        break;
                    case "POIItem":
                        ParsePOIItem(item, drawLayer);
                        break;
                }
            }

            return drawLayer;
        }

        // GraphNodeItem UUID → 위치 매핑
        private Dictionary<string, Vector3> CacheGraphNodes(JArray items)
        {
            var nodes = new Dictionary<string, Vector3>();
            foreach (JObject item in items.OfType<JObject>())
            {
                if ((string)item["type"] != "GraphNodeItem") continue;
                string uuid = (string)item["uuid"];
                JArray pos = (JArray)item["position"];
                nodes[uuid] = new Vector3((float)pos[0], (float)pos[1], (float)pos[2]);
            }
            return nodes;
        }

        // GeometryItem vertices → 선분 변환 (segments 지원)
        private void ParseGeometryItem(JObject item, AMProjDrawData.Layer drawLayer)
        {
            var vertices = item["vertices"] as JArray;
            if (vertices == null) return;

            var segments = item["segments"] as JArray;
            if (segments == null)
            {
                AddSegment(drawLayer, vertices, 0, vertices.Count);
            }
            else
            {
                foreach (JObject segment in segments.OfType<JObject>())
                {
                    AddSegment(drawLayer, vertices, (int)segment["offset"], (int)segment["count"]);
                }
            }
        }

        // vertices 배열의 일부를 Line으로 변환 (X축 반전)
        private void AddSegment(AMProjDrawData.Layer drawLayer, JArray vertices, int offset, int count)
        {
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                JArray v = (JArray)vertices[offset + i];
                points[i] = new Vector3(-(float)v[0], (float)v[1], (float)v[2]);
            }
            drawLayer.lines.Add(new AMProjDrawData.Line { points = points, loop = true });
        }

        // GraphEdgeItem from/to 노드 간 연결선
        private void ParseGraphEdgeItem(JObject item, AMProjDrawData.Layer drawLayer, Dictionary<string, Vector3> graphNodes)
        {
            string fromId = (string)item["link.from"];
            string toId = (string)item["link.to"];

            if (!graphNodes.TryGetValue(fromId, out Vector3 from) ||
                !graphNodes.TryGetValue(toId, out Vector3 to)) return;

            drawLayer.lines.Add(new AMProjDrawData.Line
            {
                points = new[]
                {
                    new Vector3(-from.x, from.y, from.z),
                    new Vector3(-to.x, to.y, to.z)
                }
            });
        }

        // POIItem center 위치 및 text.display 텍스트 추출
        private void ParsePOIItem(JObject item, AMProjDrawData.Layer drawLayer)
        {
            var center = item["center"] as JArray;
            var textDisplay = item["text.display"] as JObject;
            if (center == null || textDisplay == null) return;

            string displayText = ResolveDisplayText(textDisplay);
            if (string.IsNullOrEmpty(displayText)) return;

            drawLayer.pois.Add(new AMProjDrawData.POI
            {
                position = new Vector3(-(float)center[0], (float)center[1], (float)center[2]),
                displayText = displayText
            });
        }

        // 표시 텍스트 결정 (ko_KR 우선, 비어있지 않은 첫 번째 값 대체)
        private string ResolveDisplayText(JObject textDisplay)
        {
            string text = textDisplay["ko_KR"]?.Value<string>();
            if (!string.IsNullOrEmpty(text)) return text;

            foreach (var prop in textDisplay.Properties())
            {
                string val = prop.Value?.Value<string>();
                if (!string.IsNullOrEmpty(val)) return val;
            }
            return null;
        }

        #endregion
    }
}
