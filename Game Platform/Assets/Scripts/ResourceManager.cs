using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    static public ResourceManager Instance;

    public IDictionary<string, GameInfo> GameInfoSet { get; private set; }
    public Texture2D[] PlayerHeadTextures { get; private set; }

    private void Awake()
    {
        Instance = this;
        //初始化 游戏记录信息
        {
            GameInfoSet = new Dictionary<string, GameInfo>();
            GameInfoSet["Wolfman_P8"] = new GameInfo("Wolfman_P8", "狼人杀8人", "Wolfman_P8");
        }
        //初始化 头像记录信息
        {
            PlayerHeadTextures = new Texture2D[0];
            Texture2D[] textures = Resources.LoadAll<Texture2D>("Profile Pictrues");
            PlayerHeadTextures = textures;
        }
    }


    #region 游戏条目类
    public class GameInfo
    {
        public GameInfo(string id, string name, string activity)
        {
            GameId = id;
            GameName = name;
            GameActivity = activity;
        }

        public string GameId { get; private set; }
        public string GameName { get; private set; }
        public string GameActivity { get; private set; }
    }
    #endregion
}
