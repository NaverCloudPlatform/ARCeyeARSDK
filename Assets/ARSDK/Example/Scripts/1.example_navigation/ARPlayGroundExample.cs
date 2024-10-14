using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ARCeye;

public class ARPlayGroundExample : MonoBehaviour
{
    [SerializeField]
    private ARPlayGround m_ARPlayGround;

    [SerializeField]
    private AMProjStageReader m_AMProjStageReader;

    
    [Header("UI")]
    [SerializeField]
    private Transform m_StageButtonsArea;

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

    [SerializeField]
    private Text m_NaviMessageText;


    private Camera m_MainCamera;
    private string m_CurrStage;



    private void Awake()
    {
        Application.targetFrameRate = 60;
        m_MainCamera = Camera.main;
        HideDestRect();

        m_AMProjStageReader.Load(m_ARPlayGround.amprojFilePath, stages => {
            GenerateStageButtons(m_StageButtonsArea, stages);
        });
    }

    public void Reset()
    {
        m_ARPlayGround.Reset();
    }

    public void SetStage(string stageName)
    {
        if(m_CurrStage == stageName)
        {
            return;
        }
        
        m_ARPlayGround.SetStage(stageName);

        m_MapViewController.Show();

        // 스테이지가 할당되면 MapView를 활성화.
        m_MapViewController.Show(true, false);

        m_CurrStage = stageName;
    }

    private void GenerateStageButtons(Transform stageButtonsArea, List<string> stages)
    {
        var defaultButton = stageButtonsArea.GetComponentInChildren<Button>();

        foreach(string stageName in stages)
        {
            Button stageButton = Instantiate(defaultButton, stageButtonsArea);
            stageButton.gameObject.name = $"Button_SetStage_{stageName}";
            stageButton.onClick.RemoveAllListeners();
            stageButton.onClick.AddListener(()=>{
                m_ARPlayGround.SetStage(stageName); 
            });

            Text buttonText = stageButton.GetComponentInChildren<Text>();
            buttonText.text = $"SetStage\n{stageName}";
        }

        defaultButton.gameObject.SetActive(false);
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
        DestinationCellView defaultCell = GetDefaultCell();
        // 조건에 해당하는 POI만 출력.
        // var filterdPOIItems = poiItems.FindAll(
        //     e => 
        //         // Cafe
        //         e.dpcode == 110700 ||
        //         e.dpcode == 111100 ||

        //         // Toliets
        //         e.dpcode == 133000 ||
        //         e.dpcode == 133003 ||
        //         e.dpcode == 133004 ||
        //         e.dpcode == 133113 ||

        //         // Info
        //         e.dpcode == 133130 ||

        //         // 계단
        //         e.dpcode == 171105 ||

        //         e.name == "송도달빛축제공원행 승강장"
        // );

        var filterdPOIItems = poiItems;

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

    public void ShowDestRect()
    {
        m_ShowDestRectButton.gameObject.SetActive(false);
        m_HideDestRectButton.gameObject.SetActive(true);
        m_DestinationScrollRect.gameObject.SetActive(true);
    }

    public void HideDestRect()
    {
        m_ShowDestRectButton.gameObject.SetActive(true);
        m_HideDestRectButton.gameObject.SetActive(false);
        m_DestinationScrollRect.gameObject.SetActive(false);
    }


    /* ------------------------------------- */
    /*              Navigation               */
    /* ------------------------------------- */

    public void LoadNavigation(LayerPOIItem poiItem)
    {
        if(m_CurrStage == poiItem.stageName)
        {
            m_ARPlayGround.LoadNavigation(poiItem, PathFindingType.Default);
        }
        else
        {
            Debug.Log("다른 층의 목적지를 선택. 사용자에게 이동 수단을 선택하는 UI를 출력하거나 지정된 수단으로 경로 탐색");
            m_ARPlayGround.LoadNavigation(poiItem, PathFindingType.ElevatorOnly);
        }
    }

    public void UnloadNavigation()
    {
        m_ARPlayGround.UnloadNavigation();
    }

    public void OnNavigationStarted()
    {
        Debug.Log("Navigation is started");
        m_NaviMessageText.gameObject.SetActive(false);
        HideDestRect();
    }

    public void OnNavigationEnded()
    {
        Debug.Log("Navigation is ended");
        m_DistanceText.gameObject.SetActive(false);
        m_NaviMessageText.gameObject.SetActive(false);
    }

    public void OnNavigationFailed()
    {
        Debug.Log("Navigation is failed");
    }

    public void OnNavigationReSearched()
    {
        Debug.Log("Navigation is re-searched");
    }

    public void UpdateRemainingDistance(float distance)
    {
        m_DistanceText.text = string.Format("Distance : {0} m", distance.ToString("N1"));
    }

    public void OnDestinationArrived()
    {
        Debug.Log("Arrive at destination");
        m_DistanceText.gameObject.SetActive(false);
        m_NaviMessageText.gameObject.SetActive(true);
        m_NaviMessageText.text = "목적지 도착!";
    }

    public void OnTransitMovingStarted(ConnectionType transitType, string destStageName)
    {
        // transit 노드 진입 시 즉시 stage를 변경해야 하는지 확인
        if(CheckLevelEquality(destStageName))
        {
            // VL 인식 없이 Stage를 변경
            SetStage(destStageName);
        }
        else
        {
            // VL 인식 대기를 위해 Map 화면 비활성화
            m_MapViewController.Hide();
        }
    }

    public void OnTransitMovingEnded()
    {
        Debug.Log("Transit moving ended");
        m_MapViewController.Show(true, false);
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

    public bool CheckLevelEquality(string nextStage)
    {
        // 사전에 설정한 데이터. 현재 층과 다음 층이 어떤 조합일때 같은 층에서의 스테이지 전환인지 설정.
        return 
            (m_CurrStage == "GND" && nextStage == "1F") ||
            (m_CurrStage == "1F" && nextStage == "GND");
    }

    /* ------------------------------------- */
    /*         Custom Range Events           */
    /* ------------------------------------- */

    public void OnCustomRangeEntered(string uuid, string name)
    {
        Debug.Log($"Enter custom range ({uuid} - {name})");
    }

    public void OnCustomRangeExited(string uuid, string name)
    {
        Debug.Log($"Exit custom range ({uuid} - {name})");
    }
}