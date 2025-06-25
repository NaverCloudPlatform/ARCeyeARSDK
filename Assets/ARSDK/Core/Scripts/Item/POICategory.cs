using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    [System.Serializable]
    public struct Translation
    {
        public Locale locale;
        public string text;
    }

    [Serializable]
    public class POICategory
    {
        public List<Translation> labels;
        public List<int> dpcode;

        public bool HasCode(int code)
        {
            foreach (int elem in dpcode)
            {
                if (elem == code)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetLabelByLocale(Locale locale)
        {
            Translation pair = labels.Find(item => item.locale == locale);
            return pair.text;
        }
    }
}