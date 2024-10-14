using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class Billboard : MonoBehaviour
    {
        public enum RotationMode {
            NONE, AXIS_Y, AXIS_Z, CAMERA, AXIS_Y_FLIP
        }

        private Camera m_TargetCamera;
        public Camera targetCamera {
            get => m_TargetCamera;
            set => m_TargetCamera = value;
        }

        private RotationMode m_RotationMode;
        public RotationMode rotationMode {
            get => m_RotationMode;
            set => m_RotationMode = value;
        }

        
        void LateUpdate()
        {
            ApplyBillboard();   
        }

        private void ApplyBillboard()
        {
            if(m_TargetCamera == null) {
                m_TargetCamera = Camera.main;
            }

            if(m_RotationMode == RotationMode.NONE) {
                return;
            }

            Vector3 camEuler = m_TargetCamera.transform.rotation.eulerAngles;

            if(m_RotationMode == RotationMode.AXIS_Y) {
                transform.rotation = Quaternion.Euler(0, camEuler.y, 0);
            } else if(m_RotationMode == RotationMode.AXIS_Y_FLIP) {
                transform.rotation = Quaternion.Euler(0, camEuler.y + 180, 0);
            } else if(m_RotationMode == RotationMode.AXIS_Z) {
                transform.rotation = Quaternion.Euler(0, 0, camEuler.z);
            } else {
                transform.rotation = Quaternion.Euler(camEuler.x, camEuler.y, camEuler.z);
            }
        }
    }
}