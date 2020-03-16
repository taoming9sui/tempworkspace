using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 该脚本主要用来适配Canvas的显示，得到最佳分辨率
/// </summary>
public class CanvasAdapter : MonoBehaviour
{
    public CanvasScaler canvasScaler;
    private int m_resolutionWidth = 0;
    private int m_resolutionHeight = 0;

    private void Update()
    {
        if (m_resolutionWidth != Screen.width || m_resolutionHeight != Screen.height)
        {
            OnResolutionChanged();
            m_resolutionWidth = Screen.width;
            m_resolutionHeight = Screen.height;
        }
    }

    private void OnResolutionChanged()
    {
        AdaptRadio();
    }

    private void AdaptRadio()
    {
        float screenRadio = (float)Screen.width / (float)Screen.height;
        float canvasRadio = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;
        if (screenRadio > canvasRadio)
        {
            //横屏模式
            canvasScaler.matchWidthOrHeight = 1f;
        }
        else
        {
            //竖屏模式
            canvasScaler.matchWidthOrHeight = 0f;
        }
    }

}
