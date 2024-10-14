using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye 
{
public class MultiMediaGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject m_VideoPrefab;
    [SerializeField]
    private GameObject m_AudioPrefab;

    public void Start() {
    }

    public GameObject GenerateVideo() {
        return Instantiate(m_VideoPrefab);
    }

    public GameObject GenerateAudio() {
        return Instantiate(m_AudioPrefab);
    }
}
}
