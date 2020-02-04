using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadInvoker : MonoBehaviour {

    readonly object Lock = new object();
    readonly static Queue<Action> ToBeDispatched = new Queue<Action>();

    public void Invoke(Action action)
    {
        lock (Lock)
        {
            ToBeDispatched.Enqueue(action);
        }
    }

    private void doExecuteOnMainThread()
    {
        lock (Lock)
        {
            while (ToBeDispatched.Count > 0)
            {
                Action action = ToBeDispatched.Dequeue();
                action();
            }
        }
    }
    private void Update()
    {
        doExecuteOnMainThread();
        OnUpdate();
    }
    protected virtual void OnUpdate() { }

    /*private void FixedUpdate()
    {
        doExecuteOnMainThread();
        OnFixedUpdate();
    }
    protected virtual void OnFixedUpdate() { }

    private void LateUpdate()
    {
        doExecuteOnMainThread();
        OnLateUpdate();
    }
    protected virtual void OnLateUpdate() { }*/
}
