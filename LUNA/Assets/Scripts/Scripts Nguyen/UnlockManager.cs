using System;
using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }
    public static event Action OnDataUpdated;

    private HashSet<string> unlockedKeys = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string GetKey(UnlockType type, string id) => $"{type}_{id}";

    public bool IsUnlocked(UnlockType type, string id)
    {
        return unlockedKeys.Contains(GetKey(type, id));
    }

    public void Unlock(UnlockType type, string id)
    {
        string key = GetKey(type, id);
        if (!unlockedKeys.Contains(key))
        {
            unlockedKeys.Add(key);
            OnDataUpdated?.Invoke();
        }
    }

    public void Lock(UnlockType type, string id)
    {
        string key = GetKey(type, id);
        if (unlockedKeys.Contains(key))
        {
            unlockedKeys.Remove(key);
            OnDataUpdated?.Invoke();
        }
    }

    public void ClearAll()
    {
        unlockedKeys.Clear();
        OnDataUpdated?.Invoke();
    }
}