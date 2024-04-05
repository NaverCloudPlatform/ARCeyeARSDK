using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class View : MonoBehaviour
    {
        private void Awake()
        {
            if(!GetComponent<CanvasGroup>())
            {
                gameObject.AddComponent<CanvasGroup>();
            }
        }

        public virtual void Show(bool value)
        {
            gameObject.SetActive(value);   
        }
    }
}