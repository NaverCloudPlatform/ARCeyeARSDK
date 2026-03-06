using System.Collections;
using UnityEngine;

namespace ARCeye
{
    public class UnitySignPOI : UnityModel
    {
        private Billboard m_Billboard;
        private SignPOIRenderer m_SignPOIRenderer;


        private void Awake()
        {
            m_Billboard = gameObject.AddComponent<Billboard>();
            m_Billboard.rotationMode = Billboard.RotationMode.AXIS_Y;

            m_SignPOIRenderer = GetComponent<SignPOIRenderer>();
            if (m_SignPOIRenderer == null)
            {
                NativeLogger.Print(LogLevel.ERROR, "[UnitySignPOI] SignPOIRenderer component is missing.");
                return;
            }

            SetOpacity(0);

            InitLayerInModel("ARItem");
        }

        public void SetType(int type)
        {

        }

        public void SetIcon(Sprite icon)
        {
            m_SignPOIRenderer.SetIcon(icon);

            // Icon을 설정한 뒤에 SetActive(false)를 통해 opacity를 0으로 설정해야 한다.
            // SetIcon을 호출하기 전까지는 POIIconRenderer가 설정되어 있지 않기 때문.
            SetActive(false);
        }

        public void SetLabel(string content)
        {
            m_SignPOIRenderer.SetLabel(content);
        }

        public void SetAutoRotateMode(int rotationMode)
        {
            m_Billboard.rotationMode = (Billboard.RotationMode)rotationMode;
        }

        public override void Fade(float duration, bool fadeIn, System.Action onComplete = null)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            StartCoroutine(FadeInternal(duration, fadeIn, onComplete));
        }

        private IEnumerator FadeInternal(float duration, bool fadeIn, System.Action onComplete)
        {
            float start = m_SignPOIRenderer.GetOpacity();
            float end = fadeIn ? 1 : 0;

            bool isFinished = false;
            float accumTime = 0;

            while (!isFinished)
            {
                float t = accumTime / duration;

                float a = Mathf.Lerp(start, end, t);

                SetOpacity(a);

                yield return null;

                accumTime += Time.deltaTime;

                if (accumTime >= duration)
                {
                    isFinished = true;
                }
            }

            SetOpacity(end);
        }

        public override void SetOpacity(float opacity)
        {
            m_SignPOIRenderer.SetOpacity(opacity);
        }
    }
}