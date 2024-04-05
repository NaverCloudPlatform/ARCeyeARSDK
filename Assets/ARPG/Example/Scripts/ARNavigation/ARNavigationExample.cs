using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ARCeye;
using System;

public class ARNavigationExample : MonoBehaviour
{
    [Header("ARCeye")]
    [SerializeField]
    private ARPlayGround m_ARPlayGround;
    [SerializeField]
    private VLSDKManager m_VLSDKManager;


    [Header("ViewController")]
    private SplashViewController m_SplashViewController;
    private ScanViewController m_ScanViewController;
    private MapViewController m_MapViewController;
    private AroundViewController m_AroundViewController;
    private NavigationViewController m_NavigationViewController;

    private LayerPOIItem m_CurrDestination;


    private void Awake()
    {
        m_SplashViewController = UIUtils.FindViewController<SplashViewController>();
        m_ScanViewController = UIUtils.FindViewController<ScanViewController>();
        m_MapViewController = UIUtils.FindViewController<MapViewController>();
        m_AroundViewController = UIUtils.FindViewController<AroundViewController>();
        m_NavigationViewController = UIUtils.FindViewController<NavigationViewController>();
    }

    private IEnumerator Start()
    {
        m_SplashViewController.Show();

        yield return new WaitForSeconds(2);

        m_SplashViewController.HideWithFadeOut();
        m_ScanViewController.ShowWithFadeIn();
    }

    public void StartScanning()
    {
        m_VLSDKManager.StartSession();
    }

    public void StartRescanning()
    {
        m_VLSDKManager.ResetSession();
        m_VLSDKManager.StartSession();

        m_NavigationViewController.ShowRescanGuideInTransitMoving(false);
    }


    /* --------------------------------------- */
    /*           VLSDKManager Events           */
    /* --------------------------------------- */
    
    public void OnVLStateChanged(TrackerState trackerState)
    {
        if(m_ARPlayGround.IsNaviMode)
        {
            HandleStateInNaviMode(trackerState);
        }
        else
        {
            HandleStateInAroundMode(trackerState);
        }
    }

    private void HandleStateInAroundMode(TrackerState trackerState)
    {
        switch(trackerState)
        {
            case TrackerState.INITIAL:
                m_MapViewController.Hide();
                m_AroundViewController.Hide();

                // VL 초기화 상태가 되면 ARSDK 리셋.
                m_ARPlayGround.Reset();
                break;
            case TrackerState.VL_PASS:
                m_ScanViewController.Hide();
                m_AroundViewController.Show();
                break;
            case TrackerState.VL_OUT_OF_SERVICE:
                Debug.LogWarning("서비스 영역을 벗어났습니다.");
                break;
        }
    }

    private void HandleStateInNaviMode(TrackerState trackerState)
    {
        switch(trackerState)
        {
            case TrackerState.INITIAL:
                m_MapViewController.Hide();
                m_NavigationViewController.ShowRemainingDistance(false);
                
                // VL 초기화 상태가 되면 ARSDK 리셋.
                m_ARPlayGround.Reset();
                break;
            case TrackerState.VL_PASS:
                m_NavigationViewController.ShowMainView();
                break;
            case TrackerState.VL_OUT_OF_SERVICE:
                Debug.LogWarning("서비스 영역을 벗어났습니다.");
                break;
        }
    }


    /* --------------------------------------- */
    /*           ARPlayGround Events           */
    /* --------------------------------------- */

    public void OnStageChanged(string name, string label)
    {
        m_MapViewController.ChangeStage(name, label);
    }

    public void OnPOIList(List<LayerPOIItem> poiItems)
    {
        m_AroundViewController.AssignPOIList(poiItems, selectedPOIItem => {
            LoadNavigation(selectedPOIItem);
        });
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
    }

    public void OnChangedToShrinked()
    {
        m_ARPlayGround.ActivateNextStep(true);
    }


    /* ------------------------------------- */
    /*              Navigation               */
    /* ------------------------------------- */

    public void LoadNavigation(LayerPOIItem poiItem)
    {
        m_AroundViewController.Hide();
        
        m_CurrDestination = poiItem;

        string currStage = m_ARPlayGround.GetStageName();
        string destStage = poiItem.stageName;
        
        if (currStage == destStage) {
            m_ARPlayGround.LoadNavigation(poiItem);
            m_NavigationViewController.ShowMainView();
        } else {
            m_NavigationViewController.ShowTransitSelectView(currStage, destStage);
        }
    }

    public void LoadNavigationWithTransit(int type) {
        PathFindingType pathFindingType = PathFindingType.Default;

        switch (type) {
            case 0:
                pathFindingType = PathFindingType.Default;
                break;
            case 1:
                pathFindingType = PathFindingType.EscalatorOnly;
                break;
            case 2:
                pathFindingType = PathFindingType.ElevatorOnly;
                break;
            case 3:
                pathFindingType = PathFindingType.StairOnly;
                break;
            default:
                pathFindingType = PathFindingType.Default;
                break;
        }

        m_ARPlayGround.LoadNavigation(m_CurrDestination, pathFindingType);
    }

    public void UnloadNavigation()
    {
        m_AroundViewController.ShowMainView();
        m_NavigationViewController.Hide();

        m_MapViewController.Show();

        m_ARPlayGround.UnloadNavigation();
    }

    public void OnNavigationStarted()
    {
        m_NavigationViewController.ShowMainView();
    }

    public void OnNavigationEnded()
    {
        m_AroundViewController.ShowMainView();
        m_NavigationViewController.Hide();
    }

    public void OnNavigationFailed()
    {
        Debug.Log("Failed to find path");
    }

    public void UpdateRemainingDistance(float distance)
    {
        m_NavigationViewController.UpdateRemainingDistance(distance);
    }

    public void OnDestinationArrived()
    {
        m_NavigationViewController.ShowArrivedView();
    }

    public void OnTransitMovingStarted(ConnectionType transitType, string destStageName)
    {
        string currStage = m_ARPlayGround.GetStageName();
        m_NavigationViewController.ShowTransitMovingView(transitType, currStage, destStageName);

        m_VLSDKManager.StopSession();
    }

    public void OnTransitMovingEnded()
    {
        m_NavigationViewController.ShowTransitArrivedView(()=>{
            m_VLSDKManager.StartSession();
        });
    }

    public void OnTransitMovingFailed(string detectedStageName, string destinationStageName)
    {
        Debug.LogWarning($"Transit moving failed. Please reset and try rescanning (detected: {detectedStageName}, destination: {destinationStageName})");

        m_NavigationViewController.ShowRescanGuideInTransitMoving(true);

        m_VLSDKManager.StopSession();
        m_NavigationViewController.EnableScanButton(true);
    }


    private void OnApplicationPause(bool pauseStatus)
    {
        bool isPaused = pauseStatus;
        if(isPaused)
        {
            m_VLSDKManager.ResetSession();
        }
    }
}
