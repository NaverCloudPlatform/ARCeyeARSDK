using System;
using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;

public class AroundViewController : ViewController<AroundView>
{
    [Header("ViewController")]
    [SerializeField]
    private MapViewController m_MapViewController;

    [Header("UI")]
    [SerializeField]
    private POIIconInfo m_POIGrayIconInfo;

    [Header("Contents")]
    [SerializeField]
    private Transform m_ScrollViewContents;

    [SerializeField]
    private Transform m_FullViewContents;

    [SerializeField]
    private Transform m_CategoryViewContents;

    [SerializeField]
    private GameObject m_ScrollView;

    [SerializeField]
    private GameObject m_FullView;

    private string m_StringFilter = "";

    private CategoryCell m_SelectedCategory;

    private List<DestinationCellView> m_FullViewCellList = new List<DestinationCellView>();

    private NearbyCalculator m_NearbyCalculator;


    protected override void Awake()
    {
        base.Awake();
        m_NearbyCalculator = new NearbyCalculator();
    }


    public void ActivateFullSearchView() {
        FilterPOIListByString("");

        m_ScrollView.SetActive(false);
        m_FullView.SetActive(true);
    }

    public void ActivateScrollView() {
        m_FullView.SetActive(false);
        m_ScrollView.SetActive(true);

        if(m_SelectedCategory != null) {
            ResetPOIListFilter();
            m_SelectedCategory.Deselect();
            m_SelectedCategory = null;
        }
    }

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
        ActivateScrollView();
        m_View.ShowDestinationView();
    }

    public void AssignPOICategories(List<POICategory> categories) 
    {
        CategoryCell cell = GetCategoryCell();
        foreach(var elem in categories) 
        {
            GameObject cellGO = Instantiate(cell.gameObject, m_CategoryViewContents);
            CategoryCell cellView = cellGO.GetComponent<CategoryCell>();
            cellView.Initialize(elem);
            cellView.SetOnClickDelegate(()=> {
                if (m_SelectedCategory != null) {
                    POICategory currSelection = m_SelectedCategory.GetCategory();
                    // Unfilter list if current category was tapped
                    if (currSelection.labels == elem.labels) {
                        UnfilterPOIListByCategory(elem);
                        cellView.Deselect();

                        m_SelectedCategory = null;
                        return;
                    } 
                    // Unfilter current category & filter by new category
                    else {
                        UnfilterPOIListByCategory(currSelection);
                        m_SelectedCategory.Deselect();

                        FilterPOIListByCategory(elem);
                        cellView.Select();
                        m_SelectedCategory = cellView;
                    }
                }
                else {
                    FilterPOIListByCategory(elem);
                    cellView.Select();
                    m_SelectedCategory = cellView;
                }
            });
        }

        GameObject.Destroy(cell.gameObject);
    }

    public void AssignPOIList(List<LayerPOIItem> poiItems, System.Action<LayerPOIItem> loadNavigationEvent)
    {
        DestinationCellView defaultCell = GetScrollViewCell();
        // 조건에 해당하는 POI만 출력.
        var filteredPOIItems = poiItems.FindAll(
            e => 
                // dpcode로 필터링.
                // Cafe
                // e.dpcode == 110700 ||
                // e.dpcode == 111100 ||

                // // 이름으로 필터링.
                // e.name.Contains("화장실")
                e.usage > 0
        );

        filteredPOIItems.Sort((a, b) => a.name.CompareTo(b.name));

        foreach(var elem in filteredPOIItems)
        {
            GameObject cellGO = Instantiate(defaultCell.gameObject, m_ScrollViewContents);
            DestinationCellView cellView = cellGO.GetComponent<DestinationCellView>();
            Sprite icon = m_POIGrayIconInfo.GetSprite(elem.dpcode);

            cellView.Initialize(icon, elem);
            cellView.RegisterAction(()=> loadNavigationEvent(elem) );
        }

        GameObject.Destroy(defaultCell.gameObject);


        m_FullViewCellList.Clear();

        DestinationCellView fullViewCell = GetFullViewCell();

        foreach(var elem in poiItems)
        {
            GameObject cellGO = Instantiate(fullViewCell.gameObject, m_FullViewContents);
            DestinationCellView cellView = cellGO.GetComponent<DestinationCellView>();
            Sprite icon = m_POIGrayIconInfo.GetSprite(elem.dpcode);

            cellView.Initialize(icon, elem);
            cellView.RegisterAction(()=> loadNavigationEvent(elem) );

            m_FullViewCellList.Add(cellView);
        }

        SortPOIListByName();

        GameObject.Destroy(fullViewCell.gameObject);
    }

    private DestinationCellView GetScrollViewCell()
    {
        return m_ScrollViewContents.GetComponentInChildren<DestinationCellView>();
    }

    private DestinationCellView GetFullViewCell()
    {
        return m_FullViewContents.GetComponentInChildren<DestinationCellView>();
    }

    private CategoryCell GetCategoryCell()
    {
        return m_FullView.transform.GetComponentInChildren<CategoryCell>();
    }

    public void FilterPOIListByString(string filter)
    {
        m_StringFilter = filter;

        if (filter == "") {
            ResetPOIListFilter();
            if (m_SelectedCategory != null) {
                FilterPOIListByCategory(m_SelectedCategory.GetCategory());
            }
            return;
        }

        for(int i = 0; i < m_FullViewContents.childCount; i++) {
            GameObject child = m_FullViewContents.GetChild(i).gameObject;
            if (child.activeSelf) {
                DestinationCellView cellView = child.GetComponent<DestinationCellView>();

                if(cellView != null && !cellView.GetName().ToLowerInvariant().Contains(filter.ToLowerInvariant())) {
                    child.SetActive(false);
                }
            }
        }
    }

    public void UnfilterPOIListByString()
    {
        for(int i = 0; i < m_FullViewContents.childCount; i++) {
            GameObject child = m_FullViewContents.GetChild(i).gameObject;
            if (!child.activeSelf) {
                DestinationCellView cellView = child.GetComponent<DestinationCellView>();

                if(cellView != null) {
                    if (m_SelectedCategory != null && m_SelectedCategory.GetCategory().HasCode(cellView.GetCode())) {

                    } else {
                        child.SetActive(true);
                    }
                }
            }
        }
    }

    public void FilterPOIListByCategory(POICategory category)
    {
        for(int i = 0; i < m_FullViewContents.childCount; i++) {
            GameObject child = m_FullViewContents.GetChild(i).gameObject;
            if (child.activeSelf) {
                DestinationCellView cellView = child.GetComponent<DestinationCellView>();

                if(cellView != null && !category.HasCode(cellView.GetCode())) {
                    child.SetActive(false);
                }
            }
        }
    }

    public void ResetPOIListFilter() 
    {
        for(int i = 0; i < m_FullViewContents.childCount; i++) {
            GameObject child = m_FullViewContents.GetChild(i).gameObject;
            if (!child.activeSelf) {
                child.SetActive(true);
            }
        }
    }

    public void UnfilterPOIListByCategory(POICategory category)
    {
        for(int i = 0; i < m_FullViewContents.childCount; i++) {
            GameObject child = m_FullViewContents.GetChild(i).gameObject;
            if (!child.activeSelf) {
                DestinationCellView cellView = child.GetComponent<DestinationCellView>();

                if(cellView != null) {
                    if (m_StringFilter != "" && !cellView.GetName().Contains(m_StringFilter)) {
                        continue;
                    }

                    child.SetActive(true);
                }
            }
        }
    }


    // 현재 스테이지 설정. NearbyCalculator가 현재 층이 어디인지를 알 수 있도록 한다. 
    public void SetStage(string stageName)
    {
        m_NearbyCalculator.SetCurrStage(stageName);
    }

    /// <summary>
    ///   내주변 기반 거리순 정렬 여부 토글. Nearby 토글의 OnValueChanged(bool) 이벤트를 통해 실행한다.
    /// </summary>
    public void ToggleNearbySorting(bool isOn)
    {
        if(isOn)
        {
            SortPOIListByDistance();
        }
        else
        {
            SortPOIListByName();
        }
    }

    private void SortPOIListByDistance()
    {
        CalculateDistance();
        
        SortPOIList((a, b) => a.Distance.CompareTo(b.Distance));
        
        ShowDistanceText(m_FullViewCellList);
    }

    private void SortPOIListByName()
    {
        // m_FullViewCellList에 저장된 DestinationCellView을 이름순 정렬.
        SortPOIList((a, b) => a.Name.CompareTo(b.Name));

        HideDistanceText(m_FullViewCellList);
    }

    private void CalculateDistance()
    {
        // m_FullViewCellList에 저장된 DestinationCellView을 거리순 정렬.
        foreach(var cellView in m_FullViewCellList)
        {
            // 현재 위치부터 POI까지의 거리 계산.
            float dist = m_NearbyCalculator.CalculateDistance(cellView.poiItem);
            cellView.SetDistance(dist);
        }
    }

    private void SortPOIList(Comparison<DestinationCellView> comparison)
    {
        m_FullViewCellList.Sort(comparison);

        for(int i=0 ; i<m_FullViewCellList.Count ; i++)
        {
            m_FullViewCellList[i].transform.SetAsLastSibling();
        }
    }

    private void ShowDistanceText(List<DestinationCellView> cellViewList)
    {
        foreach(var cellView in cellViewList)
        {
            // 거리 텍스트 활성화.
            cellView.ShowDistanceText();
        }
    }

    private void HideDistanceText(List<DestinationCellView> cellViewList)
    {
        foreach(var cellView in cellViewList)
        {
            // 거리 텍스트 비활성화.
            cellView.HideDistanceText();
        }
    }
}
