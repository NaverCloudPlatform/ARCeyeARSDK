using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using TMPro;

public class NavigationTransitSelectView : View
{
    public TMP_Text m_CurrStageText;
    public TMP_Text m_DestStageText;

    public void Initialize(string currStage, string destStage)
    {
        m_CurrStageText.text = currStage;
        m_DestStageText.text = destStage;
    }
}
