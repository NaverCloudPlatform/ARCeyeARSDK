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

    private string m_CurrStage;


    private void Awake()
    {
        Application.targetFrameRate = 60;
        HideDestRect();
    }

    public void Reset()
    {
        m_ARPlayGround.Reset();
    }

    public void SetStage(string stageName)
    {
        m_ARPlayGround.SetStage(stageName);

        // 스테이지가 할당되면 MapView를 활성화.
        m_MapViewController.Show(true, false);

        m_CurrStage = stageName;
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
            m_ARPlayGround.LoadNavigation(poiItem, ConnectionType.Default);
        }
        else
        {
            Debug.Log("다른 층의 목적지를 선택. 사용자에게 이동 수단을 선택하는 UI를 출력하거나 지정된 수단으로 경로 탐색");
            m_ARPlayGround.LoadNavigation(poiItem, ConnectionType.Elevator);
        }
    }

    public void UnloadNavigation()
    {
        m_ARPlayGround.UnloadNavigation();
    }

    public void OnNavigationStarted()
    {
        Debug.Log("Navigation is started");
        m_DistanceText.gameObject.SetActive(true);
        m_NaviMessageText.gameObject.SetActive(false);
        HideDestRect();
    }

    public void OnNavigationEnded()
    {
        Debug.Log("Navigation is ended");
        m_DistanceText.gameObject.SetActive(false);
        m_NaviMessageText.gameObject.SetActive(false);
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
        Debug.Log("Transit moving started : " + transitType + ", " + destStageName);
        m_MapViewController.Hide();
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
}