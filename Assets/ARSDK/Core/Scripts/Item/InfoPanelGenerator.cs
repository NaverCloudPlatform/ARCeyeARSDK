using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPanelGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject m_InfoPanelPrefab;

    public void Start()
    {
    }

    public GameObject GenerateInfoPanel()
    {
        return Instantiate(m_InfoPanelPrefab);
    }
}
