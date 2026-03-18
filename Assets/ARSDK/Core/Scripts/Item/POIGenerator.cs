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
        private UnitySignPOI m_SignPOIPrefab;
        public UnitySignPOI SignPOIPrefab
        {
            get => m_SignPOIPrefab;
            set => m_SignPOIPrefab = value;
        }

        [Header("Map POI")]
        private Camera m_MapCamera;

        [SerializeField]
        private UnityMapPOI m_MapPOIPrefab;
        public UnityMapPOI MapPOIPrefab
        {
            get => m_MapPOIPrefab;
            set => m_MapPOIPrefab = value;
        }

        [Header("POI Info")]

        [SerializeField]
        private POIIconInfo m_POIIconInfo;
        public POIIconInfo POIIconInfo
        {
            get => m_POIIconInfo;
            set => m_POIIconInfo = value;
        }

        public GameObject GenerateSignPOI()
        {
            return Instantiate(m_SignPOIPrefab.gameObject);
        }

        public UnityMapPOI GenerateMapPOI()
        {
            GameObject go = Instantiate(m_MapPOIPrefab.gameObject);

            UnityMapPOI mapPOI = go.GetComponent<UnityMapPOI>();

            if (m_MapCamera == null)
            {
                m_MapCamera = FindFirstObjectByType<MapCamera>().Camera;
            }

            mapPOI.targetCamera = m_MapCamera;

            return mapPOI;
        }

        public void SetIconCodeToSignPOI(UnitySignPOI signPOI, int code)
        {
            Sprite icon = m_POIIconInfo.GetSprite(code);
            signPOI.SetIcon(icon);
        }

        public void SetIconCodeToMapPOI(UnityMapPOI mapPOI, int code)
        {
            Sprite icon = m_POIIconInfo.GetSprite(code);
            mapPOI.SetIcon(icon);
        }
    }
}