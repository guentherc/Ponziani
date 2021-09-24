using System;
using System.Drawing;

namespace PonzianiComponents
{
    // Copied and adjusted from .net core source code
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    internal struct HSLColor
    {
        private const int ShadowAdj = -333;
        private const int HilightAdj = 500;

        private const int Range = 240;
        private const int HLSMax = Range;
        private const int RGBMax = 255;
        private const int Undefined = HLSMax * 2 / 3;

        private int hue;
        private int saturation;
        private int luminosity;

        private bool isSystemColors_Control;

        public HSLColor(Color color)
        {
            isSystemColors_Control = (color.ToKnownColor() == SystemColors.Control.ToKnownColor());
            int r = color.R;
            int g = color.G;
            int b = color.B;
            int max, min;
            int sum, dif;
            int Rdelta, Gdelta, Bdelta;

            max = Math.Max(Math.Max(r, g), b);
            min = Math.Min(Math.Min(r, g), b);
            sum = max + min;

            luminosity = (((sum * HLSMax) + RGBMax) / (2 * RGBMax));

            dif = max - min;
            if (dif == 0)
            {
                saturation = 0;
                hue = Undefined;
            }
            else
            {
                if (luminosity <= (HLSMax / 2))
                    saturation = (int)(((dif * (int)HLSMax) + (sum / 2)) / sum);
                else
                    saturation = (int)((int)((dif * (int)HLSMax) + (int)((2 * RGBMax - sum) / 2))
                                        / (2 * RGBMax - sum));

                Rdelta = (int)((((max - r) * (int)(HLSMax / 6)) + (dif / 2)) / dif);
                Gdelta = (int)((((max - g) * (int)(HLSMax / 6)) + (dif / 2)) / dif);
                Bdelta = (int)((((max - b) * (int)(HLSMax / 6)) + (dif / 2)) / dif);

                if ((int)r == max)
                    hue = Bdelta - Gdelta;
                else if ((int)g == max)
                    hue = (HLSMax / 3) + Rdelta - Bdelta;
                else
                    hue = ((2 * HLSMax) / 3) + Gdelta - Rdelta;

                if (hue < 0)
                    hue += HLSMax;
                if (hue > HLSMax)
                    hue -= HLSMax;
            }
        }

        public Color Darker(float percDarker)
        {
            if (isSystemColors_Control)
            {
                if (percDarker == 0.0f)
                {
                    return SystemColors.ControlDark;
                }
                else if (percDarker == 1.0f)
                {
                    return SystemColors.ControlDarkDark;
                }
                else
                {
                    Color dark = SystemColors.ControlDark;
                    Color darkDark = SystemColors.ControlDarkDark;

                    int dr = dark.R - darkDark.R;
                    int dg = dark.G - darkDark.G;
                    int db = dark.B - darkDark.B;

                    return Color.FromArgb((byte)(dark.R - (byte)(dr * percDarker)),
                                          (byte)(dark.G - (byte)(dg * percDarker)),
                                          (byte)(dark.B - (byte)(db * percDarker)));
                }
            }
            else
            {
                int oneLum = 0;
                int zeroLum = NewLuma(ShadowAdj, true);

                return ColorFromHLS(hue, zeroLum - (int)((zeroLum - oneLum) * percDarker), saturation);
            }
        }

        public Color Lighter(float percLighter)
        {
            if (isSystemColors_Control)
            {
                if (percLighter == 0.0f)
                {
                    return SystemColors.ControlLight;
                }
                else if (percLighter == 1.0f)
                {
                    return SystemColors.ControlLightLight;
                }
                else
                {
                    Color light = SystemColors.ControlLight;
                    Color lightLight = SystemColors.ControlLightLight;

                    int dr = light.R - lightLight.R;
                    int dg = light.G - lightLight.G;
                    int db = light.B - lightLight.B;

                    return Color.FromArgb((byte)(light.R - (byte)(dr * percLighter)),
                                          (byte)(light.G - (byte)(dg * percLighter)),
                                          (byte)(light.B - (byte)(db * percLighter)));
                }
            }
            else
            {
                int zeroLum = luminosity;
                int oneLum = NewLuma(HilightAdj, true);
                return ColorFromHLS(hue, zeroLum + (int)((oneLum - zeroLum) * percLighter), saturation);
            }
        }

        private int NewLuma(int n, bool scale)
        {
            return NewLuma(luminosity, n, scale);
        }

        private int NewLuma(int luminosity, int n, bool scale)
        {
            if (n == 0)
                return luminosity;

            if (scale)
            {
                if (n > 0)
                {
                    return (int)(((int)luminosity * (1000 - n) + (Range + 1L) * n) / 1000);
                }
                else
                {
                    return (int)(((int)luminosity * (n + 1000)) / 1000);
                }
            }

            int newLum = luminosity;
            newLum += (int)((long)n * Range / 1000);

            if (newLum < 0)
                newLum = 0;
            if (newLum > HLSMax)
                newLum = HLSMax;

            return newLum;
        }

        private Color ColorFromHLS(int hue, int luminosity, int saturation)
        {
            byte r, g, b;
            int magic1, magic2;

            if (saturation == 0)
            {
                r = g = b = (byte)((luminosity * RGBMax) / HLSMax);
                if (hue != Undefined)
                {

                }
            }
            else
            {
                if (luminosity <= (HLSMax / 2))
                    magic2 = (int)((luminosity * ((int)HLSMax + saturation) + (HLSMax / 2)) / HLSMax);
                else
                    magic2 = luminosity + saturation - (int)(((luminosity * saturation) + (int)(HLSMax / 2)) / HLSMax);
                magic1 = 2 * luminosity - magic2;

                r = (byte)(((HueToRGB(magic1, magic2, (int)(hue + (int)(HLSMax / 3))) * (int)RGBMax + (HLSMax / 2))) / (int)HLSMax);
                g = (byte)(((HueToRGB(magic1, magic2, hue) * (int)RGBMax + (HLSMax / 2))) / HLSMax);
                b = (byte)(((HueToRGB(magic1, magic2, (int)(hue - (int)(HLSMax / 3))) * (int)RGBMax + (HLSMax / 2))) / (int)HLSMax);
            }
            return Color.FromArgb(r, g, b);
        }

        private int HueToRGB(int n1, int n2, int hue)
        {
            if (hue < 0)
                hue += HLSMax;

            if (hue > HLSMax)
                hue -= HLSMax;

            if (hue < (HLSMax / 6))
                return (n1 + (((n2 - n1) * hue + (HLSMax / 12)) / (HLSMax / 6)));
            if (hue < (HLSMax / 2))
                return (n2);
            if (hue < ((HLSMax * 2) / 3))
                return (n1 + (((n2 - n1) * (((HLSMax * 2) / 3) - hue) + (HLSMax / 12)) / (HLSMax / 6)));
            else
                return (n1);

        }

    }
}
