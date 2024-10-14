using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class ViewController<T> : MonoBehaviour where T : View
    {
        [SerializeField]
        protected T m_View;

        protected virtual void Awake()
        {
            if (m_View != null)
            {
                return;
            }

            m_View = GetComponentInChildren<T>(true);

            if (m_View != null)
            {
                return;
            }

            m_View = FindObjectOfType<T>(true);

            if (m_View == null)
            {
                Debug.LogError($"{this.GetType()}의 {typeof(T)}을 찾을 수 없음");
            }
        }

        public virtual void Show(bool value)
        {
            if (m_View == null)
            {
                return;
            }

            m_View.gameObject.SetActive(value);
        }

        public virtual void Show()
        {
            Show(true);
        }

        public virtual void Hide()
        {
            Show(false);
        }
    }
}