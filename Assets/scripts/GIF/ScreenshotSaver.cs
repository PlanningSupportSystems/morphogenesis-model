using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Linq;
//using UnityEditor.SearchService;


//using System.Drawing;
//using System.Drawing.Imaging;

//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats.Gif;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;

public class ScreenshotSaver : MonoBehaviour
{
    public static List<byte[]> imageList = new List<byte[]>();
    private List<string> filenames = new List<string>();
    public string NI = "screensaver";

    string path;// = @"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\prototipando\prototipando\prototipando\imagens salvas";
                //C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\@PSS\morphogenesis model\@imagens geradas\novas img
    public int frameRate = 10; // Frames per second for GIF
    private List<Texture2D> capturedFrames;// = new List<Texture2D>();
    public GameObject menu_foradafoto;


//    [DllImport("__Internal")]
//    private static extern void SaveFile(string filename, byte[] content, int length);

//    [DllImport("__Internal")]
//    private static extern void SaveAllFiles(string[] filenames, byte[][] contents, int[] lengths, int count);

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFramesZip(string framesJson, string zipName);
    [DllImport("__Internal")]
    private static extern void DownloadFramesGif(string framesJson, string gifName, int delayMs);

    [DllImport("__Internal")]
    private static extern void DownloadFramesZipWithGif(string framesJson, string zipName, string gifName, int delayMs);

#endif

    //    [DllImport("__Internal")]
    //private static extern void DownloadImage(string base64String, string fileName);

    public ScreenshotSaver()
    {
         //@"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\prototipando\prototipando\prototipando\imagens salvas";
         //@"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\@PSS\morphogenesis model\@imagens geradas\novas img";
        path = @"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\@PSS\morphogenesis model\@imagens geradas\sprint webgl\";
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
//        menu_foradafoto = GameObject.Find("Canvas");
//        //        Debug.Log("fototela screenshot, menu: " + (menu_foradafoto != null));
//        if (menu_foradafoto != null)
//        {
//            menu_foradafoto.SetActive(false);
//        }
//        //        Debug.Log("menu ativo: " + menu_foradafoto.activeSelf);

        yield return new WaitForEndOfFrame();

        RawImage rawImage = GameObject.Find("RawImage_Mundo")?.GetComponent<RawImage>();
        RenderTexture rt = rawImage != null ? rawImage.texture as RenderTexture : null;

        if (rt == null)
        {
            Debug.LogWarning("FotoTela: RawImage_Mundo sem RenderTexture.");
            yield break;
        }

        RenderTexture anterior = RenderTexture.active;
        Texture2D screenshot = null;// new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        try
        {
            RenderTexture.active = rt;

            screenshot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
//            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

//            //private List<Texture2D> capturedFrames;// = new List<Texture2D>();
//            capturedFrames = new List<Texture2D>();
//            capturedFrames.Add(screenshot);

            byte[] pngBytes = screenshot.EncodeToPNG();

            Directory.CreateDirectory(path);
            filename = imageList.Count.ToString("D4") + " " + filename;
            string filePath = Path.Combine(path, filename + ".png");
            File.WriteAllBytes(filePath, pngBytes);

//            System.IO.File.WriteAllBytes(path + filename + ".jpg", pngBytes);
            //        Debug.Log("imagem salva " + filename + " em: " + path);

            imageList.Add(pngBytes);
            filenames.Add(filename);
        }
        finally
        {
            RenderTexture.active = anterior;

            if (screenshot != null)
                Destroy(screenshot);
        }

        //        Debug.Log("salvando imagens " + imageList.Count);

        //        string base64String = System.Convert.ToBase64String(pngBytes);
        //        string fileName = "screenshot.jpg";

        // Chamada à função JavaScript
        //        DownloadImage(base64String, fileName);
        //        if (menu_foradafoto != null)
        //        {
        //            menu_foradafoto.SetActive(true);
        //        }

    }

    public void SaveGIF()
    {
        if (imageList == null || imageList.Count == 0)
        {
            Debug.LogWarning("SaveGIF: nenhuma imagem foi gerada.");
            return;
        }

        Directory.CreateDirectory(path);
        string gifPath = Path.Combine(path, "cenasaglomeradas.gif");

        AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();

        try
        {
            // Criar um FileStream para o arquivo GIF
            using (FileStream fs = new FileStream(gifPath, FileMode.Create))
            {
                gifEncoder.Start(fs);
                Debug.Log("GIF Encoder started.");
                gifEncoder.SetDelay(1000 / frameRate);
                gifEncoder.SetRepeat(5);
                gifEncoder.SetDispose(1);

                var framesOrdenados = filenames
                    .Select((nome, i) => new { nome, bytes = imageList[i] })
                    .OrderBy(f => f.nome)
                    .ToList();

                foreach (var frameData in framesOrdenados)
//                    foreach (byte[] imageBytes in imageList)
                    {
                        Texture2D frame = new Texture2D(2, 2, TextureFormat.RGB24, false);
                    frame.LoadImage(frameData.bytes);

                    gifEncoder.AddFrame(frame);

                    Destroy(frame);
                }

                gifEncoder.Finish();
                Debug.Log("GIF Encoder finished.");
            }
            Debug.Log("GIF saved at: " + gifPath);

        }

        catch (IOException e)
        {
            Debug.LogError("Failed to save GIF: " + e.Message);
        }
    }

    public void BaixarZipWebGL()
    {
        if (imageList == null || imageList.Count == 0)
        {
            Debug.LogWarning("BaixarZipWebGL: nenhuma imagem foi gerada.");
            return;
        }

        var framesOrdenados = filenames
            .Select((nome, i) => new { nome, bytes = imageList[i] })
            .OrderBy(f => f.nome)
            .ToList();

        List<string> itens = new List<string>();

        foreach (var frame in framesOrdenados)
        {
            string nome = frame.nome.Replace("\\", "_").Replace("\"", "'");
            string base64 = System.Convert.ToBase64String(frame.bytes);
            itens.Add("{\"name\":\"" + nome + ".png\",\"data\":\"" + base64 + "\"}");
        }

        string json = "[" + string.Join(",", itens) + "]";

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFramesZip(json, "cenasaglomeradas.zip");
#else
    ZiparImagens();
#endif
    }

    public void BaixarGifWebGL()
    {
        if (imageList == null || imageList.Count == 0)
        {
            Debug.LogWarning("BaixarGifWebGL: nenhuma imagem foi gerada.");
            return;
        }

        var framesOrdenados = filenames
            .Select((nome, i) => new { nome, bytes = imageList[i] })
            .OrderBy(f => f.nome)
            .ToList();

        List<string> itens = new List<string>();

        foreach (var frame in framesOrdenados)
        {
            string nome = frame.nome.Replace("\\", "_").Replace("\"", "'");
            string base64 = System.Convert.ToBase64String(frame.bytes);
            itens.Add("{\"name\":\"" + nome + ".png\",\"data\":\"" + base64 + "\"}");
        }

        string json = "[" + string.Join(",", itens) + "]";
        int delayMs = 1000 / 2;// frameRate;

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFramesGif(json, "cenasaglomeradas.gif", delayMs);
#else
    Debug.Log("BaixarGifWebGL: no Editor, o GIF via JavaScript so roda no WebGL/browser.");
#endif
    }

    public void BaixarZipComGifWebGL()
    {
        if (imageList == null || imageList.Count == 0)
        {
            Debug.LogWarning("BaixarZipComGifWebGL: nenhuma imagem foi gerada.");
            return;
        }

        var framesOrdenados = filenames
            .Select((nome, i) => new { nome, bytes = imageList[i] })
            .OrderBy(f => f.nome)
            .ToList();

        List<string> itens = new List<string>();

        foreach (var frame in framesOrdenados)
        {
            string nome = frame.nome.Replace("\\", "_").Replace("\"", "'");
            string base64 = System.Convert.ToBase64String(frame.bytes);
            itens.Add("{\"name\":\"" + nome + ".png\",\"data\":\"" + base64 + "\"}");
        }

        string json = "[" + string.Join(",", itens) + "]";
        int delayMs = 1000 / frameRate;

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFramesZipWithGif(json, "cenasaglomeradas.zip", "cenasaglomeradas.gif", delayMs);
#else
    ZiparImagens();
#endif
    }

    public bool TemImagens()
    {
        return imageList != null && imageList.Count > 0;
    }

    private IEnumerator CaptureScreenshotCoroutine(string filename)
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();


        byte[] jpgBytes = screenshot.EncodeToJPG();

        System.IO.File.WriteAllBytes(filename + "L.jpg", jpgBytes);

        imageList.Add(jpgBytes);
        filenames.Add(filename);

        Debug.Log("salvando imagens " + imageList.Count);


        Destroy(screenshot);
    }


    public void ZiparImagens()//string zipFilePath)
    {
        if (imageList == null || imageList.Count == 0)
        {
            Debug.LogWarning("ZiparImagens: nenhuma imagem foi gerada.");
            return;
        }

        if (filenames == null || filenames.Count != imageList.Count)
        {
            Debug.LogWarning("ZiparImagens: lista de nomes inconsistente com a lista de imagens.");
            return;
        }

        Directory.CreateDirectory(path);
        //        string path = @"C:\Users\danie\OneDrive\projeto vinicius uff\desenvolvimento\prototipando\prototipando\prototipando\imagens salvas";
        string zipFilePath = Path.Combine(path, "cenasaglomeradas.zip");

        using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
        {
            for (int i = 0; i < imageList.Count; i++)
            {
                string entryName = filenames[i] + ".png";
                ZipArchiveEntry zipEntry = archive.CreateEntry(entryName);

                using (BinaryWriter writer = new BinaryWriter(zipEntry.Open()))
                {
                    writer.Write(imageList[i]);
                }
            }
        }

        Debug.Log($"ZIP file created at: {zipFilePath}");
    }

    public void inicializarImagens()
    {
        imageList.Clear();
        filenames.Clear();

        string zipFilePath = Path.Combine(path, "cenasaglomeradas.zip");
        if (File.Exists(zipFilePath))
            File.Delete(zipFilePath);

    }
}
