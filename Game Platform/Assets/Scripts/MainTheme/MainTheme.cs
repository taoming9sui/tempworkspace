using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainTheme : GameActivity
{
    public GameObject cameraObj;
    public GameObject canvasObj;
    public GameObject sceneObj;


    #region 活动触发器
    public override void OnActivityEnabled(Object param)
    {
        SetStage("connect");
    }
    public override void OnDisconnect()
    {
        ConnectErrorModel("show");
    }
    public override void OnConnect()
    {
        SetStage("login");
    }
    public override void OnMessage(JObject jsonData)
    {
        try
        {

            string type = jsonData.GetValue("Type").ToString();
            if(type == "Server_Center")
            {
                Mask(false); //隐藏遮罩
                JObject data = (JObject)jsonData.GetValue("Data");
                string action = data.GetValue("Action").ToString();
                switch (action)
                {
                    case "LoginSuccess":
                        this.LoginSuccess();
                        break;
                    case "RegisterSuccess":
                        this.RegisterSuccess();
                        break;
                    case "Tip":
                        {
                            string content = data.GetValue("Content").ToString();
                            this.TipModel("show", content);
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
    public void ConnectErrorModel(string code)
    {
        GameObject connecterror_model = canvasObj.transform.Find("connecterror_model").gameObject;
        switch (code)
        {
            case "show":
                {
                    connecterror_model.SetActive(true);
                }
                break;
            case "confirm":
                {       
                    connecterror_model.SetActive(false);
                    StartCoroutine(DoAction_Delay(()=>
                    {
                        GameManager.Instance.SocketConnect();
                    }, 0.2f));
                }
                break;
        }
    }
    public void TipModel(string code)
    {
        GameObject tip_model = canvasObj.transform.Find("tip_model").gameObject;
        Text tip_text = tip_model.transform.Find("tip_text").GetComponent<Text>();
        switch (code)
        {
            case "confirm":
                {
                    tip_model.SetActive(false);
                }
                break;
        }
    }
    public void TipModel(string code, string tip)
    {
        GameObject tip_model = canvasObj.transform.Find("tip_model").gameObject;
        Text tip_text = tip_model.transform.Find("tip_text").GetComponent<Text>();
        switch (code)
        {
            case "show":
                {
                    tip_text.text = tip;
                    tip_model.SetActive(true);
                }
                break;
        }
    }
    public void Mask(bool show)
    {
        GameObject mask = canvasObj.transform.Find("mask").gameObject;
        mask.SetActive(show);
    }
    public void SetStage(string code)
    {
        GameObject connect_panel = canvasObj.transform.Find("connect").gameObject;
        GameObject login_panel = canvasObj.transform.Find("login").gameObject;
        GameObject register_panel = canvasObj.transform.Find("register").gameObject;
        connect_panel.SetActive(false);
        login_panel.SetActive(false);
        register_panel.SetActive(false);
        switch (code)
        {     
            case "connect":
                {
                    connect_panel.SetActive(true);
                    StartCoroutine(DoAction_Delay(() =>
                    {
                        GameManager.Instance.SocketConnect();
                    }, 0.2f));
                }
                break;
            case "login":
                {
                    login_panel.SetActive(true);
                }
                break;
            case "register":
                {
                    register_panel.SetActive(true);
                }
                break;
        }
    }
    public void ButtonAction(string code)
    {

        switch (code)
        {
            case "login-login":
                SendLogin();
                break;
            case "login-register":
                SetStage("register");
                break;
            case "register-back":
                SetStage("login");
                break;
            case "register-commit":
                SendRegister();
                break;
        }
    }
    #endregion

    private IEnumerator DoAction_Delay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
    private void SendLogin()
    {
        GameObject login_panel = canvasObj.transform.Find("login").gameObject;
        string playerId = login_panel.transform.Find("playerid_input").GetComponent<InputField>().text;
        string password = login_panel.transform.Find("password_input").GetComponent<InputField>().text;
        //尝试登录
        GameManager.Instance.PlayerLogin(playerId, password);
        //显示遮罩
        Mask(true);
    }
    private void SendRegister()
    {
        GameObject register_panel = canvasObj.transform.Find("register").gameObject;

        string playerId = register_panel.transform.Find("playerid_input").GetComponent<InputField>().text;
        string password = register_panel.transform.Find("password_input").GetComponent<InputField>().text;
        string repeat = register_panel.transform.Find("repeat_input").GetComponent<InputField>().text;
        if(password == repeat)
        {
            JObject registerJson = new JObject();
            registerJson.Add("Type", "Client_Center");
            JObject data = new JObject();
            {
                data.Add("Action", "Register");
                data.Add("PlayerId", playerId);
                data.Add("Password", password);
            }
            registerJson.Add("Data", data); ;
            GameManager.Instance.SendMessage(registerJson);
            Mask(true);
        }
        else
        {
            TipModel("show", "两次密码输入不一致");
        }
    }
    private void LoginSuccess()
    {
        GameManager.Instance.SetActivity("Hall");
    }
    private void RegisterSuccess()
    {
        TipModel("show", "注册成功");
        SetStage("login");
    }
}
