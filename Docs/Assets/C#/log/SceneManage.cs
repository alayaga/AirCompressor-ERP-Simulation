using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneManage : MonoBehaviour
{
    //音频组件
    public AudioSource audio;

    // 跳转到加载场景
    public void GoToMainScene()
    {
        // 1. 加载加载场景
        SceneManager.LoadScene("加载场景");
        
        
    }
    
    // 加载主菜单场景
    public void LoadMainMenuScene()
    {
        Debug.Log("加载主菜单场景");
        // 假设主菜单场景名称为"MainMenu"，可以根据实际情况修改
        //停止播放背景音乐
        audio.Stop();
        SceneManager.LoadScene("开始界面");
    }

    // 退出游戏
    public void ExitGame()
    {
        Debug.Log("游戏退出！"); 
        
        // 针对不同环境的退出处理
        #if UNITY_EDITOR
        // 在编辑器中停止播放
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 在实际应用中退出
        Application.Quit();
        #endif
    }
}