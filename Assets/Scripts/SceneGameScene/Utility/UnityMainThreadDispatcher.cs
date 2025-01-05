using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                Action action = _executionQueue.Dequeue();
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error executing action: " + ex.ToString());
                }
            }
        }
    }

    public static void Enqueue(Action action)
    {
        if (action == null)
            throw new ArgumentNullException("action");

        Debug.Log("Enqueue action: " + action.Method.ToString());
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}
