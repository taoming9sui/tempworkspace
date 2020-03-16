using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownBar : MonoBehaviour
{
    public Text countdownText;
    public RectTransform countdownRect;

    private float m_currentTime = 0f;
    private float m_maxTime = 0f;

    public void StartCountdown(float time)
    {
        m_currentTime = time;
        m_maxTime = time;
    }


    private void Update()
    {
        float dt = Time.deltaTime;
        if(m_currentTime > 0f)
        {
            m_currentTime -= dt;
        }
        else
        {
            m_currentTime = 0f;
        }

        {
            //更新时间条
            RectTransform rootRect = this.GetComponent<RectTransform>();
            float width = rootRect.rect.width;
            float k = m_maxTime != 0f ? 1f - m_currentTime / m_maxTime : 1f;
            countdownRect.offsetMin = new Vector2(k * width, 0);
            //更新倒数文本
            countdownText.text = m_currentTime.ToString("f1");
        }

    }


}
