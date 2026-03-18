using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ARCeye
{
    public class AMProjVisualizer : MonoBehaviour
    {
        private JObject m_Root;
        private string m_JsonStr;
        private bool m_ReadAMProjFinished;


        public void Load(string amprojFilePath)
        {
            m_ReadAMProjFinished = false;
            StartCoroutine(LoadInternal(amprojFilePath));
        }

        private IEnumerator LoadInternal(string amprojFilePath)
        {
            yield return AMProjFileReader.ReadCoroutine(amprojFilePath, json => m_JsonStr = json);

            m_ReadAMProjFinished = true;

            if (!string.IsNullOrEmpty(m_JsonStr))
            {
                m_Root = JObject.Parse(m_JsonStr);
            }
        }

        public void Visualize(string stageName)
        {
            NativeLogger.Print(LogLevel.DEBUG, $"[AMProjVisualizer] Visualizing amproj stage. stageName={stageName}");
            StartCoroutine(VisualizeInternal(stageName));
        }

        private IEnumerator VisualizeInternal(string stageName)
        {
            yield return new WaitUntil(() => m_ReadAMProjFinished);

            // 기존에 추가되어 있던 요소들을 제거.
            Reset();

            // 새로운 요소들 추가.
            JObject stageObject = LoadStage(stageName);
            if (stageObject == null)
            {
                yield break;
            }

            JArray layersObject = LoadAllLayers(stageObject);

            // 모든 레이어를 순회하면서 vertices 그리기.
            foreach (JObject layer in layersObject)
            {
                AddLinesInLayer(layer);
            }
        }

        public void Reset()
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private JObject LoadStage(string stageName)
        {
            var stageObjects = GetStageObjects();
            if (stageObjects == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[AMProjVisualizer] No stage list found in amproj file.");
                return null;
            }

            return stageObjects
                    .OfType<JObject>()
                    .FirstOrDefault(obj => obj["name"]?.ToString() == stageName);
        }

        private JArray GetStageObjects()
        {
            int version = m_Root["version"]?.Value<int>() ?? 1;

            switch (version)
            {
                case 1:
                case 2:
                    return GetStageObjectsV1();
                case 3:
                    return GetStageObjectsV3();
                default:
                    NativeLogger.Print(LogLevel.WARNING, $"[AMProjVisualizer] Unsupported amproj version: {version}. Falling back to V1 format.");
                    return GetStageObjectsV1();
            }
        }

        private JArray GetStageObjectsV1()
        {
            return (JArray)m_Root["stages"];
        }

        private JArray GetStageObjectsV3()
        {
            return m_Root["root"]?["children"] as JArray;
        }

        private JArray LoadAllLayers(JObject stageObject)
        {
            return (JArray)stageObject["layers"];
        }

        private void AddLinesInLayer(JObject layer)
        {
            string layerName = (string)layer["name"];
            JArray layerColor = (JArray)layer["color"];
            JArray items = (JArray)layer["items"];
            if (items.Count == 0)
            {
                return;
            }

            GameObject lineRendererGo = new GameObject($"amproj - {layerName}");
            lineRendererGo.transform.parent = transform;

            Color stageColor = new Color((float)layerColor[0], (float)layerColor[1], (float)layerColor[2]);

            var graphNodePositions = CacheGraphNodeItemPositions(items);

            foreach (JObject item in items)
            {
                string itemName = (string)item["name"];
                string type = (string)item["type"];

                if (type == "GeometryItem")
                {
                    AddGeometryItemLines(item, layerName, itemName, stageColor, lineRendererGo.transform);
                }
                else if (type == "GraphEdgeItem")
                {
                    string fromId = (string)item["link.from"];
                    string toId = (string)item["link.to"];

                    Vector3 from = graphNodePositions[fromId];
                    Vector3 to = graphNodePositions[toId];

                    AddGraphEdgeLines(from, to, layerName, stageColor, lineRendererGo.transform);
                }
            }
        }

        private Dictionary<string, Vector3> CacheGraphNodeItemPositions(JArray items)
        {
            Dictionary<string, Vector3> graphNodeItems = new Dictionary<string, Vector3>();

            foreach (JObject item in items)
            {
                string type = (string)item["type"];

                if (type == "GraphNodeItem")
                {
                    string uuid = (string)item["uuid"];
                    JArray positionArr = (JArray)item["position"];
                    Vector3 position = new Vector3((float)positionArr[0], (float)positionArr[1], (float)positionArr[2]);

                    graphNodeItems.Add(uuid, position);
                }
            }

            return graphNodeItems;
        }

        private void AddGeometryItemLines(JObject item, string layerName, string itemName, Color color, Transform parent)
        {
            JArray vertices = (JArray)item["vertices"];
            if (vertices == null)
            {
                return;
            }

            string lineName = $"{layerName} {itemName}";
            JArray segments = (JArray)item["segments"];

            if (segments == null)
            {
                AddGeometryItemLineSegment(lineName, color, parent, vertices, 0, vertices.Count);
            }
            else
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    JObject segment = (JObject)segments[i];
                    int offset = (int)segment["offset"];
                    int count = (int)segment["count"];

                    AddGeometryItemLineSegment(lineName, color, parent, vertices, offset, count);
                }
            }
        }

        private void AddGeometryItemLineSegment(string itemName, Color stageColor, Transform parent, JArray vertices, int offset, int count)
        {
            LineRenderer lineRenderer = CreateLineRenderer(itemName, stageColor, parent);
            lineRenderer.positionCount = count;

            for (int i = 0; i < count; i++)
            {
                JArray vertex = (JArray)vertices[offset + i];

                float x = (float)vertex[0];
                float y = (float)vertex[1];
                float z = (float)vertex[2];

                lineRenderer.SetPosition(i, new Vector3(-x, y, z));
            }
        }

        private void AddGraphEdgeLines(Vector3 from, Vector3 to, string layerName, Color color, Transform parent)
        {
            string lineName = $"{layerName}";
            LineRenderer lineRenderer = CreateLineRenderer(lineName, color, parent);
            lineRenderer.positionCount = 2;

            lineRenderer.SetPosition(0, new Vector3(-from.x, from.y, from.z));
            lineRenderer.SetPosition(1, new Vector3(-to.x, to.y, to.z));
        }

        private LineRenderer CreateLineRenderer(string itemName, Color stageColor, Transform parent)
        {
            GameObject go = new GameObject(itemName);
            go.layer = LayerMask.NameToLayer("AMProjViz");
            go.transform.parent = parent;

            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));

            lineRenderer.sharedMaterial.SetColor("_Color", stageColor);
            lineRenderer.startColor = stageColor;
            lineRenderer.endColor = stageColor;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.loop = true;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            return lineRenderer;
        }
    }
}
