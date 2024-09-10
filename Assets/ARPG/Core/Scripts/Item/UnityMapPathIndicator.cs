using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityMapPathIndicator : UnityModel
    {
        private List<LineRenderer> m_PathRenderers = new List<LineRenderer>();
        private Material m_PathMaterial;

        private void Awake()
        {
            gameObject.name = "MapPathIndicator";
            gameObject.layer = LayerMask.NameToLayer("Map");
        }

        public int AddPath()
        {
            GameObject pathGo = new GameObject("Path");
            pathGo.layer = LayerMask.NameToLayer("Map");
            pathGo.transform.SetParent(transform);

            var pathRenderer = pathGo.AddComponent<LineRenderer>();

            pathRenderer.transform.localRotation = Quaternion.Euler(90, 0, 0);
            pathRenderer.numCornerVertices = 5;
            pathRenderer.alignment = LineAlignment.TransformZ;
            pathRenderer.material = m_PathMaterial;

            m_PathRenderers.Add(pathRenderer);

            // 생성된 Path의 index를 리턴.
            return m_PathRenderers.Count - 1;
        }

        public void SetMaterial(Material material)
        {
            m_PathMaterial = material;

            foreach(var pathRenderer in m_PathRenderers)
            {
                pathRenderer.material = m_PathMaterial;
            }
        }

        public void SetPath(int pathIndex, float[] path, GameObject beginBulletPrefab, GameObject endBulletPrefab) {
            if(pathIndex < 0 || m_PathRenderers.Count <= pathIndex)
            {
                Debug.LogError($"PathRenderer index is out of bound! (PathRenderers Count {m_PathRenderers.Count}, pathIndex {pathIndex})");
                return;
            }

            var beginBulletGo = Instantiate(beginBulletPrefab, transform);
            var endBulletGo = Instantiate(endBulletPrefab, transform);
            
            List<Vector3> positions = new List<Vector3>();

            for(int i=0 ; i<path.Length / 3 ; i++)
            {
                Vector3 currPosition = GetPosition(path, i);

                if(i == 0) {
                    positions.Add(currPosition);
                    continue;
                }

                Vector3 prevPosition = GetPosition(path, i - 1);
                float dist = (currPosition - prevPosition).magnitude;

                if(dist < 1.5f) {
                    continue;
                }

                positions.Add(currPosition);
            }

            var pathRenderer = m_PathRenderers[pathIndex];

            pathRenderer.positionCount = positions.Count;
            pathRenderer.SetPositions(positions.ToArray());

            Vector3 margin = new Vector3(0, 0.1f, 0);
            beginBulletGo.transform.position = pathRenderer.GetPosition(0) + margin;
            endBulletGo.transform.position = pathRenderer.GetPosition(pathRenderer.positionCount - 1) + margin;
        }

        private Vector3 GetPosition(float[] path, int index) {
            return new Vector3(-path[index * 3 + 0], path[index * 3 + 1] + 0.05f, path[index * 3 + 2]);
        }
    }
}