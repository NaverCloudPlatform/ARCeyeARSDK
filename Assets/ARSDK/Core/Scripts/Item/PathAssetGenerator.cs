using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class PathAssetGenerator : MonoBehaviour
    {
        [SerializeField]
        private Texture m_Arrow;

        [SerializeField]
        private Texture m_Path;

        private Material m_PathMaterial;
        public Material PathMaterial
        {
            get
            {
                if (m_PathMaterial == null)
                {
                    m_PathMaterial = new Material(Shader.Find("ARPG/UnlitCustomDepth"));
                    m_PathMaterial.mainTexture = m_Path;
                    m_PathMaterial.SetTexture("_MainTex", m_Path);
                    m_PathMaterial.renderQueue = 2800;
                }

                return m_PathMaterial;
            }
        }

        [SerializeField]
        private Texture m_BeginBullet;
        public Texture BeginBullet => m_BeginBullet;

        [SerializeField]
        private Texture m_EndBullet;
        public Texture EndBullet => m_EndBullet;

        [SerializeField]
        private float m_PathWidth = 1.0f;
        public float PathWidth => m_PathWidth;


        private void Awake()
        {
            CheckTexturesAssigned();
            AssignTextureToMapArrow();
        }

        private void CheckTexturesAssigned()
        {
            if (m_Arrow == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[PathAssetGenerator] Arrow texture is not assigned.");
            }

            if (m_Path == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[PathAssetGenerator] Path texture is not assigned.");
            }

            if (m_BeginBullet == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[PathAssetGenerator] BeginBullet texture is not assigned.");
            }

            if (m_EndBullet == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[PathAssetGenerator] EndBullet texture is not assigned.");
            }
        }

        private void AssignTextureToMapArrow()
        {
            if (m_Arrow == null)
            {
                NativeLogger.Print(LogLevel.WARNING, "[PathAssetGenerator] Arrow texture is not assigned. Skipping MapArrow texture assignment.");
                return;
            }

            var mapArrow = GameObject.FindObjectOfType<MapArrow>();
            if (mapArrow == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[PathAssetGenerator] MapArrow not found in the scene.");
                return;
            }

            var renderer = mapArrow.GetComponent<MeshRenderer>();
            var material = renderer.material;
            material.mainTexture = m_Arrow;
        }

        public GameObject GenerateBullet(string name, Texture bullet)
        {
            GameObject bulletObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bulletObject.name = name;
            bulletObject.layer = LayerMask.NameToLayer("Map");

            var renderer = bulletObject.GetComponent<MeshRenderer>();
            var shader = Shader.Find("ARPG/UnlitCustomDepth");

            renderer.material = new Material(shader);
            renderer.material.mainTexture = bullet;

            var transform = bulletObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(90, 0, 0);
            transform.localScale = new Vector3(2.0f * m_PathWidth, 2.0f * m_PathWidth, 2.0f * m_PathWidth);

            return bulletObject;
        }
    }
}