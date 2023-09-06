using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityMapPathIndicator : UnityModel
    {
        private LineRenderer m_LineRenderer;
        private GameObject m_BeginBullet;
        private GameObject m_EndBullet;

        private void Awake()
        {
            gameObject.name = "MapPathIndicator";
            gameObject.layer = LayerMask.NameToLayer("Map");
            m_LineRenderer = gameObject.AddComponent<LineRenderer>();

            m_LineRenderer.transform.localRotation = Quaternion.Euler(90, 0, 0);
            m_LineRenderer.alignment = LineAlignment.TransformZ;
        }

        public void SetMaterial(Material material)
        {
            m_LineRenderer.material = material;   
        }

        public void SetBullet(GameObject beginBullet, GameObject endBullet)
        {
            // 기존의 bullet 제거.
            GameObject.Destroy(m_BeginBullet);
            GameObject.Destroy(m_EndBullet);

            // 새로운 경로에 새로운 bullet 생성.
            m_BeginBullet = beginBullet;
            m_EndBullet = endBullet;

            m_BeginBullet.transform.parent = m_LineRenderer.transform;
            m_EndBullet.transform.parent = m_LineRenderer.transform;
        }

        public void SetPath(float[] path) {
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

                if(dist < 2) {
                    continue;
                }

                positions.Add(currPosition);
            }

            m_LineRenderer.positionCount = positions.Count;
            m_LineRenderer.SetPositions(positions.ToArray());

            Vector3 margin = new Vector3(0, 0.1f, 0);
            m_BeginBullet.transform.position = m_LineRenderer.GetPosition(0) + margin;
            m_EndBullet.transform.position = m_LineRenderer.GetPosition(m_LineRenderer.positionCount - 1) + margin;
        }

        private Vector3 GetPosition(float[] path, int index) {
            return new Vector3(-path[index * 3 + 0], path[index * 3 + 1] + 0.05f, path[index * 3 + 2]);
        }
    }
}