using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    [Serializable]
    public class StageResource
    {
        public string stage;
        public string layerInfo;
        public string ibl;
        public string mapModel;
        public string mapHeightField;
    }

    public class StageConfig : ScriptableObject
    {
        public List<StageResource> stages = new List<StageResource>();
    }
}
