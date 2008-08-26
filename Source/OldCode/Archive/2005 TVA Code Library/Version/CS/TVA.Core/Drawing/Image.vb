' PCP: 04/11/2007

Option Strict On

Imports System.IO
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Namespace Drawing

    Public NotInheritable Class Image

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Shared Function Resize(ByVal originalImage As Bitmap, ByVal newSize As Size) As Bitmap

            Return Resize(originalImage, newSize, False)

        End Function

        Public Shared Function Resize(ByVal originalImage As Bitmap, ByVal newSize As Size, _
                ByVal disposeOriginal As Boolean) As Bitmap

            Dim resizedImage As Bitmap = Nothing
            Dim resizedImageGraphic As Graphics = Nothing

            If Not (originalImage.PixelFormat = PixelFormat.Format1bppIndexed OrElse _
                    originalImage.PixelFormat = PixelFormat.Format4bppIndexed OrElse _
                    originalImage.PixelFormat = PixelFormat.Format8bppIndexed OrElse _
                    originalImage.PixelFormat = PixelFormat.Undefined OrElse _
                    originalImage.PixelFormat = PixelFormat.DontCare OrElse _
                    originalImage.PixelFormat = PixelFormat.Format16bppArgb1555 OrElse _
                    originalImage.PixelFormat = PixelFormat.Format16bppGrayScale) Then
                ' We have an image that we can resize; Images of certain pixel formats cannot be resized.
                ' See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfSystemDrawingGraphicsClassFromImageTopic.asp

                ' Send a proportionally resized image of the original.
                If originalImage.Width > originalImage.Height Then
                    ' Original image has landscape orientation.
                    resizedImage = New Bitmap(newSize.Width, Convert.ToInt32(System.Math.Floor((newSize.Width * originalImage.Height) / originalImage.Width)), originalImage.PixelFormat)
                ElseIf originalImage.Width < originalImage.Height Then
                    ' Original image has portrait orientation.
                    resizedImage = New Bitmap(newSize.Height, Convert.ToInt32(System.Math.Floor((newSize.Height * originalImage.Height) / originalImage.Width)), originalImage.PixelFormat)
                Else
                    ' Original image is square.
                    resizedImage = New Bitmap(newSize.Width, newSize.Width, originalImage.PixelFormat)
                End If

                resizedImageGraphic = Graphics.FromImage(resizedImage)  ' Get the drawing canvas from bitmap.
                With resizedImageGraphic
                    ' Create high quality resized image.
                    .CompositingQuality = CompositingQuality.HighQuality
                    .SmoothingMode = SmoothingMode.HighQuality
                    .InterpolationMode = InterpolationMode.HighQualityBicubic
                    .DrawImage(originalImage, 0, 0, resizedImage.Width, resizedImage.Height)
                End With

                resizedImageGraphic.Dispose()
            Else
                Throw New NotSupportedException(String.Format("Images of pixel format ""{0}"" cannot be resized.", originalImage.PixelFormat.ToString()))
            End If
            If disposeOriginal Then originalImage.Dispose()

            Return resizedImage

        End Function

        Public Shared Function Crop(ByVal originalImage As Bitmap, ByVal newSize As Size) As Bitmap

            Return Crop(originalImage, newSize, False)

        End Function

        Public Shared Function Crop(ByVal originalImage As Bitmap, ByVal newSize As Size, ByVal disposeOriginal As Boolean) As Bitmap

            Return Crop(originalImage, New Point(0, 0), newSize, disposeOriginal)

        End Function

        Public Shared Function Crop(ByVal originalImage As Bitmap, ByVal upperLeftCorner As Point, _
                ByVal newSize As Size) As Bitmap

            Return Crop(originalImage, upperLeftCorner, newSize, False)

        End Function

        Public Shared Function Crop(ByVal originalImage As Bitmap, ByVal upperLeftCorner As Point, _
                ByVal newSize As Size, ByVal disposeOriginal As Boolean) As Bitmap

            Dim croppedImage As New Bitmap(newSize.Width, newSize.Height)
            Using croppedImageGraphic As Graphics = Graphics.FromImage(croppedImage)
                croppedImageGraphic.DrawImage(originalImage, 0, 0, New Rectangle(upperLeftCorner, newSize), GraphicsUnit.Pixel)
            End Using
            If disposeOriginal Then originalImage.Dispose()

            Return croppedImage

        End Function

        Public Shared Function ConvertTo(ByVal originalImage As Bitmap, ByVal newFormat As ImageFormat) As Bitmap

            Return ConvertTo(originalImage, newFormat, False)

        End Function

        Public Shared Function ConvertTo(ByVal originalImage As Bitmap, ByVal newFormat As ImageFormat, _
                ByVal disposeOriginal As Boolean) As Bitmap

            Dim newImage As Bitmap = Nothing
            Dim newImageStream As New MemoryStream()

            originalImage.Save(newImageStream, newFormat)   ' Save image to memory stream in the specified format.
            newImage = New Bitmap(newImageStream)           ' Create new bitmap from the memory stream.
            If disposeOriginal Then originalImage.Dispose() ' Dispose the original if requested.

            Return newImage

        End Function

        Public Shared Function CaptureScreenshot(ByVal screenshotSize As Size) As Bitmap

            Return CaptureScreenshot(New Point(0, 0), screenshotSize)

        End Function

        Public Shared Function CaptureScreenshot(ByVal upperLeftCorner As Point, ByVal screenshotSize As Size) As Bitmap

            Return CaptureScreenshot(upperLeftCorner, screenshotSize, ImageFormat.Bmp)

        End Function

        Public Shared Function CaptureScreenshot(ByVal screenshotSize As Size, ByVal imageFormat As ImageFormat) As Bitmap

            Return CaptureScreenshot(New Point(0, 0), screenshotSize, imageFormat)

        End Function

        Public Shared Function CaptureScreenshot(ByVal upperLeftCorner As Point, ByVal screenshotSize As Size, _
                ByVal imageFormat As ImageFormat) As Bitmap

            ' Create a blank image of the specified size.
            Dim screenshot As New Bitmap(screenshotSize.Width, screenshotSize.Height)
            Using screenshotGraphic As Graphics = Graphics.FromImage(screenshot)
                ' Copy the area of the screen to the blank image.
                screenshotGraphic.CopyFromScreen(upperLeftCorner.X, upperLeftCorner.Y, 0, 0, screenshotSize)
            End Using

            ' We'll return the captured screenshot in the specified image format.
            Return ConvertTo(screenshot, imageFormat, True)

        End Function

    End Class

End Namespace