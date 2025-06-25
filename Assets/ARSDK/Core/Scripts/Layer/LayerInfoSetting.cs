using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class Layer
    {
        private LayerData m_LayerData = new LayerData();
        public LayerData data => m_LayerData;

        public string layerName
        {
            get => m_LayerData.layerName;
            set => m_LayerData.layerName = value;
        }

        public bool linkToStage
        {
            get => m_LayerData.linkToStage;
            set => m_LayerData.linkToStage = value;
        }

        public string stageName
        {
            get => m_LayerData.stageName;
            set => m_LayerData.stageName = value;
        }

        public int depth
        {
            get => m_LayerData.depth;
            set => m_LayerData.depth = value;
        }

        public Layer parent
        {
            get => m_LayerData.parent;
            set => m_LayerData.parent = value;
        }

        public string layerInfoCode
        {
            get => m_LayerData.layerInfoCode;
            set => m_LayerData.layerInfoCode = value;
        }

        public bool foldout
        {
            get => m_LayerData.foldout;
            set => m_LayerData.foldout = value;
        }

        public bool isRemoved
        {
            get => m_LayerData.isRemoved;
            set => m_LayerData.isRemoved = value;
        }

        public List<Layer> subLayers;

        public Layer(LayerData data)
        {
            this.subLayers = new List<Layer>();
            this.m_LayerData = data;
        }

        public Layer(int depth)
        {
            this.m_LayerData = new LayerData();

            this.depth = depth;
            this.subLayers = new List<Layer>();
            this.isRemoved = false;

            this.foldout = true;
        }

        public void AddChild(Layer layer)
        {
            subLayers.Add(layer);
        }
    }

    [Serializable]
    public class LayerData
    {
        public string layerName;
        public bool linkToStage;
        public string stageName;
        public int depth;

        [HideInInspector]
        public Layer parent;
        [HideInInspector]
        public string layerInfoCode;

        // Editor features.
        public bool foldout;
        public bool isRemoved;
    }

    [Serializable]
    public class SerializedLayer
    {
        public LayerData data;
        public int childCount;

        public SerializedLayer(Layer layer)
        {
            this.data = layer.data;
            this.childCount = layer.subLayers.Count;
        }

        public Layer Deserialize()
        {
            return new Layer(data);
        }
    }

    [Serializable]
    public class SerializedLayerTree
    {
        public List<SerializedLayer> layerList;

        public SerializedLayerTree()
        {
            layerList = new List<SerializedLayer>();
        }

        public SerializedLayerTree(Layer layer) : this()
        {
            SerializeFromLayer(layer);
        }

        public void SerializeFromLayer(Layer layer)
        {
            layerList.Clear();
            SerializeAll(layer);
        }

        private void SerializeAll(Layer layer)
        {
            layerList.Add(new SerializedLayer(layer));
            foreach (var child in layer.subLayers)
            {
                SerializeAll(child);
            }
        }

        public Layer Deserialize()
        {
            if (layerList.Count == 0)
                return null;

            int index = 0;
            Layer root = DeserializeAll(ref index);
            return root;
        }

        private Layer DeserializeAll(ref int index)
        {
            int currentIndex = index;
            Layer current = layerList[currentIndex].Deserialize();

            index++;
            for (int i = 0; i < layerList[currentIndex].childCount; i++)
            {
                current.AddChild(DeserializeAll(ref index));
            }

            return current;
        }

        public bool HasData()
        {
            return layerList.Count > 0;
        }
    }

    [Serializable]
    public class LayerInfoSetting : ScriptableObject
    {
        [field: SerializeField, Tooltip("Layer hierarchy registered in the ARC eye console")]
        private SerializedLayerTree m_LayerTree = new SerializedLayerTree(new Layer(0));
        public SerializedLayerTree layerTree => m_LayerTree;

        public Layer layer => m_LayerTree.Deserialize();
    }
}