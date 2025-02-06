using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        private UnityEvent<float> m_OnRotate;
        public UnityEvent<float> onRotate => m_OnRotate;

        [SerializeField]
        private UnityEvent<Vector2> m_OnTouchUp;
        public UnityEvent<Vector2> onTouchUp => m_OnTouchUp;

        [SerializeField]
        private Image m_Pointer1;
        [SerializeField]
        private Image m_Pointer2;

        private Vector2 m_CanvasSize;

        private State m_TouchState = State.OneTouchUp;

        private Vector2 m_PrevTouch1Position = Vector2.zero;
        private Vector2 m_PrevTouch2Position = Vector2.zero;


        private int touchCount {
            get {
#if ENABLE_INPUT_SYSTEM
    #if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
                if(IsLeftAltPressed()) {
                    return Mouse.current.leftButton.isPressed ? 2 : 0;
                } else {
                    return Mouse.current.leftButton.isPressed ? 1 : 0;
                }
    #else
                int activeTouches = 0;

                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.isInProgress)
                    {
                        activeTouches++;
                    }
                }

                return activeTouches;
    #endif
#else
    #if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
                bool isTouched = Input.GetMouseButton(0);
                if(Input.GetKey(KeyCode.LeftAlt)) {
                    return isTouched ? 2 : 0;
                } else {
                    return isTouched ? 1 : 0;
                }
    #else
                return Input.touchCount;
    #endif
#endif
            }
        }

        private Vector2 rawTouchPosition {
            get {
#if ENABLE_INPUT_SYSTEM
    #if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
                return Mouse.current.position.ReadValue();
    #else
                return Touchscreen.current.primaryTouch.position.ReadValue();
    #endif
#else
                return Input.mousePosition;
#endif
            }
        }

        private Vector2 touchPosition {
            get {
                Vector2 scaledTouchPosition;

                scaledTouchPosition.x = rawTouchPosition.x * m_CanvasSize.x / Screen.width;
                scaledTouchPosition.y = rawTouchPosition.y * m_CanvasSize.y / Screen.height;

                return scaledTouchPosition;
            }
        }

        private Vector2 GetTouchPosition(int fingerId) {
#if ENABLE_INPUT_SYSTEM
    #if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
            if(fingerId == 0) {
                return touchPosition;
            } else {
                return m_CanvasSize - touchPosition;
            }
    #else
            var touch = Touchscreen.current.touches[fingerId];
            return touch.position.ReadValue();
    #endif
#else
            Touch touch = Input.GetTouch(fingerId);
            return touch.position;
#endif
        }

        private void Start() {
            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();

            float canvasWidth = canvasRT.rect.width;
            float canvasHeight = canvasRT.rect.height;

            m_CanvasSize = new Vector2(canvasWidth, canvasHeight);
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

                    // TwoTouch rotation 진행.
                    Vector2 currDir = (GetTouchPosition(0) - GetTouchPosition(1)).normalized;
                    Vector2 initDir = (m_PrevTouch1Position - m_PrevTouch2Position).normalized;
                    float deltaDeg = Vector2.SignedAngle(initDir, currDir);

                    m_OnRotate?.Invoke(deltaDeg);

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
            if(IsLeftAltPressed()) {
                m_Pointer1.gameObject.SetActive(true);
                m_Pointer2.gameObject.SetActive(true);

                m_Pointer1.rectTransform.anchoredPosition = GetTouchPosition(0);
                m_Pointer2.rectTransform.anchoredPosition = GetTouchPosition(1);
            } else {
                m_Pointer1.gameObject.SetActive(false);
                m_Pointer2.gameObject.SetActive(false);
            }
        }

        private bool IsLeftAltPressed() {
#if ENABLE_INPUT_SYSTEM
    #if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
            if (Keyboard.current == null)
                return false;

            return Keyboard.current.leftAltKey.isPressed;
    #else
            return false;
    #endif
#else
            return Input.GetKey(KeyCode.LeftAlt);
#endif
        }
    }
}