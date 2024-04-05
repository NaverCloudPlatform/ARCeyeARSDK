using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public static class UIUtils
    {
        public static T FindViewController<T>() where T : MonoBehaviour
        {
            var vc = Object.FindObjectOfType<T>();
            if (vc == null)
            {
                Debug.LogError($"[ARNavigationExample] Failed to find '{typeof(T)}'");
            }
            return vc;
        }
    }
}