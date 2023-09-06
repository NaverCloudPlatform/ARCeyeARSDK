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
        private GameObject m_DestinationRecommendView;

        [SerializeField] 
        private GameObject m_AroundView;

        [SerializeField] 
        private GameObject m_NaviStartedView;

        [SerializeField]
        private GameObject m_NaviArrivedView;

        [SerializeField]
        private GameObject m_TransitSelectView;

        [SerializeField]
        private GameObject m_TransitMovingView;

        [SerializeField]
        private GameObject m_TransitArrivedView;

        [SerializeField]
        private GameObject m_StageView;


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

        public void UpdateScanView(bool show) {
            if (m_ScanView) {
                m_ScanView.SetActive(show);
            }

            if (show) {
                HideStageView();
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

            if (show)
                ShowStageView();
        }

        public void UpdateNavigationStartedView(bool show) {
            if (m_NaviStartedView) {
                m_NaviStartedView.SetActive(show);
            }

            if (show)
                ShowStageView();
        }

        public void UpdateNavigationArrivedView(bool show) {
            if (m_NaviArrivedView) {
                m_NaviArrivedView.SetActive(show);
            }

            if (show)
                ShowStageView();
        }

        public void ShowTransitSelectView(string currStage, string destStage) {
            if (m_TransitSelectView) {
                m_TransitSelectView.SetActive(true);

                m_TransitSelectView.transform.Find("Stage Info/DynamicText_CurrentStage").gameObject.GetComponent<Text>().text = currStage;
                m_TransitSelectView.transform.Find("Stage Info/DynamicText_DestinationStage").gameObject.GetComponent<Text>().text = destStage;
            }

            HideStageView();
        }

        public void HideTransitSelectView() {
            if (m_TransitSelectView) {
                m_TransitSelectView.SetActive(false);
            }
        }

        public void ShowTransitMovingView(int transitType, string currStage, string destStage) {
            if (m_TransitMovingView) {
                m_TransitMovingView.gameObject.SetActive(true);

                // Configure transit icon
                {
                    if (transitType == (int)ConnectionType.Escalator) {
                        m_TransitMovingView.transform.Find("Image_Elevator").gameObject.SetActive(false);
                        m_TransitMovingView.transform.Find("Image_Stair").gameObject.SetActive(false);

                        m_TransitMovingView.transform.Find("Image_Escalator").gameObject.SetActive(true);
                    }

                    else if (transitType == (int)ConnectionType.Elevator) {
                        m_TransitMovingView.transform.Find("Image_Escalator").gameObject.SetActive(false);
                        m_TransitMovingView.transform.Find("Image_Stair").gameObject.SetActive(false);

                        m_TransitMovingView.transform.Find("Image_Elevator").gameObject.SetActive(true);
                    }

                    else if (transitType == (int)ConnectionType.Stair) {
                        m_TransitMovingView.transform.Find("Image_Escalator").gameObject.SetActive(false);
                        m_TransitMovingView.transform.Find("Image_Elevator").gameObject.SetActive(false);

                        m_TransitMovingView.transform.Find("Image_Stair").gameObject.SetActive(true);
                    }
                }

                m_TransitMovingView.transform.Find("DynamicText_StageInfo").gameObject.GetComponent<Text>().text = destStage;
                m_TransitMovingView.transform.Find("Start Stage/DynamicText_CurrentStage").gameObject.GetComponent<Text>().text = currStage;
                m_TransitMovingView.transform.Find("Destination Stage/DynamicText_DestStage").gameObject.GetComponent<Text>().text = destStage;
            }

            HideStageView();
        }

        public void HideTransitMovingView() {
            if (m_TransitMovingView) {
                m_TransitMovingView.SetActive(false);
            }
        }

        public void ShowTransitArrivedView(string destStage) {
            if (m_TransitArrivedView) {
                m_TransitArrivedView.SetActive(true);

                m_TransitArrivedView.transform.Find("Arrived Icon/DynamicText_DestStage").gameObject.GetComponent<Text>().text = destStage;
            }

            HideStageView();
        }

        public void HideTransitArrivedView() {
            if (m_TransitArrivedView) {
                m_TransitArrivedView.SetActive(false);
            }
        }

        public void ShowStageView() {
            if (m_StageView) {
                m_StageView.SetActive(true);
            }
        }

        public void HideStageView() {
            if (m_StageView) {
                m_StageView.SetActive(false);
            }
        }

        public void SetStageName(string stageName) {
            if (m_StageView) {
                m_StageView.GetComponentInChildren<Text>().text = stageName;
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