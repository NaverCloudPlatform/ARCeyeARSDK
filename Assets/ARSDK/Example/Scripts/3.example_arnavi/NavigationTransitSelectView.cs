using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using UnityEngine.UI;

public class NavigationTransitSelectView : View
{
    public Text m_CurrStageText;
    public Text m_DestStageText;

    public void Initialize(string currStage, string destStage)
    {
        m_CurrStageText.text = currStage;
        m_DestStageText.text = destStage;
    }
}
