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
        NativeGallery.GetImageFromGallery((file) => // ������ ����
        {
            if (string.IsNullOrEmpty(file)) // ����ڰ� ����� ���
            {
                Debug.Log("Image selection canceled.");
                return;
            }

            Debug.Log("opened");
            FileInfo selected = new FileInfo(file); // ���� ����

            // �뷮 ����
            if(selected.Length > 50000000) // 50,000,000byte = 50MB
            {
                return;
            }

            //�ҷ�����
            if (!string.IsNullOrEmpty(file))
            {
                // �ҷ���!
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
        tex.LoadImage(temp); // �츮�� ¥�� �ִ� �Լ����� �ٸ���.

        tex = ConvertToReadableTexture(tex);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)); // Raw image��� �̰� ����, img.texture = tex�� ��.

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
