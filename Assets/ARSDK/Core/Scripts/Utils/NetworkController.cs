using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using UnityEngine;
using AOT;

namespace ARCeye
{
    public class NetworkController : MonoBehaviour
    {
        private static NetworkController s_Instance;

        /* -- Native plugin -- */
        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif

        
        [DllImport(dll)] private static extern void SetGetFuncNative(GetFuncDelegate func);
        [DllImport(dll)] private static extern void SetPostFuncNative(PostFuncDelegate func);

        public delegate IntPtr GetFuncDelegate(string url);
        public delegate IntPtr PostFuncDelegate(RequestParams requestParam);

        private static GetResponse m_GetResponse = new GetResponse();
        private static Response m_PostResponse = new Response();
        private static IntPtr m_GetResponsePtr;
        private static IntPtr m_PostResponsePtr;

        private static UnityWebRequest m_WebRequest;
        private static bool m_IsFinished;
        private static IntPtr m_BytePtr;   // content data bytes.

        void Awake()
        {
            m_IsFinished = false;
            s_Instance = this;

            SetGetFuncNative( Get );
            SetPostFuncNative( Post );
        }

        [MonoPInvokeCallback(typeof(GetFuncDelegate))]
        unsafe private static IntPtr Get(string url)
        {
            m_IsFinished = false;

            MainThreadDispatcher.Instance().Enqueue(()=>{
                RequestParams requestParam = new RequestParams();
                requestParam.url = url;
                requestParam.method = 0;
                
                s_Instance.UploadGet(requestParam);     
            });

            while(!m_IsFinished){}

            // 기존에 할당 되어있던 응답이 있을 경우 삭제.
            Marshal.FreeHGlobal(m_GetResponsePtr);

            // 전달 받은 응답에 대한 구조체를 marshalling
            m_GetResponsePtr = Marshal.AllocHGlobal(Marshal.SizeOf(m_GetResponse));
            Marshal.StructureToPtr(m_GetResponse, m_GetResponsePtr, false);

            return m_GetResponsePtr;
        }

        // LoadNavigationNative를 호출하면 ARPG 내부에서 호출하는 Post 메서드.
        [MonoPInvokeCallback(typeof(PostFuncDelegate))]
        unsafe private static IntPtr Post(RequestParams requestParam)
        {
            m_IsFinished = false;
            
            MainThreadDispatcher.Instance().Enqueue(()=>{
                s_Instance.UploadPost(requestParam);
            });

            while(!m_IsFinished){}

            // 기존에 할당 되어있던 응답이 있을 경우 삭제.
            Marshal.FreeHGlobal(m_PostResponsePtr);

            // 전달 받은 응답에 대한 구조체를 marshalling
            m_PostResponsePtr = Marshal.AllocHGlobal(Marshal.SizeOf(m_PostResponse));
            Marshal.StructureToPtr(m_PostResponse, m_PostResponsePtr, false);

            return m_PostResponsePtr;
        }

        private void UploadGet(RequestParams requestParam)
        {
            StartCoroutine( UploadGetInternal(requestParam) );
        }

        private void UploadPost(RequestParams requestParam)
        {
            StartCoroutine( UploadPostInternal(requestParam) );
        }

        IEnumerator UploadGetInternal(RequestParams requestParam)
        {
            Marshal.FreeHGlobal(m_BytePtr);

            yield return Request(requestParam);

            // 다운로드 받은 데이터를 native로 전달하기 위해 메모리 할당.
            // 크기가 동적인 배열은 MarshalAs로 전달할 수 없기 때문에 IntPtr로 전달.
            var bytes = m_WebRequest.downloadHandler.data;
            int bytesLength = bytes.Length;
            m_BytePtr = Marshal.AllocHGlobal(bytesLength);

            Marshal.Copy(bytes, 0, m_BytePtr, bytesLength);

            m_GetResponse.data = m_BytePtr;
            m_GetResponse.length = bytesLength;
            m_GetResponse.status_code = (int) m_WebRequest.responseCode;
            
            yield return new WaitUntil(()=>m_WebRequest.isDone);

            m_WebRequest.Dispose();
            
            m_IsFinished = true;
        }

        IEnumerator UploadPostInternal(RequestParams requestParam)
        {
            yield return Request(requestParam);

            m_PostResponse.status_code = (int) m_WebRequest.responseCode;
            m_PostResponse.text = m_WebRequest.downloadHandler.text;

            yield return new WaitUntil(()=>m_WebRequest.isDone);

            m_WebRequest.Dispose();
            
            m_IsFinished = true;
        }

        IEnumerator Request(RequestParams requestParam)
        {
            CreateRequest(requestParam);

            yield return m_WebRequest.SendWebRequest();

            if(m_WebRequest.result == UnityWebRequest.Result.ConnectionError) {
                NativeLogger.Print(LogLevel.ERROR, "[NetworkController] ConnectionError - " + m_WebRequest.error);
            }

            if(m_WebRequest.result == UnityWebRequest.Result.ProtocolError) {
                NativeLogger.Print(LogLevel.ERROR, "[NetworkController] ProtocolError - " + m_WebRequest.error);
            }
        }

        /*
            DefineEnumClass(HttpMethod, int32_t,
                GET, POST, PUT, DELETE
            )
        */
        private UnityWebRequest CreateRequest(RequestParams requestParam) {
            m_WebRequest = new UnityWebRequest();
            m_WebRequest.url = requestParam.url;
            m_WebRequest.redirectLimit = 0;
            m_WebRequest.timeout = requestParam.timeout_milliseconds;

            if(requestParam.method == 0) {
                m_WebRequest.method = "GET";
            } else {
                m_WebRequest.method = "POST";
                m_WebRequest.SetRequestHeader("Content-Type", "application/json");
                m_WebRequest.uploadHandler = GenerateUploadHandler(requestParam);
            }        

            m_WebRequest.downloadHandler = GenerateDownloadHandler();

            return m_WebRequest;
        }

        private UploadHandler GenerateUploadHandler(RequestParams requestParam)
        {
            byte[] bodyBuffer = new UTF8Encoding().GetBytes(requestParam.body);
            return new UploadHandlerRaw(bodyBuffer);
        }

        private DownloadHandler GenerateDownloadHandler()
        {
            return new DownloadHandlerBuffer();
        }
    }

    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class RequestParams {
        public string url;
        public string body;
        [MarshalAs(UnmanagedType.I4)]
        public int method;
        [MarshalAs(UnmanagedType.I4)]
        public int content_type;
        public int timeout_milliseconds = 2000;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class Response {
        [MarshalAs(UnmanagedType.I4)]
        public int status_code;
        [MarshalAs(UnmanagedType.LPStr)]
        public string text;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class GetResponse {
        [MarshalAs(UnmanagedType.I4)]
        public int status_code;
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr data;
        [MarshalAs(UnmanagedType.I4)]
        public int length;
    }
}