using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class JsonBinder
{
    private JToken m_jsonObj;
    private IDictionary<string, System.Action<JToken, int[]>> m_bindActions = new Dictionary<string, System.Action<JToken, int[]>>();

    public JsonBinder(JObject jObject)
    {
        m_jsonObj = jObject;
    }

    public JsonBinder(JArray jArray)
    {
        m_jsonObj = jArray;
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
        while(pathPartList.Count > 0)
        {
            string path = string.Join("", pathPartList);
            string templatePath = "";
            List<int> arrParamList = new List<int>();
            {
                Regex reg = new Regex(@"\[(\d+)\]");
                MatchCollection matchCollection = reg.Matches(path);
                foreach(Match match in matchCollection)
                {
                    string arridx = match.Groups[1].Value;
                    arrParamList.Add(int.Parse(arridx));
                }
                templatePath = reg.Replace(path, "[d]");
            }

            if (m_bindActions.ContainsKey(templatePath))
            {
                System.Action<JToken, int[]> bindAction = m_bindActions[templatePath];
                JToken pathValue = m_jsonObj.SelectToken(path);
                bindAction(pathValue, arrParamList.ToArray());
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

            if (m_bindActions.ContainsKey(templatePath))
            {
                System.Action<JToken, int[]> bindAction = m_bindActions[templatePath];
                JToken pathValue = m_jsonObj.SelectToken(path);
                bindAction(pathValue, arrParamList.ToArray());
                break;
            }
            pathPartList.RemoveAt(pathPartList.Count - 1);
        }
    }

    public void AddBind(string templatePath, System.Action<JToken, int[]> updateAction)
    {
        //绑定更新事件
        m_bindActions.Add(templatePath, updateAction);
    }

    public void RemoveBind(string templatePath)
    {
        //解绑更新事件
        m_bindActions.Remove(templatePath);
    }

    public void ClearBind()
    {
        m_bindActions.Clear();
    }
}
