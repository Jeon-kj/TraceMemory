using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.UI;

public class PlayerProperties : MonoBehaviour
{
    //public InputField inputName;
    //public Image inputImg;

    private string playerName;
    private Texture2D playerImage;
    private string playerImageFileName;
    private string playerGender;

    public void SetName(string name) { playerName = name; }
    public void SetImage(Texture2D image) { playerImage = image; }

    public void SetImageFileName(string imagefileName) { playerImageFileName = imagefileName; }
    public void GenderIsMale() { playerGender = "Male"; }
    public void GenderIsFemale() { playerGender = "Female"; }

    
    public string GetName() { return playerName; }

    public Texture2D GetImage() { return playerImage; }

    public string GetImageFileName() { return playerImageFileName; }

    public string GetGender() { return playerGender; }
}
