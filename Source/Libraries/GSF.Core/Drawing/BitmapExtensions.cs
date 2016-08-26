//******************************************************************************************************
//  BitmapExtensions.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/11/2007 - Pinal C. Patel
//       Original version of source code generated.
//  09/12/2008 - J. Ritchie Carroll
//       Converted to C# extensions.
//  10/02/2008 - Pinal C. Patel
//       Entered code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GSF.Interop;
using GSF.IO;

namespace GSF.Drawing
{
    /// <summary>
    /// Defines extension functions related to bitmap image manipulation.
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Returns a resized <see cref="Bitmap"/> image of the original.
        /// </summary>
        /// <param name="originalImage">The original <see cref="Bitmap"/> image to be resized.</param>
        /// <param name="newSize">The <see cref="Size"/> to which the original image is to be resized.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        /// <example>
        /// This example shows how to resize an image:
        /// <code>
        /// using System;
        /// using GSF.Drawing;
        /// using System.Drawing;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Load the original image.
        ///         Bitmap original = (Bitmap)Bitmap.FromFile("Original.jpg");
        ///         // Resize the original image.
        ///         Bitmap originalResized = original.Resize(new Size(800, 600));
        ///         // Save the resized image to file.
        ///         originalResized.Save("OriginalResized.jpg");
        ///
        ///         // Clean-up.
        ///         original.Dispose();
        ///         originalResized.Dispose();
        ///         
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static Bitmap Resize(this Image originalImage, Size newSize)
        {
            return originalImage.Resize(newSize, false);
        }

        /// <summary>
        /// Returns a resized <see cref="Bitmap"/> image of the original.
        /// </summary>
        /// <param name="originalImage">The original <see cref="Bitmap"/> image to be resized.</param>
        /// <param name="newSize">The <see cref="Size"/> to which the original image is to be resized.</param>
        /// <param name="disposeOriginal">true if the original image is to be disposed after resizing it; otherwise false.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        /// <example>
        /// This example shows how to resize an image and dispose the original image that was resized:
        /// <code>
        /// using System;
        /// using System.Drawing;
        /// using GSF.Drawing;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Load original, resize it, and dispose original.
        ///         using (Bitmap resized = ((Bitmap)Bitmap.FromFile("Original.jpg")).Resize(new Size(800, 600), true))
        ///         {
        ///             // Save the resized image to file.
        ///             resized.Save("OriginalResized.jpg");
        ///         }
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static Bitmap Resize(this Image originalImage, Size newSize, bool disposeOriginal)
        {
            Bitmap resizedImage;

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
                    resizedImage = new Bitmap(newSize.Width,
                                              (int)Math.Floor((double)(newSize.Width * originalImage.Height) / (double)originalImage.Width),
                                              originalImage.PixelFormat);
                }
                else if (originalImage.Width < originalImage.Height)
                {
                    // Original image has portrait orientation.
                    resizedImage = new Bitmap(newSize.Height,
                                              (int)Math.Floor((double)(newSize.Height * originalImage.Height) / (double)originalImage.Width),
                                              originalImage.PixelFormat);
                }
                else
                {
                    // Original image is square.
                    resizedImage = new Bitmap(newSize.Width, newSize.Width, originalImage.PixelFormat);
                }
                // Match the resolution of resized image to the original.
                resizedImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

                // Create high quality resized image.
                using (Graphics resizedImageGraphic = Graphics.FromImage(resizedImage))
                {
                    resizedImageGraphic.CompositingQuality = CompositingQuality.HighQuality;
                    resizedImageGraphic.SmoothingMode = SmoothingMode.HighQuality;
                    resizedImageGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    resizedImageGraphic.DrawImage(originalImage, 0, 0, resizedImage.Width, resizedImage.Height);
                }

                if (disposeOriginal)
                    originalImage.Dispose();   // Dispose original if indicated.
            }
            else
            {
                throw (new NotSupportedException(string.Format("Images of pixel format \"{0}\" cannot be resized.", originalImage.PixelFormat.ToString())));
            }

            return resizedImage;
        }

        /// <summary>
        /// Returns a cropped <see cref="Bitmap"/> image of the original.
        /// </summary>
        /// <param name="originalImage">The original <see cref="Bitmap"/> image to be cropped.</param>
        /// <param name="croppedArea">The <see cref="Rectangle"/> area of the original image to be cropped.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        /// <example>
        /// This example shows how to crop an image:
        /// <code>
        /// using System;
        /// using System.Drawing;
        /// using GSF.Drawing;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Load the original image.
        ///         Bitmap original = (Bitmap)Bitmap.FromFile("Original.jpg");
        ///         // Crop the original image.
        ///         Bitmap originalCropped = original.Crop(new Rectangle(0, 0, 300, 300));
        ///         // Save the cropped image to file.
        ///         originalCropped.Save("OriginalCropped.jpg");
        ///         
        ///         // Clean-up.
        ///         original.Dispose();
        ///         originalCropped.Dispose();
        ///
        ///         Console.ReadLine();
        ///     }
        /// }        
        /// </code>
        /// </example>
        public static Bitmap Crop(this Image originalImage, Rectangle croppedArea)
        {
            return Crop(originalImage, croppedArea, false);
        }

        /// <summary>
        /// Returns a cropped <see cref="Bitmap"/> image of the original.
        /// </summary>
        /// <param name="originalImage">The original <see cref="Bitmap"/> image to be cropped.</param>
        /// <param name="croppedArea">The <see cref="Rectangle"/> area of the original image to be cropped.</param>
        /// <param name="disposeOriginal">true if the original image is to be disposed after cropping it; otherwise false.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        /// <example>
        /// This example shows how to crop an image and dispose the original image that was cropped:
        /// <code>
        /// using System;
        /// using System.Drawing;
        /// using GSF.Drawing;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Load original, crop it, and dispose original.
        ///         using (Bitmap cropped = ((Bitmap)Bitmap.FromFile("Original.jpg")).Crop(new Rectangle(0, 0, 300, 300), true))
        ///         {
        ///             // Save the cropped image to file.
        ///             cropped.Save("OriginalCropped.jpg");
        ///         }
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static Bitmap Crop(this Image originalImage, Rectangle croppedArea, bool disposeOriginal)
        {
            // Create a crop of the original image.
            Bitmap croppedImage = new Bitmap(croppedArea.Width, croppedArea.Height, originalImage.PixelFormat);

            croppedImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

            using (Graphics croppedImageGraphic = Graphics.FromImage(croppedImage))
            {
                croppedImageGraphic.DrawImage(originalImage, 0, 0, croppedArea, GraphicsUnit.Pixel);
            }

            if (disposeOriginal)
                originalImage.Dispose();   // Dispose original if indicated.

            return croppedImage;
        }

        /// <summary>
        /// Converts a <see cref="Bitmap"/> image to the specified <see cref="ImageFormat"/>.
        /// </summary>
        /// <param name="originalImage">The <see cref="Bitmap"/> image to be converted.</param>
        /// <param name="newFormat">The new <see cref="ImageFormat"/> of the image.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        /// <example>
        /// This example shows how to convert the format of an image:
        /// <code>
        /// using System;
        /// using System.Drawing;
        /// using System.Drawing.Imaging;
        /// using GSF.Drawing;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Load the original image.
        ///         Bitmap original = (Bitmap)Bitmap.FromFile("Original.jpg");
        ///         // Convert the original image.
        ///         Bitmap originalGif = original.ConvertTo(ImageFormat.Gif);
        ///         // Save the converted image to file.
        ///         originalGif.Save("OriginalGif.gif");
        ///
        ///         // Clean-up.
        ///         original.Dispose();
        ///         originalGif.Dispose();
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static Bitmap ConvertTo(this Image originalImage, ImageFormat newFormat)
        {
            return originalImage.ConvertTo(newFormat, false);
        }

        /// <summary>
        /// Converts a <see cref="Bitmap"/> image to the specified <see cref="ImageFormat"/>.
        /// </summary>
        /// <param name="originalImage">The <see cref="Bitmap"/> image to be converted.</param>
        /// <param name="newFormat">The new <see cref="ImageFormat"/> of the image.</param>
        /// <param name="disposeOriginal">true if the original image is to be disposed after converting it; otherwise false.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        /// <example>
        /// This example shows how to convert the format of an image and dispose the original image that was converted:
        /// <code>
        /// using System;
        /// using System.Drawing;
        /// using System.Drawing.Imaging;
        /// using GSF.Drawing;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Load original, convert it, and dispose original.
        ///         using (Bitmap converted = ((Bitmap)Bitmap.FromFile("Original.jpg")).ConvertTo(ImageFormat.Gif))
        ///         {
        ///             // Save the converted image to file.
        ///             converted.Save("OriginalGif.gif");
        ///         }
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static Bitmap ConvertTo(this Image originalImage, ImageFormat newFormat, bool disposeOriginal)
        {
            Bitmap newImage;

            using (BlockAllocatedMemoryStream newImageStream = new BlockAllocatedMemoryStream())
            {
                // Save image to memory stream in the specified format.
                originalImage.Save(newImageStream, newFormat);

                // Create new bitmap from the memory stream.
                newImage = new Bitmap(newImageStream);

                // Dispose original if indicated.
                if (disposeOriginal)
                    originalImage.Dispose();

                return newImage;
            }
        }

        /// <summary>
        /// Converts from an array of pixel data to a <see cref="Bitmap"/> image.
        /// </summary>
        /// <param name="width">The width of the bitmap.</param>
        /// <param name="pixelData">The values of individual pixels in ARGB format.</param>
        /// <returns>A bitmap image converted from the pixel data.</returns>
        public static Bitmap FromPixelData(int width, uint[] pixelData)
        {
            int height = pixelData.Length / width;
            GCHandle? gchPixelData = null;

            try
            {
                gchPixelData = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                return new Bitmap(width, height, width * sizeof(uint), PixelFormat.Format32bppPArgb, gchPixelData.GetValueOrDefault().AddrOfPinnedObject());
            }
            finally
            {
                gchPixelData?.Free();
            }
        }

        /// <summary>
        /// Converts from a bitmap image to an array of pixel data.
        /// </summary>
        /// <param name="bitmap">The bitmap image to be converted.</param>
        /// <returns>The pixel data contained in the image.</returns>
        public static uint[] ToPixelData(this Bitmap bitmap)
        {
            uint[] pixelData = new uint[bitmap.Width * bitmap.Height];
            BitmapData bitmapData = null;
            GCHandle? gchPixelData = null;

            try
            {
                gchPixelData = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
                WindowsApi.CopyMemory(bitmapData.Scan0, gchPixelData.GetValueOrDefault().AddrOfPinnedObject(), (uint)(bitmapData.Stride * bitmapData.Height));
                return pixelData;
            }
            finally
            {
                gchPixelData?.Free();

                if ((object)bitmapData != null)
                    bitmap.UnlockBits(bitmapData);
            }
        }
    }
}