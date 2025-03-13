using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IdentityCanvas : MonoBehaviour
{
    Text introduction1;
    Text introduction2;
    Text identity;
    Text partnerIdentity;
    Text touchScreen;
    Image portrait;
    Image partnerPortrait;

    bool ready = false;
    
    float fadeDuration = 2.0f; // ÆäÀÌµå ÀÎ ½Ã°£ (ÃÊ)
    float elapsedTime = 0f;
    Text currentText;
    Image currentImage;
    float touchScreenBlink;

    string preLifeName = SceneController.preLifeToTransfer;
    string playerGender = SceneController.genderToTransfer;
    string partnerName;

    Dictionary<string, string> maleDictionary = new Dictionary<string, string>() 
    {
        { "¸ù·æ", "ÃáÇâ" },
        { "°ß¿ì", "Á÷³à" },
        { "¿Â´Þ", "Æò°­" }
    };
    Dictionary<string, string> femaleDictionary = new Dictionary<string, string>()
    {
        { "ÃáÇâ", "¸ù·æ" },
        { "Á÷³à", "°ß¿ì" },
        { "Æò°­", "¿Â´Þ" }
    };

    private void Awake()
    {

        introduction1 = transform.Find("Introduction1").GetComponent<Text>();
        introduction2 = transform.Find("Introduction2").GetComponent<Text>();

        identity = transform.Find("Identity").GetComponent<Text>();
        partnerIdentity = transform.Find("PartnerIdentity").GetComponent<Text>();

        portrait = transform.Find("Portrait/Image").GetComponent<Image>();
        partnerPortrait = transform.Find("PartnerPortrait/Image").GetComponent<Image>();

        touchScreen = transform.Find("TouchScreen").GetComponent<Text>();
    }

    private void Start()
    {
        partnerName = playerGender == "Male" ? maleDictionary[preLifeName] : femaleDictionary[preLifeName];
        identity.text = identity.text.Replace("*", preLifeName);
        partnerIdentity.text = partnerIdentity.text.Replace("*", partnerName);
        portrait.sprite = Resources.Load<Sprite>(preLifeName);
        partnerPortrait.sprite = Resources.Load<Sprite>(partnerName);
        StartFadeIn(introduction1);
    }

    private void Update()
    {
        if (currentText != touchScreen && currentText != null)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            SetTextAlpha(currentText, alpha);
            if (currentImage != null) SetImageAlpha(currentImage, alpha);

            if (alpha >= 1f)
            {
                ContinueFadeInSequence();
            }
        }
        else if(currentText == touchScreen)
        {
            if (ready)
            {
                if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
                {
                    SceneManager.UnloadSceneAsync("Identity");
                }
            }
            elapsedTime += touchScreenBlink;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            SetTextAlpha(currentText, alpha);

            if (alpha >= 0.5f)
            {
                touchScreenBlink = -Time.deltaTime;
            }
            if(alpha <= 0f)
            {
                touchScreenBlink = Time.deltaTime;
            }
        }
    }

    private void SetTextAlpha(Text text, float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void StartFadeIn(Text text, Image image=null)
    {
        currentText = text;
        currentImage = image;
        elapsedTime = 0f;
    }

    private void ContinueFadeInSequence()
    {
        if (currentText == introduction1)
        {
            StartFadeIn(identity, portrait);
        }
        else if (currentText == identity)
        {
            StartFadeIn(introduction2);            
        }
        else if (currentText == introduction2)
        {
            StartFadeIn(partnerIdentity, partnerPortrait);
        }
        else if (currentText == partnerIdentity)
        {
            ready = true;
            touchScreenBlink = Time.deltaTime;
            StartFadeIn(touchScreen);            
        }
    }
}
