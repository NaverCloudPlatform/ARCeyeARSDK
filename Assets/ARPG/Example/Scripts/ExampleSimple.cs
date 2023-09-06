using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ARCeye;

public class ExampleSimple : MonoBehaviour
{
    public Text m_StageName;


    public void OnStageChanged(string stageName)
    {
        m_StageName.text = $"Stage : {stageName}";
    }

    public void OnPOIList(List<LayerPOIItem> poiItems)
    {
        foreach(var item in poiItems)
        {
            Debug.Log($"{item.name}, {item.stageName}, {POIGenerator.ConvertToName(item.dpcode)}");
        }
    }
}
