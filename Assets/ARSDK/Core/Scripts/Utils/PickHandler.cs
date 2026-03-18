using UnityEngine;
#if ARSDK_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ARCeye
{
    public class PickHandler : MonoBehaviour
    {
        private Camera m_MainCamera;
        private GameObject m_CurrentPick;
        private Vector2 m_TouchPos;
        private bool m_PickUpdated = false;

        void Start()
        {
            m_MainCamera = Camera.main;
        }

        void Update()
        {
            if (m_PickUpdated)
            {
                m_PickUpdated = false;
                ItemGenerator.OnGestureReceived(GestureType.TAPPED, m_CurrentPick);
            }

            if (!TryGetInputPosition(out m_TouchPos))
            {
                return;
            }

            Ray ray = m_MainCamera.ScreenPointToRay(m_TouchPos);

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider != null && hit.collider.transform.parent)
            {
                GameObject picked = hit.collider.transform.parent.gameObject;

                if (m_CurrentPick != null && m_CurrentPick.name != picked.name)
                {
                    ItemGenerator.OnGestureReceived(GestureType.UNTAPPED, m_CurrentPick);
                }

                m_CurrentPick = picked;
                m_PickUpdated = true;
                return;
            }

            CancelCurrentPick();
        }

        private bool TryGetInputPosition(out Vector2 position)
        {
            position = default;

#if ARSDK_INPUT_SYSTEM
            if (Application.isMobilePlatform)
            {
                if (Touchscreen.current == null) return false;

                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began) continue;

                    position = touch.position.ReadValue();
                    return true;
                }

                return false;
            }

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return false;

            position = Mouse.current.position.ReadValue();
            return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Application.isMobilePlatform)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase != TouchPhase.Began) continue;

                    position = touch.position;
                    return true;
                }

                return false;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.LinuxPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor)
            {
                if (!Input.GetMouseButtonDown(0)) return false;

                position = Input.mousePosition;
                return true;
            }

            return false;
#else
            return false;
#endif
        }

        private void CancelCurrentPick()
        {
            if (m_CurrentPick == null) return;

            ItemGenerator.OnGestureReceived(GestureType.UNTAPPED, m_CurrentPick);
            m_CurrentPick = null;
        }
    }
}