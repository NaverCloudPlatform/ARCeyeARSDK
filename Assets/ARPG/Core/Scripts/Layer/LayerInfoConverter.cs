using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ARCeye
{
    public class LayerInfoConverter : MonoBehaviour
    {
        [SerializeField]
        protected LayerInfoSetting m_LayerInfoSetting;

        protected Dictionary<string, string> m_StageNameByLayerName = new Dictionary<string, string>();

        protected virtual void Awake()
        {
            Layer rootLayer = m_LayerInfoSetting.layer;
            rootLayer.parent = null;

            FindStageName(rootLayer);
            // PrintMatches();

            CheckError();
        }

        /// <summary>
        ///   LayerInfoSetting의 값을 이용하여 스테이지 
        /// </summary>
        protected virtual void FindStageName(Layer layer)
        {
            if(layer.parent == null)
            {
                layer.layerInfoCode = layer.layerName;    
            }
            else
            {
                layer.layerInfoCode = layer.parent.layerInfoCode + "_" + layer.layerName;
            }

            if(layer.linkToStage)
            {
                string stageName = layer.stageName.Trim();
                if(string.IsNullOrEmpty(stageName))
                {
                    Debug.LogWarning($"LayerInfo({layer.layerInfoCode})에 스테이지 이름이 할당되지 않았습니다.");
                }
                else
                {
                    m_StageNameByLayerName.Add(layer.layerInfoCode, stageName);
                }
            }
            else if(layer.subLayers != null && layer.subLayers.Count > 0)
            {
                foreach(var elem in layer.subLayers)
                {
                    elem.parent = layer;
                    FindStageName(elem);
                }
            }
        }

        public string Convert(string layerInfo)
        {
            var registerLayerInfos = m_StageNameByLayerName.Keys.ToList();

            string[] layerElem = layerInfo.Split("_");
            string layerInfoSub = "";
            string result = "";

            for(int i=0 ; i<layerElem.Length ; i++)
            {
                if(i == 0)
                {
                    layerInfoSub = layerElem[i];
                }
                else
                {
                    layerInfoSub += "_" + layerElem[i];
                }

                string stageName = registerLayerInfos.Find(e => e == layerInfoSub);
                if(!string.IsNullOrEmpty(stageName))
                {
                    result = m_StageNameByLayerName[stageName];
                    break;
                }
            }

            if(string.IsNullOrEmpty(result))
            {
                Debug.LogError($"인식 된 VL 영역 {layerInfo}과 매칭되는 스테이지 이름을 찾을 수 없습니다.");
            }

            return result;
        }

        protected virtual void PrintMatches()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var elem in m_StageNameByLayerName)
            {
                sb.Append($"stage : {elem.Value} -- layer : {elem.Key}\n");
            }

            Debug.Log("LayerInfo\n" + sb.ToString());
        }

        protected virtual void CheckError()
        {
            if(m_StageNameByLayerName.Count == 0)
            {
                Debug.LogError("[ARSDK] LayerInfoSetting이 정상적으로 설정되지 않았습니다. ARPG > Core > LayerInfoSetting.asset 파일에서 로케이션 계층과 매칭되는 Stage 이름 정보를 입력해주세요.");
            }

            List<string> emptyStageNameLayers = new List<string>();
            foreach(var elem in m_StageNameByLayerName)
            {
                if(string.IsNullOrEmpty(elem.Value))
                {
                    emptyStageNameLayers.Add(elem.Key);
                }
            }
        }
    }
}