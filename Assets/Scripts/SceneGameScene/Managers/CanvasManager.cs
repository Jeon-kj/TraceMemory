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

    private AuxiliaryCanvas auxiliaryCanvas;

    private void Awake()
    {
        auxiliaryCanvas = AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();
    }



    public void TurnOffAndOn(GameObject obj1, GameObject obj2)
    {
        Debug.Log($"obj1 : {obj1}, obj2 : {obj2}, AuxiliaryCanvas : {AuxiliaryCanvas}");
        if (obj1 != null) obj1.SetActive(false);
        if (obj2 != null) obj2.SetActive(true);
        if (AuxiliaryCanvas.activeSelf == true && obj2 != AlwaysOnCanvas && obj2 != AuxiliaryCanvas)
            auxiliaryCanvas.UpdateCurrCanvas(obj2.name);        
    }
}
