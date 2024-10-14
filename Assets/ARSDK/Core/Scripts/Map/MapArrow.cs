using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class MapArrow : MonoBehaviour
    {
        private void Awake()
        {
            int layerIndex = LayerMask.NameToLayer("MapArrow");
            gameObject.layer = layerIndex;
        }
    }
}