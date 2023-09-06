using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityMapPOIPool : UnityModel
    {
        public static string atlasFullpath;

        private List<UnityMapPOI> m_MapPOILists = new List<UnityMapPOI>();

        private int m_FontSize;

        public override void Initialize()
        {
            base.Initialize();

            var mapLayerMask = LayerMask.NameToLayer("Map");

            var children = gameObject.GetComponentsInChildren<Transform>();
            foreach(var child in children)
            {
                child.gameObject.layer = mapLayerMask;
            }
            gameObject.layer = mapLayerMask;
        }

        //// Configuration
        public void SetFontSize(float fontSize)
        {
            // Aliasing이 발생하지 않는 수준으로 scale 적용.
            float scaledFontSize = fontSize * 2;

            // 앞으로 생성될 MapPOI들을 위해 저장.
            m_FontSize = (int) scaledFontSize;
            
            // 기존에 추가 되어 있던 MapPOI들의 fontSize를 변경.
            foreach(var mapPOI in m_MapPOILists) {
                mapPOI.SetFontSize(m_FontSize);
            }
        }

        public void SetOutlineThickness(float thickness)
        {
            // Debug.Log("Set Outline thickness : " + thickness);
        }

        public void InsertPOIEntity(UnityMapPOI mapPOI, int id, int dpCode, string label, Vector3 position, int drawType)
        {
            // Debug.Log("Insert POI Entity : " + label);
            // MapPOIPool을 MapPOI들의 root로 설정.
            mapPOI.transform.parent = transform;

            Vector3 unityCoord = new Vector3(-position.x, position.y, position.z);

            mapPOI.transform.localPosition = unityCoord;
            mapPOI.SetLabel(label);
            mapPOI.SetFontSize(m_FontSize);

            m_MapPOILists.Add(mapPOI);
        }

        public void RemoveAllMapPOIs()
        {
            foreach(var mapPOI in m_MapPOILists) {
                Destroy(mapPOI.gameObject);
            }
            m_MapPOILists.Clear();
        }

        public void SetConfigFullpath(string atlasFullpath)
        {
            Debug.Log("Set altas fullpath : " + atlasFullpath);
        }
    }
}