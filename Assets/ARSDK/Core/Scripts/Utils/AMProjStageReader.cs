using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

namespace ARCeye
{
    public class AMProjStageReader : MonoBehaviour
    {
        private JObject m_Root;
        private string m_JsonStr;


        public void Load(string amprojFilePath, UnityAction<Dictionary<string, float>> finishCallback)
        {
            StartCoroutine(LoadInternal(amprojFilePath, finishCallback));
        }

        private IEnumerator LoadInternal(string amprojFilePath, UnityAction<Dictionary<string, float>> finishCallback)
        {
            yield return AMProjFileReader.ReadCoroutine(amprojFilePath, json => m_JsonStr = json);

            if (string.IsNullOrEmpty(m_JsonStr))
            {
                NativeLogger.Print(LogLevel.ERROR, "[AMProjStageReader] Failed to parse amproj file. File content is empty.");
                yield break;
            }

            m_Root = JObject.Parse(m_JsonStr);

            Dictionary<string, float> stages = ParseStages();

            finishCallback.Invoke(stages);
        }

        private Dictionary<string, float> ParseStages()
        {
            int version = m_Root["version"]?.Value<int>() ?? 1;

            switch (version)
            {
                case 1:
                case 2:
                    return ParseStagesV1();
                case 3:
                    return ParseStagesV3();
                default:
                    NativeLogger.Print(LogLevel.WARNING, $"[AMProjStageReader] Unsupported amproj version: {version}. Falling back to V1 format.");
                    return ParseStagesV1();
            }
        }

        private Dictionary<string, float> ParseStagesV1()
        {
            var stages = new Dictionary<string, float>();
            var stageObjects = (JArray)m_Root["stages"];
            if (stageObjects == null) return stages;

            foreach (var stageElem in stageObjects)
            {
                stages.Add(stageElem["name"].Value<string>(), stageElem["height"].Value<float>());
            }
            return stages;
        }

        private Dictionary<string, float> ParseStagesV3()
        {
            var stages = new Dictionary<string, float>();
            var rootChildren = m_Root["root"]?["children"] as JArray;
            if (rootChildren == null) return stages;

            foreach (var child in rootChildren)
            {
                if (child["type"]?.Value<string>() == "Stage" && child["usage"]?.Value<int>() != 2)
                {
                    stages.Add(child["name"].Value<string>(), child["elevation"].Value<float>());
                }
            }
            return stages;
        }

    }
}