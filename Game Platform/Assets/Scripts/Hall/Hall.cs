using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
    public override void OnConnect()
    {
    }
    public override void OnMessage(JObject jsonData)
    {
        try
        {

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    #endregion
}
