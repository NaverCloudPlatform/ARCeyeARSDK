using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class NaviItemGenerator : MonoBehaviour
    {
        public UnityTurnSpot TurnSpotPrefab;

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
