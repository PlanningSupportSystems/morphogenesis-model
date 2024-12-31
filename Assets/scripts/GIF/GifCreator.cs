using UnityEngine;
using System.Collections.Generic;
using System.IO;
//using GifEncoder; // Assuming you've added the GifEncoder library to your project

public class GifCreator : MonoBehaviour
{
    public List<byte[]> jpgList; // List of byte arrays for your JPG images
    public int delayBetweenFrames = 100; // Delay between frames in milliseconds

    public void CreateGif(string outputFilePath)
    {
        // Create a new GifEncoder
        //        var gifEncoder = new GifEncoder(outputFilePath, delayBetweenFrames);

        // Loop through each image in the jpgList
        foreach (var jpgBytes in jpgList)
        {
            // Load the JPG image into a Texture2D
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(jpgBytes);

            // Convert the Texture2D to a Bitmap
            //            System.Drawing.Bitmap bitmap;
            using (MemoryStream ms = new MemoryStream(jpgBytes))
            {
                //              bitmap = new System.Drawing.Bitmap(ms);
            }

            // Add the frame to the GIF
            //            gifEncoder.AddFrame(bitmap);

            // Clean up
            Destroy(texture);
            //            bitmap.Dispose();
        }

        // Finalize the GIF
        //       gifEncoder.FinishEncoding();
    }
}
