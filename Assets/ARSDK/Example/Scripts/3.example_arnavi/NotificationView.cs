using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationView : MonoBehaviour
{
    [SerializeField]
    private Image m_Panel;

    [SerializeField]
    private TMP_Text m_Message;
    
    [SerializeField]
    private Image m_Icon;

    private static NotificationView s_Instance;

    [SerializeField]
    private List<Sprite> m_IconTypes;

    public enum Type {
        NONE = 0,
        NAVIGATION = 1,
    }

    private Coroutine m_CurrCoroutine = null;
    
    void Awake() {
        if(s_Instance == null) {
            s_Instance = this;
            s_Instance.gameObject.SetActive(true);
            s_Instance.SetOpacity(0.0f);
        }
    }

    public static NotificationView Instance()
    {
        if (s_Instance == null)
        {
            s_Instance = FindObjectOfType<NotificationView>(true);
        }

        return s_Instance;
    }

    public void Show(string text, Type type = Type.NONE, float showDuration = 3.0f) {
        gameObject.SetActive(true);

        SetText(text);

        switch(type) {
            case Type.NAVIGATION:
                if(m_IconTypes.Count > 0) {
                    m_Icon.rectTransform.sizeDelta = new Vector2(36, 56);
                    SetIcon(m_IconTypes[0]);
                } else {
                    m_Icon.gameObject.SetActive(false);
                    m_Icon.rectTransform.sizeDelta = new Vector2(64, 64);
                }
                break;
            default:
                SetIcon(null);
                break;
        }

        SetOpacity(1.0f);

        m_CurrCoroutine = StartCoroutine ( RunCoroutineInternal(() => {
            Fade(false, 0.3f);
        }, showDuration) );
    }

    public void Hide() {
        if(m_CurrCoroutine != null) {
            StopCoroutine(m_CurrCoroutine);
            m_CurrCoroutine = null;
        }

        if(IsShowing()) {
            SetOpacity(0.0f);
        }
    }

    public bool IsShowing() {
        return GetOpacity() > 0.0f;
    }

    private void SetText(string text) {
        m_Message.text = text;
    }

    private void SetIcon(Sprite icon) {
        // Icon is optional.
        // Set to null to remove icon.
        if (icon == null) {
            m_Icon.rectTransform.sizeDelta = new Vector2(64, 64);
            m_Icon.gameObject.SetActive(false);
            return;
        }

        m_Icon.sprite = icon;

        if (!m_Icon.gameObject.activeSelf) {
            m_Icon.gameObject.SetActive(true);
        }
    }

    private void SetIconByType(Type type) {
        switch(type) {
            case Type.NAVIGATION:
                if(m_IconTypes.Count > 0) {
                    m_Icon.rectTransform.sizeDelta = new Vector2(36, 56);
                    SetIcon(m_IconTypes[0]);
                } else {
                    m_Icon.gameObject.SetActive(false);
                    m_Icon.rectTransform.sizeDelta = new Vector2(64, 64);
                }
                break;
            default:
                SetIcon(null);
                break;
        }
    }

    private void SetOpacity(float opacity) {
        if (m_Message) {
            Color col = m_Message.color;
            col.a = opacity;
            m_Message.color = col;
        }

        if (m_Icon) {
            Color col = m_Icon.material.color;
            col.a = opacity;
            m_Icon.color = col;
        }

        if (m_Panel) {
            Color col = m_Panel.material.color;
            if (opacity > 0.75f) {
                col.a = 0.75f;
            } else {
                col.a = opacity;
            }
            m_Panel.color = col;
        }
    }

    private float GetOpacity() {
        if (m_Message) {
            return m_Message.color.a;
        }

        if (m_Icon) {
            return m_Icon.color.a;
        }

        if (m_Panel) {
            if (m_Panel.color.a == 0.75f) {
                return 1.0f;
            } 
            return m_Panel.color.a;
        }

        return 0.0f;
    }

    private void Fade(bool fadeIn, float duration) {
        if(!gameObject.activeSelf) {
            return;
        }

        StartCoroutine( FadeInternal(duration, fadeIn) );
    }

    private IEnumerator FadeInternal(float duration, bool fadeIn)
    {
        float start = m_Message.color.a;
        float end = fadeIn ? 1 : 0;

        // Slightly transparent panel.
        float panelStart = m_Panel.color.a;
        float panelEnd = fadeIn ? 0.75f : 0;

        bool isFinished = false;
        float accumTime = 0;

        Color currColor = new Color(m_Message.color.r, m_Message.color.g, m_Message.color.b, m_Message.color.a);

        while(!isFinished)
        {
            float t = accumTime / duration;

            float a = Mathf.Lerp(start, end, t);

            float panelAlpha = Mathf.Lerp(panelStart, panelEnd, t);
            
            currColor.a = a;
            m_Message.color = currColor;
            m_Icon.color = currColor;

            currColor.a = panelAlpha;
            m_Panel.color = currColor;

            yield return null;

            accumTime += Time.deltaTime;

            if(accumTime >= duration) {
                isFinished = true;
            }
        }

        // 최종 투명도로 설정.
        currColor.a = end;
        m_Message.color = currColor;
        m_Icon.color = currColor;

        currColor.a = panelEnd;
        m_Panel.color = currColor;

        m_CurrCoroutine = null;
    }

    private IEnumerator RunCoroutineInternal(System.Action action, float delay) {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}
