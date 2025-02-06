using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARCeye;

public class CategoryGenerator : MonoBehaviour
{
    [SerializeField]
    private List<POICategory> m_Categories;

    // Start is called before the first frame update
    void Start()
    {

        var controller = UIUtils.FindViewController<AroundViewController>();
        if(controller) {
            controller.AssignPOICategories(m_Categories);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
