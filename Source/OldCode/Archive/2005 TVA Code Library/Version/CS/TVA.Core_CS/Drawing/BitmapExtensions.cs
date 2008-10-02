//*******************************************************************************************************
//  BitmapExtensions.cs
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
//      Converted to C# extensions.
//
//*******************************************************************************************************

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace TVA.Drawing
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
        /// This sample shows show to resize an image:
        /// <code>
        /// using System;
        /// using TVA.Drawing;
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
        public static Bitmap Resize(this Bitmap originalImage, Size newSize)
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
        /// This sample shows how to resize an image and dispose the original image that was resized:
        /// <code>
        /// using System;
        /// using System.Drawing;
        /// using TVA.Drawing;
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
        public static Bitmap Resize(this Bitmap originalImage, Size newSize, bool disposeOriginal)
        {
            Bitmap resizedImage = null;

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

                if (disposeOriginal) originalImage.Dispose();   // Dispose original if indicated.
            }
            else
            {
                throw (new NotSupportedException(string.Format("Images of pixel format \"{0}\" cannot be resized.", originalImage.PixelFormat.ToString())));
            }

            return resizedImage;
        }

        ///// <summary>
        ///// Returns a cropped <see cref="Bitmap"/> image of the original.
        ///// </summary>
        ///// <param name="originalImage">The original <see cref="Bitmap"/> image to be cropped.</param>
        ///// <param name="croppedSize">The <see cref="Size"/> to which the original image is to be cropped.</param>
        ///// <returns>A <see cref="Bitmap"/> instance.</returns>
        //public static Bitmap Crop(this Bitmap originalImage, Size croppedSize)
        //{
        //    return originalImage.Crop(croppedSize, false);
        //}

        ///// <summary>
        ///// Returns a cropped <see cref="Bitmap"/> image of the original.
        ///// </summary>
        ///// <param name="originalImage">The original <see cref="Bitmap"/> image to be cropped.</param>
        ///// <param name="croppedSize">The <see cref="Size"/> to which the original image is to be cropped.</param>
        ///// <param name="disposeOriginal">true if the original image is to be disposed; otherwise false.</param>
        ///// <returns>A <see cref="Bitmap"/> instance.</returns>
        //public static Bitmap Crop(this Bitmap originalImage, Size croppedSize, bool disposeOriginal)
        //{
        //    return originalImage.Crop(new Point(0, 0), croppedSize, disposeOriginal);
        //}

        ///// <summary>
        ///// Returns a cropped <see cref="Bitmap"/> image of the original.
        ///// </summary>
        ///// <param name="originalImage">The original <see cref="Bitmap"/> image to be cropped.</param>
        ///// <param name="startingPoint">The starting <see cref="Point"/> in the upper-left corner of the original image from where it is to be cropped.</param>
        ///// <param name="croppedSize">The <see cref="Size"/> to which the original image is to be cropped.</param>
        ///// <returns>A <see cref="Bitmap"/> instance.</returns>
        //public static Bitmap Crop(this Bitmap originalImage, Point startingPoint, Size croppedSize)
        //{
        //    return originalImage.Crop(startingPoint, croppedSize, false);
        //}

        ///// <summary>
        ///// Returns a cropped <see cref="Bitmap"/> image of the original.
        ///// </summary>
        ///// <param name="originalImage">The original <see cref="Bitmap"/> image to be cropped.</param>
        ///// <param name="startingPoint">The starting <see cref="Point"/> in the upper-left corner of the original image from where it is to be cropped.</param>
        ///// <param name="croppedSize">The <see cref="Size"/> to which the original image is to be cropped.</param>
        ///// <param name="disposeOriginal">true if the original image is to be disposed; otherwise false.</param>
        ///// <returns>A <see cref="Bitmap"/> instance.</returns>
        //public static Bitmap Crop(this Bitmap originalImage, Point startingPoint, Size croppedSize, bool disposeOriginal)
        //{
        //    // Create a crop of the original image.
        //    Bitmap croppedImage = new Bitmap(croppedSize.Width, croppedSize.Height, originalImage.PixelFormat);
        //    croppedImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);
        //    using (Graphics croppedImageGraphic = Graphics.FromImage(croppedImage))
        //    {
        //        croppedImageGraphic.DrawImage(originalImage, 0, 0, new Rectangle(startingPoint, croppedSize), GraphicsUnit.Pixel);
        //    }

        //    if (disposeOriginal) originalImage.Dispose();   // Dispose original if indicated.

        //    return croppedImage;
        //}

        /// <summary>
        /// Returns a cropped <see cref="Bitmap"/> image of the original.
        /// </summary>
        /// <param name="originalImage">The original <see cref="Bitmap"/> image to be cropped.</param>
        /// <param name="croppedArea">The <see cref="Rectangle"/> area of the original image to be cropped.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        public static Bitmap Crop(this Bitmap originalImage, Rectangle croppedArea)
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
        public static Bitmap Crop(this Bitmap originalImage, Rectangle croppedArea, bool disposeOriginal)
        {
            // Create a crop of the original image.
            Bitmap croppedImage = new Bitmap(croppedArea.Width, croppedArea.Height, originalImage.PixelFormat);
            croppedImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);
            using (Graphics croppedImageGraphic = Graphics.FromImage(croppedImage))
            {
                croppedImageGraphic.DrawImage(originalImage, 0, 0, croppedArea, GraphicsUnit.Pixel);
            }

            if (disposeOriginal) originalImage.Dispose();   // Dispose original if indicated.

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
    }
}