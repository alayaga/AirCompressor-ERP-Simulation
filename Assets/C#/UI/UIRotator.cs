using UnityEngine;

public class UIRotator : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("旋转速度（度/秒）")]
    [SerializeField]
    private float rotationSpeed = 90f;
    
    [Tooltip("是否启用自动旋转")]
    [SerializeField]
    private bool autoRotate = true;
    
    [Tooltip("是否顺时针旋转")]
    [SerializeField]
    private bool clockwise = true;
    
    void Update()
    {
        if (autoRotate)
        {
            RotateUI();
        }
    }

    private void RotateUI()
    {
        // 计算旋转角度
        float rotationAngle = rotationSpeed * Time.deltaTime;
        
        // 根据方向调整旋转角度
        if (!clockwise)
        {
            rotationAngle = -rotationAngle;
        }
        
        // 围绕Z轴旋转
        transform.Rotate(0f, 0f, rotationAngle);
    }
 
}