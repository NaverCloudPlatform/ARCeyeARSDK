using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;

public class AroundView : View
{
    [SerializeField]
    private AroundMainView m_MainView;

    [SerializeField]
    private AroundDestinationView m_DestinationView;


    public void ShowMainView()
    {
        base.Show(true);

        HideAllView();
        m_MainView.Show(true);
    }

    public void ShowDestinationView()
    {
        base.Show(true);
        
        HideAllView();
        m_DestinationView.Show(true);
    }

    private void HideAllView()
    {
        m_MainView.Show(false);
        m_DestinationView.Show(false);
    }
}
