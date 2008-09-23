//*******************************************************************************************************
//  Image.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/11/2007 - Pinal C. Patel
//       Original version of source code generated.
//  09/12/2008 - J. Ritchie Carroll
//      Converted to C# (some available as extensions).
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace TVA.Drawing
{
    public static class Image
    {
        public static Bitmap Resize(this Bitmap originalImage, Size newSize)
        {
            return originalImage.Resize(newSize, false);
        }

        public static Bitmap Resize(this Bitmap originalImage, Size newSize, bool disposeOriginal)
        {
            Bitmap resizedImage = null;
            Graphics resizedImageGraphic = null;

            if (!(originalImage.PixelFormat == PixelFormat.Format1bppIndexed ||
                  originalImage.PixelFormat == PixelFormat.Format4bppIndexed ||
                  originalImage.PixelFormat == PixelFormat.Format8bppIndexed ||
                  originalImage.PixelFormat == PixelFormat.Undefined ||
                  originalImage.PixelFormat == PixelFormat.DontCare ||
                  originalImage.PixelFormat == PixelFormat.Format16bppArgb1555 ||
                  originalImage.PixelFormat == PixelFormat.Format16bppGrayScale))
            {
                // We have an image that we can resize; Images of certain pixel formats cannot be resized.
                // See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfSystemDrawingGraphicsClassFromImageTopic.asp

                // Send a proportionally resized image of the original.
                if (originalImage.Width > originalImage.Height)
                {
                    // Original image has landscape orientation.
                    resizedImage = new Bitmap(newSize.Width, (int)System.Math.Floor((double)(newSize.Width * originalImage.Height) / (double)originalImage.Width), originalImage.PixelFormat);
                }
                else if (originalImage.Width < originalImage.Height)
                {
                    // Original image has portrait orientation.
                    resizedImage = new Bitmap(newSize.Height, (int)System.Math.Floor((double)(newSize.Height * originalImage.Height) / (double)originalImage.Width), originalImage.PixelFormat);
                }
                else
                {
                    // Original image is square.
                    resizedImage = new Bitmap(newSize.Width, newSize.Width, originalImage.PixelFormat);
                }

                // Get the drawing canvas from bitmap.
                resizedImageGraphic = Graphics.FromImage(resizedImage);

                // Create high quality resized image.
                resizedImageGraphic.CompositingQuality = CompositingQuality.HighQuality;
                resizedImageGraphic.SmoothingMode = SmoothingMode.HighQuality;
                resizedImageGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                resizedImageGraphic.DrawImage(originalImage, 0, 0, resizedImage.Width, resizedImage.Height);
                resizedImageGraphic.Dispose();
            }
            else
            {
                throw (new NotSupportedException(string.Format("Images of pixel format \"{0}\" cannot be resized.", originalImage.PixelFormat.ToString())));
            }

            if (disposeOriginal) originalImage.Dispose();

            return resizedImage;
        }

        public static Bitmap Crop(this Bitmap originalImage, Size newSize)
        {
            return originalImage.Crop(newSize, false);
        }

        public static Bitmap Crop(this Bitmap originalImage, Size newSize, bool disposeOriginal)
        {
            return originalImage.Crop(new Point(0, 0), newSize, disposeOriginal);
        }

        public static Bitmap Crop(this Bitmap originalImage, Point upperLeftCorner, Size newSize)
        {
            return originalImage.Crop(upperLeftCorner, newSize, false);
        }

        public static Bitmap Crop(this Bitmap originalImage, Point upperLeftCorner, Size newSize, bool disposeOriginal)
        {
            Bitmap croppedImage = new Bitmap(newSize.Width, newSize.Height);

            using (Graphics croppedImageGraphic = Graphics.FromImage(croppedImage))
            {
                croppedImageGraphic.DrawImage(originalImage, 0, 0, new Rectangle(upperLeftCorner, newSize), GraphicsUnit.Pixel);
            }

            if (disposeOriginal) originalImage.Dispose();

            return croppedImage;
        }

        public static Bitmap ConvertTo(this Bitmap originalImage, ImageFormat newFormat)
        {
            return originalImage.ConvertTo(newFormat, false);
        }

        public static Bitmap ConvertTo(this Bitmap originalImage, ImageFormat newFormat, bool disposeOriginal)
        {
            Bitmap newImage = null;
            MemoryStream newImageStream = new MemoryStream();

            originalImage.Save(newImageStream, newFormat); // Save image to memory stream in the specified format.

            newImage = new Bitmap(newImageStream); // Create new bitmap from the memory stream.

            if (disposeOriginal) originalImage.Dispose(); // Dispose the original if requested.

            return newImage;
        }

        public static Bitmap CaptureScreenshot(Size screenshotSize)
        {
            return CaptureScreenshot(new Point(0, 0), screenshotSize);
        }

        public static Bitmap CaptureScreenshot(Point upperLeftCorner, Size screenshotSize)
        {
            return CaptureScreenshot(upperLeftCorner, screenshotSize, ImageFormat.Bmp);
        }

        public static Bitmap CaptureScreenshot(Size screenshotSize, ImageFormat imageFormat)
        {
            return CaptureScreenshot(new Point(0, 0), screenshotSize, imageFormat);
        }

        public static Bitmap CaptureScreenshot(Point upperLeftCorner, Size screenshotSize, ImageFormat imageFormat)
        {
            // Create a blank image of the specified size.
            Bitmap screenshot = new Bitmap(screenshotSize.Width, screenshotSize.Height);

            using (Graphics screenshotGraphic = Graphics.FromImage(screenshot))
            {
                // Copy the area of the screen to the blank image.
                screenshotGraphic.CopyFromScreen(upperLeftCorner.X, upperLeftCorner.Y, 0, 0, screenshotSize);
            }

            // We'll return the captured screenshot in the specified image format.
            return ConvertTo(screenshot, imageFormat, true);
        }
    }
}