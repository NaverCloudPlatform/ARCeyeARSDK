using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class View : MonoBehaviour
    {
        public virtual void Show(bool value)
        {
            gameObject.SetActive(value);   
        }
    }
}