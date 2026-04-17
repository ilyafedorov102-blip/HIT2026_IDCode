using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonScene : MonoBehaviour
{
    public string scene;
    public void Button_Scene()
    {
        SceneManager.LoadScene(scene);
    }
    public void Button_Exit()
    {
        Application.Quit();
    }
}
