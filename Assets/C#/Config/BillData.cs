using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 单据配置数据（ScriptableObject）
/// 每类单据（销售订单、生产工单等）创建一份资产，包含预填数据和角色按钮权限
/// 替代旧的 BillButtonConfig
/// </summary>
[CreateAssetMenu(fileName = "BillData_SalesOrder", menuName = "AirCompressor/BillData", order = 1)]
public class BillData : ScriptableObject
{
    [System.Serializable]
    public class TableRow
    {
        [Tooltip("该行的各列数据")]
        public string[] columns;
    }

    [System.Serializable]
    public class RoleButtonSetting
    {
        [Tooltip("角色名称，如：销售员、销售主管、仓管员、PMC、技术员、财务、生产主管、工人等")]
        public string roleName;

        [Tooltip("该角色在当前单据上可见的按钮")]
        public List<Interactables.ActionType> visibleButtons;
    }

    [System.Serializable]
    public class RolePrefillOverride
    {
        [Tooltip("角色名称")]
        public string roleName;

        [Tooltip("该角色对应的预填输入框数据")]
        public string[] prefillInputData;

        [Tooltip("该角色对应的预填表格数据")]
        public TableRow[] prefillTableData;

        [Tooltip("该角色对应的预览表格数据（打开时显示，点填写后替换）")]
        public TableRow[] previewTableData;

        [Tooltip("该角色对应的签字区域提示文字（如工人姓名），覆盖默认 signHintText")]
        public string signHintText;
    }

    [Header("单据标识")]
    [Tooltip("单据类型")]
    public UIManager.UIType billType;

    [Tooltip("单据名称（中文显示用）")]
    public string billName;

    [Header("预填数据")]
    [Tooltip("点击「填写」后自动填入输入框的内容")]
    public string[] prefillInputData;

    [Tooltip("点击「填写」后自动填入表格的行数据（完整数据，会替换预览数据）")]
    public TableRow[] prefillTableData;

    [Header("预览数据")]
    [Tooltip("打开单据时预先显示的表格行（部分数据，不点填写就能看到）。点填写后会被 prefillTableData 替换")]
    public TableRow[] previewTableData;

    [Header("上传附件")]
    [Tooltip("点击「填写」后切换显示的附件图片（如 PDF 图标）")]
    public Sprite fillAttachmentSprite;

    [Header("表格列配置")]
    [Tooltip("表格列标题（与prefillTableData每行的列一一对应）")]
    public string[] tableColumnHeaders;

    [Header("弹窗与横幅提示文字")]
    [Tooltip("点击「填写」后的横幅文字")]
    public string fillBannerText = "已填写";

    [Tooltip("点击「保存」后的横幅文字")]
    public string saveBannerText = "已保存成功！";

    [Tooltip("点击「提交」后的横幅文字")]
    public string submitBannerText = "已提交！";

    [Tooltip("点击「审核」后的横幅文字")]
    public string approveBannerText = "已审核！";

    [Tooltip("审核时确认弹窗的文字（如：是否下推？）")]
    public string approveConfirmText = "是否下推？";

    [Tooltip("审核下推后的横幅文字")]
    public string pushDownBannerText = "已下推！";

    [Tooltip("点击「发货」后的横幅文字")]
    public string shipBannerText = "已通知仓库发货";

    [Tooltip("是否允许发货（勾选后点击发货按钮直接通过，不勾选则弹警告）")]
    public bool shipAllowed = false;

    [Tooltip("发货条件不满足时的警告弹窗文字")]
    public string shipFailAlertText = "条件不满足，无法发货";

    [Tooltip("点击「签字」后的横幅文字")]
    public string signBannerText = "已签名！";

    [Header("签字区域")]
    [Tooltip("签字区域下方显示的文字（如工人姓名），由 GetSignHintForRole 获取")]
    public string signHintText = "";

    [Tooltip("勾选后点击「填写」也会显示签字区域文字（适用于领料单等工人自填自签的场景）；不勾选则只有「签字」按钮才显示")]
    public bool showSignHintOnFill = false;

    [Tooltip("签字按钮下方的固定文本内容（如声明文字），填写时显示。支持 \\n 换行")]
    [Multiline(3)]
    public string fillSignFixedContent = "";

    [Header("角色按钮权限")]
    [Tooltip("每种角色对应的按钮权限（精确匹配 + 模糊匹配）")]
    public List<RoleButtonSetting> roleSettings;

    [Header("角色预填数据覆盖")]
    [Tooltip("按角色覆盖预填数据（精确匹配 >模糊匹配），未匹配则使用默认 prefillInputData / prefillTableData")]
    public List<RolePrefillOverride> rolePrefillOverrides;

    /// <summary>
    /// 获取指定角色在当前单据上的可见按钮列表
    /// 匹配逻辑：精确匹配 >模糊匹配（Config角色名是Flow目标NPC的子串）
    /// </summary>
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
                Debug.Log($"[BillData] 模糊匹配: Config角色'{setting.roleName}' >Flow目标'{roleName}'");
                return setting.visibleButtons;
            }
        }

        Debug.LogWarning($"[BillData] 未找到角色'{roleName}'的按钮配置，使用默认空列表");
        return new List<Interactables.ActionType>();
    }

    /// <summary>获取角色对应的预填输入框数据，未匹配返回 null</summary>
    public string[] GetPrefillInputForRole(string roleName)
    {
        var matched = MatchRole(rolePrefillOverrides, roleName);
        return matched?.prefillInputData;
    }

    /// <summary>获取角色对应的预填表格数据，未匹配返回 null</summary>
    public TableRow[] GetPrefillTableForRole(string roleName)
    {
        var matched = MatchRole(rolePrefillOverrides, roleName);
        return matched?.prefillTableData;
    }

    /// <summary>获取角色对应的预览表格数据（打开时显示），未匹配返回默认 previewTableData</summary>
    public TableRow[] GetPreviewTableForRole(string roleName)
    {
        var matched = MatchRole(rolePrefillOverrides, roleName);
        return matched != null && matched.previewTableData != null && matched.previewTableData.Length > 0
            ? matched.previewTableData
            : previewTableData;
    }

    /// <summary>获取角色对应的签字区域提示文字，未匹配返回默认 signHintText</summary>
    public string GetSignHintForRole(string roleName)
    {
        var matched = MatchRole(rolePrefillOverrides, roleName);
        return matched != null && !string.IsNullOrEmpty(matched.signHintText)
            ? matched.signHintText
            : signHintText;
    }

    private RolePrefillOverride MatchRole(List<RolePrefillOverride> list, string roleName)
    {
        if (string.IsNullOrEmpty(roleName) || list == null) return null;

        // 精确匹配
        foreach (var item in list)
        {
            if (item.roleName.Equals(roleName, System.StringComparison.OrdinalIgnoreCase))
                return item;
        }
        // 模糊匹配
        foreach (var item in list)
        {
            if (!string.IsNullOrEmpty(item.roleName) &&
                roleName.Contains(item.roleName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[BillData] 预填模糊匹配: Config'{item.roleName}' >Flow'{roleName}'");
                return item;
            }
        }
        return null;
    }
}

/// <summary>
/// BillData 加载器（从 Resources/BillConfigs 加载所有 BillData 资产）
/// 替代旧的 BillButtonConfigLoader
/// </summary>
public static class BillDataLoader
{
    private static Dictionary<UIManager.UIType, BillData> _cache;

    public static void Initialize()
    {
        if (_cache != null) return;

        _cache = new Dictionary<UIManager.UIType, BillData>();
        var configs = Resources.LoadAll<BillData>("BillConfigs");

        foreach (var config in configs)
        {
            if (!_cache.ContainsKey(config.billType))
            {
                _cache[config.billType] = config;
                Debug.Log($"[BillDataLoader] 加载配置: {config.billType} >{config.billName}");
            }
            else
            {
                Debug.LogWarning($"[BillDataLoader] 重复配置: {config.billType}，跳过");
            }
        }

        Debug.Log($"[BillDataLoader] 初始化完成，共加载 {_cache.Count} 个单据配置");
    }

    public static BillData GetConfig(UIManager.UIType billType)
    {
        if (_cache == null) Initialize();

        if (_cache.TryGetValue(billType, out var config))
            return config;

        Debug.LogWarning($"[BillDataLoader] 未找到配置: {billType}");
        return null;
    }

    public static List<Interactables.ActionType> GetButtonsForRole(UIManager.UIType billType, string roleName)
    {
        var config = GetConfig(billType);
        return config != null ? config.GetButtonsForRole(roleName) : new List<Interactables.ActionType>();
    }

    /// <summary>
    /// 判断某单据是否已配置
    /// </summary>
    public static bool HasConfig(UIManager.UIType billType)
    {
        if (_cache == null) Initialize();
        return _cache.ContainsKey(billType);
    }
}
