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
using System.IO;
using System.Runtime.InteropServices;

namespace DevIL.Unmanaged
{
    public static class IL
    {
        private static bool _init = false;
        private static Object s_sync = new Object();
        private static int s_ref = 0;

        public static bool IsInitialized
        {
            get
            {
                return _init;
            }
        }

        internal static void AddRef()
        {
            lock (s_sync) {
                if (s_ref == 0) {
                    IL.Initialize();
                    ILU.Initialize();
                }
                s_ref++;
            }
        }

        internal static void Release()
        {
            lock (s_sync) {
                if (s_ref != 0) {
                    s_ref--;

                    if (s_ref == 0) {
                        IL.Shutdown();
                    }
                }
            }
        }

        #region IL Methods

        public static bool ActiveFace(int faceNum)
        {
            if (faceNum >= 0) {
                return ilActiveFace((uint)faceNum);
            }
            return false;
        }

        public static bool ActiveImage(int imageNum)
        {
            if (imageNum >= 0) {
                return ilActiveImage((uint)imageNum);
            }
            return false;
        }

        public static bool ActiveLayer(int layerNum)
        {
            if (layerNum >= 0) {
                return ilActiveLayer((uint)layerNum);
            }
            return false;
        }

        public static bool ActiveMipMap(int mipMapNum)
        {
            if (mipMapNum >= 0) {
                return ilActiveMipmap((uint)mipMapNum);
            }
            return false;
        }

        /* TODO
        ///InProfile: char*
        ///OutProfile: char*
        [DllImportAttribute(ILDLL, EntryPoint = "ilApplyProfile", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilApplyProfile(IntPtr InProfile, IntPtr OutProfile);
        */

        public static void BindImage(ImageID imageID)
        {
            if (imageID.ID >= 0) {
                ilBindImage((uint)imageID.ID);
            }
        }

        public static bool Blit(ImageID srcImageID, int destX, int destY, int destZ, int srcX, int srcY, int srcZ, int width, int height, int depth)
        {
            if (srcImageID.ID >= 0) {
                return ilBlit((uint)srcImageID.ID, destX, destY, destZ, (uint)srcX, (uint)srcY, (uint)srcZ, (uint)width, (uint)height, (uint)depth);
            }
            return false;
        }

        public static int CloneCurrentImage()
        {
            return (int)ilCloneCurImage();
        }

        /* TODO: Needs further investigation
        public static byte[] CompressDXT(byte[] data, int width, int height, int depth, CompressedDataFormat dxtFormat) {
            if(data == null || data.Length == 0) {
                return null;
            }

            unsafe {
                fixed(byte* ptr = data) {
                    uint sizeOfData = 0;
                    IntPtr compressedData = ilCompressDXT(new IntPtr(ptr), (uint) width, (uint) height, (uint) depth, (uint) dxtFormat, ref sizeOfData);
                    if(compressedData == IntPtr.Zero) {
                        return null;
                    }

                    byte[] dataToReturn = MemoryHelper.ReadByteBuffer(compressedData, (int) sizeOfData);

                    //Memory leak, DevIL allocates data for us, how do we free it? Function is not like the others where we can create data to
                    //get filled or get the size needed.

                    return dataToReturn;
                }
            }
        }*/

        public static bool ConvertImage(DataFormat destFormat, DataType destType)
        {
            return ilConvertImage((uint)destFormat, (uint)destType);
        }

        public static bool ConvertPalette(PaletteType palType)
        {
            return ilConvertPal((uint)palType);
        }

        public static bool CopyImage(ImageID srcImageID)
        {
            return ilCopyImage((uint)srcImageID.ID);
        }

        /// <summary>
        /// Copies the currently bounded image data to a managed byte array that gets returned. The image copied is specified by the offsets and lengths supplied.
        /// Conversions to the format/data type are handled automatically.
        /// </summary>
        /// <param name="xOffset">X Offset</param>
        /// <param name="yOffset">Y Offset</param>
        /// <param name="zOffset">Z Offset</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <param name="format">Data format to convert to</param>
        /// <param name="dataType">Data type to convert to</param>
        /// <returns>Managed byte array, or null if the operation failed</returns>
        public static byte[] CopyPixels(int xOffset, int yOffset, int zOffset, int width, int height, int depth, DataFormat format, DataType dataType)
        {
            int dataSize = MemoryHelper.GetDataSize(width, height, depth, format, dataType);
            byte[] data = new byte[dataSize];

            unsafe {
                fixed (byte* ptr = data) {
                    uint size = ilCopyPixels((uint)xOffset, (uint)yOffset, (uint)zOffset, (uint)width, (uint)height, (uint)depth, (uint)format, (uint)dataType, new IntPtr(ptr));

                    //Zero denotes something went wrong
                    if (size == 0) {
                        return null;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// DevIL will copy the currently bounded image data to the specified pointer. The image copied is specified by the offsets and lengths supplied.
        /// Conversions to the format/data type are handled automatically.
        /// </summary>
        /// <param name="xOffset">X Offset</param>
        /// <param name="yOffset">Y Offset</param>
        /// <param name="zOffset">Z Offset</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <param name="format">Data format to convert to</param>
        /// <param name="dataType">Data type to convert to</param>
        /// <param name="data">Pointer to memory that the data will be copied to</param>
        /// <returns>True if the operation succeeded or not</returns>
        public static bool CopyPixels(int xOffset, int yOffset, int zOffset, int width, int height, int depth, DataFormat format, DataType dataType, IntPtr data)
        {
            if (data == IntPtr.Zero)
                return false;

            return ilCopyPixels((uint)xOffset, (uint)yOffset, (uint)zOffset, (uint)width, (uint)height, (uint)depth, (uint)format, (uint)dataType, data) > 0;
        }

        //Looks like it creates an empty image for either next/mip/layer for current image, then creates
        //N "next" images for the subimage
        public static bool CreateSubImage(SubImageType subImageType, int subImageCount)
        {
            //Returns 0 if something happened.
            if (ilCreateSubImage((uint)subImageType, (uint)subImageCount) != 0) {
                return true;
            }
            return false;
        }

        public static void DeleteImage(ImageID imageID)
        {
            //Dont delete default, and valid images are non-negative
            if (imageID > 0)
                return;

            ilDeleteImage((uint)imageID.ID);
        }

        public static void DeleteImages(ImageID[] imageIDs)
        {
            uint[] ids = new uint[imageIDs.Length];
            for (int i = 0; i < imageIDs.Length; i++) {
                ids[i] = (uint)imageIDs[i].ID;
            }

            UIntPtr size = new UIntPtr((uint)ids.Length);

            ilDeleteImages(size, ids);
        }

        public static ImageType DetermineImageType(String fileName)
        {
            if (String.IsNullOrEmpty(fileName)) {
                return ImageType.Unknown;
            }
            return (ImageType)ilDetermineType(fileName);
        }

        public static ImageType DetermineImageType(byte[] lump)
        {
            if (lump == null || lump.Length == 0)
                return ImageType.Unknown;

            uint size = (uint)lump.Length;

            unsafe {
                fixed (byte* ptr = lump) {
                    return (ImageType)ilDetermineTypeL(new IntPtr(ptr), size);
                }
            }
        }

        /// <summary>
        /// Determines the image type from the specified file extension.
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns></returns>
        public static ImageType DetermineImageTypeFromExtension(String extension)
        {
            if (String.IsNullOrEmpty(extension))
                return ImageType.Unknown;

            return (ImageType)ilTypeFromExt(extension);
        }

        /// <summary>
        /// Disables an enable bit.
        /// </summary>
        /// <param name="mode">Enable bit to disable</param>
        /// <returns>True if disabled</returns>
        public static bool Disable(ILEnable mode)
        {
            return ilDisable((uint)mode);
        }


        /// <summary>
        /// Enables an enable bit.
        /// </summary>
        /// <param name="mode">Enable bit to enable</param>
        /// <returns>True if enabled</returns>
        public static bool Enable(ILEnable mode)
        {
            return ilEnable((uint)mode);
        }


        /// <summary>
        /// Creates an image and returns the image's id.
        /// </summary>
        /// <returns>Generated image id</returns>
        public static ImageID GenerateImage()
        {
            return new ImageID((int)ilGenImage());
        }

        /// <summary>
        /// Batch generates images and returns an array of the generated image ids.
        /// </summary>
        /// <param name="count">Number of images to generate</param>
        /// <returns>Generated images</returns>
        public static ImageID[] GenerateImages(int count)
        {
            UIntPtr num = new UIntPtr((uint)count);
            uint[] images = new uint[count];
            ilGenImages(num, images);

            ImageID[] copy = new ImageID[count];
            for (int i = 0; i < count; i++) {
                copy[i] = new ImageID((int)images[i]);
            }

            return copy;
        }

        /* Needs investigation
        public static byte[] GetAlphaData(DataType dataType) {
            //Returns a pointer that gets allocated, we don't have a way to release the memory?
        }*/

        public static bool GetBoolean(ILBooleanMode mode)
        {
            return (ilGetInteger((uint)mode) == 0) ? false : true;
        }

        public static int GetInteger(ILIntegerMode mode)
        {
            return ilGetInteger((uint)mode);
        }

        public static byte[] GetDxtcData(CompressedDataFormat dxtcFormat)
        {
            uint bufferSize = ilGetDXTCData(IntPtr.Zero, 0, (uint)dxtcFormat);
            if (bufferSize == 0) {
                return null;
            }
            byte[] buffer = new byte[bufferSize];

            unsafe {
                fixed (byte* ptr = buffer) {
                    ilGetDXTCData(new IntPtr(ptr), bufferSize, (uint)dxtcFormat);
                }
            }
            return buffer;
        }

        /// <summary>
        /// Gets the last set error.
        /// </summary>
        /// <returns>Error type</returns>
        public static ErrorType GetError()
        {
            return (ErrorType)ilGetError();
        }

        /// <summary>
        /// Gets the total (uncompressed) data of the currently bound image.
        /// </summary>
        /// <returns>Image data</returns>
        public static byte[] GetImageData()
        {
            IntPtr ptr = ilGetData();

            if (ptr == IntPtr.Zero) {
                return null;
            }

            int size = ilGetInteger((uint)ILDefines.IL_IMAGE_SIZE_OF_DATA);

            return MemoryHelper.ReadByteBuffer(ptr, size);
        }

        /// <summary>
        /// Gets an unmanaged pointer to the uncompressed data of the currently bound image.
        /// </summary>
        /// <returns>Unmanaged pointer to the image data</returns>
        public static IntPtr GetData()
        {
            return ilGetData();
        }

        public static byte[] GetPaletteData()
        {
            PaletteType type = (PaletteType)ilGetInteger((uint)ILDefines.IL_PALETTE_TYPE);
            int palColumnCount = ilGetInteger((uint)ILDefines.IL_PALETTE_NUM_COLS);
            int bpp = MemoryHelper.GetPaletteComponentCount(type);

            int size = bpp * palColumnCount;

            //Getting a pointer directly to the palette data, so dont need to free
            IntPtr ptr = ilGetPalette();

            return MemoryHelper.ReadByteBuffer(ptr, size);
        }

        /// <summary>
        /// Gets the currently set global data format.
        /// </summary>
        /// <returns>Data format</returns>
        public static DataFormat GetDataFormat()
        {
            return (DataFormat)ilGetInteger((uint)ILDefines.IL_FORMAT_MODE);
        }

        /// <summary>
        /// Gets the currently set global compressed data format.
        /// </summary>
        /// <returns>Compressed data format</returns>
        public static CompressedDataFormat GetDxtcFormat()
        {
            return (CompressedDataFormat)ilGetInteger((uint)ILDefines.IL_DXTC_FORMAT);
        }

        /// <summary>
        /// Gets the currently set global data type.
        /// </summary>
        /// <returns>Data type</returns>
        public static DataType GetDataType()
        {
            return (DataType)ilGetInteger((uint)ILDefines.IL_TYPE_MODE);
        }

        /// <summary>
        /// Gets the currently set jpg save format state.
        /// </summary>
        /// <returns></returns>
        public static JpgSaveFormat GetJpgSaveFormat()
        {
            return (JpgSaveFormat)ilGetInteger((uint)ILDefines.IL_JPG_SAVE_FORMAT);
        }

        /// <summary>
        /// Gets the currently set global origin location.
        /// </summary>
        /// <returns>Image origin</returns>
        public static OriginLocation GetOriginLocation()
        {
            return (OriginLocation)ilGetInteger((uint)ILDefines.IL_ORIGIN_MODE);
        }

        /// <summary>
        /// Gets the currently set string value for the state.
        /// </summary>
        /// <param name="mode">String state type</param>
        /// <returns>String value</returns>
        public static String GetString(ILStringMode mode)
        {
            IntPtr ptr = ilGetString((uint)mode);

            if (ptr != IntPtr.Zero) {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return String.Empty;
        }

        /// <summary>
        /// Gets information about the currently bound image.
        /// </summary>
        /// <returns>Image Info</returns>
        public static ImageInfo GetImageInfo()
        {
            ImageInfo info = new ImageInfo();
            info.Format = (DataFormat)ilGetInteger(ILDefines.IL_IMAGE_FORMAT);
            info.DxtcFormat = (CompressedDataFormat)ilGetInteger(ILDefines.IL_DXTC_DATA_FORMAT);
            info.DataType = (DataType)ilGetInteger(ILDefines.IL_IMAGE_TYPE);
            info.PaletteType = (PaletteType)ilGetInteger(ILDefines.IL_PALETTE_TYPE);
            info.PaletteBaseType = (DataFormat)ilGetInteger(ILDefines.IL_PALETTE_BASE_TYPE);
            info.CubeFlags = (CubeMapFace)ilGetInteger(ILDefines.IL_IMAGE_CUBEFLAGS);
            info.Origin = (OriginLocation)ilGetInteger(ILDefines.IL_IMAGE_ORIGIN);
            info.Width = ilGetInteger(ILDefines.IL_IMAGE_WIDTH);
            info.Height = ilGetInteger(ILDefines.IL_IMAGE_HEIGHT);
            info.Depth = ilGetInteger(ILDefines.IL_IMAGE_DEPTH);
            info.BitsPerPixel = ilGetInteger(ILDefines.IL_IMAGE_BITS_PER_PIXEL);
            info.BytesPerPixel = ilGetInteger(ILDefines.IL_IMAGE_BYTES_PER_PIXEL);
            info.Channels = ilGetInteger(ILDefines.IL_IMAGE_CHANNELS);
            info.Duration = ilGetInteger(ILDefines.IL_IMAGE_DURATION);
            info.SizeOfData = ilGetInteger(ILDefines.IL_IMAGE_SIZE_OF_DATA);
            info.OffsetX = ilGetInteger(ILDefines.IL_IMAGE_OFFX);
            info.OffsetY = ilGetInteger(ILDefines.IL_IMAGE_OFFY);
            info.PlaneSize = ilGetInteger(ILDefines.IL_IMAGE_PLANESIZE);
            info.FaceCount = ilGetInteger(ILDefines.IL_NUM_FACES) + 1;
            info.ImageCount = ilGetInteger(ILDefines.IL_NUM_IMAGES) + 1;
            info.LayerCount = ilGetInteger(ILDefines.IL_NUM_LAYERS) + 1;
            info.MipMapCount = ilGetInteger(ILDefines.IL_NUM_MIPMAPS) + 1;
            info.PaletteBytesPerPixel = ilGetInteger(ILDefines.IL_PALETTE_BPP);
            info.PaletteColumnCount = ilGetInteger(ILDefines.IL_PALETTE_NUM_COLS);
            return info;
        }

        /// <summary>
        /// Gets the quantization state.
        /// </summary>
        /// <returns>Quantization state</returns>
        public static Quantization GetQuantization()
        {
            return (Quantization)ilGetInteger((uint)ILDefines.IL_QUANTIZATION_MODE);
        }

        /// <summary>
        /// Initializes the DevIL subsystem. This needs to be called before any other function
        /// is called. The wrapper will filter out subsequent calls until Shutdown() is called.
        /// </summary>
        public static void Initialize()
        {
            if (!_init) {
                ilInit();
                _init = true;
            }
        }

        /// <summary>
        /// Checks if the enable bit is disabled.
        /// </summary>
        /// <param name="mode">Enable bit</param>
        /// <returns>True if disabled, false otherwise</returns>
        public static bool IsDisabled(ILEnable mode)
        {
            return ilIsDisabled((uint)mode);
        }

        /// <summary>
        /// Checks if the enable bit is enabled.
        /// </summary>
        /// <param name="mode">Enable bit</param>
        /// <returns>True if enabled, false otherwise</returns>
        public static bool IsEnabled(ILEnable mode)
        {
            return ilIsEnabled((uint)mode);
        }

        /// <summary>
        /// Converts the currently bound image data to the specified compressed format. The conversion
        /// occurs for each surface in the image (next image, and each image's mip map chain). This is identical to looping over
        /// these surfaces and calling SurfaceToDxtcData(CompressedDataFormat).
        /// </summary>
        /// <param name="format">Compressed format to convert image data to</param>
        /// <returns>True if the operation was successful</returns>
        public static bool ImageToDxtcData(CompressedDataFormat format)
        {
            return ilImageToDxtcData((uint)format);
        }

        /// <summary>
        /// Checks if the imageID is in fact an image.
        /// </summary>
        /// <param name="imageID">Image ID</param>
        /// <returns>True if an image, false otherwise</returns>
        public static bool IsImage(ImageID imageID)
        {
            if (imageID.ID < 0)
                return false;

            return ilIsImage((uint)imageID.ID);
        }

        /// <summary>
        /// Checks if the specified file is a valid image of the specified type.
        /// </summary>
        /// <param name="imageType">Image type</param>
        /// <param name="filename">Filename of the image</param>
        /// <returns>True if the file is of the specified image type, false otherwise</returns>
        public static bool IsValid(ImageType imageType, String filename)
        {
            if (imageType == ImageType.Unknown || String.IsNullOrEmpty(filename))
                return false;

            return ilIsValid((uint)imageType, filename);
        }

        /// <summary>
        /// Checks if the raw data is a valid image of the specified type.
        /// </summary>
        /// <param name="imageType">Image type</param>
        /// <param name="data">Raw data</param>
        /// <returns>True if the raw data is of the specified image type, false otherwise.</returns>
        public static bool IsValid(ImageType imageType, byte[] data)
        {
            if (imageType == ImageType.Unknown || data == null || data.Length == 0)
                return false;

            unsafe {
                fixed (byte* ptr = data) {
                    return ilIsValidL((uint)imageType, new IntPtr(ptr), (uint)data.Length);
                }
            }
        }

        public static bool LoadImage(ImageType imageType, String filename)
        {
            return ilLoad((uint)imageType, filename);
        }

        public static bool LoadImageFromStream(ImageType imageType, Stream stream)
        {
            if (imageType == ImageType.Unknown || stream == null || !stream.CanRead)
                return false;

            byte[] rawData = MemoryHelper.ReadStreamFully(stream, 0);
            uint size = (uint)rawData.Length;
            bool flag = false;
            unsafe {
                fixed (byte* ptr = rawData) {
                    flag = ilLoadL((uint)imageType, new IntPtr(ptr), size);
                }
            }

            return flag;
        }

        public static bool LoadImageFromStream(Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return false;

            byte[] rawData = MemoryHelper.ReadStreamFully(stream, 0);
            uint size = (uint)rawData.Length;
            bool flag = false;
            ImageType imageExtension = DetermineImageType(rawData);
            unsafe {
                fixed (byte* ptr = rawData) {
                    flag = ilLoadL((uint)imageExtension, new IntPtr(ptr), size);
                }
            }

            return flag;
        }

        /// <summary>
        /// Tries to read raw data of an image that was dumped to a file.
        /// </summary>
        /// <param name="filename">File to laod from</param>
        /// <param name="width">Known image width</param>
        /// <param name="height">Known image height</param>
        /// <param name="depth">Known image depth</param>
        /// <param name="componentCount">Number of components for each pixel (1, 3, or 4)</param>
        /// <returns></returns>
        public static bool LoadRawData(String filename, int width, int height, int depth, int componentCount)
        {
            if (String.IsNullOrEmpty(filename) || width < 1 || height < 1 || depth < 1)
                return false;

            if (componentCount != 1 || componentCount != 3 || componentCount != 4)
                return false;

            return ilLoadData(filename, (uint)width, (uint)height, (uint)depth, (byte)componentCount);
        }

        public static bool LoadRawData(byte[] data, int width, int height, int depth, int componentCount)
        {
            if (width < 1 || height < 1 || depth < 1)
                return false;

            if (componentCount != 1 || componentCount != 3 || componentCount != 4)
                return false;

            uint size = (uint)data.Length;

            unsafe {
                fixed (byte* ptr = data) {
                    return ilLoadDataL(new IntPtr(ptr), size, (uint)width, (uint)height, (uint)depth, (byte)componentCount);
                }
            }
        }

        /// <summary>
        /// Overlays the source image, over the currently bound image at the offsets specified. This basically
        /// performs a blit behind the scenes, so set blit parameters accordingly.
        /// </summary>
        /// <param name="srcImageID">Source image id</param>
        /// <param name="destX">Destination x offset</param>
        /// <param name="destY">Destination y offset</param>
        /// <param name="destZ">Destination z offset</param>
        /// <returns></returns>
        public static bool OverlayImage(ImageID srcImageID, int destX, int destY, int destZ)
        {
            if (srcImageID.ID < 0) {
                return false;
            }

            return ilOverlayImage((uint)srcImageID.ID, destX, destY, destZ);
        }

        public static bool SaveImage(ImageType type, String filename)
        {
            return ilSave((uint)type, filename);
        }

        public static bool SaveImageToStream(ImageType imageType, Stream stream)
        {
            if (imageType == ImageType.Unknown || stream == null || !stream.CanWrite)
                return false;

            uint size = ilSaveL((uint)imageType, IntPtr.Zero, 0);

            if (size == 0)
                return false;

            byte[] buffer = new byte[size];

            unsafe {
                fixed (byte* ptr = buffer) {
                    if (ilSaveL((uint)imageType, new IntPtr(ptr), size) == 0)
                        return false;
                }
            }

            stream.Write(buffer, 0, buffer.Length);

            return true;
        }

        public static void PushAttribute(AttributeBits bits)
        {
            ilPushAttrib((uint)bits);
        }

        public static void SetBoolean(ILBooleanMode mode, bool value)
        {
            ilSetInteger((uint)mode, (value) ? 1 : 0);
        }

        public static bool SetCompressionAlgorithm(CompressionAlgorithm compressAlgorithm)
        {
            return ilCompressFunc((uint)compressAlgorithm);
        }

        public static bool SetDataFormat(DataFormat dataFormat)
        {
            return ilFormatFunc((uint)dataFormat);
        }

        /// <summary>
        /// Uploads the data to replace the currently bounded image's data. Ensure they're the same size before calling.
        /// </summary>
        /// <param name="data">Data to set</param>
        /// <returns>True if the operation was successful or not</returns>
        public static bool SetImageData(byte[] data)
        {
            unsafe {
                fixed (byte* ptr = data) {
                    return ilSetData(new IntPtr(ptr));
                }
            }
        }

        /// <summary>
        /// Sets the time duration of the currently bounded image should be displayed for (in an animation sequence).
        /// </summary>
        /// <param name="duration">Duration</param>
        /// <returns>True if the operation was successful or not</returns>
        public static bool SetDuration(int duration)
        {
            if (duration < 0)
                return false;

            return ilSetDuration((uint)duration);
        }

        public static void SetDxtcFormat(CompressedDataFormat format)
        {
            ilSetInteger((uint)ILDefines.IL_DXTC_FORMAT, (int)format);
        }

        public static bool SetDataType(DataType dataType)
        {
            return ilTypeFunc((uint)dataType);
        }

        public static void SetKeyColor(Color color)
        {
            SetKeyColor(color.R, color.G, color.B, color.A);
        }

        public static void SetMemoryHint(MemoryHint hint)
        {
            ilHint((uint)ILDefines.IL_MEM_SPEED_HINT, (uint)hint);
        }

        public static void SetCompressionHint(CompressionHint hint)
        {
            ilHint((uint)ILDefines.IL_COMPRESSION_HINT, (uint)hint);
        }

        public static void SetJpgSaveFormat(JpgSaveFormat format)
        {
            ilSetInteger((uint)ILDefines.IL_JPG_SAVE_FORMAT, (int)format);
        }

        public static void SetInteger(ILIntegerMode mode, int value)
        {
            ilSetInteger((uint)mode, value);
        }

        public static void SetOriginLocation(OriginLocation origin)
        {
            ilOriginFunc((uint)origin);
        }

        public static void SetString(ILStringMode mode, String value)
        {
            if (value == null) {
                value = String.Empty;
            }

            ilSetString((uint)mode, value);
        }

        public static void SetQuantization(Quantization mode)
        {
            ilSetInteger((uint)ILDefines.IL_QUANTIZATION_MODE, (int)mode);
        }

        public static bool SetPixels(int xOffset, int yOffset, int zOffset, int width, int height, int depth, DataFormat format, DataType dataType, byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;

            if (xOffset < 0 || yOffset < 0 || zOffset < 0 || width < 1 || height < 1 || depth < 1)
                return false;

            uint size = (uint)data.Length;

            unsafe {
                fixed (byte* ptr = data) {
                    ilSetPixels(xOffset, yOffset, zOffset, (uint)width, (uint)height, (uint)depth, (uint)format, (uint)dataType, new IntPtr(ptr));
                }
            }
            return true;
        }

        /// <summary>
        /// Shuts DevIL's subsystem down, freeing up memory allocated for images. After this call is made, to use the wrapper again you
        /// need to call Initialize().
        /// </summary>
        public static void Shutdown()
        {
            if (_init) {
                ilShutDown();
                _init = false;
            }
        }

        /// <summary>
        /// Converts the currently bound surface (image, mipmap, etc) to the specified compressed format.
        /// </summary>
        /// <param name="format">Comrpessed format</param>
        /// <returns>True if the operation was successful or not.</returns>
        public static bool SurfaceToDxtcData(CompressedDataFormat format)
        {
            return ilSurfaceToDxtcData((uint)format);
        }

        /// <summary>
        /// Resets the currently bounded image with the new parameters. This destroys all existing data.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="bytesPerComponent"></param>
        /// <param name="format"></param>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SetTexImage(int width, int height, int depth, DataFormat format, DataType dataType, byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;

            byte bpp = (byte)MemoryHelper.GetFormatComponentCount(format);

            unsafe {
                fixed (byte* ptr = data) {
                    return ilTexImage((uint)width, (uint)height, (uint)depth, bpp, (uint)format, (uint)dataType, new IntPtr(ptr));
                }
            }

        }

        /// <summary>
        /// Resets the currently bounded image with the new parameters. This destroys all existing data.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SetTexImageDxtc(int width, int height, int depth, CompressedDataFormat format, byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;
            unsafe {
                fixed (byte* ptr = data) {
                    return ilTexImageDxtc(width, height, depth, (uint)format, new IntPtr(ptr));
                }
            }
        }

        #endregion

        #region Library Info

        public static String GetVendorName()
        {
            IntPtr value = ilGetString(ILDefines.IL_VENDOR);
            if (value != IntPtr.Zero) {
                return Marshal.PtrToStringAnsi(value);
            }
            return "DevIL";
        }

        public static String GetVersion()
        {
            IntPtr value = ilGetString(ILDefines.IL_VERSION_NUM);
            if (value != IntPtr.Zero) {
                return Marshal.PtrToStringAnsi(value);
            }
            return "Unknown Version";
        }

        public static String[] GetImportExtensions()
        {
            IntPtr value = ilGetString(ILDefines.IL_LOAD_EXT);
            if (value != IntPtr.Zero) {
                String ext = Marshal.PtrToStringAnsi(value);
                String[] exts = ext.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < exts.Length; i++) {
                    String str = exts[i];
                    //Fix for what looks like a bug: Two entries don't have a space between them, whatmore the dds is
                    //a duplicate anyways
                    if (str.Equals("dcmdds")) {
                        str = str.Substring(0, "dcm".Length);
                    }
                    exts[i] = "." + str;
                }
                return exts;
            }
            return new String[0];
        }

        public static String[] GetExportExtensions()
        {
            IntPtr value = ilGetString(ILDefines.IL_SAVE_EXT);
            if (value != IntPtr.Zero) {
                String ext = Marshal.PtrToStringAnsi(value);
                String[] exts = ext.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < exts.Length; i++) {
                    exts[i] = "." + exts[i];
                }

                return exts;
            }
            return new String[0];
        }
        #endregion

        #region delegates

        private delegate bool ilClampNTSC_();
        private delegate void ilClearColor_(float red, float green, float blue, float alpha);
        private delegate bool ilClearImage_();
        private delegate bool ilApplyPalette_([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);
        private delegate void ilSetKeyColor_(float red, float green, float blue, float alpha);
        private delegate bool ilSaveRawData_([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName);
        private delegate bool ilSavePalette_([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);
        private delegate bool ilSetAlpha_(double alphaValue);
        private delegate void ilModulateAlpha_(double alphaValue);
        private delegate bool ilLoadPalette_([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String fileName);
        private delegate bool ilLoadImage_([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);
        private delegate void ilPopAttribute_();
        private delegate bool ilInvertSurfaceDxtcDataAlpha_();
        private delegate bool ilDxtcDataToImage_();
        private delegate bool ilDxtcDataToSurface_();
        private delegate void ilFlipSurfaceDxtcData_();
        private delegate bool ilDefaultImage_();
        private delegate bool ilActiveFace_(uint Number);
        private delegate bool ilActiveImage_(uint Number);
        private delegate bool ilActiveLayer_(uint Number);
        private delegate bool ilActiveMipmap_(uint Number);
        private delegate bool ilApplyProfile_(IntPtr InProfile, IntPtr OutProfile);
        private delegate void ilBindImage_(uint Image);
        private delegate bool ilBlit_(uint Source, int DestX, int DestY, int DestZ, uint SrcX, uint SrcY, uint SrcZ, uint Width, uint Height, uint Depth);
        private delegate uint ilCloneCurImage_();
        private delegate IntPtr ilCompressDXT_(IntPtr Data, uint Width, uint Height, uint Depth, uint DXTCFormat, ref	uint DXTCSize);
        private delegate bool ilCompressFunc_(uint Mode);
        private delegate bool ilConvertImage_(uint DestFormat, uint DestType);
        private delegate bool ilConvertPal_(uint DestFormat);
        private delegate bool ilCopyImage_(uint Src);
        private delegate uint ilCopyPixels_(uint XOff, uint YOff, uint ZOff, uint Width, uint Height, uint Depth, uint Format, uint Type, IntPtr Data);
        private delegate uint ilCreateSubImage_(uint Type, uint Num);
        private delegate void ilDeleteImage_(uint Num);
        private delegate void ilDeleteImages_(UIntPtr Num, uint[] Images);
        private delegate uint ilDetermineType_([InAttribute()]	[MarshalAs(UnmanagedType.LPStr)]	String FileName);
        private delegate uint ilDetermineTypeL_(IntPtr Lump, uint Size);
        private delegate bool ilDisable_(uint Mode);
        private delegate bool ilEnable_(uint Mode);
        private delegate bool ilFormatFunc_(uint Mode);
        private delegate void ilGenImages_(UIntPtr Num, uint[] Images);
        private delegate uint ilGenImage_();
        private delegate IntPtr ilGetAlpha_(uint Type);
        private delegate IntPtr ilGetData_();
        private delegate uint ilGetDXTCData_(IntPtr Buffer, uint BufferSize, uint DXTCFormat);
        private delegate uint ilGetError_();
        private delegate int ilGetInteger_(uint Mode);
        private delegate IntPtr ilGetPalette_();
        private delegate IntPtr ilGetString_(uint StringName);
        private delegate void ilHint_(uint Target, uint Mode);
        private delegate void ilInit_();
        private delegate bool ilImageToDxtcData_(uint Format);
        private delegate bool ilIsDisabled_(uint Mode);
        private delegate bool ilIsEnabled_(uint Mode);
        private delegate bool ilIsImage_(uint Image);
        private delegate bool ilIsValid_(uint Type, [InAttribute()]	[MarshalAs(UnmanagedType.LPStr)]	String FileName);
        private delegate bool ilIsValidL_(uint Type, IntPtr Lump, uint Size);
        private delegate bool ilLoad_(uint Type, [InAttribute()]	[MarshalAs(UnmanagedType.LPStr)]	String FileName);
        private delegate bool ilLoadL_(uint Type, IntPtr Lump, uint Size);
        private delegate bool ilOriginFunc_(uint Mode);
        private delegate bool ilOverlayImage_(uint Source, int XCoord, int YCoord, int ZCoord);
        private delegate void ilPushAttrib_(uint Bits);
        private delegate bool ilSave_(uint Type, [InAttribute()]	[MarshalAs(UnmanagedType.LPStr)]	String FileName);
        private delegate bool ilSaveImage_([InAttribute()]	[MarshalAs(UnmanagedType.LPStr)]	String FileName);
        private delegate uint ilSaveL_(uint Type, IntPtr Lump, uint Size);
        private delegate bool ilSetData_(IntPtr Data);
        private delegate bool ilSetDuration_(uint Duration);
        private delegate void ilSetInteger_(uint Mode, int Param);
        private delegate void ilSetPixels_(int XOff, int YOff, int ZOff, uint Width, uint Height, uint Depth, uint Format, uint Type, IntPtr Data);
        private delegate void ilSetString_(uint Mode, [InAttribute()]	[MarshalAsAttribute(UnmanagedType.LPStr)]	String String);
        private delegate void ilShutDown_();
        private delegate bool ilSurfaceToDxtcData_(uint Format);
        private delegate bool ilTexImage_(uint Width, uint Height, uint Depth, byte Bpp, uint Format, uint Type, IntPtr Data);
        private delegate bool ilTexImageDxtc_(int Width, int Height, int Depth, uint DxtFormat, IntPtr Data);
        private delegate uint ilTypeFromExt_([InAttribute()]	[MarshalAsAttribute(UnmanagedType.LPStr)]	String FileName);
        private delegate bool ilTypeFunc_(uint Mode);
        private delegate bool ilLoadData_([InAttribute()]	[MarshalAsAttribute(UnmanagedType.LPStr)]	String FileName, uint Width, uint Height, uint Depth, byte Bpp);
        private delegate bool ilLoadDataL_(IntPtr Lump, uint Size, uint Width, uint Height, uint Depth, byte Bpp);


        private static ilClampNTSC_ _ilClampNTSC;
        private static ilClearColor_ _ilClearColor;
        private static ilClearImage_ _ilClearImage;
        private static ilApplyPalette_ _ilApplyPalette;
        private static ilSetKeyColor_ _ilSetKeyColor;
        private static ilSaveRawData_ _ilSaveRawData;
        private static ilSavePalette_ _ilSavePalette;
        private static ilSetAlpha_ _ilSetAlpha;
        private static ilModulateAlpha_ _ilModulateAlpha;
        private static ilLoadPalette_ _ilLoadPalette;
        private static ilLoadImage_ _ilLoadImage;
        private static ilPopAttribute_ _ilPopAttribute;
        private static ilInvertSurfaceDxtcDataAlpha_ _ilInvertSurfaceDxtcDataAlpha;
        private static ilDxtcDataToImage_ _ilDxtcDataToImage;
        private static ilDxtcDataToSurface_ _ilDxtcDataToSurface;
        private static ilFlipSurfaceDxtcData_ _ilFlipSurfaceDxtcData;
        private static ilDefaultImage_ _ilDefaultImage;

        private static ilActiveFace_ _ilActiveFace;
        private static ilActiveImage_ _ilActiveImage;
        private static ilActiveLayer_ _ilActiveLayer;
        private static ilActiveMipmap_ _ilActiveMipmap;
        private static ilApplyProfile_ _ilApplyProfile;
        private static ilBindImage_ _ilBindImage;
        private static ilBlit_ _ilBlit;
        private static ilCloneCurImage_ _ilCloneCurImage;
        private static ilCompressDXT_ _ilCompressDXT;
        private static ilCompressFunc_ _ilCompressFunc;
        private static ilConvertImage_ _ilConvertImage;
        private static ilConvertPal_ _ilConvertPal;
        private static ilCopyImage_ _ilCopyImage;
        private static ilCopyPixels_ _ilCopyPixels;
        private static ilCreateSubImage_ _ilCreateSubImage;
        private static ilDeleteImage_ _ilDeleteImage;
        private static ilDeleteImages_ _ilDeleteImages;
        private static ilDetermineType_ _ilDetermineType;
        private static ilDetermineTypeL_ _ilDetermineTypeL;
        private static ilDisable_ _ilDisable;
        private static ilEnable_ _ilEnable;
        private static ilFormatFunc_ _ilFormatFunc;
        private static ilGenImages_ _ilGenImages;
        private static ilGenImage_ _ilGenImage;
        private static ilGetAlpha_ _ilGetAlpha;
        private static ilGetData_ _ilGetData;
        private static ilGetDXTCData_ _ilGetDXTCData;
        private static ilGetError_ _ilGetError;
        private static ilGetInteger_ _ilGetInteger;
        private static ilGetPalette_ _ilGetPalette;
        private static ilGetString_ _ilGetString;
        private static ilHint_ _ilHint;
        private static ilInit_ _ilInit;
        private static ilImageToDxtcData_ _ilImageToDxtcData;
        private static ilIsDisabled_ _ilIsDisabled;
        private static ilIsEnabled_ _ilIsEnabled;
        private static ilIsImage_ _ilIsImage;
        private static ilIsValid_ _ilIsValid;
        private static ilIsValidL_ _ilIsValidL;
        private static ilLoad_ _ilLoad;
        private static ilLoadL_ _ilLoadL;
        private static ilOriginFunc_ _ilOriginFunc;
        private static ilOverlayImage_ _ilOverlayImage;
        private static ilPushAttrib_ _ilPushAttrib;
        private static ilSave_ _ilSave;
        private static ilSaveImage_ _ilSaveImage;
        private static ilSaveL_ _ilSaveL;
        private static ilSetData_ _ilSetData;
        private static ilSetDuration_ _ilSetDuration;
        private static ilSetInteger_ _ilSetInteger;
        private static ilSetPixels_ _ilSetPixels;
        private static ilSetString_ _ilSetString;
        private static ilShutDown_ _ilShutDown;
        private static ilSurfaceToDxtcData_ _ilSurfaceToDxtcData;
        private static ilTexImage_ _ilTexImage;
        private static ilTexImageDxtc_ _ilTexImageDxtc;
        private static ilTypeFromExt_ _ilTypeFromExt;
        private static ilTypeFunc_ _ilTypeFunc;
        private static ilLoadData_ _ilLoadData;
        private static ilLoadDataL_ _ilLoadDataL;
        #endregion

        static IL()
        {
            if (Environment.Is64BitProcess) {
                _ilClampNTSC = ilClampNTSC_x64;
                _ilClearColor = ilClearColor_x64;
                _ilClearImage = ilClearImage_x64;
                _ilApplyPalette = ilApplyPalette_x64;
                _ilSetKeyColor = ilSetKeyColor_x64;
                _ilSaveRawData = ilSaveRawData_x64;
                _ilSavePalette = ilSavePalette_x64;
                _ilSetAlpha = ilSetAlpha_x64;
                _ilModulateAlpha = ilModulateAlpha_x64;
                _ilLoadPalette = ilLoadPalette_x64;
                _ilLoadImage = ilLoadImage_x64;
                _ilPopAttribute = ilPopAttribute_x64;
                _ilInvertSurfaceDxtcDataAlpha = ilInvertSurfaceDxtcDataAlpha_x64;
                _ilDxtcDataToImage = ilDxtcDataToImage_x64;
                _ilDxtcDataToSurface = ilDxtcDataToSurface_x64;
                _ilFlipSurfaceDxtcData = ilFlipSurfaceDxtcData_x64;
                _ilDefaultImage = ilDefaultImage_x64;
                _ilActiveFace = ilActiveFace_x64;
                _ilActiveImage = ilActiveImage_x64;
                _ilActiveLayer = ilActiveLayer_x64;
                _ilActiveMipmap = ilActiveMipmap_x64;
                _ilApplyProfile = ilApplyProfile_x64;
                _ilBindImage = ilBindImage_x64;
                _ilBlit = ilBlit_x64;
                _ilCloneCurImage = ilCloneCurImage_x64;
                _ilCompressDXT = ilCompressDXT_x64;
                _ilCompressFunc = ilCompressFunc_x64;
                _ilConvertImage = ilConvertImage_x64;
                _ilConvertPal = ilConvertPal_x64;
                _ilCopyImage = ilCopyImage_x64;
                _ilCopyPixels = ilCopyPixels_x64;
                _ilCreateSubImage = ilCreateSubImage_x64;
                _ilDeleteImage = ilDeleteImage_x64;
                _ilDeleteImages = ilDeleteImages_x64;
                _ilDetermineType = ilDetermineType_x64;
                _ilDetermineTypeL = ilDetermineTypeL_x64;
                _ilDisable = ilDisable_x64;
                _ilEnable = ilEnable_x64;
                _ilFormatFunc = ilFormatFunc_x64;
                _ilGenImages = ilGenImages_x64;
                _ilGenImage = ilGenImage_x64;
                _ilGetAlpha = ilGetAlpha_x64;
                _ilGetData = ilGetData_x64;
                _ilGetDXTCData = ilGetDXTCData_x64;
                _ilGetError = ilGetError_x64;
                _ilGetInteger = ilGetInteger_x64;
                _ilGetPalette = ilGetPalette_x64;
                _ilGetString = ilGetString_x64;
                _ilHint = ilHint_x64;
                _ilInit = ilInit_x64;
                _ilImageToDxtcData = ilImageToDxtcData_x64;
                _ilIsDisabled = ilIsDisabled_x64;
                _ilIsEnabled = ilIsEnabled_x64;
                _ilIsImage = ilIsImage_x64;
                _ilIsValid = ilIsValid_x64;
                _ilIsValidL = ilIsValidL_x64;
                _ilLoad = ilLoad_x64;
                _ilLoadL = ilLoadL_x64;
                _ilOriginFunc = ilOriginFunc_x64;
                _ilOverlayImage = ilOverlayImage_x64;
                _ilPushAttrib = ilPushAttrib_x64;
                _ilSave = ilSave_x64;
                _ilSaveImage = ilSaveImage_x64;
                _ilSaveL = ilSaveL_x64;
                _ilSetData = ilSetData_x64;
                _ilSetDuration = ilSetDuration_x64;
                _ilSetInteger = ilSetInteger_x64;
                _ilSetPixels = ilSetPixels_x64;
                _ilSetString = ilSetString_x64;
                _ilShutDown = ilShutDown_x64;
                _ilSurfaceToDxtcData = ilSurfaceToDxtcData_x64;
                _ilTexImage = ilTexImage_x64;
                _ilTexImageDxtc = ilTexImageDxtc_x64;
                _ilTypeFromExt = ilTypeFromExt_x64;
                _ilTypeFunc = ilTypeFunc_x64;
                _ilLoadData = ilLoadData_x64;
                _ilLoadDataL = ilLoadDataL_x64;
            } else {
                _ilClampNTSC = ilClampNTSC_x32;
                _ilClearColor = ilClearColor_x32;
                _ilClearImage = ilClearImage_x32;
                _ilApplyPalette = ilApplyPalette_x32;
                _ilSetKeyColor = ilSetKeyColor_x32;
                _ilSaveRawData = ilSaveRawData_x32;
                _ilSavePalette = ilSavePalette_x32;
                _ilSetAlpha = ilSetAlpha_x32;
                _ilModulateAlpha = ilModulateAlpha_x32;
                _ilLoadPalette = ilLoadPalette_x32;
                _ilLoadImage = ilLoadImage_x32;
                _ilPopAttribute = ilPopAttribute_x32;
                _ilInvertSurfaceDxtcDataAlpha = ilInvertSurfaceDxtcDataAlpha_x32;
                _ilDxtcDataToImage = ilDxtcDataToImage_x32;
                _ilDxtcDataToSurface = ilDxtcDataToSurface_x32;
                _ilFlipSurfaceDxtcData = ilFlipSurfaceDxtcData_x32;
                _ilDefaultImage = ilDefaultImage_x32;
                _ilActiveFace = ilActiveFace_x32;
                _ilActiveImage = ilActiveImage_x32;
                _ilActiveLayer = ilActiveLayer_x32;
                _ilActiveMipmap = ilActiveMipmap_x32;
                _ilApplyProfile = ilApplyProfile_x32;
                _ilBindImage = ilBindImage_x32;
                _ilBlit = ilBlit_x32;
                _ilCloneCurImage = ilCloneCurImage_x32;
                _ilCompressDXT = ilCompressDXT_x32;
                _ilCompressFunc = ilCompressFunc_x32;
                _ilConvertImage = ilConvertImage_x32;
                _ilConvertPal = ilConvertPal_x32;
                _ilCopyImage = ilCopyImage_x32;
                _ilCopyPixels = ilCopyPixels_x32;
                _ilCreateSubImage = ilCreateSubImage_x32;
                _ilDeleteImage = ilDeleteImage_x32;
                _ilDeleteImages = ilDeleteImages_x32;
                _ilDetermineType = ilDetermineType_x32;
                _ilDetermineTypeL = ilDetermineTypeL_x32;
                _ilDisable = ilDisable_x32;
                _ilEnable = ilEnable_x32;
                _ilFormatFunc = ilFormatFunc_x32;
                _ilGenImages = ilGenImages_x32;
                _ilGenImage = ilGenImage_x32;
                _ilGetAlpha = ilGetAlpha_x32;
                _ilGetData = ilGetData_x32;
                _ilGetDXTCData = ilGetDXTCData_x32;
                _ilGetError = ilGetError_x32;
                _ilGetInteger = ilGetInteger_x32;
                _ilGetPalette = ilGetPalette_x32;
                _ilGetString = ilGetString_x32;
                _ilHint = ilHint_x32;
                _ilInit = ilInit_x32;
                _ilImageToDxtcData = ilImageToDxtcData_x32;
                _ilIsDisabled = ilIsDisabled_x32;
                _ilIsEnabled = ilIsEnabled_x32;
                _ilIsImage = ilIsImage_x32;
                _ilIsValid = ilIsValid_x32;
                _ilIsValidL = ilIsValidL_x32;
                _ilLoad = ilLoad_x32;
                _ilLoadL = ilLoadL_x32;
                _ilOriginFunc = ilOriginFunc_x32;
                _ilOverlayImage = ilOverlayImage_x32;
                _ilPushAttrib = ilPushAttrib_x32;
                _ilSave = ilSave_x32;
                _ilSaveImage = ilSaveImage_x32;
                _ilSaveL = ilSaveL_x32;
                _ilSetData = ilSetData_x32;
                _ilSetDuration = ilSetDuration_x32;
                _ilSetInteger = ilSetInteger_x32;
                _ilSetPixels = ilSetPixels_x32;
                _ilSetString = ilSetString_x32;
                _ilShutDown = ilShutDown_x32;
                _ilSurfaceToDxtcData = ilSurfaceToDxtcData_x32;
                _ilTexImage = ilTexImage_x32;
                _ilTexImageDxtc = ilTexImageDxtc_x32;
                _ilTypeFromExt = ilTypeFromExt_x32;
                _ilTypeFunc = ilTypeFunc_x32;
                _ilLoadData = ilLoadData_x32;
                _ilLoadDataL = ilLoadDataL_x32;
            }
        }

        #region native Method wrapper

        public static bool ClampNTSC()
        {
            return _ilClampNTSC();
        }

        public static void ClearColor(float red, float green, float blue, float alpha)
        {
            _ilClearColor(red, green, blue, alpha);
        }

        public static bool ClearImage()
        {
            return _ilClearImage();
        }

        public static void SetKeyColor(float red, float green, float blue, float alpha)
        {
            _ilSetKeyColor(red, green, blue, alpha);
        }

        public static bool SaveRawData([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName)
        {
            return _ilSaveRawData(FileName);
        }

        public static bool SavePalette([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName)
        {
            return _ilSavePalette(FileName);
        }

        public static bool SetAlpha(double alphaValue)
        {
            return _ilSetAlpha(alphaValue);
        }

        public static void PopAttribute()
        {
            _ilPopAttribute();
        }

        public static void ModulateAlpha(double alphaValue)
        {
            _ilModulateAlpha(alphaValue);
        }

        public static bool LoadPalette([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String fileName)
        {
            return _ilLoadPalette(fileName);
        }

        public static bool LoadImage([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName)
        {
            return _ilLoadImage(FileName);
        }

        public static bool InvertSurfaceDxtcDataAlpha()
        {
            return _ilInvertSurfaceDxtcDataAlpha();
        }
        public static bool DxtcDataToImage()
        {
            return _ilDxtcDataToImage();
        }

        public static bool DxtcDataToSurface()
        {
            return _ilDxtcDataToSurface();
        }

        /// <summary>
        /// Flips the currently bound surface (image, mipmap, etc)'s dxtc data.
        /// </summary>
        public static void FlipSurfaceDxtcData()
        {
            _ilFlipSurfaceDxtcData();
        }

        /// <summary>
        /// Initializes the currently bound image to the default image - a 128x128 checkerboard texture.
        /// </summary>
        /// <returns>True if successful</returns>
        public static bool DefaultImage()
        {
            return _ilDefaultImage();
        }

        private static bool ilActiveFace(uint Number)
        {
            return _ilActiveFace(Number);
        }

        private static bool ilActiveImage(uint Number)
        {
            return _ilActiveImage(Number);
        }

        private static bool ilActiveLayer(uint Number)
        {
            return _ilActiveLayer(Number);
        }

        private static bool ilActiveMipmap(uint Number)
        {
            return _ilActiveMipmap(Number);
        }

        private static bool ilApplyProfile(IntPtr InProfile, IntPtr OutProfile)
        {
            return _ilApplyProfile(InProfile, OutProfile);
        }

        private static void ilBindImage(uint Image)
        {
            _ilBindImage(Image);
        }

        private static bool ilBlit(uint Source, int DestX, int DestY, int DestZ, uint SrcX, uint SrcY, uint SrcZ,
            uint Width, uint Height, uint Depth)
        {
            return _ilBlit(Source, DestX, DestY, DestZ, SrcX, SrcY, SrcZ, Width, Height, Depth);
        }

        private static uint ilCloneCurImage()
        {
            return _ilCloneCurImage();
        }

        private static IntPtr ilCompressDXT(IntPtr Data, uint Width, uint Height, uint Depth, uint DXTCFormat,
            ref uint DXTCSize)
        {
            return _ilCompressDXT(Data, Width, Height, Depth, DXTCFormat, ref DXTCSize);
        }

        private static bool ilCompressFunc(uint Mode)
        {
            return _ilCompressFunc(Mode);
        }

        private static bool ilConvertImage(uint DestFormat, uint DestType)
        {
            return _ilConvertImage(DestFormat, DestType);
        }

        private static bool ilConvertPal(uint DestFormat)
        {
            return _ilConvertPal(DestFormat);
        }

        private static bool ilCopyImage(uint Src)
        {
            return _ilCopyImage(Src);
        }

        private static uint ilCopyPixels(uint XOff, uint YOff, uint ZOff, uint Width, uint Height, uint Depth,
            uint Format, uint Type, IntPtr Data)
        {
            return _ilCopyPixels(XOff, YOff, ZOff, Width, Height, Depth, Format, Type, Data);
        }

        private static uint ilCreateSubImage(uint Type, uint Num)
        {
            return _ilCreateSubImage(Type, Num);
        }

        private static void ilDeleteImage(uint Num)
        {
            _ilDeleteImage(Num);
        }

        private static void ilDeleteImages(UIntPtr Num, uint[] Images)
        {
            _ilDeleteImages(Num, Images);
        }

        private static uint ilDetermineType([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName)
        {
            return _ilDetermineType(FileName);
        }

        private static uint ilDetermineTypeL(IntPtr Lump, uint Size)
        {
            return _ilDetermineTypeL(Lump, Size);
        }

        private static bool ilDisable(uint Mode)
        {
            return _ilDisable(Mode);
        }

        private static bool ilEnable(uint Mode)
        {
            return _ilEnable(Mode);
        }

        private static bool ilFormatFunc(uint Mode)
        {
            return _ilFormatFunc(Mode);
        }

        private static void ilGenImages(UIntPtr Num, uint[] Images)
        {
            _ilGenImages(Num, Images);
        }

        private static uint ilGenImage()
        {
            return _ilGenImage();
        }

        private static IntPtr ilGetAlpha(uint Type)
        {
            return _ilGetAlpha(Type);
        }

        private static IntPtr ilGetData()
        {
            return _ilGetData();
        }

        private static uint ilGetDXTCData(IntPtr Buffer, uint BufferSize, uint DXTCFormat)
        {
            return _ilGetDXTCData(Buffer, BufferSize, DXTCFormat);
        }

        private static uint ilGetError()
        {
            return _ilGetError();
        }

        internal static int ilGetInteger(uint Mode)
        {
            return _ilGetInteger(Mode);
        }

        private static IntPtr ilGetPalette()
        {
            return _ilGetPalette();
        }

        private static IntPtr ilGetString(uint StringName)
        {
            return _ilGetString(StringName);
        }

        private static void ilHint(uint Target, uint Mode)
        {
            _ilHint(Target, Mode);
        }

        private static void ilInit()
        {
            _ilInit();
        }

        private static bool ilImageToDxtcData(uint Format)
        {
            return _ilImageToDxtcData(Format);
        }

        private static bool ilIsDisabled(uint Mode)
        {
            return _ilIsDisabled(Mode);
        }

        private static bool ilIsEnabled(uint Mode)
        {
            return _ilIsEnabled(Mode);
        }

        private static bool ilIsImage(uint Image)
        {
            return _ilIsImage(Image);
        }

        private static bool ilIsValid(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName)
        {
            return _ilIsValid(Type, FileName);
        }

        private static bool ilIsValidL(uint Type, IntPtr Lump, uint Size)
        {
            return _ilIsValidL(Type, Lump, Size);
        }

        private static bool ilLoad(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName)
        {
            return _ilLoad(Type, FileName);
        }

        private static bool ilLoadL(uint Type, IntPtr Lump, uint Size)
        {
            return _ilLoadL(Type, Lump, Size);
        }

        private static bool ilOriginFunc(uint Mode)
        {
            return _ilOriginFunc(Mode);
        }

        private static bool ilOverlayImage(uint Source, int XCoord, int YCoord, int ZCoord)
        {
            return _ilOverlayImage(Source, XCoord, YCoord, ZCoord);
        }

        private static void ilPushAttrib(uint Bits)
        {
            _ilPushAttrib(Bits);
        }

        private static bool ilSave(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName)
        {
            return _ilSave(Type, FileName);
        }

        public static bool SaveImage([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName)
        {
            return _ilSaveImage(FileName);
        }

        private static uint ilSaveL(uint Type, IntPtr Lump, uint Size)
        {
            return _ilSaveL(Type, Lump, Size);
        }

        private static bool ilSetData(IntPtr Data)
        {
            return _ilSetData(Data);
        }

        private static bool ilSetDuration(uint Duration)
        {
            return _ilSetDuration(Duration);
        }

        private static void ilSetInteger(uint Mode, int Param)
        {
            _ilSetInteger(Mode, Param);
        }

        private static void ilSetPixels(int XOff, int YOff, int ZOff, uint Width, uint Height, uint Depth,
            uint Format, uint Type, IntPtr Data)
        {
            _ilSetPixels(XOff, YOff, ZOff, Width, Height, Depth, Format, Type, Data);
        }

        private static void ilSetString(uint Mode,
            [InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String String)
        {
            _ilSetString(Mode, String);
        }

        private static void ilShutDown()
        {
            _ilShutDown();
        }

        private static bool ilSurfaceToDxtcData(uint Format)
        {
            return _ilSurfaceToDxtcData(Format);
        }

        private static bool ilTexImage(uint Width, uint Height, uint Depth, byte Bpp, uint Format, uint Type,
            IntPtr Data)
        {
            return _ilTexImage(Width, Height, Depth, Bpp, Format, Type, Data);
        }

        private static bool ilTexImageDxtc(int Width, int Height, int Depth, uint DxtFormat, IntPtr Data)
        {
            return _ilTexImageDxtc(Width, Height, Depth, DxtFormat, Data);
        }

        private static uint ilTypeFromExt([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName)
        {
            return _ilTypeFromExt(FileName);
        }

        private static bool ilTypeFunc(uint Mode)
        {
            return _ilTypeFunc(Mode);
        }

        private static bool ilLoadData([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName,
            uint Width, uint Height, uint Depth, byte Bpp)
        {
            return _ilLoadData(FileName, Width, Height, Depth, Bpp);
        }

        private static bool ilLoadDataL(IntPtr Lump, uint Size, uint Width, uint Height, uint Depth, byte Bpp)
        {
            return _ilLoadDataL(Lump, Size, Width, Height, Depth, Bpp);
        }

        #endregion

        #region native x64
        private const String ILDLL_x64 = "DevIL64.dll";

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilClampNTSC", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilClampNTSC_x64();

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilClearColour", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilClearColor_x64(float red, float green, float blue, float alpha);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilClearImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilClearImage_x64();
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilApplyPal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilApplyPalette_x64([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilKeyColour", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetKeyColor_x64(float red, float green, float blue, float alpha);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSaveData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSaveRawData_x64([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSavePal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSavePalette_x64([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSetAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSetAlpha_x64(double alphaValue);


        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilPopAttrib", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilPopAttribute_x64();

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilModAlpha", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilModulateAlpha_x64(double alphaValue);


        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilLoadPal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadPalette_x64([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String fileName);


        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilLoadImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadImage_x64([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);


        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilInvertSurfaceDxtcDataAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilInvertSurfaceDxtcDataAlpha_x64();

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDxtcDataToImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDxtcDataToImage_x64();

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDxtcDataToSurface", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDxtcDataToSurface_x64();
        /// <summary>
        /// Flips the currently bound surface (image, mipmap, etc)'s dxtc data.
        /// </summary>
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilFlipSurfaceDxtcData", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilFlipSurfaceDxtcData_x64();

        /// <summary>
        /// Initializes the currently bound image to the default image - a 128x128 checkerboard texture.
        /// </summary>
        /// <returns>True if successful</returns>
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDefaultImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDefaultImage_x64();

        //Removed ilRegisterFormat to ilResetWrite. Might add the callbacks and reset mem stuff.
        //Also removed all load/saves/etc using file handles. Removed get int/bool versions using pass by ref
        //Removed SetMemory, SetRead, SetWrite, GetLumpPos

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilActiveFace", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveFace_x64(uint Number);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilActiveImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveImage_x64(uint Number);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilActiveLayer", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveLayer_x64(uint Number);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilActiveMipmap", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveMipmap_x64(uint Number);

        ///InProfile: char*
        ///OutProfile: char*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilApplyProfile", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilApplyProfile_x64(IntPtr InProfile, IntPtr OutProfile);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilBindImage", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilBindImage_x64(uint Image);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilBlit", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilBlit_x64(uint Source, int DestX, int DestY, int DestZ, uint SrcX, uint SrcY, uint SrcZ, uint Width, uint Height, uint Depth);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilCloneCurImage", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilCloneCurImage_x64();

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilCompressDXT", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilCompressDXT_x64(IntPtr Data, uint Width, uint Height, uint Depth, uint DXTCFormat, ref uint DXTCSize);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilCompressFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilCompressFunc_x64(uint Mode);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilConvertImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilConvertImage_x64(uint DestFormat, uint DestType);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilConvertPal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilConvertPal_x64(uint DestFormat);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilCopyImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilCopyImage_x64(uint Src);

        /// Return Type: sizeOfData
        ///Data: void*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilCopyPixels", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilCopyPixels_x64(uint XOff, uint YOff, uint ZOff, uint Width, uint Height, uint Depth, uint Format, uint Type, IntPtr Data);

        /// Looks like creates a subimage @ the num index and type is IL_SUB_* (Next, Mip, Layer), etc
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilCreateSubImage", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilCreateSubImage_x64(uint Type, uint Num);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDeleteImage", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilDeleteImage_x64(uint Num);

        /// Num is a Size_t
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDeleteImages", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilDeleteImages_x64(UIntPtr Num, uint[] Images);

        /// Return Type: Image Type
        ///FileName: char*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDetermineType", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilDetermineType_x64([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        /// Return Type: Image Type
        ///Lump: void*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDetermineTypeL", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilDetermineTypeL_x64(IntPtr Lump, uint Size);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilDisable", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDisable_x64(uint Mode);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilEnable", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilEnable_x64(uint Mode);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilFormatFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilFormatFunc_x64(uint Mode);

        ///Num: ILsizei->size_t->unsigned int
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGenImages", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilGenImages_x64(UIntPtr Num, uint[] Images);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGenImage", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilGenImage_x64();

        /// Return Type: ILubyte*
        ///Type: ILenum->unsigned int (Data type)
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGetAlpha", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetAlpha_x64(uint Type);

        /// Return Type: ILubyte*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGetData", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetData_x64();

        /// Returns Size of Data, set Zero for BufferSize to get size initially.
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGetDXTCData", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilGetDXTCData_x64(IntPtr Buffer, uint BufferSize, uint DXTCFormat);

        /// Return Type: Error type
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGetError", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilGetError_x64();

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGetInteger", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ilGetInteger_x64(uint Mode);

        /// Return Type: ILubyte*, need to find size via current image's pal size
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGetPalette", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetPalette_x64();

        /// Return Type: char*
        ///StringName: ILenum->unsigned int - String type enum
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilGetString", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetString_x64(uint StringName);

        ///Target: ILenum->unsigned int --> Type of hint
        ///Mode: ILenum->unsigned int ---> Hint value
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilHint", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilHint_x64(uint Target, uint Mode);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilInit", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilInit_x64();

        /// Format Type
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilImageToDxtcData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilImageToDxtcData_x64(uint Format);

        //Enable enum
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilIsDisabled", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsDisabled_x64(uint Mode);

        //Enable enum
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilIsEnabled", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsEnabled_x64(uint Mode);

        ///Checks if valid image - input is image id
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilIsImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsImage_x64(uint Image);

        ///Type: ILenum->unsigned int -- ImageType
        ///FileName: char*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilIsValid", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsValid_x64(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        /// Return Type: ILboolean->unsigned char - Image Type
        ///Lump: void*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilIsValidL", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsValidL_x64(uint Type, IntPtr Lump, uint Size);

        /// Type is Image Type
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilLoad", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoad_x64(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        /// Type is Image Type
        ///Lump: void*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilLoadL", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadL_x64(uint Type, IntPtr Lump, uint Size);

        /// Mode is origin type
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilOriginFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilOriginFunc_x64(uint Mode);

        /// SRC image, and coords are the offsets in a blit
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilOverlayImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilOverlayImage_x64(uint Source, int XCoord, int YCoord, int ZCoord);

        /// Attribute bit flags
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilPushAttrib", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilPushAttrib_x64(uint Bits);

        /// Image Type
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSave", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSave_x64(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSaveImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSaveImage_x64([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        ///ImageType, similar deal with GetDXTCData - returns size, pass in a NULL for lump to determine size
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSaveL", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilSaveL_x64(uint Type, IntPtr Lump, uint Size);

        ///Data: void*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSetData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSetData_x64(IntPtr Data);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSetDuration", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSetDuration_x64(uint Duration);

        /// IntegerMode, and param is value
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSetInteger", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetInteger_x64(uint Mode, int Param);

        ///Data: void*, dataFormat and DataType
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSetPixels", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetPixels_x64(int XOff, int YOff, int ZOff, uint Width, uint Height, uint Depth, uint Format, uint Type, IntPtr Data);

        /// Return Type: void
        ///Mode: ILenum->unsigned int
        ///String: char*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSetString", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetString_x64(uint Mode, [InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String String);

        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilShutDown", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilShutDown_x64();

        /// compressed DataFormat
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilSurfaceToDxtcData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSurfaceToDxtcData_x64(uint Format);

        /// dataFormat and DataType, destroys current data
        /// Bpp (NumChannels) bytes per pixel - e.g. 3 for RGB
        ///Data: void*
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilTexImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilTexImage_x64(uint Width, uint Height, uint Depth, byte Bpp, uint Format, uint Type, IntPtr Data);

        ///DxtcForamt is CompressedDataFormat, destroys current data
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilTexImageDxtc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilTexImageDxtc_x64(int Width, int Height, int Depth, uint DxtFormat, IntPtr Data);

        ///Image type from extension of file
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilTypeFromExt", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilTypeFromExt_x64([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName);

        ///Sets the current DataType
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilTypeFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilTypeFunc_x64(uint Mode);

        //Loads raw data from a file, bpp is only valid for 1, 3, 4
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilLoadData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadData_x64([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName, uint Width, uint Height, uint Depth, byte Bpp);

        //Loads raw data from a lump, bpp is only valid for 1, 3, 4
        [DllImportAttribute(ILDLL_x64, EntryPoint = "ilLoadDataL", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadDataL_x64(IntPtr Lump, uint Size, uint Width, uint Height, uint Depth, byte Bpp);
        #endregion

        #region native x32
        private const String ILDLL_x32 = "DevIL32.dll";
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilClampNTSC", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilClampNTSC_x32();

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilClearColour", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilClearColor_x32(float red, float green, float blue, float alpha);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilClearImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilClearImage_x32();
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilApplyPal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilApplyPalette_x32([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilKeyColour", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetKeyColor_x32(float red, float green, float blue, float alpha);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSaveData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSaveRawData_x32([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSavePal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSavePalette_x32([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSetAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSetAlpha_x32(double alphaValue);


        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilPopAttrib", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilPopAttribute_x32();

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilModAlpha", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilModulateAlpha_x32(double alphaValue);


        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilLoadPal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadPalette_x32([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String fileName);


        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilLoadImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadImage_x32([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);


        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilInvertSurfaceDxtcDataAlpha", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilInvertSurfaceDxtcDataAlpha_x32();

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDxtcDataToImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDxtcDataToImage_x32();

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDxtcDataToSurface", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDxtcDataToSurface_x32();
        /// <summary>
        /// Flips the currently bound surface (image, mipmap, etc)'s dxtc data.
        /// </summary>
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilFlipSurfaceDxtcData", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilFlipSurfaceDxtcData_x32();

        /// <summary>
        /// Initializes the currently bound image to the default image - a 128x128 checkerboard texture.
        /// </summary>
        /// <returns>True if successful</returns>
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDefaultImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDefaultImage_x32();


        //Removed ilRegisterFormat to ilResetWrite. Might add the callbacks and reset mem stuff.
        //Also removed all load/saves/etc using file handles. Removed get int/bool versions using pass by ref
        //Removed SetMemory, SetRead, SetWrite, GetLumpPos

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilActiveFace", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveFace_x32(uint Number);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilActiveImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveImage_x32(uint Number);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilActiveLayer", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveLayer_x32(uint Number);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilActiveMipmap", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilActiveMipmap_x32(uint Number);

        ///InProfile: char*
        ///OutProfile: char*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilApplyProfile", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilApplyProfile_x32(IntPtr InProfile, IntPtr OutProfile);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilBindImage", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilBindImage_x32(uint Image);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilBlit", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilBlit_x32(uint Source, int DestX, int DestY, int DestZ, uint SrcX, uint SrcY, uint SrcZ, uint Width, uint Height, uint Depth);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilCloneCurImage", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilCloneCurImage_x32();

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilCompressDXT", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilCompressDXT_x32(IntPtr Data, uint Width, uint Height, uint Depth, uint DXTCFormat, ref uint DXTCSize);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilCompressFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilCompressFunc_x32(uint Mode);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilConvertImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilConvertImage_x32(uint DestFormat, uint DestType);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilConvertPal", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilConvertPal_x32(uint DestFormat);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilCopyImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilCopyImage_x32(uint Src);

        /// Return Type: sizeOfData
        ///Data: void*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilCopyPixels", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilCopyPixels_x32(uint XOff, uint YOff, uint ZOff, uint Width, uint Height, uint Depth, uint Format, uint Type, IntPtr Data);

        /// Looks like creates a subimage @ the num index and type is IL_SUB_* (Next, Mip, Layer), etc
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilCreateSubImage", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilCreateSubImage_x32(uint Type, uint Num);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDeleteImage", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilDeleteImage_x32(uint Num);

        /// Num is a Size_t
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDeleteImages", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilDeleteImages_x32(UIntPtr Num, uint[] Images);

        /// Return Type: Image Type
        ///FileName: char*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDetermineType", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilDetermineType_x32([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        /// Return Type: Image Type
        ///Lump: void*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDetermineTypeL", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilDetermineTypeL_x32(IntPtr Lump, uint Size);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilDisable", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilDisable_x32(uint Mode);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilEnable", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilEnable_x32(uint Mode);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilFormatFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilFormatFunc_x32(uint Mode);

        ///Num: ILsizei->size_t->unsigned int
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGenImages", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilGenImages_x32(UIntPtr Num, uint[] Images);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGenImage", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilGenImage_x32();

        /// Return Type: ILubyte*
        ///Type: ILenum->unsigned int (Data type)
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGetAlpha", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetAlpha_x32(uint Type);

        /// Return Type: ILubyte*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGetData", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetData_x32();

        /// Returns Size of Data, set Zero for BufferSize to get size initially.
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGetDXTCData", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilGetDXTCData_x32(IntPtr Buffer, uint BufferSize, uint DXTCFormat);

        /// Return Type: Error type
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGetError", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilGetError_x32();

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGetInteger", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ilGetInteger_x32(uint Mode);

        /// Return Type: ILubyte*, need to find size via current image's pal size
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGetPalette", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetPalette_x32();

        /// Return Type: char*
        ///StringName: ILenum->unsigned int - String type enum
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilGetString", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ilGetString_x32(uint StringName);

        ///Target: ILenum->unsigned int --> Type of hint
        ///Mode: ILenum->unsigned int ---> Hint value
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilHint", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilHint_x32(uint Target, uint Mode);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilInit", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilInit_x32();

        /// Format Type
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilImageToDxtcData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilImageToDxtcData_x32(uint Format);

        //Enable enum
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilIsDisabled", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsDisabled_x32(uint Mode);

        //Enable enum
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilIsEnabled", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsEnabled_x32(uint Mode);

        ///Checks if valid image - input is image id
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilIsImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsImage_x32(uint Image);

        ///Type: ILenum->unsigned int -- ImageType
        ///FileName: char*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilIsValid", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsValid_x32(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        /// Return Type: ILboolean->unsigned char - Image Type
        ///Lump: void*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilIsValidL", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilIsValidL_x32(uint Type, IntPtr Lump, uint Size);

        /// Type is Image Type
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilLoad", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoad_x32(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        /// Type is Image Type
        ///Lump: void*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilLoadL", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadL_x32(uint Type, IntPtr Lump, uint Size);

        /// Mode is origin type
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilOriginFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilOriginFunc_x32(uint Mode);

        /// SRC image, and coords are the offsets in a blit
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilOverlayImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilOverlayImage_x32(uint Source, int XCoord, int YCoord, int ZCoord);

        /// Attribute bit flags
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilPushAttrib", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilPushAttrib_x32(uint Bits);

        /// Image Type
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSave", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSave_x32(uint Type, [InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSaveImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSaveImage_x32([InAttribute()] [MarshalAs(UnmanagedType.LPStr)] String FileName);

        ///ImageType, similar deal with GetDXTCData - returns size, pass in a NULL for lump to determine size
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSaveL", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilSaveL_x32(uint Type, IntPtr Lump, uint Size);

        ///Data: void*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSetData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSetData_x32(IntPtr Data);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSetDuration", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSetDuration_x32(uint Duration);

        /// IntegerMode, and param is value
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSetInteger", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetInteger_x32(uint Mode, int Param);

        ///Data: void*, dataFormat and DataType
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSetPixels", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetPixels_x32(int XOff, int YOff, int ZOff, uint Width, uint Height, uint Depth, uint Format, uint Type, IntPtr Data);

        /// Return Type: void
        ///Mode: ILenum->unsigned int
        ///String: char*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSetString", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilSetString_x32(uint Mode, [InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String String);

        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilShutDown", CallingConvention = CallingConvention.StdCall)]
        private static extern void ilShutDown_x32();

        /// compressed DataFormat
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilSurfaceToDxtcData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilSurfaceToDxtcData_x32(uint Format);

        /// dataFormat and DataType, destroys current data
        /// Bpp (NumChannels) bytes per pixel - e.g. 3 for RGB
        ///Data: void*
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilTexImage", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilTexImage_x32(uint Width, uint Height, uint Depth, byte Bpp, uint Format, uint Type, IntPtr Data);

        ///DxtcForamt is CompressedDataFormat, destroys current data
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilTexImageDxtc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilTexImageDxtc_x32(int Width, int Height, int Depth, uint DxtFormat, IntPtr Data);

        ///Image type from extension of file
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilTypeFromExt", CallingConvention = CallingConvention.StdCall)]
        private static extern uint ilTypeFromExt_x32([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName);

        ///Sets the current DataType
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilTypeFunc", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilTypeFunc_x32(uint Mode);

        //Loads raw data from a file, bpp is only valid for 1, 3, 4
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilLoadData", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadData_x32([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPStr)] String FileName, uint Width, uint Height, uint Depth, byte Bpp);

        //Loads raw data from a lump, bpp is only valid for 1, 3, 4
        [DllImportAttribute(ILDLL_x32, EntryPoint = "ilLoadDataL", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ilLoadDataL_x32(IntPtr Lump, uint Size, uint Width, uint Height, uint Depth, byte Bpp);
        #endregion

    }
}
