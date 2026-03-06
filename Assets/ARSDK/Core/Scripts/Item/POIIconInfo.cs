using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace ARCeye
{
    [Serializable]
    public class POIIconInfoItem
    {
        public int dpCode;
        public Sprite icon;
    }

    [Serializable]
    public class POIIconInfo : ScriptableObject
    {
        [SerializeField]
        private SpriteAtlas m_POIAtlas;

        [SerializeField]
        private Sprite m_DefaultIcon;

        [SerializeField]
        private List<POIIconInfoItem> m_POIInfoList;

        public Sprite GetSprite(int dpCode)
        {
            POIIconInfoItem item = m_POIInfoList.Find(e => e.dpCode == dpCode);

            if (item != null)
            {
                string iconName = item.icon.name;
                return m_POIAtlas.GetSprite(iconName);
            }
            else
            {
                return m_DefaultIcon;
            }
        }

#if UNITY_EDITOR
        [SerializeField]
        private UnityEngine.Object m_POIIconDirectory;

        public UnityEngine.Object POIIconDirectory
        {
            get { return m_POIIconDirectory; }
            set { m_POIIconDirectory = value; }
        }

        public List<POIIconInfoItem> POIInfoList
        {
            get { return m_POIInfoList; }
        }
#endif
    }
}