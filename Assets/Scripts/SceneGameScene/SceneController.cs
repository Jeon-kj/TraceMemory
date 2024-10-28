using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;


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
        SceneManager.LoadScene("Identity", LoadSceneMode.Additive);
        canvasManager.TurnOffAndOn(canvasManager.PreGameCanvas, canvasManager.QuestionCanvas);
    }
}


