using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public abstract class TurnSpotRenderer : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void SetOpacity(float opacity);
        public abstract void SetDistance(int distance);
    }
}