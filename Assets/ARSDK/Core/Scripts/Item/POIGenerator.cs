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
        public GameObject SignPOIPrefab
        {
            get => m_SignPOIPrefab;
            set => m_SignPOIPrefab = value;
        }

        [Header("Map POI")]
        private Camera m_MapCamera;

        [SerializeField]
        private GameObject m_MapPOIPrefab;
        public GameObject MapPOIPrefab
        {
            get => m_MapPOIPrefab;
            set => m_MapPOIPrefab = value;
        }

        [Header("POI Info")]

        [SerializeField]
        private POIIconInfo m_ColorPOIInfo;
        public POIIconInfo ColorPOIInfo
        {
            get => m_ColorPOIInfo;
            set => m_ColorPOIInfo = value;
        }

        [SerializeField]
        private POIIconInfo m_GrayPOIInfo;
        public POIIconInfo GrayPOIInfo
        {
            get => m_GrayPOIInfo;
            set => m_GrayPOIInfo = value;
        }


        public GameObject GenerateSignPOI()
        {
            return Instantiate(m_SignPOIPrefab);
        }

        public UnityMapPOI GenerateMapPOI()
        {
            GameObject go = Instantiate(m_MapPOIPrefab);

            UnityMapPOI mapPOI = go.GetComponent<UnityMapPOI>();

            if (m_MapCamera == null)
            {
                GameObject mapCameraGO = GameObject.FindGameObjectWithTag("MapPOICamera");
                m_MapCamera = mapCameraGO.GetComponent<Camera>();
            }

            mapPOI.targetCamera = m_MapCamera;

            return mapPOI;
        }

        public void SetIconCodeToSignPOI(UnitySignPOI signPOI, int code)
        {
            Sprite icon = m_ColorPOIInfo.GetSprite(code);
            signPOI.SetIcon(icon);
        }

        public void SetIconCodeToMapPOI(UnityMapPOI mapPOI, int code)
        {
            Sprite icon = m_ColorPOIInfo.GetSprite(code);
            mapPOI.SetIcon(icon);
        }
    }
}