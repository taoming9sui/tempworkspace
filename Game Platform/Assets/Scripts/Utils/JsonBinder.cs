using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class JsonBinder
{
    private class BindActionItem
    {
        public string TemplatePath;
        public System.Action<JToken, int[]> UpdateAction;
        public IDictionary<string, int[]> ChangeRecordSet;

        public BindActionItem(string templatePath, System.Action<JToken, int[]> updateAction)
        {
            this.TemplatePath = templatePath;
            this.UpdateAction = updateAction;
            this.ChangeRecordSet = new Dictionary<string, int[]>();
        }
    }
    private JToken m_jsonObj;
    private IDictionary<string, BindActionItem> m_bindItemSet = new Dictionary<string, BindActionItem>();

    public JsonBinder(JObject jObject)
    {
        m_jsonObj = jObject;
    }

    public JsonBinder(JArray jArray)
    {
        m_jsonObj = jArray;
    }

    public void ApplyUpdate()
    {
        foreach(BindActionItem item in m_bindItemSet.Values)
        {
            foreach(KeyValuePair<string, int[]> kv in item.ChangeRecordSet)
            {
                string jsonPath = kv.Key;
                JToken jToken = m_jsonObj.SelectToken(jsonPath);
                int[] arrParams = kv.Value;
                item.UpdateAction(jToken, arrParams);     
            }
            item.ChangeRecordSet.Clear();
        }
    }

    public void SetValue(string jsonPath, JToken value)
    {
        StringBuilder strBuffer = new StringBuilder();
        IList<string> pathPartList = new List<string>();
        //1读取路径
        for (int i = 0; i < jsonPath.Length; i++)
        {
            char c = jsonPath[i];
            if (c == '.')
            {
                string objkey = strBuffer.ToString();
                strBuffer.Clear();
                if (!string.IsNullOrEmpty(objkey))
                {
                    //记录路径
                    string pathPart = pathPartList.Count > 0 ? string.Format(".{0}", objkey) : objkey;
                    pathPartList.Add(pathPart);
                }
            }
            else if (c == '[')
            {
                string objkey = strBuffer.ToString();
                strBuffer.Clear();
                if (!string.IsNullOrEmpty(objkey))
                {
                    //记录路径
                    string pathPart = pathPartList.Count > 0 ? string.Format(".{0}", objkey) : objkey;
                    pathPartList.Add(pathPart);
                }
            }
            else if (c == ']')
            {
                if (i + 1 < jsonPath.Length)
                {
                    int arridx = int.Parse(strBuffer.ToString());
                    strBuffer.Clear();
                    //记录路径
                    string pathPart = string.Format("[{0}]", arridx);
                    pathPartList.Add(pathPart);
                }
            }
            else
            {
                strBuffer.Append(c);
            }
        }
        //2设置新值
        {
            string path = string.Join("", pathPartList);
            JToken parentToken = m_jsonObj.SelectToken(path);
            if (parentToken.Type == JTokenType.Object)
            {
                string objkey = strBuffer.ToString();
                strBuffer.Clear();
                parentToken[objkey] = value;
                //记录路径
                string pathPart = pathPartList.Count > 0 ? string.Format(".{0}", objkey) : objkey;
                pathPartList.Add(pathPart);
            }
            else if (parentToken.Type == JTokenType.Array)
            {
                int arridx = int.Parse(strBuffer.ToString());
                strBuffer.Clear();
                parentToken[arridx] = value;
                //记录路径
                string pathPart = string.Format("[{0}]", arridx);
                pathPartList.Add(pathPart);
            }
        }
        //3按照路径从下到上触发事件
        while (pathPartList.Count > 0)
        {
            string path = string.Join("", pathPartList);
            string templatePath = "";
            List<int> arrParamList = new List<int>();
            {
                Regex reg = new Regex(@"\[(\d+)\]");
                MatchCollection matchCollection = reg.Matches(path);
                foreach (Match match in matchCollection)
                {
                    string arridx = match.Groups[1].Value;
                    arrParamList.Add(int.Parse(arridx));
                }
                templatePath = reg.Replace(path, "[d]");
            }
            if (m_bindItemSet.ContainsKey(templatePath))
            {
                BindActionItem bindItem = m_bindItemSet[templatePath];
                bindItem.ChangeRecordSet[path] = arrParamList.ToArray();
                break;
            }
            pathPartList.RemoveAt(pathPartList.Count - 1);
        }
    }

    public JToken GetValue(string jsonPath)
    {
        JToken value = m_jsonObj.SelectToken(jsonPath);
        return value;
    }

    public void ForceUpdate(string jsonPath)
    {
        StringBuilder strBuffer = new StringBuilder();
        IList<string> pathPartList = new List<string>();
        //1读取路径
        for (int i = 0; i < jsonPath.Length; i++)
        {
            char c = jsonPath[i];
            if (c == '.')
            {
                string objkey = strBuffer.ToString();
                strBuffer.Clear();
                if (!string.IsNullOrEmpty(objkey))
                {
                    //记录路径
                    string pathPart = pathPartList.Count > 0 ? string.Format(".{0}", objkey) : objkey;
                    pathPartList.Add(pathPart);
                }
            }
            else if (c == '[')
            {
                string objkey = strBuffer.ToString();
                strBuffer.Clear();
                if (!string.IsNullOrEmpty(objkey))
                {
                    //记录路径
                    string pathPart = pathPartList.Count > 0 ? string.Format(".{0}", objkey) : objkey;
                    pathPartList.Add(pathPart);
                }
            }
            else if (c == ']')
            {
                if (i + 1 < jsonPath.Length)
                {
                    int arridx = int.Parse(strBuffer.ToString());
                    strBuffer.Clear();
                    //记录路径
                    string pathPart = string.Format("[{0}]", arridx);
                    pathPartList.Add(pathPart);
                }
            }
            else
            {
                strBuffer.Append(c);
            }
        }
        {
            string path = string.Join("", pathPartList);
            JToken parentToken = m_jsonObj.SelectToken(path);
            if (parentToken.Type == JTokenType.Object)
            {
                string objkey = strBuffer.ToString();
                strBuffer.Clear();
                //记录路径
                string pathPart = pathPartList.Count > 0 ? string.Format(".{0}", objkey) : objkey;
                pathPartList.Add(pathPart);
            }
            else if (parentToken.Type == JTokenType.Array)
            {
                int arridx = int.Parse(strBuffer.ToString());
                strBuffer.Clear();
                //记录路径
                string pathPart = string.Format("[{0}]", arridx);
                pathPartList.Add(pathPart);
            }
        }
        //3按照路径从下到上触发事件
        while (pathPartList.Count > 0)
        {
            string path = string.Join("", pathPartList);
            string templatePath = "";
            List<int> arrParamList = new List<int>();
            {
                Regex reg = new Regex(@"\[(\d+)\]");
                MatchCollection matchCollection = reg.Matches(path);
                foreach (Match match in matchCollection)
                {
                    string arridx = match.Groups[1].Value;
                    arrParamList.Add(int.Parse(arridx));
                }
                templatePath = reg.Replace(path, "[d]");
            }
            if (m_bindItemSet.ContainsKey(templatePath))
            {
                BindActionItem bindItem = m_bindItemSet[templatePath];
                bindItem.ChangeRecordSet[path] = arrParamList.ToArray();
                break;
            }
            pathPartList.RemoveAt(pathPartList.Count - 1);
        }
    }

    public void AddBind(string templatePath, System.Action<JToken, int[]> updateAction)
    {
        //绑定更新事件
        BindActionItem newItem = new BindActionItem(templatePath, updateAction);
        m_bindItemSet.Add(templatePath, newItem);
    }

    public void RemoveBind(string templatePath)
    {
        m_bindItemSet.Remove(templatePath);
    }

    public void ClearBind()
    {
        m_bindItemSet.Clear();
    }
}
