using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using TMPro;

public class NavigationTransitArrivedView : View
{
    [SerializeField]
    private TMP_Text m_DestText;

    [SerializeField]
    private GameObject m_NextStep;

    public void Initialize(string destStage)
    {
        m_DestText.text = destStage;
    }

    public override void Show(bool show) {
        base.Show(show);

        if (m_NextStep != null) {
            m_NextStep.SetActive(!show);
        }
    }
}
