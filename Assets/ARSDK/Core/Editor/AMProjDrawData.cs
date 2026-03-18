using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    /// <summary>
    /// amproj 시각화를 위한 드로잉 데이터 구조.
    /// </summary>
    internal static class AMProjDrawData
    {
        internal class Layer
        {
            public Color color;
            public List<Line> lines = new List<Line>();
            public List<POI> pois = new List<POI>();
        }

        internal class Line
        {
            public Vector3[] points;
            public bool loop;
        }

        internal class POI
        {
            public Vector3 position;
            public string displayText;
        }
    }
}
