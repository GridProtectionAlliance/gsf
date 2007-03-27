'*******************************************************************************************************
'  TVA.Windows.Commmon.vb - Common Functions for Windows Applications
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
'  04/25/2004 - J. Ritchie Carroll
'       Original version of source code generated
'  05/04/2006 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Forms.Common)
'  10/30/2006 - J. Ritchie Carroll
'       Added left-most, right-most, top-most and bottom-most screen bound functions
'       Fixed an issue with forms showing up off the screen (esp. when switching primary monitor)
'
'*******************************************************************************************************

Imports System.Windows.Forms
Imports TVA.Collections.Common
Imports TVA.Configuration.Common

Public NotInheritable Class Common

    Public Const LastWindowSizeSettingsCategory As String = "LastWindowSize"
    Public Const LastWindowLocationSettingsCategory As String = "LastWindowLocation"

#Region " Private Window Setting Helper Class "

    Private Class WindowSetting

        Private m_paramA As Integer
        Private m_paramB As Integer

        Public Sub New(ByVal setting As String)

            If Not String.IsNullOrEmpty(setting) Then
                If setting.Chars(0) = "{"c And setting.Chars(setting.Length() - 1) = "}"c Then
                    ' Remove surrounding braces
                    setting = setting.Substring(1, setting.Length() - 2)

                    If Not String.IsNullOrEmpty(setting) Then
                        Dim elements As String() = setting.Split(","c)
                        If elements.Length = 2 Then
                            m_paramA = CInt(elements(0).Split("="c)(1))
                            m_paramB = CInt(elements(1).Split("="c)(1))
                        End If
                    End If
                End If
            End If

        End Sub

        Public ReadOnly Property ParamA(ByVal minimumValue As Integer, ByVal maximumValue As Integer) As Integer
            Get
                Return ValidWindowParameter(m_paramA, minimumValue, maximumValue)
            End Get
        End Property

        Public ReadOnly Property ParamB(ByVal minimumValue As Integer, ByVal maximumValue As Integer) As Integer
            Get
                Return ValidWindowParameter(m_paramB, minimumValue, maximumValue)
            End Get
        End Property

        Private Function ValidWindowParameter(ByVal value As Integer, ByVal minimumValue As Integer, ByVal maximumValue As Integer) As Integer

            If value > maximumValue Then value = maximumValue
            If value < minimumValue Then value = minimumValue

            Return value

        End Function

    End Class

#End Region

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>
    ''' Saves the size and location information of the specified windowsForm to the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose size and location information is to be saved.</param>
    ''' <remarks>This function simply calls the SaveWindowSize and SaveWindowLocation functions using the default settings categories</remarks>
    Public Shared Sub SaveWindowSettings(ByVal windowsForm As Form)

        SaveWindowSize(windowsForm)
        SaveWindowLocation(windowsForm)

    End Sub

    ''' <summary>
    ''' Saves the size information of the specified windowsForm to the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose size information is to be saved.</param>
    ''' <remarks>This function uses the default settings category "LastWindowSize"</remarks>
    Public Shared Sub SaveWindowSize(ByVal windowsForm As Form)

        SaveWindowSize(windowsForm, LastWindowSizeSettingsCategory)

    End Sub

    ''' <summary>
    ''' Saves the size information of the specified windowsForm to the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose size information is to be saved.</param>
    ''' <param name="settingsCategory">Settings category used to persist form size information</param>
    Public Shared Sub SaveWindowSize(ByVal windowsForm As Form, ByVal settingsCategory As String)

        With CategorizedSettings(settingsCategory)
            If .Item(windowsForm.Name) IsNot Nothing Then
                .Item(windowsForm.Name).Value = windowsForm.Size.ToString()
            Else
                .Add(windowsForm.Name, windowsForm.Size.ToString())
            End If
        End With

        SaveSettings()

    End Sub

    ''' <summary>
    ''' Saves the location information of the specified windowsForm to the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose location information is to be saved.</param>
    ''' <remarks>This function uses the default settings category "LastWindowLocation"</remarks>
    Public Shared Sub SaveWindowLocation(ByVal windowsForm As Form)

        SaveWindowLocation(windowsForm, LastWindowLocationSettingsCategory)

    End Sub

    ''' <summary>
    ''' Saves the location information of the specified windowsForm to the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose location information is to be saved.</param>
    ''' <param name="settingsCategory">Settings category used to persist form location information</param>
    Public Shared Sub SaveWindowLocation(ByVal windowsForm As Form, ByVal settingsCategory As String)

        With CategorizedSettings(settingsCategory)
            If .Item(windowsForm.Name) IsNot Nothing Then
                .Item(windowsForm.Name).Value = windowsForm.Location.ToString()
            Else
                .Add(windowsForm.Name, windowsForm.Location.ToString())
            End If
        End With

        SaveSettings()

    End Sub

    ''' <summary>
    ''' Restores the size and location of the specified windowsForm from the size and location information saved in the 
    ''' application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose size and location is to be restored.</param>
    ''' <remarks>This function simply calls the RestoreWindowSize and RestoreWindowLocation functions using the default settings categories</remarks>
    Public Shared Sub RestoreWindowSettings(ByVal windowsForm As Form)

        RestoreWindowSize(windowsForm)
        RestoreWindowLocation(windowsForm)

    End Sub

    ''' <summary>
    ''' Restores the size of the specified windowsForm from the size information saved in the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose size is to be restored.</param>
    ''' <remarks>This function uses the default settings category "LastWindowSize"</remarks>
    Public Shared Sub RestoreWindowSize(ByVal windowsForm As Form)

        RestoreWindowSize(windowsForm, LastWindowSizeSettingsCategory)

    End Sub

    ''' <summary>
    ''' Restores the size of the specified windowsForm from the size information saved in the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose size is to be restored.</param>
    ''' <param name="settingsCategory">Settings category used to persist form size information</param>
    Public Shared Sub RestoreWindowSize(ByVal windowsForm As Form, ByVal settingsCategory As String)

        If CategorizedSettings(settingsCategory)(windowsForm.Name) IsNot Nothing Then
            ' Restore last saved window size
            Dim sizeSetting As New WindowSetting(CategorizedSettings(settingsCategory)(windowsForm.Name).Value())

            With windowsForm
                .Width = sizeSetting.ParamA(.MinimumSize.Width, GetTotalScreenWidth())
                .Height = sizeSetting.ParamB(.MinimumSize.Height, GetMaximumScreenHeight())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Restores the location of the specified windowsForm from the location information saved in the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose location is to be restored.</param>
    ''' <remarks>This function uses the default settings category "LastWindowLocation"</remarks>
    Public Shared Sub RestoreWindowLocation(ByVal windowsForm As Form)

        RestoreWindowLocation(windowsForm, LastWindowLocationSettingsCategory)

    End Sub

    ''' <summary>
    ''' Restores the location of the specified windowsForm from the location information saved in the application configuration file.
    ''' </summary>
    ''' <param name="windowsForm">The Form whose location is to be restored.</param>
    ''' <param name="settingsCategory">Settings category used to persist form location information</param>
    Public Shared Sub RestoreWindowLocation(ByVal windowsForm As Form, ByVal settingsCategory As String)

        If CategorizedSettings(settingsCategory)(windowsForm.Name) IsNot Nothing Then
            ' Restore last saved window location
            Dim locationSetting As New WindowSetting(CategorizedSettings(settingsCategory)(windowsForm.Name).Value())

            With windowsForm
                .Left = locationSetting.ParamA(GetLeftMostScreenBound(), GetRightMostScreenBound() - .MinimumSize.Width)
                .Top = locationSetting.ParamB(GetTopMostScreenBound(), GetBottomMostScreenBound() - .MinimumSize.Height)
            End With
        End If

    End Sub

    ''' <summary>
    ''' Gets the least "x" coordinate of all screens on the system
    ''' </summary>
    ''' <returns>The smallest visible "x" screen coordinate</returns>
    Public Shared Function GetLeftMostScreenBound() As Integer

        Dim leftBound As Integer

        ' Return the left-most "x" screen coordinate
        For Each display As Screen In Screen.AllScreens
            If leftBound > display.Bounds.X Then leftBound = display.Bounds.X
        Next

        Return leftBound

    End Function

    ''' <summary>
    ''' Gets the greatest "x" coordinate of all screens on the system
    ''' </summary>
    ''' <returns>The largest visible "x" screen coordinate</returns>
    Public Shared Function GetRightMostScreenBound() As Integer

        Dim rightBound As Integer

        ' Return the right-most "x" screen coordinate
        For Each display As Screen In Screen.AllScreens
            If rightBound < display.Bounds.X + display.Bounds.Width Then rightBound = display.Bounds.X + display.Bounds.Width
        Next

        Return rightBound

    End Function

    ''' <summary>
    ''' Gets the least "y" coordinate of all screens on the system
    ''' </summary>
    ''' <returns>The smallest visible "y" screen coordinate</returns>
    Public Shared Function GetTopMostScreenBound() As Integer

        Dim topBound As Integer

        ' Return the top-most "y" screen coordinate
        For Each display As Screen In Screen.AllScreens
            If topBound > display.Bounds.Y Then topBound = display.Bounds.Y
        Next

        Return topBound

    End Function

    ''' <summary>
    ''' Gets the greatest "y" coordinate of all screens on the system
    ''' </summary>
    ''' <returns>The largest visible "y" screen coordinate</returns>
    Public Shared Function GetBottomMostScreenBound() As Integer

        Dim bottomBound As Integer

        ' Return the bottom-most "y" screen coordinate
        For Each display As Screen In Screen.AllScreens
            If bottomBound < display.Bounds.Y + display.Bounds.Height Then bottomBound = display.Bounds.Y + display.Bounds.Height
        Next

        Return bottomBound

    End Function

    ''' <summary>
    ''' Gets the total width of all the screens assuming the screens are side-by-side.
    ''' </summary>
    ''' <returns>The total width of all the screens assuming the screens are side-by-side.</returns>
    Public Shared Function GetTotalScreenWidth() As Integer

        Dim totalWidth As Integer

        ' We just assume screens are side-by-side and get cumulative screen widths
        For Each display As Screen In Screen.AllScreens
            totalWidth += display.Bounds.Width
        Next

        Return totalWidth

    End Function

    ''' <summary>
    ''' Gets the height of the screen with the highest resolution.
    ''' </summary>
    ''' <returns>The height of the screen with the highest resolution.</returns>
    Public Shared Function GetMaximumScreenHeight() As Integer

        Dim maxHeight As Integer

        ' In this case we just get the largest screen height
        For Each display As Screen In Screen.AllScreens
            If maxHeight < display.Bounds.Height Then maxHeight = display.Bounds.Height
        Next

        Return maxHeight

    End Function

    ''' <summary>
    ''' Gets the height of the screen with the lowest resolution.
    ''' </summary>
    ''' <returns>The height of the screen with the lowest resolution.</returns>
    Public Shared Function GetMinimumScreenHeight() As Integer

        Dim minHeight As Integer

        ' In this case we just get the smallest screen height
        For Each display As Screen In Screen.AllScreens
            If minHeight > display.Bounds.Height Then minHeight = display.Bounds.Height
        Next

        Return minHeight

    End Function

End Class
