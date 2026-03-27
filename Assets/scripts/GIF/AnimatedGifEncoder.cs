using System.IO;
using UnityEngine;
using System;

public class AnimatedGifEncoder
{
    protected int width;
    protected int height;
    protected int transIndex;
    protected int repeat = -1;
    protected int delay = 0;
    protected bool started = false;
    protected BinaryWriter bw;
    protected MemoryStream ms;
    protected byte[] pixels;
    protected byte[] indexedPixels;
    protected int colorDepth;
    protected byte[] colorTab;
    protected bool[] usedEntry = new bool[256];
    protected int palSize = 7;
    protected int dispose = -1;
    protected bool closeStream = false;
    protected bool firstFrame = true;
    protected bool sizeSet = false;
    protected int sample = 10;

    public void SetDelay(int ms)
    {
        delay = Mathf.RoundToInt(ms / 10.0f);
    }

    public void SetDispose(int code)
    {
        if (code >= 0)
        {
            dispose = code;
        }
    }

    public void SetRepeat(int iter)
    {
        if (iter >= 0)
        {
            repeat = iter;
        }
    }

    public void Start(FileStream os)
    {
        if (os == null) return;
        bool ok = true;
        closeStream = false;
        bw = new BinaryWriter(os);
        try
        {
            WriteString("GIF89a");
        }
        catch (IOException)
        {
            ok = false;
        }
        started = ok;
    }

    public bool AddFrame(Texture2D im)
    {
        if ((im == null) || !started) return false;
        bool ok = true;
        try
        {
            if (!sizeSet) SetSize(im.width, im.height);
            // Implementation to convert Texture2D to byte array and other required steps
            AnalyzePixels(); // Convert image to pixels array and analyze
            if (firstFrame)
            {
                WriteLSD(); // Logical Screen Descriptor
                WritePalette(); // Global Color Table
                if (repeat >= 0) WriteNetscapeExt(); // Netscape extension
            }
            WriteGraphicCtrlExt(); // Write graphics control extension
            WriteImageDesc(); // Write image descriptor
            if (!firstFrame) WritePalette(); // Local color table
            WritePixels(); // Encode and write pixel data
            firstFrame = false;
        }
        catch (IOException)
        {
            ok = false;
        }
        return ok;
    }

    public void Finish()
    {
        if (!started) return;
        bool ok = true;
        try
        {
            bw.Write((byte)0x3B); // GIF file terminator
            bw.Flush();
            if (closeStream)
            {
                bw.BaseStream.Close();
            }
        }
        catch (IOException e)
        {
            ok = false;
            Debug.LogError("Failed to finish writing GIF: " + e.Message);
        }
        started = !ok;
    }

    protected void SetSize(int w, int h)
    {
        if (started && !firstFrame) return;
        width = w;
        height = h;
        if (width < 1) width = 320;
        if (height < 1) height = 240;
        sizeSet = true;
    }

    protected void AnalyzePixels()
    {
        int len = pixels.Length;
        int nPix = len / 3;
        indexedPixels = new byte[nPix];

        // Criar uma instância de NeuQuant para processar os pixels e obter a paleta de cores
        NeuQuant nq = new NeuQuant(pixels, len, sample);
        byte[] colorTab = nq.Process(); // Processar os pixels para obter a paleta de cores

        // Ajustar a paleta de cores (se necessário)
        for (int i = 0; i < colorTab.Length; i += 3)
        {
            byte temp = colorTab[i];
            colorTab[i] = colorTab[i + 2];
            colorTab[i + 2] = temp;
            usedEntry[i / 3] = false;
        }

        // Quantizar cada pixel para o índice correspondente na paleta de cores
        int k = 0;
        for (int i = 0; i < nPix; i++)
        {
            byte r = pixels[k++];
            byte g = pixels[k++];
            byte b = pixels[k++];

            // Encontrar a cor mais próxima na paleta de cores
            int minDist = int.MaxValue;
            int index = 0;

            for (int j = 0; j < colorTab.Length; j += 3)
            {
                byte rPalette = colorTab[j];
                byte gPalette = colorTab[j + 1];
                byte bPalette = colorTab[j + 2];

                // Calcular a distância euclidiana ao quadrado entre as cores
                int dist = (r - rPalette) * (r - rPalette) +
                           (g - gPalette) * (g - gPalette) +
                           (b - bPalette) * (b - bPalette);

                if (dist < minDist)
                {
                    minDist = dist;
                    index = j / 3; // Índice na paleta de cores
                }
            }

            indexedPixels[i] = (byte)index; // Atribuir o índice na paleta ao pixel
            usedEntry[index] = true;
        }

        // Limpar os pixels originais após o processamento
        pixels = null;

        // Definir profundidade de cor e tamanho da paleta (exemplo, ajuste conforme necessário)
        colorDepth = 8;
        palSize = 7;
    }


    protected void WriteGraphicCtrlExt()
    {
        bw.Write((byte)0x21);
        bw.Write((byte)0xf9);
        bw.Write((byte)4);
        int transp;
        int disp;
        if (transIndex == -1)
        {
            transp = 0;
            disp = 0;
        }
        else
        {
            transp = 1;
            disp = 2;
        }
        if (dispose >= 0)
        {
            disp = dispose & 7;
        }
        disp <<= 2;
        bw.Write((byte)(0 | disp | 0 | transp));
        WriteShort(delay);
        bw.Write((byte)transIndex);
        bw.Write((byte)0);
    }

    protected void WriteImageDesc()
    {
        bw.Write((byte)0x2c);
        WriteShort(0);
        WriteShort(0);
        WriteShort(width);
        WriteShort(height);
        if (firstFrame)
        {
            bw.Write((byte)0);
        }
        else
        {
            bw.Write((byte)0x80 | 0 | 0 | 0 | palSize);
        }
    }

    protected void WriteLSD()
    {
        WriteShort(width);
        WriteShort(height);
        bw.Write((byte)(0x80 | 0x70 | 0x00 | palSize));
        bw.Write((byte)0);
        bw.Write((byte)0);
    }

    protected void WriteNetscapeExt()
    {
        bw.Write((byte)0x21);
        bw.Write((byte)0xff);
        bw.Write((byte)11);
        WriteString("NETSCAPE2.0");
        bw.Write((byte)3);
        bw.Write((byte)1);
        WriteShort(repeat);
        bw.Write((byte)0);
    }

    protected void WritePalette()
    {
        bw.Write(colorTab, 0, colorTab.Length);
        int n = (3 * 256) - colorTab.Length;
        for (int i = 0; i < n; i++)
        {
            bw.Write((byte)0);
        }
    }

    protected void WritePixels()
    {
        LZWEncoder encoder = new LZWEncoder(width, height, indexedPixels, colorDepth);
        encoder.Encode(bw);
    }

    protected void WriteShort(int value)
    {
        bw.Write((byte)(value & 0xff));
        bw.Write((byte)((value >> 8) & 0xff));
    }

    protected void WriteString(String s)
    {
        char[] chars = s.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            bw.Write((byte)chars[i]);
        }
    }
}
