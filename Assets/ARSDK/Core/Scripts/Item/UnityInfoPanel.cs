using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityInfoPanel : UnityModel
    {
        [SerializeField]
        private TextMesh m_Text;

        [SerializeField]
        private TextMesh m_TextBack;

        [SerializeField]
        private SpriteRenderer m_Header;

        [SerializeField]
        private SpriteRenderer m_Panel;

        [SerializeField]
        public Sprite[] m_PanelSprites = new Sprite[5];

        [SerializeField]
        public Material m_PanelMaterialAlt; // alternaitve material for Type 4

        [SerializeField]
        private SpriteRenderer m_Image;

        private Transform m_CameraTransform;

        const string k_InfoPanelImage = "ARPG/InfoPanelImage";
        private const float k_TextPanelScaleOffset = 0.14f;
        private const float k_ImagePanelScaleOffset = 0.125f;


        private InfoPanelType m_Type;
        private Texture2D m_InfoPanelHeaderTexture;
        private Texture2D InfoPanelHeaderTexture
        {
            get
            {
                if (m_InfoPanelHeaderTexture == null)
                {
                    m_InfoPanelHeaderTexture = new Texture2D(2, 2);
                }
                return m_InfoPanelHeaderTexture;
            }
        }

        private Texture2D m_InfoPanelFrameTexture;
        private Texture2D InfoPanelFrameTexture
        {
            get
            {
                if (m_InfoPanelFrameTexture == null)
                {
                    m_InfoPanelFrameTexture = new Texture2D(2, 2);
                }
                return m_InfoPanelFrameTexture;
            }
        }

        private Texture2D m_InfoPanelImageTexture;
        private Texture2D InfoPanelImageTexture
        {
            get
            {
                if (m_InfoPanelImageTexture == null)
                {
                    m_InfoPanelImageTexture = new Texture2D(2, 2);
                }
                return m_InfoPanelImageTexture;
            }
        }


        public string m_TextString;
        public bool m_UseFrame;
        public bool m_UseRoundedCorner;


        private void Awake()
        {
            if (ItemGenerator.Instance.font != null)
            {
                m_Text.font = ItemGenerator.Instance.font;
            }

            var meshRenderer = m_Text.GetComponent<MeshRenderer>();
            var meshRendererBack = m_TextBack.GetComponent<MeshRenderer>();

            Texture fontTexture = m_Text.font.material.mainTexture;

            meshRenderer.material = ItemGenerator.Instance.infoPanelTextMaterial;
            meshRenderer.sharedMaterial.mainTexture = fontTexture;

            meshRendererBack.material = ItemGenerator.Instance.infoPanelTextMaterial;
            meshRendererBack.sharedMaterial.mainTexture = fontTexture;
        }

        //스크립트를 시작하면 처음 한 번 실행됨.
        private void Start()
        {
            m_CameraTransform = Camera.main.transform;

            InitLayerInModel("ARItem");
        }

        private void OnDestroy()
        {
            if (m_InfoPanelHeaderTexture != null)
            {
                Destroy(m_InfoPanelHeaderTexture);
            }

            if (m_InfoPanelFrameTexture != null)
            {
                Destroy(m_InfoPanelFrameTexture);
            }

            if (m_InfoPanelImageTexture != null)
            {
                Destroy(m_InfoPanelImageTexture);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void UseFrame(bool value)
        {
            m_UseFrame = value;
        }

        public void UseRoundedCorner(bool value)
        {
            m_UseRoundedCorner = value;
        }

        public void SetInfoPanelType(int type)
        {
            m_Type = (InfoPanelType)type;

            SetPanelSpriteByType(m_Type);
            SetOpacity(0);
        }

        private void SetPanelSpriteByType(InfoPanelType type)
        {
            switch (type)
            {
                case InfoPanelType.Text:
                    m_Panel.sprite = m_PanelSprites[1];
                    m_Image.gameObject.SetActive(false);
                    break;
                case InfoPanelType.IndicatorText:
                    m_Panel.sprite = m_PanelSprites[2];
                    m_Image.gameObject.SetActive(false);
                    break;
                case InfoPanelType.CalloutSide:
                    m_Panel.sprite = m_PanelSprites[3];
                    m_Image.gameObject.SetActive(false);
                    break;
                case InfoPanelType.CalloutDown:
                    m_Panel.sprite = m_PanelSprites[4];
                    m_Panel.material = m_PanelMaterialAlt;
                    m_Image.gameObject.SetActive(false);
                    break;
                case InfoPanelType.Image:
                    m_Panel.sprite = m_PanelSprites[1];
                    m_Image.gameObject.SetActive(true);
                    break;
                case InfoPanelType.ImageTitle:
                    m_Panel.sprite = m_PanelSprites[1];
                    m_Image.gameObject.SetActive(true);
                    break;
                case InfoPanelType.ImageDescription:
                    m_Panel.sprite = m_PanelSprites[1];
                    m_Image.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        public void SetInfoPanelText(string text)
        {
            m_TextString = text;
            m_Text.text = m_TextString;

            UpdateGameObjectName(text);

            Vector3 originalScale = transform.localScale;
            transform.localScale = new Vector3(1, 1, 1);
            transform.parent.localPosition = transform.localPosition;
            transform.localPosition = new Vector3(0, 0, 0);

            UpdatePanelSize(m_Type);

            transform.localScale = originalScale;
        }

        private void UpdateGameObjectName(string text)
        {
            string textShort;
            if (text.Length > 6)
            {
                textShort = text.Substring(0, 6) + "..";
                transform.parent.name = $"ItemInfoPanel - {textShort}";
            }
            else if (text == "*")
            {
                m_TextString = "";
                transform.parent.name = $"ItemInfoPanel";
            }
            else
            {
                transform.parent.name = $"ItemInfoPanel - {text}";
            }
        }

        private void UpdatePanelSize(InfoPanelType type)
        {
            Vector3 textSize = GetTextMeshBoundSize();
            Vector2 panelSizeProcessed = new Vector2(1.0f, 1.0f);

            switch (type)
            {
                case InfoPanelType.Text:
                    {
                        m_Panel.transform.localScale = new Vector3(k_TextPanelScaleOffset, k_TextPanelScaleOffset, 1.0f);

                        Vector2 panelSizeMin = new Vector2(0.26f, 0.26f);
                        Vector2 panelSize = new Vector2(System.Math.Max(textSize.x + 0.2f, panelSizeMin.x), System.Math.Max(textSize.y + 0.13f, panelSizeMin.y));
                        panelSizeProcessed = panelSize / k_TextPanelScaleOffset; // offset needed for 9-slicing size

                        m_Text.transform.localPosition = new Vector3(0, 0, 0);
                        break;
                    }
                case InfoPanelType.IndicatorText:
                    {
                        m_Panel.transform.localScale = new Vector3(k_TextPanelScaleOffset, k_TextPanelScaleOffset, 1.0f);

                        Vector2 panelSizeMin = new Vector2(0.7f, 0.485f);
                        Vector2 panelSize = new Vector2(System.Math.Max(textSize.x + 0.2f, panelSizeMin.x), 0.485f);
                        panelSizeProcessed = panelSize / k_TextPanelScaleOffset; // offset needed for 9-slicing size

                        m_Text.transform.localPosition = new Vector3(0, 0, 0);
                        break;
                    }
                case InfoPanelType.CalloutSide:
                    {
                        m_Panel.transform.localScale = new Vector3(k_TextPanelScaleOffset, k_TextPanelScaleOffset, 1.0f);

                        Vector2 panelSizeMin = new Vector2(0.74f, 0.51f);
                        Vector2 panelSize = new Vector2(System.Math.Max(textSize.x + 0.2f, panelSizeMin.x), System.Math.Max(textSize.y + 0.41f, panelSizeMin.y));
                        panelSizeProcessed = panelSize / k_TextPanelScaleOffset; // offset needed for 9-slicing size

                        m_Text.transform.localPosition = new Vector3(0, 0.115f, 0);
                        m_TextBack.transform.localPosition = new Vector3(0, 0.115f, 0);

                        // move entire InfoPanel object to relocate anchor at the endpoint of speech bubble
                        gameObject.transform.localPosition += new Vector3(panelSize.x * 0.5f, panelSize.y * 0.5f, 0f);
                        break;
                    }
                case InfoPanelType.CalloutDown:
                    {
                        m_Panel.transform.localScale = new Vector3(k_TextPanelScaleOffset, k_TextPanelScaleOffset, 1.0f);

                        Vector2 panelSizeMin = new Vector2(0.74f, 0.51f);
                        Vector2 panelSize = new Vector2(System.Math.Max(textSize.x + 0.2f, panelSizeMin.x), System.Math.Max(textSize.y + 0.41f, panelSizeMin.y));
                        panelSizeProcessed = panelSize / k_TextPanelScaleOffset; // offset needed for 9-slicing size

                        m_Text.transform.localPosition = new Vector3(0, 0.115f, 0);
                        m_TextBack.transform.localPosition = new Vector3(0, 0.115f, 0);

                        // move entire InfoPanel object to relocate anchor at the endpoint of speech bubble
                        gameObject.transform.localPosition += new Vector3(0f, panelSize.y * 0.5f, 0f);

                        m_Panel.material.SetFloat("_Width", panelSize.x);
                        m_Panel.material.SetFloat("_Height", panelSize.y);
                        break;
                    }
                case InfoPanelType.Image:
                    {
                        m_Panel.transform.localScale = new Vector3(k_ImagePanelScaleOffset, k_ImagePanelScaleOffset, 1.0f);

                        // decide panel height from text size
                        float panelHeight = 0.2f;
                        if (!System.String.IsNullOrEmpty(m_TextString))
                            panelHeight += textSize.y;
                        Vector2 panelSize = new Vector2(1.5f, panelHeight);

                        panelSizeProcessed = panelSize / k_ImagePanelScaleOffset; // offset needed for 9-slicing size
                        break;
                    }
                case InfoPanelType.ImageTitle:
                    {
                        m_Panel.transform.localScale = new Vector3(k_ImagePanelScaleOffset, k_ImagePanelScaleOffset, 1.0f);

                        // decide panel height from text size
                        float panelHeight = 0.2f;
                        if (!System.String.IsNullOrEmpty(m_TextString))
                            panelHeight += textSize.y;
                        Vector2 panelSize = new Vector2(1.5f, panelHeight);

                        panelSizeProcessed = panelSize / k_ImagePanelScaleOffset; // offset needed for 9-slicing size
                        break;
                    }
                case InfoPanelType.ImageDescription:
                    {
                        m_Panel.transform.localScale = new Vector3(k_ImagePanelScaleOffset, k_ImagePanelScaleOffset, 1.0f);

                        Vector2 imageSize = new Vector2(0.75f, 1.0f);

                        float panelHeight = imageSize.y + 0.2f;
                        float panelWidth = imageSize.x + 0.2f + textSize.x + 0.1f;
                        Vector2 panelSize = new Vector2(panelWidth, panelHeight);
                        panelSizeProcessed = panelSize / k_ImagePanelScaleOffset; // offset needed for 9-slicing size
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            ActivateBackSideText(type);

            m_Panel.size = panelSizeProcessed;
        }

        /// TextMesh 영역의 외곽 크기를 계산.
        /// meshRenderer.bounds를 사용할 경우 전역좌표계에서의 크기를 반환하기 때문에 회전이 적용된 상태에서의 크기를 계산하지 못한다.
        /// localBounds와 localScale을 이용하여 TextMesh의 크기를 직접 계산.
        private Vector3 GetTextMeshBoundSize()
        {
            var meshRenderer = m_Text.GetComponent<MeshRenderer>();

            var localScale = meshRenderer.transform.localScale;
            var localBoundSize = meshRenderer.localBounds.size;

            float textBoundX = localBoundSize.x * localScale.x;
            float textBoundY = localBoundSize.y * localScale.y;

            Vector3 textSize = new Vector3(textBoundX, textBoundY, 1.0f);
            return textSize;
        }

        private void ActivateFrame(bool value)
        {
            if (m_Type == InfoPanelType.Image ||
               m_Type == InfoPanelType.ImageTitle ||
               m_Type == InfoPanelType.ImageDescription)
            {
                m_Panel.gameObject.SetActive(value);
            }
            else
            {
                // Text, Callout 등은 기본 디자인을 사용. 
                m_Panel.gameObject.SetActive(true);
            }
        }

        private void ActivateRoundedCorner(bool value)
        {
            m_Image.material.SetFloat("_UseRoundedCorner", value ? 1.0f : 0.0f);
        }

        private void ActivateBackSideText(InfoPanelType type)
        {
            if (type == InfoPanelType.IndicatorText || m_Type == InfoPanelType.CalloutSide)
            {
                m_TextBack.text = m_TextString;
                m_TextBack.gameObject.SetActive(true);
            }
            else
            {
                m_TextBack.gameObject.SetActive(false);
            }
        }

        public void SetInfoPanelHeader(string imagePath)
        {
            StartCoroutine(LoadImageBuffer(imagePath, CreateInfoPanelHeader));
        }

        public void SetInfoPanelFrame(string imagePath)
        {
            StartCoroutine(LoadImageBuffer(imagePath, CreateInfoPanelFrame));
        }

        public void SetInfoPanelImage(string imagePath)
        {
            StartCoroutine(LoadImageBuffer(imagePath, CreateInfoPanelImage));
        }

        private IEnumerator LoadImageBuffer(string path, System.Action<byte[]> callback)
        {
            if (string.IsNullOrEmpty(path))
            {
                NativeLogger.Print(LogLevel.ERROR, "[UnityInfoPanel] 이미지 경로가 비어있음");
                yield break;
            }

            byte[] bytes;

            if (path.Contains("://") || path.Contains(":///"))
            {
                path = path.Trim('/');

                using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(path))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        NativeLogger.Print(LogLevel.ERROR, "Failed to load the file: " + www.error);
                        yield break;
                    }

                    bytes = www.downloadHandler.data;
                    callback?.Invoke(bytes);
                }
            }
            else
            {
                bytes = System.IO.File.ReadAllBytes(path);
                callback?.Invoke(bytes);
            }
        }

        private void CreateInfoPanelHeader(byte[] imageBytes)
        {
            Texture2D texture = InfoPanelHeaderTexture;
            texture.LoadImage(imageBytes);

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Sprite panelSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            m_Header.sprite = panelSprite;

            float panelWidth = texture.width * 0.0115f;
            float panelHeight = texture.height * 0.0115f;
            m_Header.size = new Vector2(panelWidth, panelHeight);

            m_Header.drawMode = SpriteDrawMode.Simple;
        }

        private void CreateInfoPanelFrame(byte[] imageBytes)
        {
            Texture2D texture = InfoPanelFrameTexture;
            texture.LoadImage(imageBytes);

            // 256px 이미지에 border는 90px을 기준으로 계산.
            // 다른 크기의 이미지들에 대해서도 비율에 맞게 보이도록 구현.
            float sourceBorder = 100.0f;
            float sourceWidth = 256.0f;
            float sourceHeight = 256.0f;
            float ratioWidth = sourceBorder / sourceWidth;
            float ratioHeight = sourceBorder / sourceHeight;

            float targetBorderWidth = texture.width * ratioWidth;
            float targetBorderHeight = texture.height * ratioHeight;

            float pixelsPerUnitRatio = texture.width / 256.0f;
            float pixelsPerUnit = 100 * pixelsPerUnitRatio;

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector4 border = new Vector4(targetBorderWidth, targetBorderHeight, targetBorderWidth, targetBorderHeight);
            Sprite panelSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 1, SpriteMeshType.Tight, border, true);
            m_Panel.sprite = panelSprite;

            m_Panel.drawMode = SpriteDrawMode.Sliced;
        }

        private void CreateInfoPanelImage(byte[] imageBytes)
        {
            Texture2D texture = InfoPanelImageTexture;
            texture.LoadImage(imageBytes);

            Rect rect = new Rect(0, 0, texture.width, texture.height);

            var imageHeight = 1.3f * texture.height / texture.width;
            m_Image.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f)); //마지막 인자가 위치 조정
            m_Image.GetComponent<SpriteRenderer>().size = new Vector2(1.3f, imageHeight);

            Shader shader = Shader.Find(k_InfoPanelImage);
            Material material = new Material(shader);
            m_Image.material = material;

            if (m_Type == InfoPanelType.ImageDescription)
            {
                // move text and image position
                if (!System.String.IsNullOrEmpty(m_TextString))
                {
                    m_Text.transform.localPosition -= new Vector3((0.1f + 0.75f) * 0.5f, 0.0f, 0.0f);
                    m_Image.transform.localPosition += new Vector3((0.1f + m_Text.GetComponent<MeshRenderer>().bounds.size.x) * 0.5f, 0.0f, 0.0f);
                    m_Image.GetComponent<SpriteRenderer>().size = new Vector2(0.75f, 1.0f);
                }

                m_Image.material.SetFloat("_Width", m_Image.GetComponent<SpriteRenderer>().size.x * 100f);
                m_Image.material.SetFloat("_Height", m_Image.GetComponent<SpriteRenderer>().size.y * 100f);
            }
            else
            {
                // update panel height from image size
                var scaleOffset = m_Panel.transform.localScale.x;
                m_Panel.size += new Vector2(0.0f, imageHeight / scaleOffset);

                // move text and image position
                if (!System.String.IsNullOrEmpty(m_TextString))
                {
                    m_Panel.size += new Vector2(0.0f, 0.1f / scaleOffset);

                    m_Text.transform.localPosition += new Vector3(0.0f, (0.1f + imageHeight) * 0.5f, 0.0f);
                    m_Image.transform.localPosition -= new Vector3(0.0f, (0.1f + m_Text.GetComponent<MeshRenderer>().bounds.size.y) * 0.5f, 0.0f);
                }

                m_Image.material.SetFloat("_Width", m_Image.GetComponent<SpriteRenderer>().size.x * 100f);
                m_Image.material.SetFloat("_Height", m_Image.GetComponent<SpriteRenderer>().size.y * 100f);
            }

            m_Image.drawMode = SpriteDrawMode.Simple;

            // Panel 내의 Image가 어떤것이 들어가는지 결정이 되어야 Panel의 최종 크기를 알 수 있다. 
            UpdateHeaderPosition();

            // 변경된 Material에 대한 설정 적용.
            ActivateFrame(m_UseFrame);
            ActivateRoundedCorner(m_UseRoundedCorner);
        }

        private void UpdateHeaderPosition()
        {
            Vector2 panelSize = m_Panel.size;
            Vector3 headerPosition = m_Header.transform.localPosition;
            float headerHeight = m_Header.size.y;
            float space = 0.2f;

            headerPosition.y = panelSize.y / 2.0f + headerHeight / 2.0f + space;

            m_Header.transform.localPosition = headerPosition;
        }

        public override void SetOpacity(float opacity)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            opacity = Mathf.Clamp(opacity, 0.0f, 1.0f);

            if (m_Panel.material.shader.name == "ARPG/InfoPanelPanel")
            {
                m_Panel.material.SetFloat("_Alpha", opacity);
            }
            else
            {
                var color = m_Panel.material.color;
                m_Panel.material.color = new Color(color.r, color.g, color.b, opacity);
            }

            m_Image.material.SetFloat("_Alpha", opacity);

            // Header opacity 설정.
            var headerColor = m_Header.material.color;
            headerColor.a = opacity;
            m_Header.material.color = headerColor;

            // Text opacity 설정.
            Color textCol = m_Text.GetComponent<MeshRenderer>().material.GetColor("_Color");
            textCol.a = opacity;
            m_Text.GetComponent<MeshRenderer>().material.SetColor("_Color", textCol);
        }

        // UnityModel에서 사용하는 glb fade와 다른 방식으로 동작. new 키워드로 hiding.
        public override void Fade(float duration, bool fadeIn, System.Action onComplete = null)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            StartCoroutine(FadeInternal(duration, fadeIn, onComplete));
        }

        public override void SetPickable(bool value)
        {
            if (!value)
            {
                Transform collider = transform.Find("Collider");
                if (collider != null)
                {
                    collider.gameObject.SetActive(false);
                }
            }
            else
            {
                Transform collider = transform.Find("Collider");
                if (collider != null)
                {
                    collider.gameObject.SetActive(true);
                }

                // Create collider for all children meshes.
                else
                {
                    if (m_Panel != null)
                    {
                        GameObject go = new GameObject("Collider");
                        go.transform.parent = transform;

                        go.transform.localScale = m_Panel.gameObject.transform.localScale;
                        go.transform.localPosition = new Vector3(0, 0, 0);
                        go.transform.localRotation = Quaternion.identity;

                        BoxCollider boxCollider = go.AddComponent<BoxCollider>();

                        boxCollider.size = new Vector3(m_Panel.size.x, m_Panel.size.y, 0.0f);
                    }
                }
            }
        }

        private IEnumerator FadeInternal(float duration, bool fadeIn, System.Action onComplete = null)
        {
            bool isCustomShader = m_Panel.material.shader.name == "ARPG/InfoPanelPanel";

            float start = 0.0f;
            if (isCustomShader)
            {
                start = m_Panel.material.GetFloat("_Alpha");
            }
            else
            {
                start = m_Panel.material.color.a;
            }

            float end = fadeIn ? 1 : 0;

            var textMat = m_Text.GetComponent<MeshRenderer>().material;
            Color textColor = textMat.GetColor("_Color");

            Color currColor = new Color();
            if (!isCustomShader)
            {
                currColor.r = m_Panel.material.color.r;
                currColor.g = m_Panel.material.color.g;
                currColor.b = m_Panel.material.color.b;
                currColor.a = start;
            }

            bool isFinished = false;
            float accumTime = 0;

            while (!isFinished)
            {
                float t = accumTime / duration;

                // Alpha interpolation
                float a = Mathf.Lerp(start, end, t);

                if (isCustomShader)
                {
                    m_Panel.material.SetFloat("_Alpha", a);
                }
                else
                {
                    currColor.a = a;
                    m_Panel.material.color = currColor;
                }

                m_Panel.material.SetFloat("_Alpha", a);
                m_Image.material.SetFloat("_Alpha", a);
                m_Header.material.color = currColor;

                textColor.a = a;
                textMat.SetColor("_Color", textColor);

                yield return null;

                accumTime += Time.deltaTime;

                if (accumTime >= duration)
                {
                    isFinished = true;
                }
            }

            m_Panel.material.SetFloat("_Alpha", end);
            m_Image.material.SetFloat("_Alpha", end);

            textColor.a = end;
            textMat.SetColor("_Color", textColor);

            if (onComplete != null)
            {
                onComplete.Invoke();
            }
        }
    }
}
