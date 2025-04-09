using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye
{
    public class ARSDKExample : MonoBehaviour
    {
        [SerializeField]
        private ARPlayGround m_ARPlayGround;

        [SerializeField]
        private MapCameraRig m_MapCameraRig;

        [SerializeField]
        private AMProjStageReader m_AMProjStageReader;


        [Header("Stage")]
        // Stage Example.
        [SerializeField]
        private GameObject m_StageArea;
        [SerializeField]
        private Dropdown m_StageDropdown;

        [SerializeField]
        private InputField m_LayerInfoInput;

        [SerializeField]
        private Text m_StageName;

        [SerializeField]
        private Text m_StageLabel;
        

        [Header("Map")]
        // Map Example.
        [SerializeField]
        private GameObject m_MapArea;
        [SerializeField]
        private MapScreen m_MapScreenMinimap;
        [SerializeField]
        private MapScreen m_MapScreenFullMap;
        

        [Header("POI")]
        // POI Example
        [SerializeField]
        private GameObject m_POIArea;
        [SerializeField]
        private Text m_POICount;
        [SerializeField]
        private Dropdown m_POIDropdown;

        [Header("Navigation")]
        // Navigation Example
        [SerializeField]
        private GameObject m_NavigationArea;
        [SerializeField]
        private Text m_SelectedPOIName;
        [SerializeField]
        private Text m_NavigationStatus;
        [SerializeField]
        private Text m_RemainDist;
        [SerializeField]
        private Text m_TransitStatus;

        [Header("misc.")]
        // misc. Example
        [SerializeField]
        private GameObject m_MiscArea;
        [SerializeField]
        private Text m_CustomRangeStatus;
        [SerializeField]
        private Button m_ShowNextStepButton;
        [SerializeField]
        private Button m_ShowARItemButton;

        private List<LayerPOIItem> m_LayerPOIItemList;
        private LayerPOIItem m_SelectedPOIItem;
        private PathFindingType m_PathFindingType = PathFindingType.Default;



        private void Awake()
        {
            Application.targetFrameRate = 60;

            m_AMProjStageReader.Load(m_ARPlayGround.amprojFilePath, stages => {
                GenerateStagesDropdown(stages.Keys.ToList());
            });

            ShowStageArea();
        }

        private void GenerateStagesDropdown(List<string> stages)
        {
            m_StageDropdown.ClearOptions();
            m_StageDropdown.AddOptions(stages);
        }


        /// Stage ///
        
        public void ResetARPlayGround()
        {
            m_ARPlayGround.Reset();
        }
        
        public void OnStageChanged(string name, string label)
        {
            Debug.Log($"Stage Changed: {name} - {label}");
            m_StageName.text = name;
            m_StageLabel.text = label;
        }

        public void TryUpdateStageEvent()
        {
            string stageName = m_StageDropdown.options[m_StageDropdown.value].text;
            m_ARPlayGround.TryUpdateStage(stageName);
        }

        public void ForceUpdateStageEvent()
        {
            string stageName = m_StageDropdown.options[m_StageDropdown.value].text;
            m_ARPlayGround.ForceUpdateStage(stageName);
        }

        public void SetLayerInfoEvent()
        {
            string layerInfo = m_LayerInfoInput.text;
            m_ARPlayGround.SetLayerInfo(layerInfo);
        }

        public void ForceLayerInfoEvent()
        {
            string layerInfo = m_LayerInfoInput.text;
            m_ARPlayGround.SetLayerInfo(layerInfo, forceUpdate: true);
        }


        /// Map ///
        
        public void ShowMinimap()
        {
            UnityMapPOIPool mapPOIPool = FindObjectOfType<UnityMapPOIPool>();
            mapPOIPool.ActivateMinimapMode();
            m_MapCameraRig.ChangeToFullMap(false);
            m_MapScreenMinimap.Activate(true);
            m_MapScreenFullMap.Activate(false);
        }

        public void ShowFullMap()
        {
            UnityMapPOIPool mapPOIPool = FindObjectOfType<UnityMapPOIPool>();
            mapPOIPool.ActivateFullmapMode();
            m_MapCameraRig.ChangeToFullMap(true);
            m_MapScreenMinimap.Activate(false);
            m_MapScreenFullMap.Activate(true);
        }

        public void HideMap()
        {
            m_MapScreenMinimap.Activate(false);
            m_MapScreenFullMap.Activate(false);
        }


        /// POI ///

        public void OnPOIList(List<LayerPOIItem> poiItems)
        {
            Debug.Log($"POI List: {poiItems.Count}");
            m_POICount.text = poiItems.Count.ToString();

            // Filter POI Items
            var filterdPOIItems = poiItems.FindAll(
            e => 
                // Cafe
                e.dpcode == 110700 ||

                // Toliet
                e.dpcode == 133000 ||

                // Chair
                e.dpcode == 133138 ||

                // Conference Hall.
                e.name.Contains("Conference Hall")
            );

            // Sort POI Items by name.
            filterdPOIItems.Sort((LayerPOIItem a, LayerPOIItem b) => a.name.CompareTo(b.name));

            m_POIDropdown.ClearOptions();
            List<string> poiNames = filterdPOIItems.Select(poi => poi.ToString()).ToList();
            m_POIDropdown.AddOptions(poiNames);

            m_LayerPOIItemList = filterdPOIItems;
        }

        public void OnPOIItemSelected(int index)
        {
            m_SelectedPOIItem = m_LayerPOIItemList[index];
            m_SelectedPOIName.text = $"{m_SelectedPOIItem.name}, {m_SelectedPOIItem.stageName}";
        }


        /// Navigation ///
        
        public void LoadNavigtionEvent()
        {
            m_ARPlayGround.LoadNavigation(m_SelectedPOIItem, m_PathFindingType);
        }

        public void UnloadNavigationEvent()
        {
            m_ARPlayGround.UnloadNavigation();
        }

        public void TransitTypeSelected(int index)
        {
            m_PathFindingType = (PathFindingType)index;
        }


        public void OnNavigationStarted()
        {
            Debug.Log("Navigation is started");
            m_NavigationStatus.text = "Navigation Started";
        }

        public void OnNavigationEnded()
        {
            Debug.Log("Navigation is ended");
            m_NavigationStatus.text = "Around Mode";
        }

        public void OnNavigationFailed()
        {
            Debug.Log("Navigation is failed");
            m_NavigationStatus.text = "Navigation Failed";
        }

        public void OnNavigationRerouted()
        {
            Debug.Log("Navigation is re-routed");
        }

        public void OnDistanceUpdated(float distance)
        {
            m_RemainDist.text = $"{distance.ToString("N1")}m";
        }

        public void OnDestinationArrived()
        {
            Debug.Log("Destination Arrived");
            m_NavigationStatus.text = "Destination Arrived";
        }

        public void OnTransitMovingStarted(ConnectionType transitType, string destStageName, string destStageLabel)
        {
            Debug.Log($"Transit Moving Started: {transitType}, {destStageName}, {destStageLabel}");
            m_TransitStatus.text = $"Transit Moving Started: {transitType}, {destStageName}, {destStageLabel}";
        }

        public void OnTransitMovingEnded()
        {
            Debug.Log("Transit Moving Ended");
            m_TransitStatus.text = "Transit Moving Ended";
        }

        public void OnTransitMovingFailed(string detectedStageName, string destinationStageName)
        {
            Debug.Log($"Transit Moving Failed: detected - {detectedStageName}, destination - {destinationStageName}");
            m_TransitStatus.text = $"Transit Moving Failed: detected - {detectedStageName}, destination - {destinationStageName}";
        }


        /// misc. ///
        
        public void OnCustomRangeEntered(string uuid, string name)
        {
            Debug.Log($"Custom Range Entered: {uuid}, {name}");
            m_CustomRangeStatus.text = $"Entered: {uuid}, {name}";
        }

        public void OnCustomRangeExited(string uuid, string name)
        {
            Debug.Log($"Custom Range Exited: {uuid}, {name}");
            m_CustomRangeStatus.text = $"Exited: {uuid}, {name}";
        }

        public void ShowNextStep()
        {
            string text = m_ShowNextStepButton.GetComponentInChildren<Text>().text;

            if(text == "Show NextStep")
            {
                m_ARPlayGround.ActivateNextStep(true);
                m_ShowNextStepButton.GetComponentInChildren<Text>().text = "Hide NextStep";
            }
            else
            {
                m_ARPlayGround.ActivateNextStep(false);
                m_ShowNextStepButton.GetComponentInChildren<Text>().text = "Show NextStep";
            }
        }

        public void ShowARItems()
        {
            string text = m_ShowARItemButton.GetComponentInChildren<Text>().text;

            if(text == "Show ARItem")
            {
                m_ARPlayGround.ShowARItems();
                m_ShowARItemButton.GetComponentInChildren<Text>().text = "Hide ARItem";
            }
            else
            {
                m_ARPlayGround.HideARItems();
                m_ShowARItemButton.GetComponentInChildren<Text>().text = "Show ARItem";
            }
        }


        /// Button Actions ///
        
        public void ShowStageArea()
        {
            m_StageArea.SetActive(true);
            m_MapArea.SetActive(false);
            m_POIArea.SetActive(false);
            m_NavigationArea.SetActive(false);
            m_MiscArea.SetActive(false);
        }

        public void ShowMapArea()
        {
            m_StageArea.SetActive(false);
            m_MapArea.SetActive(true);
            m_POIArea.SetActive(false);
            m_NavigationArea.SetActive(false);
            m_MiscArea.SetActive(false);
        }

        public void ShowPOIArea()
        {
            m_StageArea.SetActive(false);
            m_MapArea.SetActive(false);
            m_POIArea.SetActive(true);
            m_NavigationArea.SetActive(false);
            m_MiscArea.SetActive(false);
        }

        public void ShowNavigationArea()
        {
            m_StageArea.SetActive(false);
            m_MapArea.SetActive(false);
            m_POIArea.SetActive(false);
            m_NavigationArea.SetActive(true);
            m_MiscArea.SetActive(false);
        }

        public void ShowMiscArea()
        {
            m_StageArea.SetActive(false);
            m_MapArea.SetActive(false);
            m_POIArea.SetActive(false);
            m_NavigationArea.SetActive(false);
            m_MiscArea.SetActive(true);
        }
    }
}