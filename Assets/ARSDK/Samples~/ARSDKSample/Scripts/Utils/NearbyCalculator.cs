using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class NearbyCalculator
    {
        private Camera m_Camera;
        private string m_CurrStage;
        private FloorDistanceCalculator m_FloorDistanceCalculator;

        public NearbyCalculator()
        {
            m_Camera = Camera.main;
            m_FloorDistanceCalculator = new FloorDistanceCalculator();
        }

        public void SetCurrStage(string stageName)
        {
            m_CurrStage = stageName;
        }

        // 현재 위치와 가장 가까운 입구점까지의 거리를 계산한다.
        public float CalculateDistance(LayerPOIItem poiItem)
        {
            Vector3 currPosition = m_Camera.transform.position;
            List<Vector3> allPOIPositions = GetPOIPositions(poiItem);

            float minDist = float.MaxValue;

            for(int i=0 ; i<allPOIPositions.Count ; i++)
            {
                // 직선 거리 계산.
                Vector3 start = currPosition;
                Vector3 end = allPOIPositions[i];
                start.y = 0;
                end.y = 0;

                float dist = Vector3.Distance(currPosition, allPOIPositions[i]);

                if(dist < minDist)
                {
                    minDist = dist;
                }
            }

            // 층간 거리 계산.
            float floorDist = m_FloorDistanceCalculator.GetDistance(m_CurrStage, poiItem.stageName);
            minDist += floorDist;

            return minDist;
        }


        // POI의 좌표를 리턴. LayerPOIItem의 좌표값은 GL 좌표계 기준. 이를 Unity 좌표계로 변경한다.
        private List<Vector3> GetPOIPositions(LayerPOIItem poiItem)
        {
            Vector3 currPosition = m_Camera.transform.position;
            List<Vector3> allEntrances = poiItem.entrance;
            List<Vector3> converted = new List<Vector3>();

            foreach(var enterance in allEntrances)
            {
                Vector3 position = new Vector3();
                position.x = -enterance.x;
                position.y = enterance.y;
                position.z = enterance.z;
                converted.Add(position);
            }

            return converted;
        }
    }
}