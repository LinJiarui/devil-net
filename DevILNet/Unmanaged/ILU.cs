/*
* Copyright (c) 2012 Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Runtime.InteropServices;

namespace DevIL.Unmanaged
{
    public static class ILU
    {
        private static bool _init = false;

        public static bool IsInitialized
        {
            get
            {
                return _init;
            }
        }

        #region ILU Methods

        public static void Initialize()
        {
            if (!_init) {
                iluInit();
                _init = true;
            }
        }

        public static bool BlurAverage(int iterations)
        {
            return iluBlurAverage((uint)iterations);
        }

        public static bool BlurGaussian(int iterations)
        {
            return iluBlurGaussian((uint)iterations);
        }

        public static bool CompareImages(int otherImageID)
        {
            return iluCompareImages((uint)otherImageID);
        }

        public static bool Crop(int xOffset, int yOffset, int zOffset, int width, int height, int depth)
        {
            return iluCrop((uint)xOffset, (uint)yOffset, (uint)zOffset, (uint)width, (uint)height, (uint)depth);
        }

        public static bool EnlargeCanvas(int width, int height, int depth)
        {
            return iluEnlargeCanvas((uint)width, (uint)height, (uint)depth);
        }

        public static bool EnlargeImage(int xDimension, int yDimension, int zDimension)
        {
            return iluEnlargeImage((uint)xDimension, (uint)yDimension, (uint)zDimension);
        }

        public static String GetErrorString(ErrorType error)
        {
            //DevIL re-uses its error strings
            return Marshal.PtrToStringAnsi(iluGetErrorString((uint)error));
        }

        public static int ColorsUsed()
        {
            return (int)iluColorsUsed();
        }

        public static bool Scale(int width, int height, int depth)
        {
            return iluScale((uint)width, (uint)height, (uint)depth);
        }
        public static bool Noisify(float tolerance)
        {
            return iluNoisify(MemoryHelper.Clamp(tolerance, 0f, 1f));
        }

        public static bool Pixelize(int pixelSize)
        {
            return iluPixelize((uint)pixelSize);
        }

        public static bool SetLanguage(Language lang)
        {
            return iluSetLanguage((uint)lang);
        }

        public static bool Sharpen(float factor, int iterations)
        {
            return iluSharpen(factor, (uint)iterations);
        }

        public static String GetVendorName()
        {
            return Marshal.PtrToStringAnsi(iluGetString(ILDefines.IL_VENDOR));
        }

        public static String GetVersionNumber()
        {
            return Marshal.PtrToStringAnsi(iluGetString(ILDefines.IL_VERSION_NUM));
        }

        public static void SetImagePlacement(Placement placement)
        {
            iluImageParameter(ILUDefines.ILU_PLACEMENT, (uint)placement);
        }

        public static Placement GetImagePlacement()
        {
            return (Placement)iluGetInteger(ILUDefines.ILU_PLACEMENT);
        }

        public static void SetSamplingFilter(SamplingFilter filter)
        {
            iluImageParameter(ILUDefines.ILU_FILTER, (uint)filter);
        }

        public static SamplingFilter GetSamplingFilter()
        {
            return (SamplingFilter)iluGetInteger(ILUDefines.ILU_FILTER);
        }

        public static void Region(PointF[] points)
        {
            if (points == null || points.Length < 3) {
                return;
            }
            iluRegionf(points, (uint)points.Length);
        }

        public static void Region(PointI[] points)
        {
            if (points == null || points.Length < 3) {
                return;
            }
            iluRegioni(points, (uint)points.Length);
        }

        #endregion

        #region delegates
        private delegate void iluInit_();
        private delegate bool iluBlurAverage_(uint iterations);
        private delegate bool iluBlurGaussian_(uint iterations);
        private delegate bool iluCompareImages_(uint otherImage);
        private delegate bool iluCrop_(uint offsetX, uint offsetY, uint offsetZ, uint width, uint height, uint depth);
        private delegate bool iluEnlargeCanvas_(uint width, uint height, uint depth);
        private delegate bool iluEnlargeImage_(uint xDim, uint yDim, uint zDim);
        private delegate IntPtr iluGetErrorString_(uint error);
        private delegate uint iluColorsUsed_();
        private delegate bool iluScale_(uint width, uint height, uint depth);
        private delegate bool iluPixelize_(uint pixelSize);
        private delegate bool iluSharpen_(float factor, uint iterations);
        private delegate IntPtr iluGetString_(uint name);
        private delegate void iluImageParameter_(uint pName, uint param);
        private delegate int iluGetInteger_(uint mode);
        private delegate void iluRegionf_(PointF[] points, uint num);
        private delegate void iluRegioni_(PointI[] points, uint num);
        private delegate bool iluNoisify_(float tolerance);
        private delegate bool iluSetLanguage_(uint language);

        private delegate bool Convolution_(int[] matrix, int scale, int bias);
        private delegate bool Alienify_();
        private delegate bool Equalize_();
        private delegate bool Contrast_(float contrast);
        private delegate bool EdgeDetectE_();
        private delegate bool EdgeDetectP_();
        private delegate bool EdgeDetectS_();
        private delegate bool Emboss_();
        private delegate bool FlipImage_();
        private delegate bool BuildMipMaps_();
        private delegate bool GammaCorrect_(float gamma);
        private delegate bool InvertAlpha_();
        private delegate bool Mirror_();
        private delegate bool Negative_();
        private delegate bool ReplaceColor_(byte red, byte green, byte blue, float tolerance);
        private delegate bool Rotate_(float angle);
        private delegate bool Rotate3D_(float x, float y, float z, float angle);
        private delegate bool Saturate_(float saturation);
        private delegate bool Saturate2_(float red, float green, float blue, float saturation);
        private delegate bool ScaleAlpha_(float scale);
        private delegate bool ScaleColors_(float red, float green, float blue);
        private delegate bool SwapColors_();
        private delegate bool Wave_(float angle);


        private static iluInit_ _iluInit;
        private static iluBlurAverage_ _iluBlurAverage;
        private static iluBlurGaussian_ _iluBlurGaussian;
        private static iluCompareImages_ _iluCompareImages;
        private static iluCrop_ _iluCrop;
        private static iluEnlargeCanvas_ _iluEnlargeCanvas;
        private static iluEnlargeImage_ _iluEnlargeImage;
        private static iluGetErrorString_ _iluGetErrorString;
        private static iluColorsUsed_ _iluColorsUsed;
        private static iluScale_ _iluScale;
        private static iluPixelize_ _iluPixelize;
        private static iluSharpen_ _iluSharpen;
        private static iluGetString_ _iluGetString;
        private static iluImageParameter_ _iluImageParameter;
        private static iluGetInteger_ _iluGetInteger;
        private static iluRegionf_ _iluRegionf;
        private static iluRegioni_ _iluRegioni;
        private static iluNoisify_ _iluNoisify;
        private static iluSetLanguage_ _iluSetLanguage;

        private static Convolution_ _Convolution;
        private static Alienify_ _Alienify;
        private static Equalize_ _Equalize;
        private static Contrast_ _Contrast;
        private static EdgeDetectE_ _EdgeDetectE;
        private static EdgeDetectP_ _EdgeDetectP;
        private static EdgeDetectS_ _EdgeDetectS;
        private static Emboss_ _Emboss;
        private static FlipImage_ _FlipImage;
        private static BuildMipMaps_ _BuildMipMaps;
        private static GammaCorrect_ _GammaCorrect;
        private static InvertAlpha_ _InvertAlpha;
        private static Mirror_ _Mirror;
        private static Negative_ _Negative;
        private static ReplaceColor_ _ReplaceColor;
        private static Rotate_ _Rotate;
        private static Rotate3D_ _Rotate3D;
        private static Saturate_ _Saturate;
        private static Saturate2_ _Saturate2;
        private static ScaleAlpha_ _ScaleAlpha;
        private static ScaleColors_ _ScaleColors;
        private static SwapColors_ _SwapColors;
        private static Wave_ _Wave;

        #endregion

        static ILU()
        {
            if (Environment.Is64BitProcess) {
                _iluInit = iluInit_x64;
                _iluBlurAverage = iluBlurAverage_x64;
                _iluBlurGaussian = iluBlurGaussian_x64;
                _iluCompareImages = iluCompareImages_x64;
                _iluCrop = iluCrop_x64;
                _iluEnlargeCanvas = iluEnlargeCanvas_x64;
                _iluEnlargeImage = iluEnlargeImage_x64;
                _iluGetErrorString = iluGetErrorString_x64;
                _iluColorsUsed = iluColorsUsed_x64;
                _iluScale = iluScale_x64;
                _iluPixelize = iluPixelize_x64;
                _iluSharpen = iluSharpen_x64;
                _iluGetString = iluGetString_x64;
                _iluImageParameter = iluImageParameter_x64;
                _iluGetInteger = iluGetInteger_x64;
                _iluRegionf = iluRegionf_x64;
                _iluRegioni = iluRegioni_x64;
                _iluNoisify = iluNoisify_x64;
                _iluSetLanguage = iluSetLanguage_x64;
                _Convolution = Convolution_x64;
                _Alienify = Alienify_x64;
                _Equalize = Equalize_x64;
                _Contrast = Contrast_x64;
                _EdgeDetectE = EdgeDetectE_x64;
                _EdgeDetectP = EdgeDetectP_x64;
                _EdgeDetectS = EdgeDetectS_x64;
                _Emboss = Emboss_x64;
                _FlipImage = FlipImage_x64;
                _BuildMipMaps = BuildMipMaps_x64;
                _GammaCorrect = GammaCorrect_x64;
                _InvertAlpha = InvertAlpha_x64;
                _Mirror = Mirror_x64;
                _Negative = Negative_x64;
                _ReplaceColor = ReplaceColor_x64;
                _Rotate = Rotate_x64;
                _Rotate3D = Rotate3D_x64;
                _Saturate = Saturate_x64;
                _Saturate2 = Saturate2_x64;
                _ScaleAlpha = ScaleAlpha_x64;
                _ScaleColors = ScaleColors_x64;
                _SwapColors = SwapColors_x64;
                _Wave = Wave_x64;
            } else {
                _iluInit = iluInit_x32;
                _iluBlurAverage = iluBlurAverage_x32;
                _iluBlurGaussian = iluBlurGaussian_x32;
                _iluCompareImages = iluCompareImages_x32;
                _iluCrop = iluCrop_x32;
                _iluEnlargeCanvas = iluEnlargeCanvas_x32;
                _iluEnlargeImage = iluEnlargeImage_x32;
                _iluGetErrorString = iluGetErrorString_x32;
                _iluColorsUsed = iluColorsUsed_x32;
                _iluScale = iluScale_x32;
                _iluPixelize = iluPixelize_x32;
                _iluSharpen = iluSharpen_x32;
                _iluGetString = iluGetString_x32;
                _iluImageParameter = iluImageParameter_x32;
                _iluGetInteger = iluGetInteger_x32;
                _iluRegionf = iluRegionf_x32;
                _iluRegioni = iluRegioni_x32;
                _iluNoisify = iluNoisify_x32;
                _iluSetLanguage = iluSetLanguage_x32;
                _Convolution = Convolution_x32;
                _Alienify = Alienify_x32;
                _Equalize = Equalize_x32;
                _Contrast = Contrast_x32;
                _EdgeDetectE = EdgeDetectE_x32;
                _EdgeDetectP = EdgeDetectP_x32;
                _EdgeDetectS = EdgeDetectS_x32;
                _Emboss = Emboss_x32;
                _FlipImage = FlipImage_x32;
                _BuildMipMaps = BuildMipMaps_x32;
                _GammaCorrect = GammaCorrect_x32;
                _InvertAlpha = InvertAlpha_x32;
                _Mirror = Mirror_x32;
                _Negative = Negative_x32;
                _ReplaceColor = ReplaceColor_x32;
                _Rotate = Rotate_x32;
                _Rotate3D = Rotate3D_x32;
                _Saturate = Saturate_x32;
                _Saturate2 = Saturate2_x32;
                _ScaleAlpha = ScaleAlpha_x32;
                _ScaleColors = ScaleColors_x32;
                _SwapColors = SwapColors_x32;
                _Wave = Wave_x32;
            }
        }

        #region native method wrapper


        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix">3x3 Matrix (row major)</param>
        /// <param name="scale"></param>
        /// <param name="bias"></param>
        /// <returns></returns>
        public static bool Convolution(int[] matrix, int scale, int bias)
        {
            return _Convolution(matrix, scale, bias);
        }

        public static bool Alienify()
        {
            return _Alienify();
        }

        public static bool Equalize()
        {
            return _Equalize();
        }

        public static bool Contrast(float contrast)
        {
            return _Contrast(contrast);
        }

        public static bool EdgeDetectE()
        {
            return _EdgeDetectE();
        }

        public static bool EdgeDetectP()
        {
            return _EdgeDetectP();
        }

        public static bool EdgeDetectS()
        {
            return _EdgeDetectS();
        }

        public static bool Emboss()
        {
            return _Emboss();
        }

        public static bool FlipImage()
        {
            return _FlipImage();
        }

        public static bool BuildMipMaps()
        {
            return _BuildMipMaps();
        }

        public static bool GammaCorrect(float gamma)
        {
            return _GammaCorrect(gamma);
        }

        public static bool InvertAlpha()
        {
            return _InvertAlpha();
        }

        public static bool Mirror()
        {
            return _Mirror();
        }

        public static bool Negative()
        {
            return _Negative();
        }

        public static bool ReplaceColor(byte red, byte green, byte blue, float tolerance)
        {
            return _ReplaceColor(red, green, blue, tolerance);
        }

        public static bool Rotate(float angle)
        {
            return _Rotate(angle);
        }

        public static bool Rotate3D(float x, float y, float z, float angle)
        {
            return _Rotate3D(x, y, z, angle);
        }

        public static bool Saturate(float saturation)
        {
            return _Saturate(saturation);
        }

        public static bool Saturate(float red, float green, float blue, float saturation)
        {
            return _Saturate2(red, green, blue, saturation);
        }

        public static bool ScaleAlpha(float scale)
        {
            return _ScaleAlpha(scale);
        }

        public static bool ScaleColors(float red, float green, float blue)
        {
            return _ScaleColors(red, green, blue);
        }

        public static bool SwapColors()
        {
            return _SwapColors();
        }

        public static bool Wave(float angle)
        {
            return _Wave(angle);
        }

        private static void iluInit()
        {
            _iluInit();
        }

        private static bool iluBlurAverage(uint iterations)
        {
            return _iluBlurAverage(iterations);
        }

        private static bool iluBlurGaussian(uint iterations)
        {
            return _iluBlurGaussian(iterations);
        }

        private static bool iluCompareImages(uint otherImage)
        {
            return _iluCompareImages(otherImage);
        }

        private static bool iluCrop(uint offsetX, uint offsetY, uint offsetZ, uint width, uint height, uint depth)
        {
            return _iluCrop(offsetX, offsetY, offsetZ, width, height, depth);
        }

        private static bool iluEnlargeCanvas(uint width, uint height, uint depth)
        {
            return _iluEnlargeCanvas(width, height, depth);
        }

        private static bool iluEnlargeImage(uint xDim, uint yDim, uint zDim)
        {
            return _iluEnlargeImage(xDim, yDim, zDim);
        }

        private static IntPtr iluGetErrorString(uint error)
        {
            return _iluGetErrorString(error);
        }

        private static uint iluColorsUsed()
        {
            return _iluColorsUsed();
        }

        private static bool iluScale(uint width, uint height, uint depth)
        {
            return _iluScale(width, height, depth);
        }

        private static bool iluPixelize(uint pixelSize)
        {
            return _iluPixelize(pixelSize);
        }

        private static bool iluSharpen(float factor, uint iterations)
        {
            return _iluSharpen(factor, iterations);
        }

        private static IntPtr iluGetString(uint name)
        {
            return _iluGetString(name);
        }

        private static void iluImageParameter(uint pName, uint param)
        {
            _iluImageParameter(pName, param);
        }

        private static int iluGetInteger(uint mode)
        {
            return _iluGetInteger(mode);
        }

        private static void iluRegionf(PointF[] points, uint num)
        {
            _iluRegionf(points, num);
        }

        private static void iluRegioni(PointI[] points, uint num)
        {
            _iluRegioni(points, num);
        }

        private static bool iluNoisify(float tolerance)
        {
            return _iluNoisify(tolerance);
        }

        private static bool iluSetLanguage(uint language)
        {
            return _iluSetLanguage(language);
        }
        #endregion

        #region native x64
        private const String ILUDLL_x64 = "ILU64.dll";

        [DllImport(ILUDLL_x64, EntryPoint = "iluInit", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluInit_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluBlurAvg", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluBlurAverage_x64(uint iterations);

        [DllImport(ILUDLL_x64, EntryPoint = "iluBlurGaussian", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluBlurGaussian_x64(uint iterations);

        [DllImport(ILUDLL_x64, EntryPoint = "iluCompareImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluCompareImages_x64(uint otherImage);

        [DllImport(ILUDLL_x64, EntryPoint = "iluCrop", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluCrop_x64(uint offsetX, uint offsetY, uint offsetZ, uint width, uint height, uint depth);

        [DllImport(ILUDLL_x64, EntryPoint = "iluEnlargeCanvas", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluEnlargeCanvas_x64(uint width, uint height, uint depth);

        [DllImport(ILUDLL_x64, EntryPoint = "iluEnlargeImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluEnlargeImage_x64(uint xDim, uint yDim, uint zDim);

        [DllImport(ILUDLL_x64, EntryPoint = "iluErrorString", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr iluGetErrorString_x64(uint error);

        [DllImport(ILUDLL_x64, EntryPoint = "iluColoursUsed", CallingConvention = CallingConvention.StdCall)]
        private static extern uint iluColorsUsed_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluScale", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluScale_x64(uint width, uint height, uint depth);

        [DllImport(ILUDLL_x64, EntryPoint = "iluPixelize", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluPixelize_x64(uint pixelSize);

        [DllImport(ILUDLL_x64, EntryPoint = "iluSharpen", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluSharpen_x64(float factor, uint iterations);

        [DllImport(ILUDLL_x64, EntryPoint = "iluGetString", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr iluGetString_x64(uint name);

        [DllImport(ILUDLL_x64, EntryPoint = "iluImageParameter", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluImageParameter_x64(uint pName, uint param);

        [DllImport(ILUDLL_x64, EntryPoint = "iluGetInteger", CallingConvention = CallingConvention.StdCall)]
        private static extern int iluGetInteger_x64(uint mode);

        [DllImport(ILUDLL_x64, EntryPoint = "iluRegionfv", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluRegionf_x64(PointF[] points, uint num);

        [DllImport(ILUDLL_x64, EntryPoint = "iluRegioniv", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluRegioni_x64(PointI[] points, uint num);

        [DllImport(ILUDLL_x64, EntryPoint = "iluNoisify", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluNoisify_x64(float tolerance);

        [DllImport(ILUDLL_x64, EntryPoint = "iluSetLanguage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluSetLanguage_x64(uint language);

        [DllImport(ILUDLL_x64, EntryPoint = "iluConvolution", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Convolution_x64(int[] matrix, int scale, int bias);
        [DllImport(ILUDLL_x64, EntryPoint = "iluAlienify", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Alienify_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluEqualize", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Equalize_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluContrast", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Contrast_x64(float contrast);

        [DllImport(ILUDLL_x64, EntryPoint = "iluEdgeDetectE", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool EdgeDetectE_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluEdgeDetectP", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool EdgeDetectP_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluEdgeDetectS", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool EdgeDetectS_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluEmboss", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Emboss_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluFlipImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool FlipImage_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluBuildMipmaps", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool BuildMipMaps_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluGammaCorrect", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GammaCorrect_x64(float gamma);

        [DllImport(ILUDLL_x64, EntryPoint = "iluInvertAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool InvertAlpha_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluMirror", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Mirror_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluNegative", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Negative_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluReplaceColour", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool ReplaceColor_x64(byte red, byte green, byte blue, float tolerance);

        [DllImport(ILUDLL_x64, EntryPoint = "iluRotate", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Rotate_x64(float angle);

        [DllImport(ILUDLL_x64, EntryPoint = "iluRotate3D", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Rotate3D_x64(float x, float y, float z, float angle);

        [DllImport(ILUDLL_x64, EntryPoint = "iluSaturate1f", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Saturate_x64(float saturation);

        [DllImport(ILUDLL_x64, EntryPoint = "iluSaturate4f", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Saturate2_x64(float red, float green, float blue, float saturation);

        [DllImport(ILUDLL_x64, EntryPoint = "iluScaleAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool ScaleAlpha_x64(float scale);

        [DllImport(ILUDLL_x64, EntryPoint = "iluScaleColours", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool ScaleColors_x64(float red, float green, float blue);

        [DllImport(ILUDLL_x64, EntryPoint = "iluSwapColours", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool SwapColors_x64();

        [DllImport(ILUDLL_x64, EntryPoint = "iluWave", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Wave_x64(float angle);
        #endregion

        #region native x32
        private const String ILUDLL_x32 = "ILU32.dll";

        [DllImport(ILUDLL_x32, EntryPoint = "iluInit", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluInit_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluBlurAvg", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluBlurAverage_x32(uint iterations);

        [DllImport(ILUDLL_x32, EntryPoint = "iluBlurGaussian", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluBlurGaussian_x32(uint iterations);

        [DllImport(ILUDLL_x32, EntryPoint = "iluCompareImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluCompareImages_x32(uint otherImage);

        [DllImport(ILUDLL_x32, EntryPoint = "iluCrop", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluCrop_x32(uint offsetX, uint offsetY, uint offsetZ, uint width, uint height, uint depth);

        [DllImport(ILUDLL_x32, EntryPoint = "iluEnlargeCanvas", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluEnlargeCanvas_x32(uint width, uint height, uint depth);

        [DllImport(ILUDLL_x32, EntryPoint = "iluEnlargeImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluEnlargeImage_x32(uint xDim, uint yDim, uint zDim);

        [DllImport(ILUDLL_x32, EntryPoint = "iluErrorString", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr iluGetErrorString_x32(uint error);

        [DllImport(ILUDLL_x32, EntryPoint = "iluColoursUsed", CallingConvention = CallingConvention.StdCall)]
        private static extern uint iluColorsUsed_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluScale", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluScale_x32(uint width, uint height, uint depth);

        [DllImport(ILUDLL_x32, EntryPoint = "iluPixelize", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluPixelize_x32(uint pixelSize);

        [DllImport(ILUDLL_x32, EntryPoint = "iluSharpen", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluSharpen_x32(float factor, uint iterations);

        [DllImport(ILUDLL_x32, EntryPoint = "iluGetString", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr iluGetString_x32(uint name);

        [DllImport(ILUDLL_x32, EntryPoint = "iluImageParameter", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluImageParameter_x32(uint pName, uint param);

        [DllImport(ILUDLL_x32, EntryPoint = "iluGetInteger", CallingConvention = CallingConvention.StdCall)]
        private static extern int iluGetInteger_x32(uint mode);

        [DllImport(ILUDLL_x32, EntryPoint = "iluRegionfv", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluRegionf_x32(PointF[] points, uint num);

        [DllImport(ILUDLL_x32, EntryPoint = "iluRegioniv", CallingConvention = CallingConvention.StdCall)]
        private static extern void iluRegioni_x32(PointI[] points, uint num);

        [DllImport(ILUDLL_x32, EntryPoint = "iluNoisify", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluNoisify_x32(float tolerance);

        [DllImport(ILUDLL_x32, EntryPoint = "iluSetLanguage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool iluSetLanguage_x32(uint language);

        [DllImport(ILUDLL_x32, EntryPoint = "iluConvolution", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Convolution_x32(int[] matrix, int scale, int bias);
        [DllImport(ILUDLL_x32, EntryPoint = "iluAlienify", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Alienify_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluEqualize", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Equalize_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluContrast", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Contrast_x32(float contrast);

        [DllImport(ILUDLL_x32, EntryPoint = "iluEdgeDetectE", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool EdgeDetectE_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluEdgeDetectP", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool EdgeDetectP_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluEdgeDetectS", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool EdgeDetectS_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluEmboss", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Emboss_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluFlipImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool FlipImage_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluBuildMipmaps", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool BuildMipMaps_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluGammaCorrect", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GammaCorrect_x32(float gamma);

        [DllImport(ILUDLL_x32, EntryPoint = "iluInvertAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool InvertAlpha_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluMirror", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Mirror_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluNegative", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Negative_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluReplaceColour", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool ReplaceColor_x32(byte red, byte green, byte blue, float tolerance);

        [DllImport(ILUDLL_x32, EntryPoint = "iluRotate", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Rotate_x32(float angle);

        [DllImport(ILUDLL_x32, EntryPoint = "iluRotate3D", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Rotate3D_x32(float x, float y, float z, float angle);

        [DllImport(ILUDLL_x32, EntryPoint = "iluSaturate1f", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Saturate_x32(float saturation);

        [DllImport(ILUDLL_x32, EntryPoint = "iluSaturate4f", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Saturate2_x32(float red, float green, float blue, float saturation);

        [DllImport(ILUDLL_x32, EntryPoint = "iluScaleAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool ScaleAlpha_x32(float scale);

        [DllImport(ILUDLL_x32, EntryPoint = "iluScaleColours", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool ScaleColors_x32(float red, float green, float blue);

        [DllImport(ILUDLL_x32, EntryPoint = "iluSwapColours", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool SwapColors_x32();

        [DllImport(ILUDLL_x32, EntryPoint = "iluWave", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool Wave_x32(float angle);
        #endregion
    }
}
