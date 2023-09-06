using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class POIGenerator : MonoBehaviour
    {
        [Header("Sign POI")]
        [SerializeField]
        private GameObject m_SignPOIPrefab;

        [SerializeField]
        private List<GameObject> m_SignPOIIconModels;

        [Header("Map POI")]
        private Camera m_MapCamera;

        [SerializeField]
        private GameObject m_MapPOIPrefab;

        [SerializeField]
        private List<Sprite> m_MapPOIIconSprites;


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

        public void SetIconCode(UnitySignPOI signPOI, int code) {
            string iconName = ConvertToName(code);

            var iconPrefab = m_SignPOIIconModels.Find(e => e.name == iconName);
            if(iconPrefab == null)
            {
                Debug.LogWarning(iconName);
            }
            else
            {
                var icon = Instantiate(iconPrefab);
                signPOI.SetIcon(icon);
            }
        }

        public void SetIconCode(UnityMapPOI mapPOI, int code) {
            string iconName = ConvertToName(code);

            var iconSprite = m_MapPOIIconSprites.Find(e => e.name == iconName);
            mapPOI.SetIcon(iconSprite);
        }

        public static string ConvertToName(int code) {
            switch(code) {
                case -400531:
                case -400535:
                case -400527:
                    return "Airport";
                case -400530:
                    return "AirportTransfer";
                case -400528:
                    return "AirportDeparture";
                case -400529:
                    return "AirportArrival";
                case -400534:
                case  132200:
                    return "SmallMart";
                case -400506:
                    return "Gate";
                case 509000:
                    return "Nursery";
                case 70100:
                    return "Bank";
                case 70700:
                    return "Atm";
                case 110100:
                    return "Restaurant";
                case 110700:
                case 111100:
                    return "Cafe";
                case 110800:
                    return "Pizza";
                case 111000:
                    return "FastFood";
                case 111200:
                    return "Bakery";
                case 111300:
                    return "IceCream";
                case 130100:
                    return "DepartmentStore";
                case 130300:
                    return "Fashion";
                case 130500:
                    return "ConvenienceStore";
                case 131000:
                    return "Mobile";
                case 131500:
                    return "Beauty";
                case 133000:
                    return "Toilet";
                case 133003:
                    return "ToiletMan";
                case 133004:
                    return "ToiletWoman";
                case 133101:
                    return "Elevator";
                case 133103:
                    return "Escalator";
                case 133104:
                    return "Locker";
                case 133113:
                    return "ToiletDis";
                case 133130:
                    return "Info";
                case 133134:
                    return "Cart";
                case 140100:
                    return "Hospital";
                case 140700:
                    return "Pharmacy";
                case 160303:
                    return "AirportSubway";
                case 160501:
                    return "BusStop";
                case 163001:
                    return "AirportDepartureGate";
                case 163002:
                    return "AirportArrivalGate";
                case 163017:
                    return "MovingWalk";
                case 171105:
                    return "Stairs";
                case 180100:
                    return "Building";
                default:
                    // Debug.LogWarning($"[POIGenerator] No icon is assigned to dpcode {code}");
                    return "Etc";
            }
        }
    }
}