using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye {
    public class UIViewController : MonoBehaviour
    {
        [SerializeField] 
        private GameObject m_SplashView;

        [SerializeField] 
        private GameObject m_ScanView;

        [SerializeField] 
        private Button m_ScanButton;

        [SerializeField] 
        private Button m_TransitScanButton;

        [SerializeField] 
        private GameObject m_DestinationRecommendView;

        [SerializeField] 
        private GameObject m_AroundView;

        [SerializeField] 
        private GameObject m_NaviStartedView;

        [SerializeField] 
        private GameObject m_NaviRemainingDistanceArea;

        [SerializeField]
        private Text m_DistanceText;

        [SerializeField]
        private GameObject m_NaviArrivedView;

        [SerializeField]
        private GameObject m_TransitSelectView;

        [SerializeField]
        private GameObject m_TransitMovingView;

        [SerializeField]
        private Text m_TransitMovingFailedText;

        [SerializeField]
        private GameObject m_TransitIconEscalator;

        [SerializeField]
        private GameObject m_TransitIconElevator;

        [SerializeField]
        private GameObject m_TransitIconStair;

        [SerializeField]
        private GameObject m_TransitArrivedView;        


        public void ShowSplashView() {
            if (m_SplashView) {
                m_SplashView.SetActive(true);

                StartCoroutine(HideSplashView(1.5f));
            }
        }

        private IEnumerator HideSplashView(float delay) {
            yield return new WaitForSeconds(delay);

            StartCoroutine(FadeEffect(false, 1.5f, m_SplashView.transform.Find("Image_SplashIcon").gameObject.GetComponent<Image>()));
            StartCoroutine(FadeEffect(false, 1.5f, m_SplashView.transform.Find("Image_SplashBottom").gameObject.GetComponent<Image>()));
            StartCoroutine(FadeFromBlack(1.5f, m_SplashView.GetComponent<Image>(), m_SplashView, m_ScanView, 0.5f));

            // Automatically show ScanView
            UpdateScanView(true);
        }

        public void DisableScanButton()
        {
            m_ScanButton.interactable = false;
        }

        public void DistableTransitScanButton()
        {
            m_TransitScanButton.interactable = false;
        }

        public void UpdateScanView(bool show) {
            if (m_ScanView) {
                m_ScanView.SetActive(show);
            }
        }

        public void UpdateDestinationRecommendView(bool show) {
            if (m_DestinationRecommendView) {
                m_DestinationRecommendView.SetActive(show);
            }
        }

        public void UpdateAroundView(bool show) {
            if (m_AroundView) {
                m_AroundView.SetActive(show);
            }
        }

        public void UpdateNavigationStartedView(bool show) {
            if (m_NaviStartedView) {
                m_NaviStartedView.SetActive(show);

                ShowRemainingDistance(true);
            }
        }

        public void UpdateRemainingDistance(float distance) {
            m_DistanceText.text = string.Format("{0}", distance.ToString("N0"));
        }

        public void ShowRemainingDistance(bool show) {
            m_NaviRemainingDistanceArea.gameObject.SetActive(show);
            m_DistanceText.text = string.Format("");
        }

        public void UpdateNavigationArrivedView(bool show) {
            if (m_NaviArrivedView) {
                m_NaviArrivedView.SetActive(show);
            }
        }

        public void ShowTransitSelectView(string currStage, string destStage) {
            if (m_TransitSelectView) {
                m_TransitSelectView.SetActive(true);

                m_TransitSelectView.transform.Find("Stage Info/DynamicText_CurrentStage").gameObject.GetComponent<Text>().text = currStage;
                m_TransitSelectView.transform.Find("Stage Info/DynamicText_DestinationStage").gameObject.GetComponent<Text>().text = destStage;
            }
        }

        public void HideTransitSelectView() {
            if (m_TransitSelectView) {
                m_TransitSelectView.SetActive(false);
            }
        }

        public void ShowTransitMovingView(ConnectionType transitType, string currStage, string destStage) {
            if (m_TransitMovingView) {
                m_TransitMovingView.gameObject.SetActive(true);

                // Configure transit icon
                {
                    m_TransitIconEscalator.SetActive(transitType == ConnectionType.Escalator);
                    m_TransitIconElevator.SetActive(transitType == ConnectionType.Elevator);
                    m_TransitIconStair.SetActive(transitType == ConnectionType.Stair);
                }

                m_TransitMovingView.transform.Find("DynamicText_StageInfo").gameObject.GetComponent<Text>().text = destStage;
                m_TransitMovingView.transform.Find("Start Stage/DynamicText_CurrentStage").gameObject.GetComponent<Text>().text = currStage;
                m_TransitMovingView.transform.Find("Destination Stage/DynamicText_DestStage").gameObject.GetComponent<Text>().text = destStage;

                m_TransitMovingFailedText.gameObject.SetActive(false);

                m_TransitScanButton.interactable = true;
            }
        }

        public void HideTransitMovingView() {
            if (m_TransitMovingView) {
                m_TransitMovingView.SetActive(false);
            }
        }

        public void ShowTransitArrivedView(string destStage, float showDuration, System.Action completeCallback) {
            StartCoroutine( ShowTransitArrivedViewInternal(destStage, showDuration, completeCallback));
        }

        private IEnumerator ShowTransitArrivedViewInternal(string destStage, float showDuration, System.Action completeCallback) {
            if (m_TransitArrivedView) {
                m_TransitArrivedView.SetActive(true);

                m_TransitArrivedView.transform.Find("Arrived Icon/DynamicText_DestStage").gameObject.GetComponent<Text>().text = destStage;
            }

            // HideStageView();

            yield return new WaitForSeconds(showDuration);

            HideTransitArrivedView();

            completeCallback();
        }

        public void ShowTransitMovingFailed() {
            m_TransitScanButton.interactable = true;
            m_TransitMovingFailedText.gameObject.SetActive(true);
        }

        public void HideTransitArrivedView() {
            if (m_TransitArrivedView) {
                m_TransitArrivedView.SetActive(false);
            }
        }


        // Helper function to animate UI views with a fade effect
        private IEnumerator FadeEffect(bool fadeIn, float duration, Image image, GameObject currView = null, GameObject nextView = null) {
            Color originalColor = image.color;
            float targetAlpha = (fadeIn) ? 1f : 0f;
            Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, targetAlpha);

            float startTime = Time.time;
            while (Time.time - startTime < duration) {
                float normalizedTime = (Time.time - startTime) / duration;
                image.color = Color.Lerp(originalColor, targetColor, normalizedTime);
                yield return null; // Wait for next frame
            }

            image.color = targetColor;

            if (currView) {
                currView.SetActive(false);
            } 

            if (nextView) {
                nextView.SetActive(true);
            }
        }

        private IEnumerator FadeFromBlack(float duration, Image image, GameObject currView = null, GameObject nextView = null, float delay = 0) {
            if (delay > 0) {
                yield return new WaitForSeconds(delay);
            }
            
            Color originalColor = image.color;
            Color targetColor = new Color(0f, 0f, 0f, 1f);

            float fadeDuration = duration / 2; // Shared by fade to black & fade from black

            float startTime = Time.time;
            while (Time.time - startTime < fadeDuration) {
                float normalizedTime = (Time.time - startTime) / fadeDuration;
                image.color = Color.Lerp(originalColor, targetColor, normalizedTime);
                yield return null; // Wait for next frame
            }

            image.color = targetColor;

            if (nextView) {
                nextView.SetActive(true);
            }

            originalColor = image.color;
            targetColor = new Color(0f, 0f, 0f, 0f);
            startTime = Time.time;
            while (Time.time - startTime < fadeDuration) {
                float normalizedTime = (Time.time - startTime) / fadeDuration;
                image.color = Color.Lerp(originalColor, targetColor, normalizedTime);
                yield return null; // Wait for next frame
            }

            if (currView) {
                currView.SetActive(false);
            } 
        }
    }
}