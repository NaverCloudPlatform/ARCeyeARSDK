using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScanView : View
{
    [SerializeField]
    private GameObject m_Background;

    [SerializeField]
    private GameObject m_ScanPanelGO;

    [SerializeField]
    private GameObject m_ScanGuideGO;

    [SerializeField]
    private GameObject m_ScanCompleteGO;
    
    public Button scanButton;


    private const float ScanCompleteDuration = 1.98f;

    
    public void ShowScanPanel()
    {
        m_ScanPanelGO.gameObject.SetActive(true);
    }

    public void HideScanPanel()
    {
        m_ScanPanelGO.gameObject.SetActive(false);
    }

    public void HideAllScanAnimations()
    {
        m_ScanGuideGO.gameObject.SetActive(false);
        m_ScanCompleteGO.gameObject.SetActive(false);
    }

    public void ShowScanGuide()
    {
        HideScanPanel();
        
        m_Background.gameObject.SetActive(true);
        m_ScanGuideGO.gameObject.SetActive(true);
        m_ScanCompleteGO.gameObject.SetActive(false);
    }

    public void ShowScanComplete(UnityAction finishCallback)
    {
        HideScanPanel();

        StartCoroutine( ShowScanCompleteInternal(finishCallback) );
    }

    private IEnumerator ShowScanCompleteInternal(UnityAction finishCallback)
    {
        m_Background.gameObject.SetActive(true);
        m_ScanGuideGO.gameObject.SetActive(false);
        m_ScanCompleteGO.gameObject.SetActive(true);

        yield return new WaitForSeconds(ScanCompleteDuration);

        finishCallback();

        m_Background.gameObject.SetActive(false);
    }
}
