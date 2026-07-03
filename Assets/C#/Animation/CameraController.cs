using UnityEngine;
using System.Collections;

/// <summary>
/// 双相机控制器：动画播放时切换到动画相机，Lerp到观看位置
/// 动画相机会自动创建，无需手动配置
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("玩家的主相机（MainCamera），留空自动查找")]
    public Camera playerCamera;

    [Header("Settings")]
    [Tooltip("相机过渡时长（秒）")]
    public float blendDuration = 1.5f;

    private Camera _animCamera;            // 动画相机（自动创建）
    private Vector3 _playerCamOriginalPos;
    private Quaternion _playerCamOriginalRot;
    private bool _isBlending = false;
    private AudioListener _playerAudioListener;
    private AudioListener _animAudioListener;

    private void Start()
    {
        // 自动查找玩家相机
        if (playerCamera == null)
        {
            var playerObj = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
            if (playerObj != null)
                playerCamera = playerObj.GetComponentInChildren<Camera>();
        }
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
            _playerAudioListener = playerCamera.GetComponent<AudioListener>();

        // 自动创建动画相机
        if (_animCamera == null)
        {
            var camGo = new GameObject("__AnimationCamera__");
            camGo.transform.SetParent(transform);
            _animCamera = camGo.AddComponent<Camera>();
            _animCamera.enabled = false;
            // 复制玩家相机的关键设置
            if (playerCamera != null)
            {
                _animCamera.fieldOfView = playerCamera.fieldOfView;
                _animCamera.nearClipPlane = playerCamera.nearClipPlane;
                _animCamera.farClipPlane = playerCamera.farClipPlane;
                _animCamera.clearFlags = playerCamera.clearFlags;
                _animCamera.backgroundColor = playerCamera.backgroundColor;
                _animCamera.cullingMask = playerCamera.cullingMask;
            }
            _animAudioListener = camGo.AddComponent<AudioListener>();
            _animAudioListener.enabled = false;
        }
    }

    public void SwitchToViewpoint(Transform targetViewpoint, System.Action onComplete = null)
    {
        if (_isBlending || targetViewpoint == null)
        {
            Debug.LogWarning($"[CameraController] 无法切换: isBlending={_isBlending}, viewpoint={(targetViewpoint != null ? "OK" : "NULL")}");
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(BlendToViewpoint(targetViewpoint, onComplete));
    }

    public void RestoreToPlayer(System.Action onComplete = null)
    {
        if (_isBlending)
        {
            Debug.LogWarning("[CameraController] 正在过渡中，无法恢复");
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(BlendToPlayer(onComplete));
    }

    public bool IsBlending => _isBlending;

    private IEnumerator BlendToViewpoint(Transform target, System.Action onComplete)
    {
        _isBlending = true;
        Debug.Log("[CameraController] 开始切换到观看视角");

        // 确保动画相机存在
        if (_animCamera == null)
        {
            Debug.LogError("[CameraController] 动画相机不存在！");
            _isBlending = false;
            onComplete?.Invoke();
            yield break;
        }

        // 保存玩家相机状态
        _playerCamOriginalPos = playerCamera.transform.position;
        _playerCamOriginalRot = playerCamera.transform.rotation;

        // 动画相机从玩家当前位置起步
        _animCamera.transform.SetPositionAndRotation(
            playerCamera.transform.position,
            playerCamera.transform.rotation);
        _animCamera.enabled = true;

        // AudioListener 切换
        if (_animAudioListener != null) _animAudioListener.enabled = true;
        if (_playerAudioListener != null) _playerAudioListener.enabled = false;

        // 禁用玩家相机
        playerCamera.enabled = false;

        // Lerp 到目标
        Vector3 startPos = _animCamera.transform.position;
        Quaternion startRot = _animCamera.transform.rotation;
        float elapsed = 0f;
        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / blendDuration);
            _animCamera.transform.SetPositionAndRotation(
                Vector3.Lerp(startPos, target.position, t),
                Quaternion.Slerp(startRot, target.rotation, t));
            yield return null;
        }
        _animCamera.transform.SetPositionAndRotation(target.position, target.rotation);

        Debug.Log("[CameraController] 观看视角就位");
        _isBlending = false;
        onComplete?.Invoke();
    }

    private IEnumerator BlendToPlayer(System.Action onComplete)
    {
        _isBlending = true;
        Debug.Log("[CameraController] 开始恢复玩家视角");

        Vector3 startPos = _animCamera.transform.position;
        Quaternion startRot = _animCamera.transform.rotation;
        float elapsed = 0f;
        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / blendDuration);
            _animCamera.transform.SetPositionAndRotation(
                Vector3.Lerp(startPos, _playerCamOriginalPos, t),
                Quaternion.Slerp(startRot, _playerCamOriginalRot, t));
            yield return null;
        }

        // 恢复玩家相机
        playerCamera.enabled = true;
        if (_animCamera != null) _animCamera.enabled = false;

        // AudioListener 切换回
        if (_animAudioListener != null) _animAudioListener.enabled = false;
        if (_playerAudioListener != null) _playerAudioListener.enabled = true;

        Debug.Log("[CameraController] 已恢复到玩家视角");
        _isBlending = false;
        onComplete?.Invoke();
    }
}
