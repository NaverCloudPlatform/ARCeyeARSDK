using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using UnityEngine.Events;

public class ScanViewController : ViewController<ScanView>
{
    public void ShowWithFadeIn()
    {
        base.Show(true);

        m_View.ShowScanPanel();
        m_View.HideAllScanAnimations();
        
        m_View.GetComponent<CanvasGroup>().alpha = 0;

        UIEffect.FadeIn(m_View, 0.5f);

        EnableScanButton();
    }

    public void EnableScanButton()
    {
        m_View.scanButton.interactable = true;
    }

    public void DisableScanButton()
    {
        m_View.scanButton.interactable = false;
    }

    public void ShowScanGuide()
    {
        base.Show(true);
        m_View.ShowScanGuide();
    }

    public void ShowScanComplete(UnityAction finishCallback)
    {
        m_View.ShowScanComplete(finishCallback);
    }
}
