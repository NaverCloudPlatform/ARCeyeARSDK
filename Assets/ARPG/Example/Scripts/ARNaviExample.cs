using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using ARCeye;

public class ARNaviExample : MonoBehaviour
{
    [SerializeField]
    public VLSDKManager m_VLSDKManager;

    [SerializeField]
    private ARPlayGround m_ARPlayGround;


    [Header("POI")]

    [SerializeField]
    private ScrollRect m_DestinationScrollRect;

    [SerializeField]
    private Transform  m_DestinationScrollRectContent;

    [SerializeField]
    private Button m_ShowDestRectButton;

    [SerializeField]
    private Button m_HideDestRectButton;

    
    [Header("Map")]

    [SerializeField]
    private MapViewController m_MapViewController;

    [SerializeField]
    private GameObject m_NextStep;


    [Header("Navigation")]

    [SerializeField]
    private Text m_DistanceText;

    [Header("UI Controls")]
    
    [SerializeField]
    private UIViewController m_UIViewController;

    private LayerPOIItem m_CurrDestination;

    private void Awake()
    {
        if (m_UIViewController == null) {
            return;
        }

        m_UIViewController.ShowSplashView();
    }

    public void Reset()
    {
        m_ARPlayGround.Reset();
        m_MapViewController.Hide();
    }

    public void SetStage(string stageName)
    {
        m_ARPlayGround.SetStage(stageName);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        bool isPaused = pauseStatus;
        if(isPaused)
        {
            m_VLSDKManager.ResetSession();
        }
    }

    public void StartVLSDKSession()
    {
        m_VLSDKManager.StartSession();
    }

    public void OnVLStateChanged(TrackerState state)
    {
        if(state == TrackerState.VL_PASS)
        {
            m_UIViewController.UpdateScanView(false);
            m_UIViewController.UpdateAroundView(true);

            m_MapViewController.Show(true, false);
            m_UIViewController.SetStageName(m_ARPlayGround.GetStageName());
        }
        else if(state == TrackerState.INITIAL)
        {
            m_MapViewController.Hide();
        }
    }

    /* ------------------------------------- */
    /*       POI List Visualization          */
    /* ------------------------------------- */

    public void OnPOIList(List<LayerPOIItem> poiItems)
    {       
        DestinationCellView defaultCell = GetDefaultCell();
        // 조건에 해당하는 POI만 출력.
        var filterdPOIItems = poiItems.FindAll(
            e => 
                // Cafe
                e.dpcode == 110700 ||
                e.dpcode == 111100 ||

                // Toliets
                e.dpcode == 133000 ||
                e.dpcode == 133003 ||
                e.dpcode == 133004 ||
                e.dpcode == 133113 ||

                // Info
                e.dpcode == 133130 ||

                e.name == "랩스연구소"
        );

        foreach(var elem in filterdPOIItems)
        {
            GameObject cellGO = Instantiate(defaultCell.gameObject, m_DestinationScrollRectContent);
            DestinationCellView cellView = cellGO.GetComponent<DestinationCellView>();
            cellView.Initialize(elem);
            cellView.RegisterAction(()=> LoadNavigation(elem) );
        }

        GameObject.Destroy(defaultCell.gameObject);
    }

    private DestinationCellView GetDefaultCell()
    {
        return m_DestinationScrollRectContent.GetComponentInChildren<DestinationCellView>();
    }

    /* ------------------------------------- */
    /*              Navigation               */
    /* ------------------------------------- */

    public void LoadNavigation(LayerPOIItem poiItem)
    {
        string currStage = m_ARPlayGround.GetStageName();
        string destStage = poiItem.stageName;
        
        if (currStage == destStage) {
            m_ARPlayGround.LoadNavigation(poiItem);
        } else {
            m_UIViewController.UpdateDestinationRecommendView(false);
            m_UIViewController.ShowTransitSelectView(currStage, destStage);
            m_CurrDestination = poiItem;
        }
    }

    public void LoadNavigationWithTransit(int type) {
        ConnectionType connectionType = ConnectionType.Default;

        switch (type) {
            case 0:
                connectionType = ConnectionType.Default;
                break;
            case 1:
                connectionType = ConnectionType.Escalator;
                break;
            case 2:
                connectionType = ConnectionType.Elevator;
                break;
            case 3:
                connectionType = ConnectionType.Stair;
                break;
            default:
                connectionType = ConnectionType.Default;
                break;
        }

        m_ARPlayGround.LoadNavigation(m_CurrDestination, connectionType);
    }

    public void UnloadNavigation()
    {
        Debug.Log("Navigation has been unloaded");
        m_ARPlayGround.UnloadNavigation();
    }

    public void OnNavigationStarted()
    {
        Debug.Log("Navigation is started");

        // Update UI
        m_UIViewController.UpdateDestinationRecommendView(false);
        m_UIViewController.UpdateNavigationStartedView(true);
        m_MapViewController.Show(true);
        m_MapViewController.Show(true, false);
    }

    public void UpdateRemainingDistance(float distance)
    {
        m_DistanceText.text = string.Format("{0}", distance.ToString("N0"));
    }

    public void OnDestinationArrived()
    {
        Debug.Log("Arrive at destination");

        // Update UI
        m_UIViewController.UpdateNavigationStartedView(false);
        m_UIViewController.UpdateNavigationArrivedView(true);
    }

    public void OnNavigationEnded()
    {
        Debug.Log("Navigation is ended");

        // Update UI
        m_UIViewController.UpdateNavigationStartedView(false);
        m_UIViewController.UpdateNavigationArrivedView(false);
        m_UIViewController.UpdateAroundView(true);
        m_MapViewController.Show(true);
    }

    public void OnTransitMovingStarted(int transitType)
    {
        Debug.Log("Transit moving started : " + transitType);

        // Update UI
        string currStage = m_ARPlayGround.GetStageName();
        string destStage = m_CurrDestination.stageName;

        m_UIViewController.UpdateNavigationStartedView(false);
        m_UIViewController.ShowTransitMovingView(transitType, currStage, destStage);
        m_MapViewController.Hide();

        // REMOVEME: Sample code only used for this example
        StartCoroutine(SetStageWithDelay(destStage, 2.5f));
    }

    public void OnTransitMovingEnded()
    {
        Debug.Log("Transit moving ended");

        m_UIViewController.HideTransitMovingView();
        m_UIViewController.ShowTransitArrivedView(m_CurrDestination.stageName);

        // After 2 seconds, revert to Around View
        StartCoroutine(DeactivateTransitArrived(m_UIViewController));

        m_MapViewController.Show(true, false);
    }

    private IEnumerator DeactivateTransitArrived(UIViewController controller) 
    {
        yield return new WaitForSeconds(2f);

        controller.HideTransitArrivedView();
        controller.UpdateNavigationStartedView(true);
    }

    // REMOVEME: Sample code only for this example scene
    // Manually updates scene to "2F" after 2 seconds
    private IEnumerator SetStageWithDelay(string stageName, float delay = 2f) {
        yield return new WaitForSeconds(delay);

        SetStage(stageName);
    }


    /* ------------------------------------- */
    /*           Map Mode Events             */
    /* ------------------------------------- */

    public void OnMapActivated(bool value)
    {
        m_ARPlayGround.ActivateNextStep(value);
    }

    public void OnChangedToFull()
    {
        m_ARPlayGround.ActivateNextStep(false);
        TouchSystem.Instance.onDrag.AddListener(m_MapViewController.MoveMapCamera);
        TouchSystem.Instance.onPinchZoom.AddListener(m_MapViewController.ZoomMapCamera);
    }

    public void OnChangedToShrinked()
    {
        m_ARPlayGround.ActivateNextStep(true);
        TouchSystem.Instance.onDrag.RemoveListener(m_MapViewController.MoveMapCamera);
        TouchSystem.Instance.onPinchZoom.RemoveListener(m_MapViewController.ZoomMapCamera);
    }
}