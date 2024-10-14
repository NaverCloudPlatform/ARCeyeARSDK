using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye
{
    public class UnityInfoPanel : UnityModel
    {
        [SerializeField]
        private TextMesh m_Text;

        [SerializeField]
        private TextMesh m_TextBack;

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

        public int  m_Type;
        public string m_TextString;
        public bool m_UseFrame;
        public bool m_UseRoundedCorner;


        private void Awake() {
            if(ItemGenerator.Instance.font != null) {
                m_Text.font = ItemGenerator.Instance.font;
            }

            var meshRenderer     = m_Text.GetComponent<MeshRenderer>();
            var meshRendererBack = m_TextBack.GetComponent<MeshRenderer>();
            
            Texture fontTexture = m_Text.font.material.mainTexture;

            meshRenderer.material = ItemGenerator.Instance.infoPanelTextMaterial;
            meshRenderer.sharedMaterial.mainTexture = fontTexture;

            meshRendererBack.material = ItemGenerator.Instance.infoPanelTextMaterial;
            meshRendererBack.sharedMaterial.mainTexture = fontTexture;
        }

        //스크립트를 시작하면 처음 한 번 실행됨.
        private void Start() { 
            m_CameraTransform = Camera.main.transform;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void SetInfoPanelType(int type)
        {
            m_Type = type;

            //TODO: 타입에 맞게 크기 설정하기

            // 타입에 맞게 백그라운드 이미지 설정하기

            SpriteRenderer sr = m_Panel.GetComponent<SpriteRenderer>();
            if(type ==200)
            {
                sr.sprite = m_PanelSprites[0];
            }
            else if(type==201) // Type 1. Text
            {
                sr.sprite = m_PanelSprites[1];
            }
            else if(type==202) // Type 2. Indicator Text
            {
                sr.sprite = m_PanelSprites[2];
            }
            else if(type==203) // Type 3. Callout Side
            {
                sr.sprite = m_PanelSprites[3];
            }
            else if(type==204) // Type 4. callout Down
            {
                sr.sprite = m_PanelSprites[4];
                sr.material = m_PanelMaterialAlt;
            }
            else if(type==205) // Type 5. Image
            {
                sr.sprite = m_PanelSprites[1];
            }
            else if(type==206) // Type 6. Image + Title
            {
                sr.sprite = m_PanelSprites[1];
            }
            else if(type==207) // Type 6. Image + Description
            {
                sr.sprite = m_PanelSprites[1];
            }
            
            if(type>=200 && type<=204)
                transform.Find("Image").gameObject.SetActive(false);
            // else if(m_ImagePath.IsNullOrEmty()){
            //     //TODO:
            // }
                
        }

        public void UseFrame(bool value)
        {
            m_UseFrame = value;
        }

        public void UseRoundedCorner(bool value)
        {
            m_UseRoundedCorner = value;
        }

        public void SetInfoPanelText(string text)
        {
            Vector3 originalScale = transform.localScale;
            transform.localScale = new Vector3(1, 1, 1);

            m_TextString = text;

            // set name for the game object
            string textShort;
            if(text.Length > 6) {
                textShort = text.Substring(0, 6)+"..";   
                transform.parent.name = $"itemInfoPanel - {textShort}";
            }
            else if(text=="*"){
                m_TextString = "";
                transform.parent.name = $"itemInfoPanel";
            }
            else{
                transform.parent.name = $"itemInfoPanel - {text}";
            }

            transform.parent.localPosition = transform.localPosition;
            transform.localPosition = new Vector3(0, 0, 0);

            // set text
            m_Text.text = m_TextString;

            // get bounding box of the text
            var meshRenderer = m_Text.GetComponent<MeshRenderer>();
            Vector3 textSize = meshRenderer.bounds.size;

            // set panel size
            Vector2 panelSizeProcessed = new Vector2(1.0f, 1.0f);
            if(m_Type == 201){
                var scaleOffset = 0.14f;
                m_Panel.transform.localScale = new Vector3(scaleOffset, scaleOffset, 1.0f);

                Vector2 panelSizeMin = new Vector2(0.26f, 0.26f);
                Vector2 panelSize = new Vector2(System.Math.Max(textSize.x+0.2f, panelSizeMin.x), System.Math.Max(textSize.y+0.13f, panelSizeMin.y));
                panelSizeProcessed = panelSize / scaleOffset; // offset needed for 9-slicing size

                m_Text.transform.localPosition = new Vector3(0,0,0);
            }
            else if(m_Type == 202){
                var scaleOffset = 0.14f;
                m_Panel.transform.localScale = new Vector3(scaleOffset, scaleOffset, 1.0f);

                Vector2 panelSizeMin = new Vector2(0.7f, 0.485f);
                Vector2 panelSize = new Vector2(System.Math.Max(textSize.x+0.47f, panelSizeMin.x), 0.485f);
                panelSizeProcessed = panelSize / scaleOffset; // offset needed for 9-slicing size

                m_Text.transform.localPosition = new Vector3(0,0,0);
            }
            else if(m_Type == 203){ // Type 3. Callout Side
                var scaleOffset = 0.14f;
                m_Panel.transform.localScale = new Vector3(scaleOffset, scaleOffset, 1.0f);
                
                Vector2 panelSizeMin = new Vector2(0.74f, 0.51f);
                Vector2 panelSize = new Vector2(System.Math.Max(textSize.x+0.2f, panelSizeMin.x), System.Math.Max(textSize.y+0.41f, panelSizeMin.y));
                panelSizeProcessed = panelSize / scaleOffset; // offset needed for 9-slicing size

                m_Text.transform.localPosition = new Vector3(0,0.115f,0);
                m_TextBack.transform.localPosition = new Vector3(0,0.115f,0);

                // move entire InfoPanel object to relocate anchor at the endpoint of speech bubble
                gameObject.transform.localPosition += new Vector3(panelSize.x*0.5f, panelSize.y*0.5f, 0f);
            }
            else if(m_Type == 204){ // Type 4. callout Down
                var scaleOffset = 0.14f;
                m_Panel.transform.localScale = new Vector3(scaleOffset, scaleOffset, 1.0f);
                
                Vector2 panelSizeMin = new Vector2(0.74f, 0.51f);
                Vector2 panelSize = new Vector2(System.Math.Max(textSize.x+0.2f, panelSizeMin.x), System.Math.Max(textSize.y+0.41f, panelSizeMin.y));
                panelSizeProcessed = panelSize / scaleOffset; // offset needed for 9-slicing size

                m_Text.transform.localPosition = new Vector3(0,0.115f,0);
                m_TextBack.transform.localPosition = new Vector3(0,0.115f,0);

                // move entire InfoPanel object to relocate anchor at the endpoint of speech bubble
                gameObject.transform.localPosition += new Vector3(0f, panelSize.y*0.5f, 0f);

                m_Panel.material.SetFloat("_Width", panelSize.x);
                m_Panel.material.SetFloat("_Height", panelSize.y);
            }
            else if(m_Type == 207){ // Type 7. image + description
                var scaleOffset = 0.125f;
                m_Panel.transform.localScale = new Vector3(scaleOffset, scaleOffset, 1.0f);

                Vector2 imageSize = new Vector2(0.75f, 1.0f);
                
                float panelHeight = imageSize.y + 0.2f;
                float panelWidth  = imageSize.x + 0.2f + textSize.x + 0.1f;
                Vector2 panelSize = new Vector2(panelWidth, panelHeight);
                panelSizeProcessed = panelSize / scaleOffset; // offset needed for 9-slicing size
            }
            else{
                var scaleOffset = 0.125f;
                m_Panel.transform.localScale = new Vector3(scaleOffset, scaleOffset, 1.0f);

                // decide panel height from text size
                float panelHeight = 0.2f;
                if(!System.String.IsNullOrEmpty(m_TextString))
                    panelHeight += textSize.y;
                Vector2 panelSize = new Vector2(1.5f, panelHeight);
               
                panelSizeProcessed = panelSize / scaleOffset; // offset needed for 9-slicing size
            }

            if(!m_UseFrame)
                transform.Find("Panel").gameObject.SetActive(false);
            if(!m_UseRoundedCorner)
                m_Image.material.SetFloat("_UseRoundedCorner", 0f);
            if(m_Type == 202 || m_Type == 203)
                m_TextBack.text = m_TextString;
            else
                transform.Find("TextBack").gameObject.SetActive(false);

            m_Panel.size = panelSizeProcessed;
            transform.localScale = originalScale;
        }

        public void SetInfoPanelImage(string imagePath)
        {
            StartCoroutine( LoadImageBuffer(imagePath, CreateInfoPanelImage) );
        }

        private IEnumerator LoadImageBuffer(string path, System.Action<byte[]> callback)
        {
            if(string.IsNullOrEmpty(path))
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

        private void CreateInfoPanelImage(byte[] imageBytes)
        {
            Texture2D texture = new Texture2D(0,0);
            texture.LoadImage(imageBytes);

            Rect rect = new Rect(0,0, texture.width, texture.height);
            
            var imageHeight = 1.3f*texture.height/texture.width;
            m_Image.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f)); //마지막 인자가 위치 조정
            m_Image.GetComponent<SpriteRenderer>().size = new Vector2(1.3f, imageHeight);

            Shader shader = Shader.Find(k_InfoPanelImage);
            Material material = new Material(shader);
            m_Image.material = material;

            if(m_Type==207){
                // move text and image position
                if(!System.String.IsNullOrEmpty(m_TextString)){
                    m_Text .transform.localPosition -= new Vector3((0.1f+0.75f)*0.5f, 0.0f, 0.0f);
                    m_Image.transform.localPosition += new Vector3((0.1f+m_Text.GetComponent<MeshRenderer>().bounds.size.x)*0.5f, 0.0f, 0.0f);
                    m_Image.GetComponent<SpriteRenderer>().size = new Vector2(0.75f, 1.0f);
                }

                m_Image.material.SetFloat("_Width", m_Image.GetComponent<SpriteRenderer>().size.x*100f);
                m_Image.material.SetFloat("_Height", m_Image.GetComponent<SpriteRenderer>().size.y*100f);
            }
            else {
                // update panel height from image size
                var scaleOffset = m_Panel.transform.localScale.x;
                m_Panel.size += new Vector2(0.0f, imageHeight/scaleOffset);
                
                // move text and image position
                if(!System.String.IsNullOrEmpty(m_TextString)){
                    m_Panel.size += new Vector2(0.0f, 0.1f/scaleOffset);

                    m_Text .transform.localPosition += new Vector3(0.0f, (0.1f+imageHeight)*0.5f, 0.0f);
                    m_Image.transform.localPosition -= new Vector3(0.0f, (0.1f+m_Text.GetComponent<MeshRenderer>().bounds.size.y)*0.5f, 0.0f);
                }

                m_Image.material.SetFloat("_Width", m_Image.GetComponent<SpriteRenderer>().size.x*100f);
                m_Image.material.SetFloat("_Height", m_Image.GetComponent<SpriteRenderer>().size.y*100f);
            }
        }

        // UnityModel에서 사용하는 glb fade와 다른 방식으로 동작. new 키워드로 hiding.
        public override void Fade(float duration, bool fadeIn, System.Action onComplete = null)
        {
            if(!gameObject.activeSelf) {
                return;
            }
        }
    }
}
