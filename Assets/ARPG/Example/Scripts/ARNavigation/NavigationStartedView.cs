using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using UnityEngine.UI;

public class NavigationStartedView : View
{
    [SerializeField]
    private GameObject m_RemainingDistArea;

    [SerializeField]
    private Text m_RemainingDistText;

    public void ShowRemainingDistance(bool value)
    {
        m_RemainingDistText.text = "";
        m_RemainingDistArea.gameObject.SetActive(value);
    }

    public void UpdateRemainingDistance(float distance)
    {
        m_RemainingDistText.text = string.Format("{0}", distance.ToString("N0"));
    }
}
