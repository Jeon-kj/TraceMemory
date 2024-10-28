using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject PreGameCanvas;
    public GameObject QuestionCanvas;
    public GameObject SelectPlayerCanvas;
    public GameObject AuxiliaryCanvas;
    public GameObject MiniGame1;
    public GameObject MiniGame2;
    public GameObject MiniGame3;
    public GameObject AlwaysOnCanvas;

    public void TurnOffAndOn(GameObject obj1, GameObject obj2)
    {
        obj1.SetActive(false);
        obj2.SetActive(true);
    }
}
