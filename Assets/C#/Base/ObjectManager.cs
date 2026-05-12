using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 物体管理器
/// </summary>
public class ObjectManager : MonoBehaviour
{
    #region 单例
    private static ObjectManager _instance;
    public static ObjectManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("ObjectManager 未初始化!");
            return _instance;
        }
    }
    #endregion

    public enum ObjectType
    {
        Player,
        空压机,
        原料仓储,
        卡车
    }

    private Dictionary<ObjectType, GameObject> _objectDictionary = new Dictionary<ObjectType, GameObject>();
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeObjects();
    }

    public void InitializeObjects()
    {
        if (_isInitialized) return;

        _objectDictionary.Clear();
        int childCount = transform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (i < Enum.GetValues(typeof(ObjectType)).Length)
            {
                ObjectType objectType = (ObjectType)i;
                _objectDictionary.Add(objectType, child.gameObject);
                Debug.Log($"物体初始化: {objectType} -> {child.name}");
            }
        }
        _isInitialized = true;
    }

    public GameObject GetObject(ObjectType objectType)
    {
        if (!_isInitialized) InitializeObjects();
        if (_objectDictionary.TryGetValue(objectType, out GameObject obj))
            return obj;
        
        Debug.LogError($"找不到物体: {objectType}");
        return null;
    }

    public void RegisterObject(ObjectType objectType, GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogError("注册物体不能为空");
            return;
        }

        if (!_isInitialized) _isInitialized = true;

        if (_objectDictionary.ContainsKey(objectType))
            _objectDictionary[objectType] = targetObject;
        else
            _objectDictionary.Add(objectType, targetObject);
        
        Debug.Log($"物体注册: {objectType} -> {targetObject.name}");
    }

    public void UnregisterObject(ObjectType objectType)
    {
        if (_objectDictionary.ContainsKey(objectType))
        {
            _objectDictionary.Remove(objectType);
            Debug.Log($"物体移除: {objectType}");
        }
    }
}
