using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using UnityEngine.UI;

public class NavigationTransitMovingView : View
{
    [SerializeField]
    private Text m_GuideDestStageText;

    [SerializeField]
    private Text m_StartStageText;
    [SerializeField]
    private Text m_DestStageText;

    [SerializeField]
    private GameObject m_IconEscalator;
    [SerializeField]
    private GameObject m_IconElevator;
    [SerializeField]
    private GameObject m_IconStair;

    [SerializeField]
    private Button m_ScanButton;

    [SerializeField]
    private GameObject m_TextErrorRescan;


    public void Initialize(ConnectionType transitType, string currStage, string destStageName)
    {
        m_GuideDestStageText.text = destStageName;

        m_IconEscalator.gameObject.SetActive(false);
        m_IconElevator.gameObject.SetActive(false);
        m_IconStair.gameObject.SetActive(false);

        switch(transitType)
        {
            case ConnectionType.Escalator : 
                m_IconEscalator.gameObject.SetActive(true);
                break;
            case ConnectionType.Elevator : 
                m_IconEscalator.gameObject.SetActive(true);
                break;
            case ConnectionType.Stair : 
                m_IconEscalator.gameObject.SetActive(true);
                break;
        }

        m_StartStageText.text = currStage;
        m_DestStageText.text = destStageName;

        ShowRescanGuide(false);
    }

    public void EnableScanButton(bool value)
    {
        m_ScanButton.interactable = value;   
    }

    public void ShowRescanGuide(bool value)
    {
        m_TextErrorRescan.gameObject.SetActive(value);
    }
}
