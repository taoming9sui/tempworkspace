using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public Dropdown dropdown;
    private IDictionary<Dropdown.OptionData, object> m_values = new Dictionary<Dropdown.OptionData, object>();

    public object GetSelectedValue()
    {
        int idx = dropdown.value;
        Dropdown.OptionData option = dropdown.options[idx];
        if (m_values.ContainsKey(option))
            return m_values[option];
        else
            return null;
    }

    public void AddItem(string text, object value)
    {
        Dropdown.OptionData option = new Dropdown.OptionData(text);
        dropdown.options.Add(option);
        m_values.Add(option, value);
    }

    public void ClearItems()
    {
        dropdown.ClearOptions();
        m_values.Clear();
    }

}
