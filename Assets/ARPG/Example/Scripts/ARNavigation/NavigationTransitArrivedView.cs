using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using UnityEngine.UI;

public class NavigationTransitArrivedView : View
{
    [SerializeField]
    private Text m_DestText;

    public void Initialize(string destStage)
    {
        m_DestText.text = destStage;
    }
}
