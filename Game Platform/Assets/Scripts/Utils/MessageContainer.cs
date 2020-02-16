using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageContainer : MonoBehaviour
{
    public GameObject messageTemplate;
    public Scrollbar scrollbar;
    public GameObject contentObj;
    private List<string> messageTextList = new List<string>();


    public string[] MessageTexts
    {
        get
        {
            return messageTextList.ToArray();
        }
    }

    public void AddMessage(string messageText)
    {
        //记录文本
        messageTextList.Add(messageText);
        //创建一个Message对象
        GameObject messageObj = GameObject.Instantiate(messageTemplate, contentObj.transform);
        messageObj.SetActive(true);
        Text textObj = messageObj.transform.Find("Text").GetComponent<Text>();
        textObj.text = messageText;
        //滚动到底部
        if (scrollbar.value < 0.2f || scrollbar.size > 0.8f)
            scrollbar.value = 0;
    }

    public void ClearMessage()
    {
        //清空文本
        messageTextList.Clear();
        //清空UI对象
        foreach (Transform child in contentObj.transform)
            GameObject.Destroy(child.gameObject);
    }
}
