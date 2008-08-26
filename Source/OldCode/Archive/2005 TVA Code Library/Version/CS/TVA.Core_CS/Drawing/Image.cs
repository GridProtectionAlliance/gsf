using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// PCP: 04/11/2007



namespace TVA
{
	namespace Drawing
	{
		
		public sealed class Image
		{
			
			
			private Image()
			{
				
				// This class contains only global functions and is not meant to be instantiated
				
			}
			
			public static Bitmap Resize(Bitmap originalImage, Size newSize)
			{
				
				return Resize(originalImage, newSize, false);
				
			}
			
			public static Bitmap Resize(Bitmap originalImage, Size newSize, bool disposeOriginal)
			{
				
				Bitmap resizedImage = null;
				Graphics resizedImageGraphic = null;
				
				if (!(originalImage.PixelFormat == PixelFormat.Format1bppIndexed || originalImage.PixelFormat == PixelFormat.Format4bppIndexed || originalImage.PixelFormat == PixelFormat.Format8bppIndexed || originalImage.PixelFormat == PixelFormat.Undefined || originalImage.PixelFormat == PixelFormat.DontCare || originalImage.PixelFormat == PixelFormat.Format16bppArgb1555 || originalImage.PixelFormat == PixelFormat.Format16bppGrayScale))
				{
					// We have an image that we can resize; Images of certain pixel formats cannot be resized.
					// See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfSystemDrawingGraphicsClassFromImageTopic.asp
					
					// Send a proportionally resized image of the original.
					if (originalImage.Width > originalImage.Height)
					{
						// Original image has landscape orientation.
						resizedImage = new Bitmap(newSize.Width, Convert.ToInt32(System.Math.Floor((newSize.Width * originalImage.Height) / originalImage.Width)), originalImage.PixelFormat);
					}
					else if (originalImage.Width < originalImage.Height)
					{
						// Original image has portrait orientation.
						resizedImage = new Bitmap(newSize.Height, Convert.ToInt32(System.Math.Floor((newSize.Height * originalImage.Height) / originalImage.Width)), originalImage.PixelFormat);
					}
					else
					{
						// Original image is square.
						resizedImage = new Bitmap(newSize.Width, newSize.Width, originalImage.PixelFormat);
					}
					
					resizedImageGraphic = Graphics.FromImage(resizedImage); // Get the drawing canvas from bitmap.
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
				if (disposeOriginal)
				{
					originalImage.Dispose();
				}
				
				return resizedImage;
				
			}
			
			public static Bitmap Crop(Bitmap originalImage, Size newSize)
			{
				
				return Crop(originalImage, newSize, false);
				
			}
			
			public static Bitmap Crop(Bitmap originalImage, Size newSize, bool disposeOriginal)
			{
				
				return Crop(originalImage, new Point(0, 0), newSize, disposeOriginal);
				
			}
			
			public static Bitmap Crop(Bitmap originalImage, Point upperLeftCorner, Size newSize)
			{
				
				return Crop(originalImage, upperLeftCorner, newSize, false);
				
			}
			
			public static Bitmap Crop(Bitmap originalImage, Point upperLeftCorner, Size newSize, bool disposeOriginal)
			{
				
				Bitmap croppedImage = new Bitmap(newSize.Width, newSize.Height);
				using (Graphics croppedImageGraphic = Graphics.FromImage(croppedImage))
				{
					croppedImageGraphic.DrawImage(originalImage, 0, 0, new Rectangle(upperLeftCorner, newSize), GraphicsUnit.Pixel);
				}
				
				if (disposeOriginal)
				{
					originalImage.Dispose();
				}
				
				return croppedImage;
				
			}
			
			public static Bitmap ConvertTo(Bitmap originalImage, ImageFormat newFormat)
			{
				
				return ConvertTo(originalImage, newFormat, false);
				
			}
			
			public static Bitmap ConvertTo(Bitmap originalImage, ImageFormat newFormat, bool disposeOriginal)
			{
				
				Bitmap newImage = null;
				MemoryStream newImageStream = new MemoryStream();
				
				originalImage.Save(newImageStream, newFormat); // Save image to memory stream in the specified format.
				newImage = new Bitmap(newImageStream); // Create new bitmap from the memory stream.
				if (disposeOriginal)
				{
					originalImage.Dispose(); // Dispose the original if requested.
				}
				
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
}
