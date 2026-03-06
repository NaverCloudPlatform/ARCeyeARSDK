using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityTurnSpot : UnityModel
    {
        private TurnSpotRenderer m_TurnSpotRenderer;

        private void Awake()
        {
            m_TurnSpotRenderer = GetComponent<TurnSpotRenderer>();
            if (m_TurnSpotRenderer == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[UnityTurnSpot] TurnSpotRenderer component is missing.");
                return;
            }
        }

        private void Start()
        {
            m_BillboardRotationMode = Billboard.RotationMode.AXIS_Y_FLIP;
        }

        public override void Initialize()
        {
            m_TurnSpotRenderer.Initialize();

            base.Initialize();
        }

        public override void SetOpacity(float opacity)
        {
            base.SetOpacity(opacity);

            m_TurnSpotRenderer.SetOpacity(opacity);
        }

        public void SetDistance(int distance)
        {
            m_TurnSpotRenderer.SetDistance(distance);
        }
    }
}