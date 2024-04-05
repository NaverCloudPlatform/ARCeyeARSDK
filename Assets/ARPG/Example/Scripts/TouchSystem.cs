using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ARCeye
{
    public class TouchSystem : MonoBehaviour
    {
        private static TouchSystem s_Instance = null;
        public static TouchSystem Instance {
            get {
                if(s_Instance == null) {
                    s_Instance = FindObjectOfType<TouchSystem>();
                }
                return s_Instance;
            }
        }

        private enum State {
            OneTouchUp, OneTouchDown, TwoTouchUp, TwoTouchDown
        }

        [SerializeField]
        private UnityEvent<Vector2> m_OnTouchDown;
        public UnityEvent<Vector2> onTouchDown => m_OnTouchDown;

        [SerializeField]
        private UnityEvent<Vector2> m_OnDrag;
        public UnityEvent<Vector2> onDrag => m_OnDrag;

        [SerializeField]
        private UnityEvent<float> m_OnPinchZoom;
        public UnityEvent<float> onPinchZoom => m_OnPinchZoom;

        [SerializeField]
        private UnityEvent<Vector2> m_OnTouchUp;
        public UnityEvent<Vector2> onTouchUp => m_OnTouchUp;

        [SerializeField]
        private Image m_Pointer1;
        [SerializeField]
        private Image m_Pointer2;

        private Vector2 m_ReferenceResolution;

        private State m_TouchState = State.OneTouchUp;

        private Vector2 m_PrevTouch1Position = Vector2.zero;
        private Vector2 m_PrevTouch2Position = Vector2.zero;


        private int touchCount {
            get {
#if UNITY_EDITOR
                bool isTouched = Input.GetMouseButton(0);
                if(Input.GetKey(KeyCode.LeftAlt)) {
                    return isTouched ? 2 : 0;
                } else {
                    return isTouched ? 1 : 0;
                }
#else
                return Input.touchCount;
#endif
            }
        }

        private Vector2 rawTouchPosition {
            get {
                return Input.mousePosition;
            }
        }

        private Vector2 touchPosition {
            get {
                Vector2 scaledTouchPosition;

                scaledTouchPosition.x = rawTouchPosition.x * m_ReferenceResolution.x / Screen.width;
                scaledTouchPosition.y = rawTouchPosition.y * m_ReferenceResolution.y / Screen.height;

                return scaledTouchPosition;
            }
        }

        private Vector2 GetTouchPosition(int fingerId) {
#if UNITY_EDITOR
            if(fingerId == 0) {
                return touchPosition;
            } else {
                return m_ReferenceResolution - touchPosition;
            }
#else
            Touch touch = Input.GetTouch(fingerId);
            return touch.position;
#endif
        }

        private void Awake() {
            Canvas canvas = GetComponentInParent<Canvas>();
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            m_ReferenceResolution = scaler.referenceResolution;
        }

        private void Update() {
            DrawDebugDots();

            if(touchCount == 1) {
                if(m_TouchState == State.OneTouchUp) {
                    // Touch Down. 
                    m_PrevTouch1Position = touchPosition;
                    m_TouchState = State.OneTouchDown;
                    onTouchDown.Invoke(touchPosition);
                } else if(m_TouchState == State.OneTouchDown) {
                    // Dragging.
                    Vector2 delta = touchPosition - m_PrevTouch1Position;
                    m_PrevTouch1Position = touchPosition;

                    if(delta.magnitude > 0.1f) {
                        onDrag.Invoke(delta);
                    }
                }
            } else if(touchCount > 1) {
                if(m_TouchState != State.TwoTouchDown) {
                    // TwoTouch 시작.
                    m_TouchState = State.TwoTouchDown;
                    m_PrevTouch1Position = GetTouchPosition(0);
                    m_PrevTouch2Position = GetTouchPosition(1);
                } else if(m_TouchState == State.TwoTouchDown) {
                    // TwoTouch pinch 진행.
                    float currDist = (GetTouchPosition(0) - GetTouchPosition(1)).magnitude;
                    float initDist = (m_PrevTouch1Position - m_PrevTouch2Position).magnitude;
                    float ratio = 1.0f - currDist / initDist;

                    onPinchZoom.Invoke(ratio);

                    m_PrevTouch1Position = GetTouchPosition(0);
                    m_PrevTouch2Position = GetTouchPosition(1);
                }
            } else if(touchCount == 0) {
                if(m_TouchState != State.OneTouchUp) {
                    // Touch Up.
                    m_TouchState = State.OneTouchUp;
                    onTouchUp.Invoke(touchPosition);
                }
            }
        }

        private void DrawDebugDots() {
            if(Input.GetKey(KeyCode.LeftAlt)) {
                m_Pointer1.gameObject.SetActive(true);
                m_Pointer2.gameObject.SetActive(true);

                m_Pointer1.rectTransform.anchoredPosition = GetTouchPosition(0);
                m_Pointer2.rectTransform.anchoredPosition = GetTouchPosition(1);
            } else {
                m_Pointer1.gameObject.SetActive(false);
                m_Pointer2.gameObject.SetActive(false);
            }
        }
    }
}