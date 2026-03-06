using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public abstract class MapPOIRenderer : MonoBehaviour
    {
        abstract public void SetIcon(Sprite icon);
        abstract public void SetLabel(string content);
        abstract public void SetFontSize(float fontSize);
        abstract public void ShowText(bool value);
        abstract public void ShowIcon(bool value);
    }
}