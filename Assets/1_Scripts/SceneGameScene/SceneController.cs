using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;


public class SceneController : MonoBehaviour
{
    public static string preLifeToTransfer;  // 전역으로 선언된 변수
    public static string genderToTransfer;

    CanvasManager canvasManager;

    private void Awake()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
    }

    public void TransferDataAndLoadScene(string data)
    {
        preLifeToTransfer = data;
        genderToTransfer = PhotonNetwork.LocalPlayer.GetPlayerGender();
        try
        {
            DebugCanvas.Instance.DebugLog("LoadScene");
            SceneManager.LoadScene("Identity", LoadSceneMode.Additive);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
        canvasManager.PreGameCanvas.GetComponent<PreGameCanvas>().SetActiveDisplay("readyToStart", false);
        canvasManager.TurnOffAndOn(canvasManager.PreGameCanvas, canvasManager.QuestionCanvas);
        canvasManager.QuestionCanvas.GetComponent<QuestionCanvas>().FirstTurnOnCanvas();
        canvasManager.TurnOffAndOn(null, canvasManager.AuxiliaryCanvas);
        canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().FirstTurnOnCanvas();
    }
}


