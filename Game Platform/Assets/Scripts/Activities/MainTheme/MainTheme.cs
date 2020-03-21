using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainTheme : GameActivity
{
    public GameObject cameraObj;
    public GameObject panelObj;
    public GameObject sceneObj;
    public GameObject tipModelObj;
    public AudioPlayer audioPlayer;
    public LocalizationDictionary localDic;

    #region 活动触发器
    public override void OnActivityEnabled(Object param)
    {
        if (GameManager.Instance.HasConnection)
            SetStage("login");
        else
            SetStage("connect");
        audioPlayer.PlayBGM("MainThemeBGM1");
    }
    public override void OnDisconnect()
    {
        ConnectErrorModel();
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
                            string resultCode = data.GetValue("Content").ToString();
                            string text = localDic.GetLocalText(resultCode);
                            this.TipModel(text);
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
    public void ConnectErrorModel()
    {
        GameObject modelObj = panelObj.transform.Find("connecterror_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        modelDialog.ModelShow((code) =>{
            switch (code)
            {
                case "confirm":
                    {
                        StartCoroutine(DoAction_Delay(() =>
                        {
                            GameManager.Instance.SocketConnect();
                        }, 0.2f));
                    }
                    break;
            }
        });
    }
    public void TipModel(string tip)
    {
        //1对话框对象克隆
        GameObject modelObj = GameObject.Instantiate(tipModelObj, panelObj.transform);
        //2显示该克隆对象
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        Text tip_text = modelObj.transform.Find("tip_text").GetComponent<Text>();
        tip_text.text = tip;
        //3确定后移除该克隆
        modelDialog.ModelShow((code)=>
        {
            Destroy(modelObj);
        });
    }
    public void SetStage(string code)
    {
        GameObject connect_panel = panelObj.transform.Find("connect").gameObject;
        GameObject login_panel = panelObj.transform.Find("login").gameObject;
        GameObject register_panel = panelObj.transform.Find("register").gameObject;
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
    public void ExitButton()
    {
        GameManager.Instance.QuitGame();
    }
    public void LoginButton()
    {
        SendLogin();
    }
    public void LoginEnter()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendLogin();
        }
    }
    public void RegisterPageButton()
    {
        SetStage("register");
    }
    public void RegisterBackButton()
    {
        SetStage("login");
    }
    public void RegisterCommitButton()
    {
        SendRegister();
    }
    #endregion

    private IEnumerator DoAction_Delay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
    private void SendLogin()
    {
        GameObject login_panel = panelObj.transform.Find("login").gameObject;
        string playerId = login_panel.transform.Find("playerid_input").GetComponent<InputField>().text;
        string password = login_panel.transform.Find("password_input").GetComponent<InputField>().text;
        //尝试登录
        GameManager.Instance.PlayerLogin(playerId, password);
    }
    private void SendRegister()
    {
        GameObject register_panel = panelObj.transform.Find("register").gameObject;

        string playerId = register_panel.transform.Find("playerid_input").GetComponent<InputField>().text;
        string password = register_panel.transform.Find("password_input").GetComponent<InputField>().text;
        string repeat = register_panel.transform.Find("repeat_input").GetComponent<InputField>().text;
        if(password == repeat)
        {
            //尝试登录
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
        }
        else
        {
            string text = localDic.GetLocalText("text.register.repeatpassword_wrong");
            TipModel(text);
        }
    }
    private void LoginSuccess()
    {
        GameObject prefab = ResourceManager.Instance.Local.ActivityInfoSet["Hall"].ActivityPrefab;
        GameManager.Instance.SetActivity(prefab);
    }
    private void RegisterSuccess()
    {
        string text = localDic.GetLocalText("text.register.success");
        TipModel(text);
        SetStage("login");
    }
}
