using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;

public class ScanViewController : ViewController<ScanView>
{
    public void ShowWithFadeIn()
    {
        base.Show(true);

        m_View.GetComponent<CanvasGroup>().alpha = 0;

        UIEffect.FadeIn(m_View, 1.5f);

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
}
