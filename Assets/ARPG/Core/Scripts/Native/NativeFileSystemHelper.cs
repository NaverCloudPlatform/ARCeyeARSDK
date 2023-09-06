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
            string fileUrl = Marshal.PtrToStringAnsi(filePathPtr);
            s_Instance.StartCoroutine( s_Instance.ReadText(fileUrl) );
        }

        [MonoPInvokeCallback(typeof(ReadPathFileFuncDelegate))]
        static public void ReadPathFile(IntPtr filePathPtr) {
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
        }
#endif
    }
}