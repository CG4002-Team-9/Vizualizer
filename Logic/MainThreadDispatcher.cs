using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;
    private ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

    public static MainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("MainThreadDispatcher");
                _instance = obj.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(obj);
                Debug.Log("MainThreadDispatcher instantiated.");
            }
            return _instance;
        }
    }

    public void Enqueue(Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        _actions.Enqueue(action);
    }

    void Update()
    {
        while (_actions.TryDequeue(out var action))
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error executing action on main thread: {ex.Message}");
            }
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MainThreadDispatcher Awake.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate MainThreadDispatcher destroyed.");
        }
    }
}