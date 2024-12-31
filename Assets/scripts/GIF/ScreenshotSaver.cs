using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
//using UnityEditor.SearchService;


//using System.Drawing;
//using System.Drawing.Imaging;

//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats.Gif;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;

public class ScreenshotSaver : MonoBehaviour
{
    public static List<byte[]> jpgList = new List<byte[]>();
    private List<string> filenames = new List<string>();
    public string NI = "screensaver";
    
    string path;// = @"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\prototipando\prototipando\prototipando\imagens salvas";
    public int frameRate = 10; // Frames per second for GIF
    private List<Texture2D> capturedFrames;// = new List<Texture2D>();
    public GameObject menu_foradafoto;


    [DllImport("__Internal")]
    private static extern void SaveFile(string filename, byte[] content, int length);

    [DllImport("__Internal")]
    private static extern void SaveAllFiles(string[] filenames, byte[][] contents, int[] lengths, int count);


    [DllImport("__Internal")]
    private static extern void DownloadImage(string base64String, string fileName);

    public ScreenshotSaver()
    {
        path = @"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\prototipando\prototipando\prototipando\imagens salvas";
    }

    public void Start()
    {
        //menu_foradafoto = GameObject.Find("PainelMorfogenese");
        //Debug.Log("start screenshot, menu: " + menu_foradafoto != null);
    }
    public void CaptureAndSaveScreenshot(string filename)
    {
        StartCoroutine(CaptureScreenshotCoroutine(filename));
    }

    public IEnumerator FotoTela(string filename)
    {
        menu_foradafoto = GameObject.Find("Canvas");
//        Debug.Log("fototela screenshot, menu: " + (menu_foradafoto != null));


        if (menu_foradafoto != null)
        {
            menu_foradafoto.SetActive(false);
        }
//        Debug.Log("menu ativo: " + menu_foradafoto.activeSelf);
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        //private List<Texture2D> capturedFrames;// = new List<Texture2D>();

        capturedFrames = new List<Texture2D>();
        capturedFrames.Add(screenshot);
        byte[] jpgBytes = screenshot.EncodeToJPG();

        System.IO.File.WriteAllBytes(path + filename+".jpg", jpgBytes);
//        Debug.Log("imagem salva " + filename + " em: " + path);


        jpgList.Add(jpgBytes);
        filenames.Add(filename);

//        Debug.Log("salvando imagens " + jpgList.Count);

        
//        string base64String = System.Convert.ToBase64String(jpgBytes);
//        string fileName = "screenshot.jpg";

        // Chamada à função JavaScript
        //        DownloadImage(base64String, fileName);
        if (menu_foradafoto != null)
        {
            menu_foradafoto.SetActive(true);
        }

        Destroy(screenshot);
    }

    public void SaveGIF()
    {

        string path = System.IO.Path.Combine(Application.dataPath, "screenshot.gif");
        AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();

        try
        {
            // Criar um FileStream para o arquivo GIF
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                gifEncoder.Start(fs);
                Debug.Log("GIF Encoder started.");
                gifEncoder.SetDelay(1000 / frameRate);
                gifEncoder.SetRepeat(0);

                foreach (var frame in capturedFrames)
                {
                    gifEncoder.AddFrame(frame);
                    Debug.Log("Frame added.");
                }

                gifEncoder.Finish();
                Debug.Log("GIF Encoder finished.");
            }
            Debug.Log("GIF saved at: " + path);

        }

        catch (IOException e)
        {
            Debug.LogError("Failed to save GIF: " + e.Message);
        }
    }

    private IEnumerator CaptureScreenshotCoroutine(string filename)
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();


        byte[] jpgBytes = screenshot.EncodeToJPG();

        System.IO.File.WriteAllBytes(filename+"L.jpg", jpgBytes);

        jpgList.Add(jpgBytes);
        filenames.Add(filename);

        Debug.Log("salvando imagens " + jpgList.Count);


        Destroy(screenshot);
    }


public void ZiparImagens()//string zipFilePath)
{
        //        string path = @"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\prototipando\prototipando\prototipando\imagens salvas";
        string zipFilePath = Path.Combine(path, "cenasaglomeradas.zip");


        using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
    {
        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
        {
            for (int i = 0; i < jpgList.Count; i++)
            {
                string entryName = $"image_{i + 1}.jpg";
                ZipArchiveEntry zipEntry = archive.CreateEntry(entryName);

                using (BinaryWriter writer = new BinaryWriter(zipEntry.Open()))
                {
                    writer.Write(jpgList[i]);
                }
            }
        }
    }

    Debug.Log($"ZIP file created at: {zipFilePath}");
}


    //public void SaveAllScreenshotsAsZip()
    //{
    //    int count = jpgList.Count;
    //    int[] lengths = new int[count];
    //    for (int i = 0; i < count; i++)
    //    {
    //        lengths[i] = jpgList[i].Length;
    //    }
    //    Debug.Log("salvando imagens zip" + jpgList.Count);
    //    SaveAllFiles(filenames.ToArray(), jpgList.ToArray(), lengths, count);
    //}
}
