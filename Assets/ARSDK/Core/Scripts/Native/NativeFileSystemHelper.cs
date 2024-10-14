using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using AOT;

namespace ARCeye
{
    [DefaultExecutionOrder(-1500)]
    public class NativeFileSystemHelper : MonoBehaviour
    {
        public bool useAndroidStreamingAssets = false;

#if !UNITY_EDITOR && UNITY_ANDROID
        const string dll = "ARPG-plugin";

        private static NativeFileSystemHelper s_Instance;

        public delegate bool IsFileExistsFuncDelegate(IntPtr filePath);
        public delegate void ReadFileFuncDelegate(IntPtr filePath);
        public delegate void ReadPathFileFuncDelegate(IntPtr filePath);


        [DllImport(dll)]
        private static extern void SetIsFileExistsFuncNative(IsFileExistsFuncDelegate func);

        [DllImport(dll)]
        private static extern void SetReadFileFuncNative(ReadFileFuncDelegate func);

        [DllImport(dll)]
        private static extern void OnCompleteNative(string msg);

        [DllImport(dll)]
        private static extern void OnErrorNative(string msg);

        [DllImport(dll)]
        private static extern void SetPathReadFileFuncNative(ReadPathFileFuncDelegate func);

        [DllImport(dll)]
        private static extern void OnPathCompleteNative(string msg);

        [DllImport(dll)]
        private static extern void OnPathErrorNative(string msg);

        private static bool s_IsReadingComplete = false;
        public bool isReadingComplete {
            get {
                if(useAndroidStreamingAssets) {
                    // Android streaming assets을 사용할 경우 UnityWebRequest를 통한 비동기 접근 필요.
                    return s_IsReadingComplete;
                } else {
                    // Android streaming assets을 사용하지 않으면 filesystem 사용 가능.
                    return true;
                }
            }
            set {
                s_IsReadingComplete = value;
            }
        }

        /// 아래의 Native callback 메서드들은 Android 환경에서 streaming assets을 사용할 때 사용.

        private void Awake() {
            s_Instance = this;
            SetIsFileExistsFuncNative( IsFileExistsInStreamingAssetsPath );
            SetReadFileFuncNative( ReadFile );
            SetPathReadFileFuncNative( ReadPathFile );
        }


        [MonoPInvokeCallback(typeof(IsFileExistsFuncDelegate))]
        static private bool IsFileExistsInStreamingAssetsPath(IntPtr filePathPtr) {
            return true;
        }
        
        [MonoPInvokeCallback(typeof(ReadFileFuncDelegate))]
        static public void ReadFile(IntPtr filePathPtr) {
            s_IsReadingComplete = false;

            string fileUrl = Marshal.PtrToStringAnsi(filePathPtr);
            s_Instance.StartCoroutine( s_Instance.ReadText(fileUrl) );
        }

        [MonoPInvokeCallback(typeof(ReadPathFileFuncDelegate))]
        static public void ReadPathFile(IntPtr filePathPtr) {
            s_IsReadingComplete = false;

            string fileUrl = Marshal.PtrToStringAnsi(filePathPtr);
            s_Instance.StartCoroutine( s_Instance.ReadPathText(fileUrl) );
        }

        private IEnumerator ReadText(string url) {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    OnErrorNative(www.error);
                }
                else
                {
                    OnCompleteNative(www.downloadHandler.text);
                }
            }

            s_IsReadingComplete = true; 
        }

        private IEnumerator ReadPathText(string url) {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    OnPathErrorNative(www.error);
                }
                else
                {
                    OnPathCompleteNative(www.downloadHandler.text);
                }
            } 

            s_IsReadingComplete = true;
        }
#else
        public bool isReadingComplete {
            get {
                return true;
            }
        }
#endif

        public static bool IsGLBNotAssigned(string filePath)
        {
            return filePath.Contains("RmlsZURvZXNOb3RFeGlzdHM=");
        }
    }
}