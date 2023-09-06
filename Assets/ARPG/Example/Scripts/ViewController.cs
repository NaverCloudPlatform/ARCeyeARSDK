using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class ViewController<T> : MonoBehaviour where T : View
    {
        protected T m_View;

        protected virtual void Awake()
        {
            m_View = GetComponentInChildren<T>(true);
            if(m_View == null)
            {
                Debug.LogError("ViewController 하위의 View를 찾을 수 없음");
            }
        }

        public virtual void Show(bool value)
        {
            if(m_View == null) {
                return;
            }

            m_View.gameObject.SetActive(value);
        }
    }
}