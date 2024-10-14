using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye
{
    public class UIEffect
    {
        /// <summary>
        ///   대상 오브젝트 하위의 모든 Image들에 대해 Fade 효과를 적용한다.
        /// </summary>
        public static void FadeOut(View view, float duration, System.Action completeCallback = null)
        {
            var canvasGroup = view.GetComponent<CanvasGroup>();
            view.StartCoroutine(FadeEffect(false, duration, canvasGroup, completeCallback));
        }

        public static void FadeIn(View view, float duration, System.Action completeCallback = null)
        {
            var canvasGroup = view.GetComponent<CanvasGroup>();
            view.StartCoroutine(FadeEffect(true, duration, canvasGroup, completeCallback));
        }


        /// <summary>
        ///   Helper function to animate UI views with a fade effect
        /// </summary>
        private static IEnumerator FadeEffect(bool fadeIn, float duration, CanvasGroup canvasGroup, System.Action completeCallback)
        {
            // Color originalColor = canvasGroup.alpha;
            float originalAlpha = canvasGroup.alpha;
            float targetAlpha = (fadeIn) ? 1f : 0f;
            // Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, targetAlpha);

            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                float normalizedTime = (Time.time - startTime) / duration;
                // image.color = Color.Lerp(originalColor, targetColor, normalizedTime);
                canvasGroup.alpha = Mathf.Lerp(originalAlpha, targetAlpha, normalizedTime);
                yield return null; // Wait for next frame
            }

            canvasGroup.alpha = targetAlpha;

            completeCallback?.Invoke();
        }

        private IEnumerator FadeFromBlack(float duration, Image image, GameObject currView = null, GameObject nextView = null, float delay = 0)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            Color originalColor = image.color;
            Color targetColor = new Color(0f, 0f, 0f, 1f);

            float fadeDuration = duration / 2; // Shared by fade to black & fade from black

            float startTime = Time.time;
            while (Time.time - startTime < fadeDuration)
            {
                float normalizedTime = (Time.time - startTime) / fadeDuration;
                image.color = Color.Lerp(originalColor, targetColor, normalizedTime);
                yield return null; // Wait for next frame
            }

            image.color = targetColor;

            if (nextView)
            {
                nextView.SetActive(true);
            }

            originalColor = image.color;
            targetColor = new Color(0f, 0f, 0f, 0f);
            startTime = Time.time;
            while (Time.time - startTime < fadeDuration)
            {
                float normalizedTime = (Time.time - startTime) / fadeDuration;
                image.color = Color.Lerp(originalColor, targetColor, normalizedTime);
                yield return null; // Wait for next frame
            }

            if (currView)
            {
                currView.SetActive(false);
            }
        }
    }
}