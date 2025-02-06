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
    private ScanViewController m_ScanViewController;
    private MapViewController m_MapViewController;
    private AroundViewController m_AroundViewController;
    private NavigationViewController m_NavigationViewController;

    private LayerPOIItem m_CurrDestination;
    private string m_CurrLayerInfo;
    
    private void Awake()
    {
        m_ScanViewController = UIUtils.FindViewController<ScanViewController>();
        m_MapViewController = UIUtils.FindViewController<MapViewController>();
        m_AroundViewController = UIUtils.FindViewController<AroundViewController>();
        m_NavigationViewController = UIUtils.FindViewController<NavigationViewController>();
    }

    private void Start()
    {
        m_ScanViewController.ShowWithFadeIn();
    }

    public void StartScanning()
    {
        m_VLSDKManager.StartSession();
        m_ScanViewController.ShowScanGuide();
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
        switch(trackerState)
        {
            case TrackerState.INITIAL:
                HandleInitialState();
                break;
            case TrackerState.VL_PASS:
                HandleVLPassState();
                break;
            case TrackerState.VL_OUT_OF_SERVICE:
                HandleVLOutOfService();
                break;
        }
    }

    private void HandleInitialState()
    {
        m_ARPlayGround.Reset();
        m_ARPlayGround.ActivateNextStep(false);

        if(m_ARPlayGround.IsNaviMode)
        {
            m_NavigationViewController.Hide();
        }
        else
        {
            m_AroundViewController.Hide();
        }

        NotificationView.Instance().Hide();
        m_MapViewController.Hide();
        m_ScanViewController.ShowScanGuide();
    }

    private void HandleVLPassState()
    {
        // 스캔 완료 애니메이션 출력. 애니메이션 완료 후 VL Pass 상태로 변경.
        m_ScanViewController.ShowScanComplete(()=>{
            m_ARPlayGround.ShowARItems();
            m_ARPlayGround.ActivateNextStep(true);
            m_ARPlayGround.SetLayerInfo(m_CurrLayerInfo);

            if(m_ARPlayGround.IsNaviMode)
            {
                m_NavigationViewController.ShowMainView();
            }
            else
            {
                m_AroundViewController.Show();
            }

            m_MapViewController.Show();
            m_ScanViewController.Hide();
        });
    }

    private void HandleVLOutOfService()
    {
        Debug.LogWarning("서비스 영역을 벗어났습니다.");
    }


    /* --------------------------------------- */
    /*           ARPlayGround Events           */
    /* --------------------------------------- */

    public void OnStageChanged(string name, string label)
    {
        m_MapViewController.ChangeStage(name, label);

        // Transit Moving 중에 Stage가 변경된 경우.
        if(m_ARPlayGround.IsNaviMode)
        {
            m_NavigationViewController.UpdateTransitMovingCurrStage(label);
        }
    }

    public void OnSceneLoaded(string keyname, string crscode, string localeCode)
    {
        
    }

    public void OnSceneUnloaded(string keyname)
    {

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

        bool hasStageLabel = m_ARPlayGround.GetStageLabel().Length > 0;
        bool hasDestLabel  = poiItem.stageLabel.Length > 0;

        string currStage = hasStageLabel ? m_ARPlayGround.GetStageLabel() : m_ARPlayGround.GetStageName();
        string destStage = hasDestLabel  ? poiItem.stageLabel : poiItem.stageName;
        
        if (m_ARPlayGround.GetStageName() == poiItem.stageName) {
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

    /// <summary>
    ///   네비게이션 취소 및 전체 세션 초기화. 주로 Transit Moving에서 네비게이션을 취소할 때 호출한다.
    /// </summary>
    public void UnloadNavigationWhileTransitMoving()
    {
        m_VLSDKManager.ResetSession();
        HandleInitialState();
    }

    /// <summary>
    ///   네비게이션 종료. stage가 인식된 상태에서 네비게이션을 종료할 때 호출한다.
    /// </summary>
    public void UnloadNavigation()
    {
        m_NavigationViewController.Hide();
        m_AroundViewController.ShowMainView();
        m_MapViewController.Show();

        m_ARPlayGround.UnloadNavigation();
    }

    public void OnNavigationStarted()
    {
        m_AroundViewController.Hide();
        m_NavigationViewController.ShowMainView();

        string message = "";

        switch(m_ARPlayGround.locale) 
        {
            case ARCeye.Locale.en_US:
                message = "Start the directions";
                break;
            case ARCeye.Locale.ko_KR:
                message = "길 안내를 시작합니다.";
                break;
            case ARCeye.Locale.zh_CN:
                message = "开始导航。";
                break;
            case ARCeye.Locale.zh_TW:
                message = "開始導航。";
                break;
            case ARCeye.Locale.ja_JP:
                message = "案内を開始します。";
                break;
            case ARCeye.Locale.fr_FR:
                message = "Démarrer la navigation.";
                break;
            case ARCeye.Locale.de_DE:
                message = "Navigation starten.";
                break;
            case ARCeye.Locale.it_IT:
                message = "Avvia la navigazione.";
                break;
            case ARCeye.Locale.pt_BR:
                message = "Iniciar a navegação.";
                break;
            case ARCeye.Locale.es_ES:
                message = "Iniciar la navegación.";
                break;
        }

        NotificationView.Instance().Show(message, NotificationView.Type.NAVIGATION);
    }

    public void OnNavigationEnded()
    {
        m_AroundViewController.ShowMainView();
        m_NavigationViewController.Hide();
        NotificationView.Instance().Hide();
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

    public void OnTransitMovingStarted(ConnectionType transitType, string destStageName, string destStageLabel)
    {
        bool hasStageLabel = m_ARPlayGround.GetStageLabel().Length > 0;
        bool hasDestLabel  = destStageLabel.Length > 0;

        string currStage = hasStageLabel ? m_ARPlayGround.GetStageLabel() : m_ARPlayGround.GetStageName();
        string destStage = hasDestLabel  ? destStageLabel : destStageName;

        NotificationView.Instance().Hide();

        m_NavigationViewController.ShowTransitMovingView(transitType, currStage, destStage);
        
        // 층 이동을 시작하면 기기 자세에 따른 리셋 기능 비활성화.
        m_VLSDKManager.EnableResetByDevicePose(false);

        m_ARPlayGround.HideARItems();
    }

    public void OnTransitMovingEnded()
    {
        m_NavigationViewController.ShowTransitArrivedView(()=>{
            m_VLSDKManager.ResetSession();
            m_VLSDKManager.EnableResetByDevicePose(true);
        });
    }

    public void OnTransitMovingFailed(string detectedStageName, string destinationStageName)
    {
        Debug.LogWarning($"Transit moving failed. Please reset and try rescanning (detected: {detectedStageName}, destination: {destinationStageName})");

        m_NavigationViewController.ShowRescanGuideInTransitMoving(true);
        m_NavigationViewController.EnableScanButton(true);
    }


    public void SetLayerInfo(string layerInfo)
    {
        m_CurrLayerInfo = layerInfo;
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