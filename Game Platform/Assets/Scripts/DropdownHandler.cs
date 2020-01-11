using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public Dropdown dropdown;
    private IDictionary<Dropdown.OptionData, object> m_values = new Dictionary<Dropdown.OptionData, object>();

    public T GetSelectedValue<T>()
    {
        int idx = dropdown.value;
        Dropdown.OptionData option = dropdown.options[idx];
        return (T)m_values[option];
    }

    public void AddItem<T>(string text, T value)
    {
        Dropdown.OptionData option = new Dropdown.OptionData(text);
        dropdown.options.Add(option);
        m_values.Add(option, text);
    }

    public void ClearItems()
    {
        dropdown.ClearOptions();
        m_values.Clear();
    }

}
