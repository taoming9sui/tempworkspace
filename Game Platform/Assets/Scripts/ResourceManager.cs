using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    static public ResourceManager Instance;

    public IDictionary<string, ActivityInfo> ActivityInfoSet { get; private set; }
    public IDictionary<string, GameInfo> GameInfoSet { get; private set; }
    public Texture2D[] PlayerHeadTextures { get; private set; }

    private void Awake()
    {
        Instance = this;
        //初始化 活动资源注册
        {
            ActivityInfoSet = new Dictionary<string, ActivityInfo>();
            GameObject mainTheme = Resources.Load<GameObject>("Prefabs/MainTheme/MainTheme");
            ActivityInfoSet["MainTheme"] = new ActivityInfo("MainTheme", mainTheme);
            GameObject hall = Resources.Load<GameObject>("Prefabs/Hall/Hall");
            ActivityInfoSet["Hall"] = new ActivityInfo("MainTheme", hall);
            GameObject wolfman_P8 = Resources.Load<GameObject>("Prefabs/Wolfman_P8/Wolfman_P8");
            ActivityInfoSet["Wolfman_P8"] = new ActivityInfo("Wolfman_P8", wolfman_P8);
        }
        //初始化 游戏资源注册
        {
            GameInfoSet = new Dictionary<string, GameInfo>();
            GameInfoSet["Wolfman_P8"] = new GameInfo("Wolfman_P8", "狼人杀", "Wolfman_P8");
        }
        //初始化 头像资源注册
        {
            PlayerHeadTextures = new Texture2D[0];
            Texture2D[] textures = Resources.LoadAll<Texture2D>("Images/Profile Pictrues");
            PlayerHeadTextures = textures;
        }
    }


    public class ActivityInfo
    {
        public string ActivityId { get; private set; }
        public GameObject ActivityPrefab { get; private set; }

        public ActivityInfo(string id, GameObject activityPrefab)
        {
            ActivityId = id;
            ActivityPrefab = activityPrefab;
        }
    }
    public class GameInfo
    {
        public GameInfo(string id, string text, string activityId)
        {
            GameId = id;
            GameText = text;
            ActivityId = activityId;
        }

        public string GameId { get; private set; }
        public string GameText { get; private set; }
        public string ActivityId { get; private set; }
    }
}
