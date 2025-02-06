using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using UnityEngine.Events;

namespace ARCeye
{
    public class AMProjStageReader : MonoBehaviour
    {
        private JObject m_Root;
        private string m_JsonStr;
        private bool m_ReadAMProjFinished;

        
        public void Load(string amprojFilePath, UnityAction<Dictionary<string, float>> finishCallback)
        {
            m_ReadAMProjFinished = false;
            StartCoroutine( LoadInternal(amprojFilePath, finishCallback) );
        }

        private IEnumerator LoadInternal(string amprojFilePath, UnityAction<Dictionary<string, float>> finishCallback)
        {
            yield return ReadAMProjFile(amprojFilePath);

            m_Root = JObject.Parse(m_JsonStr);

            var stageObjects = (JArray) m_Root["stages"];

            Dictionary<string, float> stages = new Dictionary<string, float>();

            foreach(var stageElem in stageObjects)
            {
                stages.Add(stageElem["name"].Value<string>(), stageElem["height"].Value<float>());
            }

            finishCallback.Invoke(stages);
        }

        private IEnumerator ReadAMProjFile(string amprojPath)
        {
            Debug.Log($"ReadAMProjFile at path : " + amprojPath);

            if (amprojPath.Contains("://") || amprojPath.Contains(":///"))
            {
                UnityWebRequest www = UnityWebRequest.Get(amprojPath);
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError( $"파일을 읽는데 문제가 발생했습니다. 다음 경로에 amproj 파일이 존재하는지 확인해주세요 : " + amprojPath);
                    Debug.LogError("Error: " + www.error);
                }
                else
                {
                    m_JsonStr = www.downloadHandler.text;
                }
            }
            else
            {
                try
                {
                    m_JsonStr = System.IO.File.ReadAllText(amprojPath);
                }
                catch(Exception e)
                {
                    Debug.LogError($"파일을 읽는데 문제가 발생했습니다. 다음 경로에 amproj 파일이 존재하는지 확인해주세요 : " + amprojPath);
                    Debug.LogError("Error: " + e);
                }
            }

            Debug.Log($"Load amproj finished");
            m_ReadAMProjFinished = true;
        }
    }
}