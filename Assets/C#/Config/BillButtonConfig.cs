using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BillButtonConfig", menuName = "AirCompressor/BillButtonConfig", order = 1)]
public class BillButtonConfig : ScriptableObject
{
    [System.Serializable]
    public class RoleButtonSetting
    {
        [Tooltip("角色名称，如：销售员、销售主管、仓管员、PMC、技术员、财务、生产主管、工人等")]
        public string roleName;

        [Tooltip("该角色在当前单据上可见的按钮")]
        public List<Interactables.ActionType> visibleButtons;
    }

    [Header("单据配置")]
    [Tooltip("单据类型")]
    public UIManager.UIType billType;

    [Tooltip("单据名称（中文显示用）")]
    public string billName;

    [Header("角色按钮配置")]
    [Tooltip("每种角色对应的按钮权限")]
    public List<RoleButtonSetting> roleSettings;

    public List<Interactables.ActionType> GetButtonsForRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            return new List<Interactables.ActionType>();

        // 1. 精确匹配
        foreach (var setting in roleSettings)
        {
            if (setting.roleName.Equals(roleName, System.StringComparison.OrdinalIgnoreCase))
                return setting.visibleButtons;
        }

        // 2. 模糊匹配：Config角色名是Flow目标NPC的子串
        //    例: Config"PMC" 匹配 Flow"PMC主管"
        //    例: Config"仓管员" 匹配 Flow"仓管员A"/"仓管员B"
        //    例: Config"班组长" 匹配 Flow"车间班组长"/"1车间班组长"
        //    例: Config"工人" 匹配 Flow"1车间工人"/"2车间工人"
        foreach (var setting in roleSettings)
        {
            if (!string.IsNullOrEmpty(setting.roleName) &&
                roleName.Contains(setting.roleName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[BillButtonConfig] 模糊匹配: Config角色'{setting.roleName}' → Flow目标'{roleName}'");
                return setting.visibleButtons;
            }
        }

        Debug.LogWarning($"[BillButtonConfig] 未找到角色'{roleName}'的按钮配置");
        return new List<Interactables.ActionType>();
    }

    public bool HasButton(string roleName, Interactables.ActionType buttonType)
    {
        var buttons = GetButtonsForRole(roleName);
        return buttons.Contains(buttonType);
    }
}

public static class BillButtonConfigLoader
{
    private static Dictionary<UIManager.UIType, BillButtonConfig> _configCache;

    public static void Initialize()
    {
        if (_configCache != null) return;

        _configCache = new Dictionary<UIManager.UIType, BillButtonConfig>();
        var configs = Resources.LoadAll<BillButtonConfig>("BillConfigs");

        foreach (var config in configs)
        {
            if (!_configCache.ContainsKey(config.billType))
            {
                _configCache[config.billType] = config;
                Debug.Log($"[BillButtonConfigLoader] 加载配置: {config.billType}");
            }
        }
    }

    public static BillButtonConfig GetConfig(UIManager.UIType billType)
    {
        if (_configCache == null)
        {
            Initialize();
        }

        if (_configCache.TryGetValue(billType, out var config))
        {
            return config;
        }

        Debug.LogWarning($"[BillButtonConfigLoader] 未找到配置: {billType}");
        return null;
    }

    public static List<Interactables.ActionType> GetButtonsForRole(UIManager.UIType billType, string roleName)
    {
        var config = GetConfig(billType);
        if (config != null)
        {
            return config.GetButtonsForRole(roleName);
        }
        return new List<Interactables.ActionType>();
    }
}