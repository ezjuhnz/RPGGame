using UnityEngine;
using System.Collections;

public class ScreenToCanvas : MonoBehaviour {
    public RectTransform m_parent;
    public Camera m_uiCamera;
    public RectTransform m_image;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetMouseButtonDown(0))
        {
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, Input.mousePosition, m_uiCamera, out anchoredPos);
            m_image.anchoredPosition = anchoredPos;
        }
	}
}
