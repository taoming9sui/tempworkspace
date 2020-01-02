using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hall : GameActivity
{
    public GameObject cameraObj;
    public GameObject canvasObj;
    public GameObject sceneObj;

    #region 活动触发器
    public override void OnActivityEnabled(Object param)
    {
    }
    public override void OnDisconnect()
    {
        GameManager.Instance.SetActivity("MainTheme");
    }
    public override void OnConnect()
    {
    }
    public override void OnMessage(JObject jsonData)
    {
        try
        {
            string type = jsonData.GetValue("Type").ToString();
            if (type == "Server_Hall")
            {
                JObject data = (JObject)jsonData.GetValue("Data");
                string action = data.GetValue("Action").ToString();
                switch (action)
                {
                    case "HallChat":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.ReceiveChat(content.GetValue("Sender").ToString(), content.GetValue("Chat").ToString());
                        }                    
                        break;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    #endregion

    #region UI交互脚本
    public void SendChatButton()
    {
        SendChat();
    }
    #endregion

    private void SendChat()
    {
        //获取输入框消息并清空
        InputField chat_input = canvasObj.transform.Find("chatpanel/chat_input").GetComponent<InputField>();
        string chat = chat_input.text;
        chat_input.text = "";
        if (chat != string.Empty)
        {
            //发送消息
            JObject dataJson = new JObject();
            dataJson.Add("Type", "Client_Hall");
            JObject data = new JObject();
            {
                data.Add("Action", "HallChat");
                data.Add("Chat", chat);
            }
            dataJson.Add("Data", data); ;
            GameManager.Instance.SendMessage(dataJson);
        }
    }
    private void ReceiveChat(string sender, string chat)
    {
        //添加一行聊天信息
        Text chat_text = canvasObj.transform.Find("chatpanel/chat_textarea/Text").GetComponent<Text>();
        chat_text.text += string.Format("{0}:{1}\n", sender, chat);
        //滚动条刷新到最底部
        StartCoroutine(DoAction_Delay(() =>
        {
            Scrollbar scrollbar = canvasObj.transform.Find("chatpanel/chat_scroll").GetComponent<Scrollbar>();
            scrollbar.value = 0;
        }, 0.1f));
    }
    private IEnumerator DoAction_Delay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}
