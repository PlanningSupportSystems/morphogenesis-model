//using Aspose.Imaging;

//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats.Gif;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;

//public class ImageToGifConverter : MonoBehaviour
//{
//    // Lista de byte arrays das imagens capturadas
//    private List<byte[]> jpgList = new List<byte[]>();
//    // Caminho onde o GIF será salvo
//    public string outputGifPath = "path/to/output/your.gif";
//    // Atraso opcional entre os frames do GIF (em milissegundos)
//    public int delay = 100;

//    void Start()
//    {
//        // Exemplo de captura de imagem e adição à lista
//        CaptureScreenshot();

//        // Crie o GIF a partir das imagens
//        CreateGifFromImages(jpgList, outputGifPath, delay);
//    }

//    void CaptureScreenshot()
//    {
//        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
//        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
//        screenshot.Apply();

//        byte[] jpgBytes = screenshot.EncodeToJPG();
//        jpgList.Add(jpgBytes);
//    }

//    void CreateGifFromImages(List<byte[]> images, string outputFile, int frameDelay)
//    {
//        using (var gif = new Image<Rgba32>(1, 1)) // Placeholder, will be replaced
//        {
//            var gifMetaData = gif.Metadata.GetGifMetadata();
//            gifMetaData.RepeatCount = 0;

//            foreach (var imageBytes in images)
//            {
//                using (var image = Image.Load<Rgba32>(imageBytes))
//                {
//                    gif.Frames.AddFrame(image.Frames[0]);
//                    var frameMetaData = image.Frames.RootFrame.Metadata.GetGifMetadata();
//                    frameMetaData.FrameDelay = frameDelay / 10; // delay in hundredths of a second
//                }
//            }

//            // Remove the placeholder frame
//            gif.Frames.RemoveFrame(0);

//            // Save the GIF to the specified path
//            gif.Save(outputFile, new GifEncoder());
//        }

//        Debug.Log("GIF criado com sucesso em " + outputFile);
//    }
//}
