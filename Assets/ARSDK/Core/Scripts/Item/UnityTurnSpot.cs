using UnityEngine;

namespace ARCeye
{
    /*
     * ============================================================
     *  TurnSpot 3D Asset Animation Guide
     * ============================================================
     *
     *  A TurnSpot asset can have up to 6 animation clips. Each clip plays
     *  when the TurnSpot enters a specific state. If a clip is missing,
     *  the state changes without animation.
     *
     *  Name            | PlayMode | Description
     *  --------------- | -------- | -----------------------------------
     *  "Idle"          | Once     | Initial state. Plays right after loading, then hides immediately.
     *  "Appearance"    | Once     | Plays when the camera enters the Rendering Range.
     *  "Looping"       | Loop     | Loops after Appearance finishes.
     *  "Action"        | Once     | Plays when the camera enters the Action Range.
     *  "A_Looping"     | Loop     | Loops after Action finishes.
     *  "Disappearance" | Once     | Plays when the camera enters the Exit Range.
     *
     *  Trigger ranges (distance from camera):
     *    - Rendering Range : 100.0m  (Idle -> Appearance)
     *    - Action Range    :  10.0m  (Looping -> Action)
     *    - Exit Range      :   3.0m  (A_Looping -> Disappearance)
     *
     *  State flow:
     *    Idle -> Appearance -> Looping -> Action -> A_Looping -> Disappearance
     * ============================================================
     */

    public class UnityTurnSpot : UnityModel
    {
        [Header("Model Paths")]
        public string TurnSpotLeft = "/Contents/Indicator/TurnSpot_L.glb";
        public string TurnSpotRight = "/Contents/Indicator/TurnSpot_R.glb";
        public string TurnSpotStraight = "/Contents/Indicator/TurnSpot_S.glb";
        public string TurnSpotUp = "/Contents/Indicator/TurnSpot_Up.glb";
        public string TurnSpotDown = "/Contents/Indicator/TurnSpot_Down.glb";
        public string Destination = "/Contents/Indicator/TurnSpot_Destination.glb";

        private void Start()
        {
            m_BillboardRotationMode = Billboard.RotationMode.AXIS_Y;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void SetOpacity(float opacity)
        {
            base.SetOpacity(opacity);
        }

        public virtual void SetDistance(int distance) { }
    }
}
