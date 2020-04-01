using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelDialog : MonoBehaviour
{
    private System.Action<ModelResult> m_callback = (result) => { };
    public enum ModelResult { Default, Cancel, Confirm }

    public void ModelShow()
    {
        m_callback = (code) => {  };
        this.gameObject.SetActive(true);
    }
    public void ModelShow(System.Action<ModelResult> callback)
    {
        m_callback = callback;
        this.gameObject.SetActive(true);
    }
    public void ModelCancel()
    {
        m_callback(ModelResult.Cancel);
        this.gameObject.SetActive(false);
    }
    public void ModelConfirm()
    {
        m_callback(ModelResult.Confirm);
        this.gameObject.SetActive(false);
    }

}
