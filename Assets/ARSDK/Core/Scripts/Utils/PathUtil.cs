using System;
using System.IO;

using UnityEngine;

namespace ARCeye
{
    public class PathUtil
    {
        /// <summary>
        ///   입력 받은 경로가 유효한 경로인지 확인. 유효한 경로 값을 리턴.
        /// </summary>
        public static string Validate(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string fullPath = Path.GetFullPath(path);

#if !UNITY_EDITOR && UNITY_ANDROID

            // GetFullPath를 이용하면 상대경로와 duplicated slash등이 사라짐.
            // 이 경우 URL streaming을 받아야 하는 gltfast의 경우 Android 환경에서 데이터를 받을 수 없음.
            // 정상적으로 스트리밍을 받기 위해 주소값 변경.
            fullPath = fullPath.Replace("/jar:file:/", "jar:file:///");

#endif

            return fullPath;
        }
    }
}