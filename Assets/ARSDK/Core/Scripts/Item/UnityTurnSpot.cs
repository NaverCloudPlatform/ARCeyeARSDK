using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityTurnSpot : UnityModel
    {
        private Transform m_CameraTransform;

        private Transform m_MeterArea;
        private Transform m_DistanceArea;

        private MeterText m_MeterText;
        private DistanceText m_DistanceText;



        private void Start()
        {
            m_CameraTransform = Camera.main.transform;
            m_BillboardRotationMode = Billboard.RotationMode.AXIS_Y_FLIP;
        }

        public override void Initialize() {
            string rootPath = GetRootPath();

            m_MeterArea = transform.Find(rootPath + "Meter_Root");
            m_DistanceArea = transform.Find(rootPath + "Distance_Root");

            if(m_MeterArea != null)
            {
                GameObject meterTextGO = new GameObject("MeterText");
                meterTextGO.transform.parent = m_MeterArea;
                m_MeterText = meterTextGO.AddComponent<MeterText>();
            }

            if(m_DistanceArea != null)
            {
                GameObject distanceTextGO = new GameObject("DistanceText");
                distanceTextGO.transform.parent = m_DistanceArea;
                m_DistanceText = distanceTextGO.AddComponent<DistanceText>();
            }

            m_MeterText.ResetTransform();
            m_DistanceText.ResetTransform();

            base.Initialize();
        }

        private bool IsDestination()
        {
            Transform child = transform.GetChild(0);
            Transform model = child.GetChild(0);
            return model.name.Contains("Destination");
        }

        public override void SetOpacity(float opacity)
        {
            base.SetOpacity(opacity);

            // Initialize가 호출되기 전에 SetOpacity가 호출되는 경우 방지.
            if(m_MeterText == null || m_DistanceArea == null) {
                return;
            }

            m_MeterText.SetOpacity(opacity);
            m_DistanceText.SetOpacity(opacity);
        }

        private string GetRootPath() {
            string rootRPath = "Scene/TurnSpot_R_Root/TurnSpot_Diamond/";
            string rootLPath = "Scene/TurnSpot_L_Root/TurnSpot_Diamond/";
            string rootSPath = "Scene/TurnSpot_S_Root/TurnSpot_Diamond/";
            string rootDPath = "Scene/TurnSpot_Destination_Root/Destination_Rotation/Diamond_Root/";

            bool isL = transform.Find(rootLPath) != null;
            bool isR = transform.Find(rootRPath) != null;
            bool isS = transform.Find(rootSPath) != null;
            bool isD = transform.Find(rootDPath) != null;

            if(isL) {
                return rootLPath;
            } else if(isR) {
                return rootRPath;
            } else if(isD) {
                return rootDPath;
            } else if(isS) {
                return rootSPath;
            } else {
                NativeLogger.Print(LogLevel.VERBOSE, "[UnityTurnSpot] GetRootPath : Failed to find the text root of TurnSpot");
                return "";
            }
        }
        
        public void SetLabel(string label) 
        {
            // Model 로드 이전에 label의 값을 설정할 경우.
            if(m_DistanceText == null) {
                return;
            }

            // 숫자가 한 자리수일 경우 가운데 정렬.
            int value = int.Parse(label);
            if(value < 10) {
                label = label + " ";
            }

            m_DistanceText.SetLabel(label);
        }
    }
}