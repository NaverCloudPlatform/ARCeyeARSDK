using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;

public class NavigationView : View
{
    [SerializeField]
    private NavigationStartedView m_StartedView;

    [SerializeField]
    private NavigationArrivedView m_ArrivedView;

    [SerializeField]
    private NavigationTransitArrivedView m_TransitArrivedView;

    [SerializeField]
    private NavigationTransitMovingView m_TransitMovingView;

    [SerializeField]
    private NavigationTransitSelectView m_TransitSelectView;


    public void ShowStartedView()
    {
        base.Show(true);

        HideAllViews();
        m_StartedView.Show(true);
        m_StartedView.ShowRemainingDistance(true);
    }

    public void ShowArrivedView()
    {
        base.Show(true);

        HideAllViews();
        m_ArrivedView.Show(true);
    }

    public void ShowTransitSelectView(string currStage, string destStage)
    {
        base.Show(true);
        
        HideAllViews();

        m_TransitSelectView.Show(true);
        m_TransitSelectView.Initialize(currStage, destStage);
    }

    public void ShowTransitMovingView(ConnectionType transitType, string currStage, string destStageName)
    {
        base.Show(true);

        HideAllViews();

        m_TransitMovingView.Show(true);
        m_TransitMovingView.Initialize(transitType, currStage, destStageName);
    }

    public void ShowTransitArrivedView(string destStageName)
    {
        base.Show(true);

        HideAllViews();
        m_TransitArrivedView.Show(true);
        m_TransitArrivedView.Initialize(destStageName);
    }

    public void HideAllViews()
    {
        m_StartedView.Show(false);
        m_ArrivedView.Show(false);
        m_TransitArrivedView.Show(false);
        m_TransitMovingView.Show(false);
        m_TransitSelectView.Show(false);
    }

    public void UpdateRemainingDistance(float distance)
    {
        m_StartedView.UpdateRemainingDistance(distance);
    }

    public void ShowRemainingDistance(bool value)
    {
        m_StartedView.ShowRemainingDistance(value);
    }

    public void EnableScanButton(bool value)
    {
        m_TransitMovingView.EnableScanButton(value);
    }

    public void ShowRescanGuide(bool value)
    {
        m_TransitMovingView.ShowRescanGuide(value);
    }
}
