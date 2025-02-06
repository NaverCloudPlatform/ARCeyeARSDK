using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace ARCeye
{
    public class POIGenerator : MonoBehaviour
    {
        [Header("Sign POI")]
        [SerializeField]
        private GameObject m_SignPOIPrefab;

        [Header("Map POI")]
        private Camera m_MapCamera;

        [SerializeField]
        private GameObject m_MapPOIPrefab;

        [Header("POI Info")]

        [SerializeField]
        private POIIconInfo m_ColorPOIInfo;

        [SerializeField]
        private POIIconInfo m_GrayPOIInfo;


        public GameObject GenerateSignPOI() {
            return Instantiate(m_SignPOIPrefab);
        }

        public UnityMapPOI GenerateMapPOI() {
            GameObject go = Instantiate(m_MapPOIPrefab);
            
            UnityMapPOI mapPOI = go.GetComponent<UnityMapPOI>();

            if(m_MapCamera == null) {
                GameObject mapCameraGO = GameObject.FindGameObjectWithTag("MapPOICamera");
                m_MapCamera = mapCameraGO.GetComponent<Camera>();
            }

            mapPOI.targetCamera = m_MapCamera;

            return mapPOI;
        }

        public void SetIconCodeToSignPOI(UnitySignPOI signPOI, int code) {
            Sprite icon = m_ColorPOIInfo.GetSprite(code);
            signPOI.SetIcon(icon);
        }

        public void SetIconCodeToMapPOI(UnityMapPOI mapPOI, int code) {
            Sprite icon = m_ColorPOIInfo.GetSprite(code);
            mapPOI.SetIcon(icon);
        }
    }
}