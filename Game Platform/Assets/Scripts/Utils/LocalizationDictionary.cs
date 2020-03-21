using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalizationDictionary : MonoBehaviour
{
    public Item[] dictionaryItems;
    private IDictionary<string, string> m_dictionary;

    private void Awake()
    {
        m_dictionary = new Dictionary<string, string>();
        foreach(Item item in dictionaryItems)
        {
            m_dictionary.Add(item.unisersalText, item.localText);
        }
    }

    public string GetLocalText(string universalText)
    {
        string localText = "";
        //从通用语-》本地语
        if(m_dictionary.TryGetValue(universalText, out localText))
        {
            return localText;
        }
        return "";
    }

    [System.Serializable]
    public class Item
    {
        public string unisersalText;
        [TextArea]
        public string localText;
    }

}
