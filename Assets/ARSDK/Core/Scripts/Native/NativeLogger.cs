using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace ARCeye
{
    public enum LogLevel
    {
        VERBOSE, DEBUG, INFO, WARNING, ERROR, FATAL
    }

    class LogElem
    {
        public string str { get; set; }
        public LogLevel level { get; set; }
        public LogElem(LogLevel l, string s)
        {
            str = s;
            level = l;
        }
    }
    [DefaultExecutionOrder(-2000)]
    public class NativeLogger
    {
        public delegate void DebugLogFuncDelegate(LogLevel level, IntPtr message);

        /* -- Native plugin -- */
#if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
#else
        const string dll = "ARPG-plugin";
#endif

        [DllImport(dll)]
        private static extern void ARPG_SetDebugLogFuncNative(DebugLogFuncDelegate func);
        [DllImport(dll)]
        private static extern void ARPG_ReleaseLoggerNative();
        [DllImport(dll)]
        private static extern void ARPG_SetDebugLogLevelNative(LogLevel logLevel);


        static private int s_MaxLogsCount = 100;
        static private LinkedList<LogElem> s_LogList = new LinkedList<LogElem>();
        static private LogLevel s_LogLevel = LogLevel.VERBOSE;
        public LogLevel logLevel
        {
            get => s_LogLevel;
            set => s_LogLevel = value;
        }


        public void Initialize()
        {
            s_LogList = new LinkedList<LogElem>();
            ARPG_SetDebugLogFuncNative(DebugLog);
        }

        public void Release()
        {
            ARPG_ReleaseLoggerNative();
        }

        public void SetLogLevel(LogLevel logLevel)
        {
            ARPG_SetDebugLogLevelNative(logLevel);
        }

        [MonoPInvokeCallback(typeof(DebugLogFuncDelegate))]
        static public void DebugLog(LogLevel logLevel, IntPtr raw)
        {
            string log = Marshal.PtrToStringAnsi(raw);
            Print(logLevel, log);
        }

        static public void Print(LogLevel logLevel, string log)
        {
            if (logLevel < s_LogLevel)
            {
                return;
            }

            string currTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string msg = string.Format("[{0}] {1}", currTime, log);
            switch (logLevel)
            {
                case LogLevel.VERBOSE:
                    {
                        msg = "[VERBOSE] " + msg;
                        Debug.Log(msg);
                        break;
                    }
                case LogLevel.DEBUG:
                    {
                        msg = "[Debug] " + msg;
                        Debug.Log(msg);
                        break;
                    }
                case LogLevel.INFO:
                    {
                        msg = "[Info] " + msg;
                        Debug.Log(msg);
                        break;
                    }
                case LogLevel.WARNING:
                    {
                        msg = "[Warning] " + msg;
                        Debug.LogWarning(msg);
                        break;
                    }
                case LogLevel.ERROR:
                    {
                        msg = "[Error] " + msg;
                        Debug.LogError(msg);
                        break;
                    }
                case LogLevel.FATAL:
                    {
                        msg = "[Fatal] " + msg;
                        Debug.LogError(msg);
                        break;
                    }
            }

            if ((int)logLevel >= (int)LogLevel.INFO)
            {
                if (s_LogList == null)
                {
                    s_LogList = new LinkedList<LogElem>();
                }

                if (s_LogList.Count > s_MaxLogsCount)
                {
                    s_LogList.RemoveFirst();
                }
                s_LogList.AddLast(new LogElem(logLevel, msg));
            }
        }
    }
}