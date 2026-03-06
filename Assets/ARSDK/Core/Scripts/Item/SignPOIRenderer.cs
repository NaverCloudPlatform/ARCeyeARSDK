using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public abstract class SignPOIRenderer : MonoBehaviour
    {
        abstract public void SetIcon(Sprite icon);
        abstract public void SetLabel(string content);
        abstract public void SetOpacity(float opacity);
        abstract public float GetOpacity();
    }
}