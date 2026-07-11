using System;
using UnityEngine;

public class BVColorTest : MonoBehaviour {

    public static Color BV2Col(float _bv)
    {
        Color col = Color.white;
        float t = 4600f * ((1 / ((0.92f * _bv) + 1.7f)) + (1f / ((0.92f * _bv) + 0.62f)));

        // t to xyY
        float x = 0f, y = 0f;

        if (t >= 1667 && t <= 4000)
        {
            x = ((-0.2661239f * Mathf.Pow(10, 9)) / Mathf.Pow(t, 3)) + ((-0.2343580f * Mathf.Pow(10, 6)) / Mathf.Pow(t, 2)) + ((0.8776956f * Mathf.Pow(10, 3)) / t) + 0.179910f;
        }
        else if (t > 4000 && t <= 25000)
        {
            x = ((-3.0258469f * Mathf.Pow(10, 9)) / Mathf.Pow(t, 3)) + ((2.1070379f * Mathf.Pow(10, 6)) / Mathf.Pow(t, 2)) + ((0.2226347f * Mathf.Pow(10, 3)) / t) + 0.240390f;
        }

        if (t >= 1667 && t <= 2222)
        {
            y = -1.1063814f * Mathf.Pow(x, 3) - 1.34811020f * Mathf.Pow(x, 2) + 2.18555832f * x - 0.20219683f;
        }
        else if (t > 2222 && t <= 4000)
        {
            y = -0.9549476f * Mathf.Pow(x, 3) - 1.37418593f * Mathf.Pow(x, 2) + 2.09137015f * x - 0.16748867f;
        }
        else if (t > 4000 && t <= 25000)
        {
            y = 3.0817580f * Mathf.Pow(x, 3) - 5.87338670f * Mathf.Pow(x, 2) + 3.75112997f * x - 0.37001483f;
        }

        // xyY to XYZ, Y = 1
        var Y = (y == 0) ? 0 : 1;
        var X = (y == 0) ? 0 : (x * Y) / y;
        var Z = (y == 0) ? 0 : ((1 - x - y) * Y) / y;

        // XYZ to RGB
        col.r = 0.41847f * X - 0.15866f * Y - 0.082835f * Z;
        col.g = -0.091169f * X + 0.25243f * Y + 0.015708f * Z;
        col.b = 0.00092090f * X - 0.0025498f * Y + 0.17860f * Z;
        col.a = 1f;

        return col;
    }

    #region //bv方法
    static float bvToT(float bv)
    {
        float t;

        // make sure bv is within its bounds [-0.4, 2] otherwise the math doesnt work
        if (bv < -0.4f)
        {
            bv = -0.4f;
        }
        else if (bv > 2f)
        {
            bv = 2f;
        }

        // found it online at http://www.wikiwand.com/en/Color_index
        t = 4600f * ((1 / ((0.92f * bv) + 1.7f)) + (1 / ((0.92f * bv) + 0.62f)));

        // console.log('t: ' + t);

        return t;
    }
    static float[] tToXyy(float t)
    {
        float x = 0, y = 0, Y = 1f; // Y is the luminance, I just assume full luminance for sanity

        // approximation of CIE xyY (http://www.wikiwand.com/en/CIE_1931_color_space) using https://en.wikipedia.org/wiki/Planckian_locus 
        if (t >= 1667 && t <= 4000)
        {
            x = (-0.2661239f * (Mathf.Pow(10, 9) / Mathf.Pow(t, 3))) -
                (-0.2343580f * (Mathf.Pow(10, 6) / Mathf.Pow(t, 2))) +
                (0.8776956f * (Mathf.Pow(10, 3) / t)) + 0.179910f;
        }
        else if (t >= 4000 && t <= 25000)
        {
            x = (-3.0258469f * (Mathf.Pow(10, 9) / Mathf.Pow(t, 3))) +
                (2.1070379f * (Mathf.Pow(10, 6) / Mathf.Pow(t, 2))) +
                (0.2226347f * (Mathf.Pow(10, 3) / t)) + 0.240390f;
        }

        if (t >= 1667 && t <= 2222)
        {
            y = (-1.1063814f * Mathf.Pow(x, 3)) -
                (1.34811020f * Mathf.Pow(x, 2)) +
                (2.18555832f * x) -
                 0.20219683f;
        }
        else if (t >= 2222 && t <= 4000)
        {
            y = (-0.9549476f * Mathf.Pow(x, 3)) -
                (1.37418593f * Mathf.Pow(x, 2)) +
                (2.09137015f * x) -
                 0.16748867f;
        }
        else if (t >= 4000 && t <= 25000)
        {
            y = (3.0817580f * Mathf.Pow(x, 3)) -
                (5.87338670f * Mathf.Pow(x, 2)) +
                (3.75112997f * x) -
                 0.37001483f;
        }

        // console.log('xyY: ' + [x, y, Y]);

        return new float[] { x, y, Y };
    }
    static float[] xyYToXyz(float[] xyY)
    {
        float X, Y, Z,
            x = xyY[0],
            y = xyY[1];

        // X and Z tristimulus values calculated using https://en.wikipedia.org/wiki/CIE_1931_color_space?oldformat=true#CIE_xy_chromaticity_diagram_and_the_CIE_xyY_color_space
        Y = xyY[2];
        X = (y == 0) ? 0 : (x * Y) / y;
        Z = (y == 0) ? 0 : ((1 - x - y) * Y) / y;

        // console.log('XYZ: ' + [X, Y, Z]);

        return new float[] { X, Y, Z };
    }
    static float[] xyzToRgb(float[] xyz)
    {
        float r, g, b,
            x = xyz[0],
            y = xyz[1],
            z = xyz[2];

        // using matrix from https://www.cs.rit.edu/~ncs/color/t_convert.html#RGB%20to%20XYZ%20&%20XYZ%20to%20RGB
        r = (3.2406f * x) +
            (-1.5372f * y) +
            (-0.4986f * z);

        g = (-0.9689f * x) +
            (1.8758f * y) +
            (0.0415f * z);

        b = (0.0557f * x) +
            (-0.2040f * y) +
            (1.0570f * z);

        // make sure the values didnt overflow
        r = (r > 1) ? 1 : r;
        g = (g > 1) ? 1 : g;
        b = (b > 1) ? 1 : b;

        // console.log('rgb: ' + [r, g, b]);

        return new float[] { r, g, b };
    }
    static float[] gammaCorrect(float[] rgb)
    {
            float R, G, B,
            r = rgb[0],
            g = rgb[1],
            b = rgb[2];

        // using https://en.wikipedia.org/wiki/SRGB?oldformat=true#The_forward_transformation_.28CIE_xyY_or_CIE_XYZ_to_sRGB.29
        /*R = (r <= 0.0031308) ? 12.92 * r : ((1 + r) * Mathf.Pow(r, 1 / 2.2)) - a;
        G = (g <= 0.0031308) ? 12.92 * g : ((1 + g) * Mathf.Pow(g, 1 / 2.2)) - a;
        B = (b <= 0.0031308) ? 12.92 * b : ((1 + b) * Mathf.Pow(b, 1 / 2.2)) - a;*/

        /*R = Mathf.Pow(r, 1 / gamma);
        G = Mathf.Pow(g, 1 / gamma);
        B = Mathf.Pow(b, 1 / gamma);*/

        R = r;
        G = g / 1.05f; // idk but i messed up somewhere and this makes it look better
        B = b;

        R = (R > 1) ? 1 : R;
        G = (G > 1) ? 1 : G;
        B = (B > 1) ? 1 : B;

        // turn the 0-1 rgb value to 0-255
        return new float[] { Mathf.Round(R * 255f), Mathf.Round(G * 255f), Mathf.Round(B * 255f) };
    }

    static string rgbToHex(float[] rgb)
    {
        return '#' + Convert.ToString((int)rgb[0], 16) + Convert.ToString((int)rgb[1], 16) + Convert.ToString((int)rgb[2], 16);
    }

    public static Color bvTORGB(float _bv)
    {
        float t;
        float[] xyY, xyz, rgb, crgb;

        t = bvToT(_bv);

        xyY = tToXyy(t);

        xyz = xyYToXyz(xyY);

        rgb = xyzToRgb(xyz);

        crgb = gammaCorrect(rgb);

        Color color = new Color(crgb[0], crgb[1], crgb[2], 1f);
        Debug.Log(color.r + " " + color.g + " " + color.b);
        Debug.Log(rgbToHex(crgb));
        return color;
    }

    #endregion

    public static Color Bv2rgb(float bv) 
    {
        float t;
        float r = 0, g = 0, b = 0;

        if (bv < -0.4)
            bv = -0.4f;
        if (bv > 2.0)
            bv = 2.0f;

        if ((bv >= -0.40) && (bv < 0.00))
        {
            t = (bv + 0.40f) / (0 + 0.40f);
            r = 0.61f + (0.11f * t) + (0.1f * t * t);
        }
        else if ((bv >= 0.00) && (bv < 0.40))
        {
            t = (bv - 0) / (0.40f - 0);
            r = 0.83f + (0.17f * t);
        }
        else if ((bv >= 0.40) && (bv < 2.10))
        {
            t = (bv - 0.40f) / (2.10f - 0.40f);
            r = 1;
        }

        if ((bv >= -0.40f) && (bv < 0.00))
        {
            t = (bv + 0.40f) / (0 + 0.40f);
            g = 0.70f + (0.07f * t) + (0.1f * t * t);
        }
        else if ((bv >= 0.00) && (bv < 0.40))
        {
            t = (bv - 0) / (0.40f - 0);
            g = 0.87f + (0.11f * t);
        }
        else if ((bv >= 0.40f) && (bv < 1.60))
        {
            t = (bv - 0.40f) / (1.60f - 0.40f);
            g = 0.98f - (0.16f * t);
        }
        else if ((bv >= 1.60) && (bv < 2.00))
        {
            t = (bv - 1.60f) / (2.00f - 1.60f);
            g = 0.82f - (0.5f * t * t);
        }

        if ((bv >= -0.40) && (bv < 0.40))
        {
            t = (bv + 0.40f) / (0.40f + 0.40f);
            b = 1.00f;
        }
        else if ((bv >= 0.40) && (bv < 1.50))
        {
            t = (bv - 0.40f) / (1.50f - 0.40f);
            b = 1.00f - (0.47f * t) + (0.1f * t * t);
        }
        else if ((bv >= 1.50) && (bv < 1.94))
        {
            t = (bv - 1.50f) / (1.94f - 1.50f);
            b = 0.63f - (0.6f * t * t);
        }

        Color color = new Color(r, g, b);

        return color;
    }
}
