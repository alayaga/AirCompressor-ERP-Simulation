using System.Collections;
using UnityEngine;

public class ObjectMovementController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("目标位置数组")]
    public Vector3[] targetPositions;
    
    [Tooltip("移动到每个位置的时间（秒）")]
    public float moveDuration = 1.0f;
    
    [Tooltip("转向速度（度/秒）")]
    public float rotationSpeed = 90f;
    
    [Tooltip("是否使用平滑移动")]
    public bool useSmoothMovement = true;
    
    [Tooltip("是否启用转向效果")]
    public bool enableTurning = true;
    public IEnumerator MoveToPositionsSequentially(float delayBetweenMoves = 0f)
    {
        if (targetPositions == null || targetPositions.Length == 0)
        {
            Debug.LogWarning("目标位置数组为空，请设置目标位置");
            yield break;
        }

        // 保存初始位置
        Vector3 startPosition = transform.position;

        // 按照索引依次移动到各个位置
        for (int i = 0; i < targetPositions.Length; i++)
        {
            Debug.Log($"开始移动到位置索引: {i}");
            
            // 移动到目标位置
            yield return StartCoroutine(MoveToPosition(targetPositions[i]));
            
            Debug.Log($"完成移动到位置索引: {i}");
            
            // 等待指定时间后移动到下一个位置
            if (i < targetPositions.Length - 1) // 最后一个位置不需要等待
            {
                yield return new WaitForSeconds(delayBetweenMoves);
            }
        }

        Debug.Log("所有位置移动完成");
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        
        // 计算移动方向
        Vector3 direction = (targetPosition - startPosition).normalized;
        
        // 如果需要转向，先转向目标方向
        if (enableTurning && direction != Vector3.zero)
        {
            // 计算目标旋转
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // 转向到目标方向
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // 移动到目标位置
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            if (useSmoothMovement)
            {
                // 使用平滑插值
                t = Mathf.SmoothStep(0f, 1f, t);
            }

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            // 在移动过程中持续微调朝向，保持面向移动方向
            if (enableTurning)
            {
                Vector3 currentDirection = (targetPosition - transform.position).normalized;
                if (currentDirection != Vector3.zero)
                {
                    Quaternion currentTargetRotation = Quaternion.LookRotation(currentDirection);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, currentTargetRotation, rotationSpeed * Time.deltaTime * 0.5f);
                }
            }
            
            yield return null;
        }

        // 确保最终位置准确
        transform.position = targetPosition;
    }
}