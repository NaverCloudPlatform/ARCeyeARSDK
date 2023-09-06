using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ARCeye;

public class ExampleVLSDK : MonoBehaviour
{
    public VLSDKManager m_VLSDKManager;
    public ARPlayGround m_ARPlayGround;

    void Start() 
    {
        m_VLSDKManager.StartSession();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        bool isPaused = pauseStatus;
        if(isPaused)
        {
            m_VLSDKManager.ResetSession();
        }
    }

    public void OnFloorChanged(string floorName)
    {
        if(floorName == "066")
        {
            floorName = "1F";
        }

        m_ARPlayGround.SetStage(floorName);
    }
}
