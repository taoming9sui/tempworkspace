using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerticalContainer : MonoBehaviour
{
    public Scrollbar scrollbar;
    public GameObject contentObj;

    private IList<GameObject> m_itemList = new List<GameObject>();

    public GameObject NewItem(GameObject templatePrefab)
    {
        //创建一个新对象
        GameObject newItem = GameObject.Instantiate(templatePrefab, contentObj.transform);
        m_itemList.Add(newItem);
        //滚动到底部
        contentObj.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        StartCoroutine(DoAction_Delay(() => {
            if (scrollbar.value < 0.2f || scrollbar.size > 0.5f)
                scrollbar.value = 0;
        }, 0.1f));
        //返回对象
        return newItem;
    }

    public void ClearItems()
    {
        //清空文本
        m_itemList.Clear();
        //清空UI对象
        foreach (Transform child in contentObj.transform)
            GameObject.Destroy(child.gameObject);
    }

    private IEnumerator DoAction_Delay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}
