using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// UI管理器
/// </summary>
public class UIManager : MonoBehaviour
{
    #region 单例
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("UIManager 未初始化!");
            return _instance;
        }
    }
    #endregion

    public enum UIType
    {
        测试UI,
        通用UI,
        销售报价单,
        销售订单,
        生产工单,
        生产用料清单,
        生产用料申请单,
        生产领料单,
        物料需求单,
        采购需求申请单,
        采购申请单,
        采购订单,
        收料通知单,
        采购入库单,
        工序计划单,
        工序汇报单,
        生产汇报单,
        发货通知单,
        完工入库单,
        销售出库单,
        退出游戏UI
    }

    private Dictionary<UIType, GameObject> _uiDictionary = new Dictionary<UIType, GameObject>();
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeUI()
    {   
        if (_isInitialized) return;

        _uiDictionary.Clear();
        int childCount = transform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {   
            Transform child = transform.GetChild(i);
            if (i < Enum.GetValues(typeof(UIType)).Length)
            {   
                UIType uiType = (UIType)i;
                _uiDictionary.Add(uiType, child.gameObject);
                child.gameObject.SetActive(false);
                Debug.Log($"UI初始化: {uiType} -> {child.name}");
            }
        }
        _isInitialized = true;
    }

    public void ShowUI(UIType uiType)
    {   
        if (!_isInitialized) InitializeUI();
        if (_uiDictionary.TryGetValue(uiType, out GameObject uiObject))
        {   
            uiObject.SetActive(true);
            Debug.Log($"显示UI: {uiType}");
        }
        else
            Debug.LogError($"找不到UI: {uiType}");
    }
    
    public bool IsAnyOtherUIVisible(UIType excludeUIType)
    {   
        if (!_isInitialized) InitializeUI();
        
        foreach (var kvp in _uiDictionary)
        {   
            if (kvp.Key == excludeUIType) continue;
            if (kvp.Value.activeInHierarchy) return true;
        }
        return false;
    }

    public void HideUI(UIType uiType)
    {   
        if (!_isInitialized) InitializeUI();
        if (_uiDictionary.TryGetValue(uiType, out GameObject uiObject))
        {   
            uiObject.SetActive(false);
            Debug.Log($"隐藏UI: {uiType}");
        }
        else
            Debug.LogError($"找不到UI: {uiType}");
    }

    public void ShowOnlyUI(UIType uiType)
    {   
        if (!_isInitialized) InitializeUI();
        foreach (var ui in _uiDictionary.Values)
            ui.SetActive(false);
        ShowUI(uiType);
    }

    public GameObject GetUIObject(UIType uiType)
    {   
        if (!_isInitialized) InitializeUI();
        if (_uiDictionary.TryGetValue(uiType, out GameObject uiObject))
            return uiObject;
        
        Debug.LogError($"找不到UI: {uiType}");
        return null;
    }

    public bool IsUIVisible(UIType uiType)
    {   
        if (!_isInitialized) InitializeUI();
        if (_uiDictionary.TryGetValue(uiType, out GameObject uiObject))
            return uiObject.activeInHierarchy;
        return false;
    }
}
