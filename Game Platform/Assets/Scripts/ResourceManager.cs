using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    static public ResourceManager Instance;

    public ResourceMeta[] metaList;

    [System.Serializable]
    public class ActivityInfo
    {
        public string ActivityId;
        public GameObject ActivityPrefab;
    }
    [System.Serializable]
    public class GameInfo
    {
        public string GameId;
        public string GameName;
        public string ActivityId;
    }
    [System.Serializable]
    public class ResourceMeta
    {
        public ActivityInfo[] activityInfos;
        public IDictionary<string, ActivityInfo> ActivityInfoSet;
        public GameInfo[] gameInfos;
        public IDictionary<string, GameInfo> GameInfoSet;
        public Texture2D[] playerHeadTextures;
        public Texture2D[] PlayerHeadTextures { get { return playerHeadTextures; } }

        public void Init()
        {
            ActivityInfoSet = new Dictionary<string, ActivityInfo>();
            foreach(ActivityInfo info in activityInfos)
                ActivityInfoSet.Add(info.ActivityId, info);

            GameInfoSet = new Dictionary<string, GameInfo>();
            foreach (GameInfo info in gameInfos)
                GameInfoSet.Add(info.GameId, info);
        }
    }

   

    private void Awake()
    {
        Instance = this;
        foreach(ResourceMeta meta in metaList)
        {
            meta.Init();
        }
    }

    public ResourceMeta Local
    {
        get { return metaList[0]; }
    }

}
