using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.AdaptivePerformance.Provider;

public class LoadGallery : MonoBehaviour
{
    public Image img;

    public void OnClickImageLoad()
    {
        NativeGallery.GetImageFromGallery((file) => // 갤러리 열기
        {
            if (string.IsNullOrEmpty(file)) // 사용자가 취소한 경우
            {
                Debug.Log("Image selection canceled.");
                return;
            }

            Debug.Log("opened");
            FileInfo selected = new FileInfo(file); // 사진 고르기

            // 용량 제한
            if(selected.Length > 50000000) // 50,000,000byte = 50MB
            {
                return;
            }

            //불러오기
            if (!string.IsNullOrEmpty(file))
            {
                // 불러와!
                StartCoroutine(LoadImage(file));
            }
        });
    }

    IEnumerator LoadImage(string path)
    {
        yield return null;

        byte[] fileData = File.ReadAllBytes(path);
        string fileName = Path.GetFileName(path).Split('.')[0];
        string savePath = Application.persistentDataPath + "/Image"; // Debug.Log(Application.persistentDataPath);

        if(Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        File.WriteAllBytes(savePath + fileName + ".png", fileData);

        var temp = File.ReadAllBytes(savePath + fileName + ".png");

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(temp); // 우리가 짜고 있는 함수랑은 다른거.

        tex = ConvertToReadableTexture(tex);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)); // Raw image라면 이게 없고, img.texture = tex가 됨.

        img.sprite = sprite;
    }

    public Texture2D ConvertToReadableTexture(Texture2D original)
    {
        if (original.isReadable && original.format == TextureFormat.RGBA32)
        {
            return original;
        }

        Texture2D readableTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height);
        Graphics.Blit(original, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readableTexture;
    }
}
