using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;

public class NavigationViewController : ViewController<NavigationView>
{
    [SerializeField]
    private MapViewController m_MapViewController;

    private string m_DestStageName;


    public void ShowMainView()
    {
        m_MapViewController.Show();
        m_View.ShowStartedView();
    }

    public void ShowTransitSelectView(string currStage, string destStage)
    {
        m_View.ShowTransitSelectView(currStage, destStage);
    }

    public void ShowTransitMovingView(ConnectionType transitType, string currStage, string destStageName)
    {
        m_DestStageName = destStageName;

        m_MapViewController.Hide();
        m_View.ShowTransitMovingView(transitType, currStage, destStageName);
    }

    public void ShowTransitArrivedView(System.Action completeCallback)
    {
        StartCoroutine( ShowTransitArrivedViewInternal(m_DestStageName, 1.5f, completeCallback));
    }

    private IEnumerator ShowTransitArrivedViewInternal(string destStage, float showDuration, System.Action completeCallback) {
        m_View.ShowTransitArrivedView(destStage);

        yield return new WaitForSeconds(showDuration);

        completeCallback();

        ShowMainView();
    }

    public void ShowArrivedView()
    {
        m_View.ShowArrivedView();
    }

    public void ShowRemainingDistance(bool value)
    {
        m_View.ShowRemainingDistance(value);
    }

    public void UpdateRemainingDistance(float distance)
    {
        m_View.UpdateRemainingDistance(distance);
    }

    public void EnableScanButton(bool value)
    {
        m_View.EnableScanButton(value);
    }

    public void ShowRescanGuideInTransitMoving(bool value)
    {
        m_View.ShowRescanGuide(value);
    }

    public override void Hide()
    {
        base.Hide();
        m_View.HideAllViews();
        m_View.Show(false);
    }
}
