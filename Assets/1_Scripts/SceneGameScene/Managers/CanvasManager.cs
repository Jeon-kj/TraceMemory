using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{

    [Header("Canvases")]
    public GameObject DebugCanvas;
    public GameObject PreGameCanvas;
    public GameObject QuestionCanvas;
    public GameObject SelectPlayerCanvas;
    public GameObject AuxiliaryCanvas;
    public GameObject MiniGame1;
    public GameObject MiniGame2;
    public GameObject MiniGame3;
    public GameObject AlwaysOnCanvas;

    private string currCanvas = "";

    public void TurnOffAndOn(GameObject obj1, GameObject obj2)
    {
        Debug.Log($"obj1 : {obj1}, obj2 : {obj2}, AuxiliaryCanvas : {AuxiliaryCanvas}");
        if (obj1 != null) obj1.SetActive(false);
        if (obj2 != null) obj2.SetActive(true);
        if (obj2 != null && AuxiliaryCanvas.activeSelf == true && obj2 != AlwaysOnCanvas && obj2 != AuxiliaryCanvas)
            SetCurrCanvas(obj2.name);        
    }

    public void SetCurrCanvas(string canvasName)
    {
        currCanvas = canvasName;
        Debug.Log($"current Canvas Name is \"{currCanvas}\"");
    }

    public string GetCurrCanvas() { return  currCanvas; }
}
