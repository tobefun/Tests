using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class ImageHelper
{
    static void SetPixel(Texture2D newer, int i, int j, Color c)
    {
        i = Mathf.Clamp(i, 0, newer.width);
        j = Mathf.Clamp(j, 0, newer.height);
        newer.SetPixel(i, j, c);
    }

    static Color HandleColor(Color c)
    {
        c.g = c.g + 0.01f;
        return c;
    }

    static void SaveTextureToFile(Texture2D texture, string fileName)
    {
        FileStream file = File.Open(fileName, FileMode.Open);

        if (fileName.EndsWith(".png"))
        {
            var bytes = texture.EncodeToPNG();
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
        }
        else
        {
            var bytes = texture.EncodeToJPG();
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
        }


        file.Close();
    }

    public static void ChangeImg(string file)
    {
        FileStream fs = File.Open(file, FileMode.Open);
        byte[] thebytes = new byte[fs.Length];

        fs.Read(thebytes, 0, (int)fs.Length);
        fs.Close();

        Texture2D temp = new Texture2D(1, 1);
        temp.LoadImage(thebytes);

        Color tempColor = Color.clear;

        for (int i = 0; i < temp.width; i++)
        {
            for (int j = 0; j < temp.height; j++)
            {
                tempColor = temp.GetPixel(i, j);

                temp.SetPixel(i, j, HandleColor(tempColor));
            }
        }

        int w = Random.Range(0, temp.width);
        int h = Random.Range(0, temp.height);
        tempColor = temp.GetPixel(w, h);
        tempColor = new Color(tempColor.r * 0.1f, tempColor.g, tempColor.b, tempColor.a);
        temp.SetPixel(w, h, tempColor);

        SaveTextureToFile(temp, file);
    }
}