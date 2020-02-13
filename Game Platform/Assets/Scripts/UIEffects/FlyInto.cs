using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyInto : MonoBehaviour
{
    public RectTransform rectTransform;
    public float deltaX;
    public float deltaY;
    public float deltaTime;
    private float m_time = -1f;
    private Vector2 m_primaryOffsetMin;
    private Vector2 m_primaryOffsetMax;

    private void Awake()
    {
        m_primaryOffsetMin = rectTransform.offsetMin;
        m_primaryOffsetMax = rectTransform.offsetMax;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if(m_time >= 0f)
        {
            m_time += dt;
            float k = 1f - m_time / deltaTime;
            Vector2 v1 = new Vector2(m_primaryOffsetMin.x - deltaX * k, m_primaryOffsetMin.y - deltaY * k);
            Vector2 v2 = new Vector2(m_primaryOffsetMax.x - deltaX * k, m_primaryOffsetMax.y - deltaY * k);
            rectTransform.offsetMin = v1;
            rectTransform.offsetMax = v2;
            if (m_time > deltaTime)
            {
                rectTransform.offsetMin = m_primaryOffsetMin;
                rectTransform.offsetMax = m_primaryOffsetMax;
                m_time = -1f;
            }
        }
    }

    private void OnEnable()
    {
        Show();
    }

    public void Show()
    {
        m_time = 0f;
        Vector2 v1 = new Vector2(m_primaryOffsetMin.x - deltaX, m_primaryOffsetMin.y - deltaY);
        Vector2 v2 = new Vector2(m_primaryOffsetMax.x - deltaX, m_primaryOffsetMax.y - deltaY);
        rectTransform.offsetMin = v1;
        rectTransform.offsetMax = v2;
    }

}
