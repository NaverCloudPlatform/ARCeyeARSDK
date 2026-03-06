using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class NaviSpotGenerator : MonoBehaviour
    {
        [Header("Prefab")]
        public UnityTurnSpot TurnSpotPrefab;

        [Header("Model Paths")]

        public string TurnSpotLeft = "/Contents/Indicator/TurnSpot_L.glb";
        public string TurnSpotRight = "/Contents/Indicator/TurnSpot_R.glb";
        public string TurnSpotStraight = "/Contents/Indicator/TurnSpot_S.glb";
        public string TurnSpotUp = "/Contents/Indicator/TurnSpot_U.glb";
        public string TurnSpotDown = "/Contents/Indicator/TurnSpot_D.glb";
        public string Destination = "/Contents/Indicator/TurnSpot_DestinationSpot.glb";

        public UnityTurnSpot GenerateTurnSpot()
        {
            if (TurnSpotPrefab == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[NaviSpotGenerator] TurnSpotPrefab is not assigned.");
                return null;
            }

            GameObject go = Instantiate(TurnSpotPrefab.gameObject);
            UnityTurnSpot turnSpot = go.GetComponent<UnityTurnSpot>();

            if (turnSpot == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[NaviSpotGenerator] Generated GameObject does not have a UnityTurnSpot component.");
                return null;
            }

            return turnSpot;
        }
    }
}