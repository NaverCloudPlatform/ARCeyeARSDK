using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ARCeye
{
    public class MapScreen : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private IMapTouchListener m_TouchListener;
        public  IMapTouchListener touchListener {
            get => m_TouchListener;
            set => m_TouchListener = value;
        }

        public void OnPointerDown(PointerEventData eventData) {
            
        }

        public void OnPointerUp(PointerEventData eventData) {
            touchListener?.OnTouch();
        }
    }
}