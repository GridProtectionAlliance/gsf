'*******************************************************************************************************
'  Tva.Windows.Commmon.vb - Common Functions for Windows Applications
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  ??/??/2004 - J. Ritchie Carroll
'       Original version of source code generated
'  05/04/2006 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Forms.Common)
'
'*******************************************************************************************************

Imports System.Windows.Forms
Imports Tva.Collections.Common
Imports Tva.Configuration.Common

Public NotInheritable Class Common

    Private Const LastWindowSizeSetting As String = "LastWindowSize"
    Private Const LastWindowLocationSetting As String = "LastWindowLocation"

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    Public Shared Sub SaveWindowSettings(ByVal form As System.Windows.Forms.Form)

        SaveWindowSettings(form, Nothing)

    End Sub

    Public Shared Sub SaveWindowSettings(ByVal form As System.Windows.Forms.Form, ByVal windowName As String)

        SaveWindowSize(form, windowName)
        SaveWindowLocation(form, windowName)

    End Sub

    Public Shared Sub SaveWindowSize(ByVal form As System.Windows.Forms.Form)

        SaveWindowSize(form, Nothing)

    End Sub

    Public Shared Sub SaveWindowSize(ByVal form As System.Windows.Forms.Form, ByVal windowName As String)

        If windowName Is Nothing Then windowName = form.Name
        CategorizedSettings(LastWindowSizeSetting)(windowName).Value = form.Size.ToString()
        SaveSettings()

    End Sub

    Public Shared Sub SaveWindowLocation(ByVal form As System.Windows.Forms.Form)

        SaveWindowLocation(form, Nothing)

    End Sub

    Public Shared Sub SaveWindowLocation(ByVal form As System.Windows.Forms.Form, ByVal windowName As String)

        If windowName Is Nothing Then windowName = form.Name
        CategorizedSettings(LastWindowLocationSetting)(windowName).Value = form.Location.ToString()
        SaveSettings()

    End Sub

    Public Shared Sub RestoreWindowSettings(ByVal form As System.Windows.Forms.Form)

        RestoreWindowSettings(form, Nothing)

    End Sub

    Public Shared Sub RestoreWindowSettings(ByVal form As System.Windows.Forms.Form, ByVal windowName As String)

        RestoreWindowSize(form, windowName)
        RestoreWindowLocation(form, windowName)

    End Sub

    Public Shared Sub RestoreWindowSize(ByVal form As System.Windows.Forms.Form)

        RestoreWindowSize(form, Nothing)

    End Sub

    Public Shared Sub RestoreWindowSize(ByVal form As System.Windows.Forms.Form, ByVal windowName As String)

        If windowName Is Nothing Then windowName = form.Name
        Dim sizeData As String = Trim(CategorizedSettings(LastWindowSizeSetting)(windowName).Value())

        ' Restore last window size
        If Len(sizeData) > 0 Then
            Dim params As Hashtable = GetWindowParams(sizeData)

            With form
                .Height = GetWindowParam(params, "Height", .Height, Maximum(.MinimumSize.Height, .Height), -1)
                .Width = GetWindowParam(params, "Width", .Width, Maximum(.MinimumSize.Width, .Width), -1)
            End With
        End If

    End Sub

    Public Shared Sub RestoreWindowLocation(ByVal form As System.Windows.Forms.Form)

        RestoreWindowLocation(form, Nothing)

    End Sub

    Public Shared Sub RestoreWindowLocation(ByVal form As System.Windows.Forms.Form, ByVal windowName As String)

        If windowName Is Nothing Then windowName = form.Name
        Dim locationData As String = Trim(CategorizedSettings(LastWindowLocationSetting)(windowName).Value())

        ' Restore last window location
        If Len(locationData) > 0 Then
            Dim params As Hashtable = GetWindowParams(locationData)

            With form
                .Left = GetWindowParam(params, "X", .Left, 0, Maximum(GetTotalScreenWidth() - .Width, 0))
                .Top = GetWindowParam(params, "Y", .Top, 0, Maximum(GetMaximumScreenHeight() - .Height, 0))
            End With
        End If

    End Sub

    Public Shared Function GetTotalScreenWidth() As Long

        Dim totalWidth As Long

        ' We just assume screens are side-by-side and get cumulative screen widths
        For Each screen As Screen In System.Windows.Forms.Screen.AllScreens
            totalWidth += screen.Bounds.Width
        Next

        Return totalWidth

    End Function

    Public Shared Function GetMaximumScreenHeight() As Long

        Dim maxHeight As Long

        ' In this case we just get the largest screen height
        For Each screen As Screen In System.Windows.Forms.Screen.AllScreens
            If maxHeight = 0 Or maxHeight < screen.Bounds.Height Then maxHeight = screen.Bounds.Height
        Next

        Return maxHeight

    End Function

    Public Shared Function GetMinimumScreenHeight() As Long

        Dim minHeight As Long

        ' In this case we just get the smallest screen height
        For Each screen As Screen In System.Windows.Forms.Screen.AllScreens
            If minHeight = 0 Or minHeight > screen.Bounds.Height Then minHeight = screen.Bounds.Height
        Next

        Return minHeight

    End Function

#Region " Helpers "

    Private Shared Function GetWindowParam(ByVal params As Hashtable, ByVal key As String, _
        ByVal defaultValue As Integer, ByVal minimumValue As Integer, ByVal maximumValue As Integer) As Integer

        Dim param As Integer = params(key)

        'If CInt(NotNull(param, 0)) = 0 Then param = defaultValue

        If minimumValue <> -1 Then If param < minimumValue Then param = minimumValue
        If maximumValue <> -1 Then If param > maximumValue Then param = maximumValue

        Return param

    End Function

    Private Shared Function GetWindowParams(ByVal dataSet As String) As Hashtable

        Dim params As New Hashtable

        ' Example data sets:
        '   Me.Size.ToString() = {Width=1491, Height=1082}
        '   Me.Location.ToString() = {X=1894,Y=112}

        If Len(dataSet) > 0 Then
            If dataSet.Chars(0) = "{"c And dataSet.Chars(dataSet.Length - 1) = "}"c Then
                ' Remove surrounding braces
                dataSet = dataSet.Substring(1, dataSet.Length - 2)
                If Len(dataSet) > 0 Then
                    Dim elements As String()

                    ' Get each key/value pair
                    For Each pair As String In dataSet.Split(","c)
                        ' Store key/value pair in hashtable
                        elements = pair.Split("="c)
                        If elements.Length >= 2 Then params.Add(Trim(elements(0)), CInt(elements(1)))
                    Next
                End If
            End If
        End If

        Return params

    End Function

#End Region

End Class
