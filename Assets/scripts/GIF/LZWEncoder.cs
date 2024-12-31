using System.IO;
//using System;
using UnityEngine;


public class LZWEncoder
{
    private int imgW, imgH;
    private byte[] pixAry;
    private int initCodeSize;
    private int remaining;
    private int curPixel;

    private static readonly int EOF = -1;

    private int[] htab = new int[5003];
    private int[] codetab = new int[5003];

    private int hsize = 5003;

    private int freeEnt = 0;

    private int maxbits = 12;
    private int maxmaxcode = 1 << 12;

    private int[] masks = new int[] { 0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 4095 };

    private int n_bits;
    private int maxcode;

    private int ClearCode;
    private int EOFCode;

    private int cur_accum = 0;
    private int cur_bits = 0;

    private int a_count;

    private byte[] accum = new byte[256];

    public LZWEncoder(int width, int height, byte[] pixels, int color_depth)
    {
        imgW = width;
        imgH = height;
        pixAry = pixels;
        initCodeSize = Mathf.Max(2, color_depth);
    }

    public void Encode(BinaryWriter outs)
    {
        outs.Write((byte)initCodeSize);
        remaining = imgW * imgH;
        curPixel = 0;
        Compress(initCodeSize + 1, outs);
        outs.Write((byte)0);
    }

    private void Compress(int init_bits, BinaryWriter outs)
    {
        int fcode;
        int i;
        int c;
        int ent;
        int disp;
        int hsize_reg;
        int hshift;

        n_bits = init_bits;
        maxcode = MAXCODE(n_bits);

        ClearCode = 1 << (init_bits - 1);
        EOFCode = ClearCode + 1;
        freeEnt = ClearCode + 2;

        a_count = 0;

        ent = NextPixel();

        hshift = 0;
        for (fcode = hsize; fcode < 65536; fcode *= 2)
            ++hshift;
        hshift = 8 - hshift;

        hsize_reg = hsize;
        for (i = 0; i < hsize_reg; ++i)
            htab[i] = -1;

        Output(ClearCode, outs);

        while ((c = NextPixel()) != EOF)
        {
            fcode = (c << maxbits) + ent;
            i = (c << hshift) ^ ent;

            if (htab[i] == fcode)
            {
                ent = codetab[i];
                continue;
            }
            else if (htab[i] >= 0)
            {
                disp = hsize_reg - i;
                if (i == 0)
                    disp = 1;
                bool found = false;
                do
                {
                    if ((i -= disp) < 0)
                        i += hsize_reg;

                    if (htab[i] == fcode)
                    {
                        ent = codetab[i];
                        found = true;
                        break;
                    }
                } while (htab[i] >= 0);

                if (found)
                    continue;
            }
            Output(ent, outs);
            ent = c;
            if (freeEnt < maxmaxcode)
            {
                codetab[i] = freeEnt++;
                htab[i] = fcode;
            }
            else
                ClearTable(outs);
        }
        Output(ent, outs);
        Output(EOFCode, outs);
    }

    private void ClearTable(BinaryWriter outs)
    {
        ResetCodeTable();
        freeEnt = ClearCode + 2;
        Output(ClearCode, outs);
    }

    private void ResetCodeTable()
    {
        for (int i = 0; i < hsize; ++i)
            htab[i] = -1;
    }

    private int MAXCODE(int n_bits)
    {
        return (1 << n_bits) - 1;
    }

    private void Output(int code, BinaryWriter outs)
    {
        cur_accum &= masks[cur_bits];

        if (cur_bits > 0)
            cur_accum |= (code << cur_bits);
        else
            cur_accum = code;

        cur_bits += n_bits;

        while (cur_bits >= 8)
        {
            Add((byte)(cur_accum & 0xff));
            cur_accum >>= 8;
            cur_bits -= 8;
        }

        if (a_count >= 254)
            Flush(outs);
    }

    private void Add(byte c)
    {
        accum[a_count++] = c;
    }

    private void Flush(BinaryWriter outs)
    {
        if (a_count > 0)
        {
            outs.Write((byte)a_count);
            outs.Write(accum, 0, a_count);
            a_count = 0;
        }
    }

    private int NextPixel()
    {
        if (remaining == 0)
            return EOF;

        --remaining;

        byte pix = pixAry[curPixel++];
        return pix & 0xff;
    }
}
