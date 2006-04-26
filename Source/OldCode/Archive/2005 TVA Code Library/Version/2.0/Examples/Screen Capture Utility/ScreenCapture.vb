'*******************************************************************************************************
'  ScreenCapture.vb - Automated screen capture utility
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/03/2006 - James R Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports Tva.Configuration.Common
'Imports ScreenCapture.ConfigurationSettings


Public Class ScreenCapture

    Private m_fileFormat As ImageFormat
    Private m_primaryScreenIndex As Integer
    Private m_rootFileName
    Private m_extension As String

    Private Sub ScreenCapture_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim selectedScreenIndex As Integer

        ' Make sure desired settings exist
        Settings.Add("SelectedScreen", 0)
        Settings.Add("SelectedScreen", 0)
        Settings.Add("ImageFormat", 5)
        Settings.Add("CaptureSeconds", TextBoxCaptureSeconds.Text)
        Settings.Add("CaptureFilename", TextBoxCaptureFilename.Text)
        Settings.Add("StartMinimized", CheckBoxStartMinimized.Checked)
        Settings.Add("AutoStart", CheckBoxAutoStart.Checked)
        Settings.Add("Export100Percent", CheckBox100Percent.Checked)
        Settings.Add("Export75Percent", CheckBox75Percent.Checked)
        Settings.Add("Export50Percent", CheckBox50Percent.Checked)
        Settings.Add("Export25Percent", CheckBox25Percent.Checked)
        Settings.Add("ExportCustomPercent", CheckBoxCustomPercent.Checked)
        Settings.Add("CustomPercent", TextBoxCustomPercent.Text)
        Settings.Add("FullScreenCapture", RadioButtonFullScreen.Checked)
        Settings.Add("CustomAreaCapture", RadioButtonCustomArea.Checked)
        Settings.Add("CaptureCoordinates", TextBoxCaptureCoordinates.Text)
        SaveSettings()

        ' Load user settings
        ComboBoxImageFormat.SelectedIndex = Settings("ImageFormat").GetTypedValue(5)
        TextBoxCaptureSeconds.Text = Settings("CaptureSeconds").Value()
        TextBoxCaptureFilename.Text = Settings("CaptureFilename").Value()
        CheckBoxStartMinimized.Checked = Settings("StartMinimized").GetTypedValue(False)
        CheckBoxAutoStart.Checked = Settings("AutoStart").GetTypedValue(False)
        CheckBox100Percent.Checked = Settings("Export100Percent").GetTypedValue(True)
        CheckBox75Percent.Checked = Settings("Export75Percent").GetTypedValue(False)
        CheckBox50Percent.Checked = Settings("Export50Percent").GetTypedValue(False)
        CheckBox25Percent.Checked = Settings("Export25Percent").GetTypedValue(False)
        CheckBoxCustomPercent.Checked = Settings("ExportCustomPercent").GetTypedValue(False)
        TextBoxCustomPercent.Text = Settings("CustomPercent").Value()
        RadioButtonFullScreen.Checked = Settings("FullScreenCapture").GetTypedValue(True)
        RadioButtonCustomArea.Checked = Settings("CustomAreaCapture").GetTypedValue(False)
        TextBoxCaptureCoordinates.Text = Settings("CaptureCoordinates").Value()

        ' Load screen list
        With ComboBoxSourceScreen
            For x As Integer = 0 To Screen.AllScreens.Length - 1
                .Items.Add(Screen.AllScreens(x).DeviceName)
                If Screen.AllScreens(x).Primary Then m_primaryScreenIndex = x
            Next

            ' Validate last selected screen index - screens may have been added or removed since last run
            selectedScreenIndex = Settings("SelectedScreen").GetTypedValue(5)
            If selectedScreenIndex >= 0 AndAlso selectedScreenIndex < Screen.AllScreens.Length Then
                .SelectedIndex = selectedScreenIndex
            Else
                .SelectedIndex = 0
            End If
        End With

        ' Auto start capture if requested
        If CheckBoxAutoStart.Checked Then ButtonStartCapture_Click(Nothing, Nothing)

        ' Load application minimized if requested
        If CheckBoxStartMinimized.Checked Then WindowState = FormWindowState.Minimized

    End Sub

    Private Sub ScreenCapture_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        ' Save user settings
        Settings("SelectedScreen").Value = ComboBoxSourceScreen.SelectedIndex
        Settings("ImageFormat").Value = ComboBoxImageFormat.SelectedIndex
        Settings("CaptureSeconds").Value = TextBoxCaptureSeconds.Text
        Settings("CaptureFilename").Value = TextBoxCaptureFilename.Text
        Settings("StartMinimized").Value = CheckBoxStartMinimized.Checked
        Settings("AutoStart").Value = CheckBoxAutoStart.Checked
        Settings("Export100Percent").Value = CheckBox100Percent.Checked
        Settings("Export75Percent").Value = CheckBox75Percent.Checked
        Settings("Export50Percent").Value = CheckBox50Percent.Checked
        Settings("Export25Percent").Value = CheckBox25Percent.Checked
        Settings("ExportCustomPercent").Value = CheckBoxCustomPercent.Checked
        Settings("CustomPercent").Value = TextBoxCustomPercent.Text
        Settings("FullScreenCapture").Value = RadioButtonFullScreen.Checked
        Settings("CustomAreaCapture").Value = RadioButtonCustomArea.Checked
        Settings("CaptureCoordinates").Value = TextBoxCaptureCoordinates.Text
        SaveSettings()

    End Sub

    Private Sub ComboBoxSourceScreen_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxSourceScreen.SelectedIndexChanged

        If Not RadioButtonCustomArea.Checked Then
            With Screen.AllScreens(ComboBoxSourceScreen.SelectedIndex).WorkingArea
                TextBoxCaptureCoordinates.Text = "0, 0, " & .Width & ", " & .Height
            End With
        End If

    End Sub

    Private Sub ComboBoxImageFormat_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBoxImageFormat.SelectedIndexChanged

        ' Select proper capture image format
        Select Case ComboBoxImageFormat.Text.Trim.ToLower
            Case "bmp"
                m_fileFormat = ImageFormat.Bmp
            Case "emf"
                m_fileFormat = ImageFormat.Emf
            Case "exif"
                m_fileFormat = ImageFormat.Exif
            Case "gif"
                m_fileFormat = ImageFormat.Gif
            Case "icon"
                m_fileFormat = ImageFormat.Icon
            Case "jpeg"
                m_fileFormat = ImageFormat.Jpeg
            Case "png"
                m_fileFormat = ImageFormat.Png
            Case "tiff"
                m_fileFormat = ImageFormat.Tiff
            Case "wmf"
                m_fileFormat = ImageFormat.Wmf
            Case Else
                m_fileFormat = ImageFormat.Jpeg
        End Select

    End Sub

    Private Sub TextBoxCaptureFilename_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBoxCaptureFilename.TextChanged

        Try
            ' Define filename components (we suffix file name with scale size)
            m_rootFileName = TextBoxCaptureFilename.Text.Trim
            m_extension = Path.GetExtension(m_rootFileName)
            m_rootFileName = Path.GetDirectoryName(m_rootFileName) & Path.DirectorySeparatorChar & Path.GetFileNameWithoutExtension(m_rootFileName)
        Catch
            ' We just ignore errors here most likely caused by an invalid path or filename...
        End Try

    End Sub

    Private Sub ButtonStartCapture_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonStartCapture.Click

        With CaptureImageTimer
            If .Enabled Then
                .Enabled = False
                ButtonStartCapture.Text = "Start &Capture"
            Else
                CaptureImageTimer_Tick(Nothing, Nothing)
                .Interval = Convert.ToInt32(TextBoxCaptureSeconds.Text) * 1000
                .Enabled = True
                ButtonStartCapture.Text = "Stop &Capture"
                SaveSettings()
            End If
        End With

    End Sub

    Private Sub ButtonExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonExit.Click

        Me.Close()
        End

    End Sub

    Private Sub ButtonSelectCaptureFilename_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectCaptureFilename.Click

        With Me.SaveFileDialog
            .DefaultExt = "." & ComboBoxImageFormat.Text.ToLower
            .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            .Title = "Select Screen Capture Path and Filename"
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                TextBoxCaptureFilename.Text = .FileName
            End If
        End With

    End Sub

    Private Sub CaptureImageTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles CaptureImageTimer.Tick

        Dim screenIndex, x, y, width, height As Integer
        Dim regionSize As Size
        Dim screenImage As Image

        ' Define capture screen
        screenIndex = ComboBoxSourceScreen.SelectedIndex

        ' Define capture area
        If RadioButtonFullScreen.Checked Then
            With Screen.AllScreens(screenIndex).Bounds
                x = 0
                y = 0
                width = .Width
                height = .Height
            End With
        Else
            Dim coordinates As String() = TextBoxCaptureCoordinates.Text.Trim.Split(","c)
            x = Convert.ToInt32(coordinates(0).Trim)
            y = Convert.ToInt32(coordinates(1).Trim)
            width = Convert.ToInt32(coordinates(2).Trim)
            height = Convert.ToInt32(coordinates(3).Trim)
        End If

        ' Adjust capture coordinates for multiple monitors
        If screenIndex <> m_primaryScreenIndex Then
            If screenIndex > m_primaryScreenIndex Then
                ' Adjust x index for multiple screens...
                For i As Integer = m_primaryScreenIndex + 1 To screenIndex
                    With Screen.AllScreens(i).Bounds
                        x += .Width
                    End With
                Next
            Else
                ' Adjust x index for multiple screens...
                For i As Integer = m_primaryScreenIndex - 1 To screenIndex Step -1
                    With Screen.AllScreens(i).Bounds
                        x -= .Width
                    End With
                Next
            End If

            ' Adjust y index for multiple screens...
            y += Screen.AllScreens(screenIndex).Bounds.Y
        End If

        ' Define new screen region size
        regionSize = New Size(width, height)

        ' Capture screen image
        screenImage = CaptureScreenImage(x, y, regionSize)

        If CheckBox100Percent.Checked Then SaveScaledImage(screenImage, regionSize, 100)
        If CheckBox75Percent.Checked Then SaveScaledImage(screenImage, regionSize, 75)
        If CheckBox50Percent.Checked Then SaveScaledImage(screenImage, regionSize, 50)
        If CheckBox25Percent.Checked Then SaveScaledImage(screenImage, regionSize, 25)
        If CheckBoxCustomPercent.Checked Then SaveScaledImage(screenImage, regionSize, Convert.ToInt32(TextBoxCustomPercent.Text))

        screenImage.Dispose()

    End Sub

    Private Sub SaveScaledImage(ByVal sourceImage As Image, ByVal sourceRegion As Size, ByVal scale As Integer)

        Dim imageScale As Single = scale / 100

        If scale <> 100 Then sourceImage = CreateScaledImage(sourceImage, sourceRegion, imageScale, imageScale)

        sourceImage.Save(m_rootFileName & "-" & scale & m_extension, m_fileFormat)

        If scale <> 100 Then sourceImage.Dispose()

    End Sub

    Private Function CreateScaledImage(ByVal sourceImage As Image, ByVal sourceRegion As Size, ByVal scaleX As Single, ByVal scaleY As Single) As Image

        Dim scaledImage As New Bitmap(Convert.ToInt32(sourceRegion.Width * scaleX), Convert.ToInt32(sourceRegion.Height * scaleY))

        Try
            ' Created scaled image
            Dim sourceRect As New RectangleF(0, 0, sourceRegion.Width, sourceRegion.Height)
            Dim destinationRect As New RectangleF(0, 0, sourceRegion.Width * scaleX, sourceRegion.Height * scaleY)

            With Graphics.FromImage(scaledImage)
                .DrawImage(sourceImage, destinationRect, sourceRect, GraphicsUnit.Pixel)
            End With
        Catch ex As Exception
            LogErrorMessage("Exception creating scaled image: " & ex.Message)
        End Try

        Return scaledImage

    End Function

    Private Function CaptureScreenImage(ByVal x As Integer, ByVal y As Integer, ByVal regionSize As Size) As Image

        Dim screenImage As New Bitmap(regionSize.Width, regionSize.Height)

        Try
            ' Capture screen image
            With Graphics.FromImage(screenImage)
                .CopyFromScreen(x, y, 0, 0, regionSize, CopyPixelOperation.SourceCopy)
            End With
        Catch ex As Exception
            LogErrorMessage("Exception capturing screen image: " & ex.Message)
        End Try

        Return screenImage

    End Function

    Private Sub LogErrorMessage(ByVal message As String)

        With File.AppendText(Application.StartupPath & "\ErrorLog.txt")
            .WriteLine("[" & Date.Now & "] " & message)
            .Flush()
            .Close()
        End With

    End Sub

End Class
