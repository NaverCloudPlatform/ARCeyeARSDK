using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class PickHandler : MonoBehaviour
    {
        private GameObject m_CurrentPick;
        private Vector2 m_TouchPos;
        private bool m_PickUpdated = false;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {            
            if (m_PickUpdated == true) {
                m_PickUpdated = false;

                ItemGenerator.OnGestureReceived(GestureType.TAPPED, m_CurrentPick);
            }

            bool dirty = false;
            
            // MOBILE
            if (Application.isMobilePlatform)
            {
                foreach(Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        m_TouchPos = touch.position;
                        dirty = true;
                    }
                }
            }

            // DESKTOP
            else if (Application.platform == RuntimePlatform.WindowsPlayer ||
                     Application.platform == RuntimePlatform.OSXPlayer     ||
                     Application.platform == RuntimePlatform.LinuxPlayer   ||
                     Application.platform == RuntimePlatform.WindowsEditor ||
                     Application.platform == RuntimePlatform.OSXEditor) 
            {
                if(Input.GetMouseButtonDown(0)) {
                    m_TouchPos = Input.mousePosition;
                    dirty = true;
                }
            }

            if (dirty) 
            {
                Ray ray = Camera.main.ScreenPointToRay(m_TouchPos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // Hit pickable
                    if (hit.collider != null)
                    {
                        Transform tapped = hit.collider.transform;

                        if (tapped.parent)
                        {                
                            // Replace current pick
                            if(m_CurrentPick != null && m_CurrentPick.name != tapped.parent.gameObject.name) {
                                ItemGenerator.OnGestureReceived(GestureType.UNTAPPED, m_CurrentPick);
                            }

                            m_CurrentPick = tapped.parent.gameObject;
                            m_PickUpdated = true;
                        }
                    } 

                    // Hit non-pickable
                    else 
                    {
                        // Cancel current pick
                        if(m_CurrentPick != null) {
                            ItemGenerator.OnGestureReceived(GestureType.UNTAPPED, m_CurrentPick);
                            m_CurrentPick = null;
                        }
                    }
                }

                // Hit nothing
                else 
                {
                    // Cancel current pick
                    if(m_CurrentPick != null) {
                        ItemGenerator.OnGestureReceived(GestureType.UNTAPPED, m_CurrentPick);
                        m_CurrentPick = null;
                    }
                }   
            }
        }
    }
}