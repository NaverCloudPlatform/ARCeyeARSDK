using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class FloorDistanceCalculator
    {
        private AMProjStageReader m_AMProjStageReader;
        private Dictionary<string, float> m_HeightByStageName;
        private const float k_FloorGain = 8;

        public FloorDistanceCalculator()
        {
            ARPlayGround arplayground = Object.FindFirstObjectByType<ARPlayGround>();
            m_AMProjStageReader = Object.FindFirstObjectByType<AMProjStageReader>();
            m_AMProjStageReader.Load(arplayground.amprojFilePath, stages => {
                m_HeightByStageName = stages;
            });
        }

        public float GetDistance(string startStageName, string endStageName)
        {
            float dist;
            float startHeight = 0;
            float endHeight = 0;
            try
            {
                startHeight = m_HeightByStageName[startStageName];
                endHeight = m_HeightByStageName[endStageName];
            }
            catch(KeyNotFoundException e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                dist = Mathf.Abs(startHeight - endHeight);
            }

            return dist * k_FloorGain;
        }
    }
}