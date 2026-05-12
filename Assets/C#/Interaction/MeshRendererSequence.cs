using UnityEngine;

/// <summary>
/// 物品依次显示脚本
/// 用于控制多个MeshRenderer按顺序依次显示
/// </summary>
public class MeshRendererSequence : MonoBehaviour
{
    [Header("渲染器配置")]
    [Tooltip("需要按顺序显示的MeshRenderer数组")]
    public MeshRenderer[] meshRenderers;
    
    /// <summary>
    /// 当前已显示的渲染器索引
    /// </summary>
    private int currentDisplayIndex = 0;
    
    /// <summary>
    /// 是否已显示所有渲染器
    /// </summary>
    public bool AllRenderersDisplayed { get; private set; }
    
    /// <summary>
    /// 获取当前已显示的渲染器数量
    /// </summary>
    public int DisplayedRendererCount => currentDisplayIndex + 1;
    
    private void Awake()
    {
        // 初始化所有渲染器状态
        InitializeRenderers();
    }
    
    /// <summary>
    /// 初始化渲染器状态
    /// 只显示第一个索引的渲染器，其他全部关闭
    /// </summary>
    private void InitializeRenderers()
    {
        // 重置状态
        currentDisplayIndex = 0;
        AllRenderersDisplayed = false;
        
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            Debug.LogWarning("MeshRenderer数组未设置，请在Inspector中添加MeshRenderer组件");
            return;
        }
        
        // 关闭所有渲染器
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
            {
                meshRenderers[i].enabled = false;
            }
        }
        
        // 只启用第一个渲染器（如果存在）
        if (meshRenderers[0] != null)
        {
            meshRenderers[0].enabled = true;
        }
        else
        {
            Debug.LogWarning("MeshRenderer数组的第一个元素为空引用");
        }
        
        // 如果只有一个渲染器，则标记为全部显示
        if (meshRenderers.Length == 1)
        {
            AllRenderersDisplayed = true;
        }
    }
    
    /// <summary>
    /// 公共方法：显示下一个MeshRenderer
    /// 每次调用时多显示一个渲染器，按数组索引顺序
    /// </summary>
    /// <returns>是否成功显示下一个渲染器</returns>
    public bool ShowNextRenderer()
    {
        // 检查是否已显示所有渲染器
        if (AllRenderersDisplayed)
        {
            Debug.Log("所有渲染器已经显示完毕");
            return false;
        }
        
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            Debug.LogWarning("MeshRenderer数组未设置");
            return false;
        }
        
        // 增加索引并检查边界
        currentDisplayIndex++;
        
        // 检查是否超出数组范围
        if (currentDisplayIndex < meshRenderers.Length)
        {
            // 启用下一个渲染器
            if (meshRenderers[currentDisplayIndex] != null)
            {
                meshRenderers[currentDisplayIndex].enabled = true;
                Debug.Log($"已显示索引为 {currentDisplayIndex} 的渲染器");
            }
            else
            {
                Debug.LogWarning($"索引 {currentDisplayIndex} 的MeshRenderer为空引用");
            }
            
            // 检查是否已显示所有渲染器
            if (currentDisplayIndex == meshRenderers.Length - 1)
            {
                AllRenderersDisplayed = true;
                Debug.Log("所有渲染器已全部显示");
            }
            
            return true;
        }
        else
        {
            // 超出范围，重置索引并标记为全部显示
            currentDisplayIndex = meshRenderers.Length - 1;
            AllRenderersDisplayed = true;
            Debug.Log("所有渲染器已经显示完毕");
            return false;
        }
    }
    
    /// <summary>
    /// 公共方法：重置所有渲染器到初始状态
    /// </summary>
    public void ResetSequence()
    {
        Debug.Log("重置渲染器显示序列");
        InitializeRenderers();
    }
    
    /// <summary>
    /// 公共方法：直接显示指定索引的渲染器
    /// 同时显示该索引之前的所有渲染器
    /// </summary>
    /// <param name="targetIndex">目标索引值</param>
    public void DisplayUpToIndex(int targetIndex)
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            Debug.LogWarning("MeshRenderer数组未设置");
            return;
        }
        
        // 验证索引有效性
        if (targetIndex < 0)
        {
            Debug.LogWarning("目标索引无效，将显示第一个渲染器");
            targetIndex = 0;
        }
        else if (targetIndex >= meshRenderers.Length)
        {
            Debug.LogWarning("目标索引超出范围，将显示所有渲染器");
            targetIndex = meshRenderers.Length - 1;
        }
        
        // 启用从0到目标索引的所有渲染器
        for (int i = 0; i <= targetIndex; i++)
        {
            if (meshRenderers[i] != null)
            {
                meshRenderers[i].enabled = true;
            }
            else
            {
                Debug.LogWarning($"索引 {i} 的MeshRenderer为空引用");
            }
        }
        
        // 禁用目标索引之后的所有渲染器
        for (int i = targetIndex + 1; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
            {
                meshRenderers[i].enabled = false;
            }
        }
        
        currentDisplayIndex = targetIndex;
        AllRenderersDisplayed = (currentDisplayIndex == meshRenderers.Length - 1);
        
        Debug.Log($"已显示到索引 {currentDisplayIndex} 的所有渲染器");
    }
    
    /// <summary>
    /// 检查渲染器数组配置是否有效
    /// </summary>
    /// <returns>配置是否有效</returns>
    public bool IsConfigurationValid()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            return false;
        }
        
        // 检查是否至少有一个有效的渲染器
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
            {
                return true;
            }
        }
        
        return false;
    }
}