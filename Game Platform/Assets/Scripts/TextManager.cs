using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    static public TextManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public string GetResultCodeText(string resultCode)
    {
        string text = "";
        switch (resultCode)
        {
            case "gamecenter.register.playerid_illegal":
                text = "用户ID格式不正确";
                break;
            case "gamecenter.register.password_illegal":
                text = "密码格式不正确";
                break;
            case "gamecenter.register.multiple_register":
                text = "该用户已被注册";
                break;
            case "gamecenter.register.exception":
                text = "用户注册：意外错误";
                break;
            case "gamecenter.login.idpassword_wrong":
                text = "用户名密码错误";
                break;
            case "gamecenter.login.socket_logged":
                text = "已经登录了一个账号";
                break;
            case "gamecenter.login.player_logged":
                text = "该玩家已被登录";
                break;
            case "gamecenter.login.exception":
                text = "用户登录：意外错误";
                break;
            case "gamecenter.changeplayerinfo.playername_illegal":
                text = "玩家昵称格式不正确";
                break;
            case "gamecenter.changeplayerinfo.exception":
                text = "修改玩家资料：意外错误";
                break;
            case "gamecenter.createroom.playerjoined_room":
                text = "已经加入一个房间";
                break;
            case "gamecenter.createroom.illegal_caption":
                text = "房间标题格式不正确";
                break;
            case "gamecenter.createroom.illegal_password":
                text = "房间密码格式不正确";
                break;
            case "gamecenter.joinroom.illegal_room":
                text = "房间不存在";
                break;
            case "gamecenter.joinroom.room_full":
                text = "房间已满员";
                break;
            case "gamecenter.joinroom.room_playing":
                text = "该房间正在游戏中";
                break;
            case "gamecenter.joinroom.password_wrong":
                text = "房间密码错误";
                break;
            case "gamecenter.joinroom.playerjoined_room":
                text = "已经加入一个房间";
                break;
        }
        return text;
    }

    public string GetText(string universalText)
    {
        return "";
    }

}
