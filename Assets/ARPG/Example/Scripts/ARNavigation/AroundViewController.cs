using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;

public class AroundViewController : ViewController<AroundView>
{
    [SerializeField]
    private MapViewController m_MapViewController;

    [SerializeField]
    private Transform m_DestinationContents;


    public override void Show()
    {
        base.Show();
        ShowMainView();
    }

    public override void Hide()
    {
        base.Hide();
        m_MapViewController.Hide();
    }

    public void ShowMainView()
    {
        m_MapViewController.Show();
        m_View.ShowMainView();
    }

    public void ShowDestinationView()
    {
        m_MapViewController.Hide();
        m_View.ShowDestinationView();
    }

    public void AssignPOIList(List<LayerPOIItem> poiItems, System.Action<LayerPOIItem> loadNavigationEvent)
    {
        DestinationCellView defaultCell = GetDefaultCell();
        // 조건에 해당하는 POI만 출력.
        var filterdPOIItems = poiItems.FindAll(
            e => 
                // dpcode로 필터링.
                // Cafe
                e.dpcode == 110700 ||
                e.dpcode == 111100 ||

                // 이름으로 필터링.
                e.name.Contains("화장실")
        );

        foreach(var elem in filterdPOIItems)
        {
            GameObject cellGO = Instantiate(defaultCell.gameObject, m_DestinationContents);
            DestinationCellView cellView = cellGO.GetComponent<DestinationCellView>();
            cellView.Initialize(elem);
            cellView.RegisterAction(()=> loadNavigationEvent(elem) );
        }

        GameObject.Destroy(defaultCell.gameObject);
    }

    private DestinationCellView GetDefaultCell()
    {
        return m_DestinationContents.GetComponentInChildren<DestinationCellView>();
    }
}
