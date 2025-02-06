using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

using ARCeye;

public class CategoryCell : MonoBehaviour
{   
    private TMP_Text m_Label;
    private Button m_Button;
    private Image m_Image;

    [SerializeField]
    private Sprite m_DefaultSprite;
    
    [SerializeField]
    private Sprite m_SelectedSprite;
    
    private POICategory m_Category;
    
    public void Initialize(POICategory category)
    {
        TMP_Text label = GetComponentInChildren<TMP_Text>();
        if (label != null) {
            ARPlayGround instance = FindObjectOfType<ARPlayGround>();

            if (instance != null) {
                Locale locale = instance.locale;
                label.text = category.GetLabelByLocale(locale);
                m_Label = label;
            }
        }
        
        Button button = GetComponent<Button>();
        if (button != null) {
            m_Button = button;
        }

        Image image = GetComponent<Image>();
        if (image != null) {
            m_Image = image;
        }

        m_Category = category;
    }

    public void SetOnClickDelegate(UnityAction action) 
    {
        m_Button.onClick.RemoveAllListeners();
        m_Button.onClick.AddListener(action);
    }

    public void Select() {
        m_Label.color = new Color(1, 1, 1, 1);
        m_Image.sprite = m_SelectedSprite;
    }

    public void Deselect() {
        m_Label.color = new Color(135.0f/255, 132.0f/255, 132.0f/255, 1);
        m_Image.sprite = m_DefaultSprite;
    }

    public POICategory GetCategory() {
        return m_Category;
    }
}
