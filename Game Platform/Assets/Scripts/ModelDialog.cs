using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelDialog : MonoBehaviour
{
    private System.Action<string> m_callback;

    public void ModelShow()
    {
        m_callback = (code) => {  };
        this.gameObject.SetActive(true);
    }
    public void ModelShow(System.Action<string> callback)
    {
        m_callback = callback;
        this.gameObject.SetActive(true);
    }
    public void ModelCancel()
    {
        m_callback("cancel");
        this.gameObject.SetActive(false);
    }
    public void ModelConfirm()
    {
        m_callback("confirm");
        this.gameObject.SetActive(false);
    }

}
