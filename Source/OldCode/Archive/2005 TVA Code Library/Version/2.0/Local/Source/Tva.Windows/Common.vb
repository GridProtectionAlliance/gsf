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

    ''' <summary>
    ''' Saves the size and location information of the specified form to the application configuration file.
    ''' </summary>
    ''' <param name="form">The System.Windows.Forms.Form whose size and location information is to be saved.</param>
    ''' <remarks></remarks>
    Public Shared Sub SaveWindowSettings(ByVal form As System.Windows.Forms.Form)

        SaveWindowSize(form)
        SaveWindowLocation(form)

    End Sub

    ''' <summary>
    ''' Saves the size information of the specified form to the application configuration file.
    ''' </summary>
    ''' <param name="form">The System.Windows.Forms.Form whose size information is to be saved.</param>
    ''' <remarks></remarks>
    Public Shared Sub SaveWindowSize(ByVal form As System.Windows.Forms.Form)

        CategorizedSettings(LastWindowSizeSetting)(form.Name()).Value = form.Size.ToString()
        SaveSettings()

    End Sub

    ''' <summary>
    ''' Saves the location information of the specified form to the application configuration file.
    ''' </summary>
    ''' <param name="form">The System.Windows.Forms.Form whose location information is to be saved.</param>
    ''' <remarks></remarks>
    Public Shared Sub SaveWindowLocation(ByVal form As System.Windows.Forms.Form)

        CategorizedSettings(LastWindowLocationSetting)(form.Name()).Value = form.Location.ToString()
        SaveSettings()

    End Sub

    ''' <summary>
    ''' Restores the size and location of the specified form from the size and location information saved in the 
    ''' application configuration file.
    ''' </summary>
    ''' <param name="form">The System.Windows.Forms.Form whose size and location is to be restored.</param>
    ''' <remarks></remarks>
    Public Shared Sub RestoreWindowSettings(ByVal form As System.Windows.Forms.Form)

        RestoreWindowSize(form)
        RestoreWindowLocation(form)

    End Sub

    ''' <summary>
    ''' Restores the size of the specified form from the size information saved in the application configuration file.
    ''' </summary>
    ''' <param name="form">The System.Windows.Forms.Form whose size is to be restored.</param>
    ''' <remarks></remarks>
    Public Shared Sub RestoreWindowSize(ByVal form As System.Windows.Forms.Form)

        Dim savedSize As String = CategorizedSettings(LastWindowSizeSetting)(form.Name()).Value()

        If Not String.IsNullOrEmpty(savedSize) Then
            ' Restore last saved window size.
            Dim sizeSetting As New Setting(Of Integer)(savedSize)
            With form
                .Width = Minimum(sizeSetting.ParamA(), GetTotalScreenWidth())
                .Height = Minimum(sizeSetting.ParamB(), GetMaximumScreenHeight())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Restores the location of the specified form from the location information saved in the application configuration file.
    ''' </summary>
    ''' <param name="form">The System.Windows.Forms.Form whose location is to be restored.</param>
    ''' <remarks></remarks>
    Public Shared Sub RestoreWindowLocation(ByVal form As System.Windows.Forms.Form)

        Dim savedLocation As String = CategorizedSettings(LastWindowLocationSetting)(form.Name()).Value()

        If Not String.IsNullOrEmpty(savedLocation) Then
            ' Restore last saved window location.
            Dim locationSetting As New Setting(Of Integer)(savedLocation)
            With form
                .Left = Minimum(locationSetting.ParamA(), (GetTotalScreenWidth() - .Width()))
                .Top = Minimum(locationSetting.ParamB(), (GetMaximumScreenHeight() - .Height()))
            End With
        End If

    End Sub

    ''' <summary>
    ''' Gets the total width of all the screens assuming the screens are side-by-side.
    ''' </summary>
    ''' <returns>The total width of all the screens assuming the screens are side-by-side.</returns>
    ''' <remarks></remarks>
    Public Shared Function GetTotalScreenWidth() As Long

        Dim totalWidth As Long

        ' We just assume screens are side-by-side and get cumulative screen widths
        For Each screen As Screen In System.Windows.Forms.Screen.AllScreens
            totalWidth += screen.Bounds.Width
        Next

        Return totalWidth

    End Function

    ''' <summary>
    ''' Gets the height of the screen with the highest resolution.
    ''' </summary>
    ''' <returns>The height of the screen with the highest resolution.</returns>
    ''' <remarks></remarks>
    Public Shared Function GetMaximumScreenHeight() As Long

        Dim maxHeight As Long

        ' In this case we just get the largest screen height
        For Each screen As Screen In System.Windows.Forms.Screen.AllScreens
            If maxHeight = 0 Or maxHeight < screen.Bounds.Height Then maxHeight = screen.Bounds.Height
        Next

        Return maxHeight

    End Function

    ''' <summary>
    ''' Gets the height of the screen with the lowest resolution.
    ''' </summary>
    ''' <returns>The height of the screen with the lowest resolution.</returns>
    ''' <remarks></remarks>
    Public Shared Function GetMinimumScreenHeight() As Long

        Dim minHeight As Long

        ' In this case we just get the smallest screen height
        For Each screen As Screen In System.Windows.Forms.Screen.AllScreens
            If minHeight = 0 Or minHeight > screen.Bounds.Height Then minHeight = screen.Bounds.Height
        Next

        Return minHeight

    End Function

#Region " Helpers "

    Private Class Setting(Of T)

        Private m_paramA As T
        Private m_paramB As T

        Public Sub New(ByVal setting As String)

            If Not String.IsNullOrEmpty(setting) Then
                If setting.Chars(0) = "{"c And setting.Chars(setting.Length() - 1) = "}"c Then
                    ' Remove surrounding braces
                    setting = setting.Substring(1, setting.Length() - 2)
                    If Not String.IsNullOrEmpty(setting) Then
                        Dim elements As Object() = setting.Split(New Char() {","c})
                        If elements.Length() = 2 Then
                            m_paramA = CType(elements(0).Split(New Char() {"="c})(1), T)
                            m_paramB = CType(elements(1).Split(New Char() {"="c})(1), T)
                        End If
                    End If
                End If
            End If

        End Sub

        Public ReadOnly Property ParamA() As T
            Get
                Return m_paramA
            End Get
        End Property

        Public ReadOnly Property ParamB() As T
            Get
                Return m_paramB
            End Get
        End Property

    End Class

    Private Shared Function GetWindowParam(ByVal params As Hashtable, ByVal key As String, _
        ByVal defaultValue As Integer, ByVal minimumValue As Integer, ByVal maximumValue As Integer) As Integer

        Dim param As Integer = params(key)

        'If CInt(NotNull(param, 0)) = 0 Then param = defaultValue

        If minimumValue <> -1 Then If param < minimumValue Then param = minimumValue
        If maximumValue <> -1 Then If param > maximumValue Then param = maximumValue

        Return param

    End Function

#End Region

End Class
