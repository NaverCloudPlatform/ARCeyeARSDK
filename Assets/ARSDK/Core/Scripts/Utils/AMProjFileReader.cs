using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace ARCeye
{
    /// <summary>
    /// amproj 파일 읽기 유틸리티.
    /// Android StreamingAssets 경로(jar:file://...)와 일반 파일 경로 모두 지원.
    /// </summary>
    internal static class AMProjFileReader
    {
        /// <summary>
        /// 코루틴 기반 amproj 파일 버전 읽기.
        /// </summary>
        internal static IEnumerator ReadVersionCoroutine(string filePath, Action<int> callback)
        {
            int version = 1;
            yield return ReadCoroutine(filePath, json =>
            {
                if (json != null)
                {
                    version = ParseVersion(json);
                }
            });
            callback(version);
        }

        /// <summary>
        /// Task 기반 amproj 파일 버전 읽기.
        /// </summary>
        internal static async Task<int> ReadVersionAsync(string filePath)
        {
            string json = await ReadAsync(filePath);
            return json != null ? ParseVersion(json) : 1;
        }

        /// <summary>
        /// 코루틴 기반 amproj 파일 읽기.
        /// 읽기 실패 시 callback에 null 전달.
        /// </summary>
        internal static IEnumerator ReadCoroutine(string filePath, Action<string> callback)
        {
            string json = null;

            if (filePath.Contains("://"))
            {
                using var www = UnityWebRequest.Get(filePath);
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    json = www.downloadHandler.text;
                }
            }
            else
            {
                try
                {
                    json = File.ReadAllText(filePath);
                }
                catch (Exception)
                {
                    // 호출자가 null 체크로 에러 처리.
                }
            }

            callback(json);
        }

        /// <summary>
        /// Task 기반 amproj 파일 읽기.
        /// 읽기 실패 시 null 반환.
        /// </summary>
        internal static async Task<string> ReadAsync(string filePath)
        {
            if (filePath.Contains("://"))
            {
                var www = UnityWebRequest.Get(filePath);
                var op = www.SendWebRequest();
                while (!op.isDone)
                {
                    await Task.Yield();
                }

                string json = www.result == UnityWebRequest.Result.Success
                    ? www.downloadHandler.text
                    : null;
                www.Dispose();
                return json;
            }

            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// amproj JSON 문자열에서 버전 파싱.
        /// </summary>
        internal static int ParseVersion(string json)
        {
            try
            {
                var root = Newtonsoft.Json.Linq.JObject.Parse(json);
                return (int?)root["version"] ?? 1;
            }
            catch (Exception e)
            {
                NativeLogger.Print(LogLevel.WARNING, $"[AMProjFileReader] Failed to parse amproj version. {e.Message}");
                return 1;
            }
        }
    }
}
