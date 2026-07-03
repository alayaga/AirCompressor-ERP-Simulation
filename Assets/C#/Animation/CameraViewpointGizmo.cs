using UnityEngine;

/// <summary>
/// 挂载在相机观看位置锚点空物体上，Scene窗口可视化相机视野
/// 方便在编辑器中调试摆放角度
/// </summary>
[ExecuteInEditMode]
public class CameraViewpointGizmo : MonoBehaviour
{
    [Header("预览设置")]
    [Tooltip("Scene窗口中显示的相机视锥颜色")]
    public Color gizmoColor = new Color(0f, 1f, 0.8f, 0.5f);
    [Tooltip("视锥长度倍数（越大线越长）")]
    public float frustumLength = 2f;
    [Tooltip("FOV（模拟的视野角）")]
    public float fieldOfView = 60f;

    [Header("对齐工具")]
    [Tooltip("点击后Scene视角会临时切到此锚点视角（在Inspector中无效，需配合菜单使用）")]
    public bool previewInGameWindow = false;

    private void OnDrawGizmos()
    {
        // 画一个相机图标位置
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.08f);

        // 画视锥线
        float halfFov = fieldOfView * 0.5f * Mathf.Deg2Rad;
        float h = Mathf.Tan(halfFov) * frustumLength;
        float aspect = 16f / 9f;
        float w = h * aspect;

        Vector3 pos = transform.position;
        Vector3 forward = transform.forward * frustumLength;
        Vector3 up = transform.up * h;
        Vector3 right = transform.right * w;

        Vector3 farCenter = pos + forward;
        Vector3 topLeft = farCenter + up - right;
        Vector3 topRight = farCenter + up + right;
        Vector3 bottomLeft = farCenter - up - right;
        Vector3 bottomRight = farCenter - up + right;

        // 四条边线（从相机位置到远平面四角）
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(pos, topLeft);
        Gizmos.DrawLine(pos, topRight);
        Gizmos.DrawLine(pos, bottomLeft);
        Gizmos.DrawLine(pos, bottomRight);

        // 远平面矩形
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // 中心方向线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, farCenter);

        // 标签
#if UNITY_EDITOR
        UnityEditor.Handles.Label(pos + Vector3.up * 0.3f, gameObject.name,
            new GUIStyle() { normal = new GUIStyleState() { textColor = gizmoColor }, fontSize = 12 });
#endif
    }
}
