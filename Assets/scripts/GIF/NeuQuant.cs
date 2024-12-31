using System;

public class NeuQuant
{
    protected static readonly int netsize = 256; // number of colours used
    protected static readonly int prime1 = 499;
    protected static readonly int prime2 = 491;
    protected static readonly int prime3 = 487;
    protected static readonly int prime4 = 503;
    protected static readonly int minpicturebytes = (3 * prime4);
    protected static readonly int maxnetpos = (netsize - 1);
    protected static readonly int netbiasshift = 4; // bias for colour values
    protected static readonly int ncycles = 100; // number of learning cycles

    protected static readonly int intbiasshift = 16; // bias for fractions
    protected static readonly int intbias = (((int)1) << intbiasshift);
    protected static readonly int gammashift = 10;
    protected static readonly int gamma = (((int)1) << gammashift);
    protected static readonly int betashift = 10;
    protected static readonly int beta = (intbias >> betashift); // beta = 1/1024
    protected static readonly int betagamma = (intbias << (gammashift - betashift));

    protected static readonly int initrad = (netsize >> 3); // for 256 cols, radius starts
    protected static readonly int radiusbiasshift = 6;
    protected static readonly int radiusbias = (((int)1) << radiusbiasshift);
    protected static readonly int initradius = (initrad * radiusbias); // and decreases by a
    protected static readonly int radiusdec = 30; // factor of 1/30 each cycle

    protected static readonly int alphabiasshift = 10; // alpha starts at 1
    protected static readonly int initalpha = (((int)1) << alphabiasshift);

    protected int alphadec; // biased by 10 bits

    protected static readonly int radbiasshift = 8;
    protected static readonly int radbias = (((int)1) << radbiasshift);
    protected static readonly int alpharadbshift = (alphabiasshift + radbiasshift);
    protected static readonly int alpharadbias = (((int)1) << alpharadbshift);

    protected byte[] thepicture; // the input image itself
    protected int lengthcount; // lengthcount = H*W*3

    protected int samplefac; // sampling factor 1..30

    // The network itself
    protected int[][] network; // the network itself - [netsize][4]

    protected int[] netindex = new int[256];

    // bias and freq arrays for learning
    protected int[] bias = new int[netsize];
    protected int[] freq = new int[netsize];
    protected int[] radpower = new int[initrad];

    public NeuQuant(byte[] thepic, int len, int sample)
    {
        int i;
        int[] p;

        thepicture = thepic;
        lengthcount = len;
        samplefac = sample;

        network = new int[netsize][];
        for (i = 0; i < netsize; i++)
        {
            network[i] = new int[4];
            p = network[i];
            p[0] = p[1] = p[2] = (i << (netbiasshift + 8)) / netsize;
            freq[i] = intbias / netsize; // 1/netsize
            bias[i] = 0;
        }
    }

    public byte[] Process()
    {
        Learn();
        Inxbuild();
        return ColorMap();
    }

    public byte[] ColorMap()
    {
        byte[] map = new byte[3 * netsize];
        int[] index = new int[netsize];
        for (int i = 0; i < netsize; i++)
        {
            index[network[i][3]] = i;
        }
        int k = 0;
        for (int i = 0; i < netsize; i++)
        {
            int j = index[i];
            map[k++] = (byte)(network[j][0]);
            map[k++] = (byte)(network[j][1]);
            map[k++] = (byte)(network[j][2]);
        }
        return map;
    }

    protected void Inxbuild()
    {
        int i, j, smallpos, smallval;
        int[] p;
        int[] q;
        int previouscol, startpos;

        previouscol = 0;
        startpos = 0;
        for (i = 0; i < netsize; i++)
        {
            p = network[i];
            smallpos = i;
            smallval = p[1]; // index on g
            for (j = i + 1; j < netsize; j++)
            {
                q = network[j];
                if (q[1] < smallval)
                { // index on g
                    smallpos = j;
                    smallval = q[1]; // index on g
                }
            }
            q = network[smallpos];
            if (i != smallpos)
            {
                for (int k = 0; k < 4; k++)
                {
                    int temp = q[k];
                    q[k] = p[k];
                    p[k] = temp;
                }
            }
            if (smallval != previouscol)
            {
                if (previouscol >= 0 && previouscol < netindex.Length)
                {
                    netindex[previouscol] = (startpos + i) >> 1;
                }
                for (j = previouscol + 1; j < smallval; j++)
                {
                    if (j >= 0 && j < netindex.Length)
                    {
                        netindex[j] = i;
                    }
                }
                previouscol = smallval;
                startpos = i;
            }
        }
        if (previouscol >= 0 && previouscol < netindex.Length)
        {
            netindex[previouscol] = (startpos + maxnetpos) >> 1;
        }
        for (j = previouscol + 1; j < 256; j++)
        {
            if (j >= 0 && j < netindex.Length)
            {
                netindex[j] = maxnetpos;
            }
        }
    }

    // Função auxiliar para trocar elementos em 'network'
    private void Swap(int[] a, int[] b)
    {
        int temp;
        temp = a[0];
        a[0] = b[0];
        b[0] = temp;

        temp = a[1];
        a[1] = b[1];
        b[1] = temp;

        temp = a[2];
        a[2] = b[2];
        b[2] = temp;

        temp = a[3];
        a[3] = b[3];
        b[3] = temp;
    }


    protected void Learn()
    {
        int i, j, b, g, r;
        int radius, rad, alpha, step, delta, samplepixels;
        byte[] p;
        int pix, lim;

        if (lengthcount < minpicturebytes) samplefac = 1;
        alphadec = 30 + ((samplefac - 1) / 3);
        p = thepicture;
        pix = 0;
        lim = lengthcount;
        samplepixels = lengthcount / (3 * samplefac);
        delta = samplepixels / ncycles;
        alpha = initalpha;
        radius = initradius;

        rad = radius >> radiusbiasshift;
        if (rad <= 1) rad = 0;
        for (i = 0; i < rad; i++)
            radpower[i] = alpha * (((rad * rad - i * i) * radbias) / (rad * rad));

        if (lengthcount < minpicturebytes)
        {
            step = 3;
        }
        else if ((lengthcount % prime1) != 0)
        {
            step = 3 * prime1;
        }
        else
        {
            if ((lengthcount % prime2) != 0)
            {
                step = 3 * prime2;
            }
            else
            {
                if ((lengthcount % prime3) != 0)
                {
                    step = 3 * prime3;
                }
                else
                {
                    step = 3 * prime4;
                }
            }
        }

        i = 0;
        while (i < samplepixels)
        {
            b = (p[pix + 0] & 0xff) << netbiasshift;
            g = (p[pix + 1] & 0xff) << netbiasshift;
            r = (p[pix + 2] & 0xff) << netbiasshift;
            j = Contest(b, g, r);

            Altersingle(alpha, j, b, g, r);
            if (rad != 0) Alterneigh(rad, j, b, g, r);

            pix += step;
            if (pix >= lim) pix -= lengthcount;

            i++;
            if (delta == 0) delta = 1;
            if (i % delta == 0)
            {
                alpha -= alpha / alphadec;
                radius -= radius / radiusdec;
                rad = radius >> radiusbiasshift;
                if (rad <= 1) rad = 0;
                for (j = 0; j < rad; j++)
                    radpower[j] = alpha * (((rad * rad - j * j) * radbias) / (rad * rad));
            }
        }
    }

    protected int Contest(int b, int g, int r)
    {
        int i, dist, a, biasdist, betafreq;
        int bestpos, bestbiaspos, bestd, bestbiasd;
        int[] n;

        bestd = ~(1 << 31);
        bestbiasd = bestd;
        bestpos = -1;
        bestbiaspos = bestpos;

        for (i = 0; i < netsize; i++)
        {
            n = network[i];
            dist = n[0] - b;
            if (dist < 0) dist = -dist;
            a = n[1] - g;
            if (a < 0) a = -a;
            dist += a;
            a = n[2] - r;
            if (a < 0) a = -a;
            dist += a;
            if (dist < bestd)
            {
                bestd = dist;
                bestpos = i;
            }
            biasdist = dist - ((bias[i]) >> (intbiasshift - netbiasshift));
            if (biasdist < bestbiasd)
            {
                bestbiasd = biasdist;
                bestbiaspos = i;
            }
            betafreq = (freq[i] >> betashift);
            freq[i] -= betafreq;
            bias[i] += (betafreq << gammashift);
        }
        freq[bestpos] += beta;
        bias[bestpos] -= betagamma;
        return bestbiaspos;
    }

    protected void Altersingle(int alpha, int i, int b, int g, int r)
    {
        int[] n = network[i];
        n[0] -= (alpha * (n[0] - b)) / initalpha;
        n[1] -= (alpha * (n[1] - g)) / initalpha;
        n[2] -= (alpha * (n[2] - r)) / initalpha;
    }

    protected void Alterneigh(int rad, int i, int b, int g, int r)
    {
        int j, k, lo, hi, a, m;
        int[] p;

        lo = i - rad;
        if (lo < -1) lo = -1;
        hi = i + rad;
        if (hi > netsize) hi = netsize;

        j = i + 1;
        k = i - 1;
        m = 1;
        while ((j < hi) || (k > lo))
        {
            a = radpower[m++];
            if (j < hi)
            {
                p = network[j++];
                try
                {
                    p[0] -= (a * (p[0] - b)) / alpharadbias;
                    p[1] -= (a * (p[1] - g)) / alpharadbias;
                    p[2] -= (a * (p[2] - r)) / alpharadbias;
                }
                catch (Exception) { }
            }
            if (k > lo)
            {
                p = network[k--];
                try
                {
                    p[0] -= (a * (p[0] - b)) / alpharadbias;
                    p[1] -= (a * (p[1] - g)) / alpharadbias;
                    p[2] -= (a * (p[2] - r)) / alpharadbias;
                }
                catch (Exception) { }
            }
        }
    }
}
