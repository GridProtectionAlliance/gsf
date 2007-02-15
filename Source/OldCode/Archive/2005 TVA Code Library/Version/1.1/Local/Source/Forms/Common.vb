' James Ritchie Carroll - 2004
Option Explicit On 

Imports TVA.Shared.Common
Imports TVA.Shared.String
Imports TVA.Config.Common
Imports WinForms = System.Windows.Forms

Namespace Forms

    ' Common functions for Windows form applications
    Public Class Common

        Private Const LastWindowSizeSetting As String = "LastWindowSize"
        Private Const LastWindowLocationSetting As String = "LastWindowLocation"

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Shared Sub SaveWindowSettings(ByVal Form As WinForms.Form, Optional ByVal WindowName As String = Nothing)

            SaveWindowSize(Form, WindowName)
            SaveWindowLocation(Form, WindowName)

        End Sub

        Public Shared Sub SaveWindowSize(ByVal Form As WinForms.Form, Optional ByVal WindowName As String = Nothing)

            If WindowName Is Nothing Then WindowName = Form.Name
            Variables(WindowName & "." & LastWindowSizeSetting) = Form.Size.ToString()

        End Sub

        Public Shared Sub SaveWindowLocation(ByVal Form As WinForms.Form, Optional ByVal WindowName As String = Nothing)

            If WindowName Is Nothing Then WindowName = Form.Name
            Variables(WindowName & "." & LastWindowLocationSetting) = Form.Location.ToString()

        End Sub

        Public Shared Sub RestoreWindowSettings(ByVal Form As WinForms.Form, Optional ByVal WindowName As String = Nothing)

            RestoreWindowSize(Form, WindowName)
            RestoreWindowLocation(Form, WindowName)

        End Sub

        Public Shared Sub RestoreWindowSize(ByVal Form As WinForms.Form, Optional ByVal WindowName As String = Nothing)

            If WindowName Is Nothing Then WindowName = Form.Name
            Dim SizeData As String = Trim(Variables(WindowName & "." & LastWindowSizeSetting))

            ' Restore last window size
            If Len(SizeData) > 0 Then
                Dim tblParams As Hashtable = GetWindowParams(SizeData)

                With Form
                    .Height = GetWindowParam(tblParams, "Height", .Height, Maximum(.MinimumSize.Height, .Height))
                    .Width = GetWindowParam(tblParams, "Width", .Width, Maximum(.MinimumSize.Width, .Width))
                End With
            End If

        End Sub

        Public Shared Sub RestoreWindowLocation(ByVal Form As WinForms.Form, Optional ByVal WindowName As String = Nothing)

            If WindowName Is Nothing Then WindowName = Form.Name
            Dim LocationData As String = Trim(Variables(WindowName & "." & LastWindowLocationSetting))

            ' Restore last window location
            If Len(LocationData) > 0 Then
                Dim tblParams As Hashtable = GetWindowParams(LocationData)

                With Form
                    .Left = GetWindowParam(tblParams, "X", .Left, 0, Maximum(GetTotalScreenWidth() - .Width, 0))
                    .Top = GetWindowParam(tblParams, "Y", .Top, 0, Maximum(GetMaximumScreenHeight() - .Height, 0))
                End With
            End If

        End Sub

        Private Shared Function GetWindowParam(ByVal Params As Hashtable, ByVal Key As String, ByVal DefaultValue As Integer, Optional ByVal MinimumValue As Integer = -1, Optional ByVal MaximumValue As Integer = -1) As Integer

            Dim Param As Integer = Params(Key)

            If CInt(NotNull(Param, 0)) = 0 Then Param = DefaultValue

            If MinimumValue <> -1 Then If Param < MinimumValue Then Param = MinimumValue
            If MaximumValue <> -1 Then If Param > MaximumValue Then Param = MaximumValue

            Return Param

        End Function

        Private Shared Function GetWindowParams(ByVal DataSet As String) As Hashtable

            Dim Params As New Hashtable

            ' Example data sets:
            '   Me.Size.ToString() = {Width=1491, Height=1082}
            '   Me.Location.ToString() = {X=1894,Y=112}

            If Len(DataSet) > 0 Then
                If DataSet.Chars(0) = "{"c And DataSet.Chars(DataSet.Length - 1) = "}"c Then
                    ' Remove surrounding braces
                    DataSet = DataSet.Substring(1, DataSet.Length - 2)
                    If Len(DataSet) > 0 Then
                        Dim strElems As String()

                        ' Get each key/value pair
                        For Each strKVPair As String In DataSet.Split(","c)
                            ' Store key/value pair in hashtable
                            strElems = strKVPair.Split("="c)
                            If strElems.Length >= 2 Then Params.Add(Trim(strElems(0)), CInt(strElems(1)))
                        Next
                    End If
                End If
            End If

            Return Params

        End Function

        Public Shared Function GetTotalScreenWidth() As Long

            Dim TotalWidth As Long

            ' We just assume screens are side-by-side and get cumulative screen widths
            For Each Screen As WinForms.Screen In WinForms.Screen.AllScreens
                TotalWidth += Screen.Bounds.Width
            Next

            Return TotalWidth

        End Function

        Public Shared Function GetMaximumScreenHeight() As Long

            Dim MaxHeight As Long

            ' In this case we just get the largest screen height
            For Each Screen As WinForms.Screen In WinForms.Screen.AllScreens
                If MaxHeight = 0 Or MaxHeight < Screen.Bounds.Height Then MaxHeight = Screen.Bounds.Height
            Next

            Return MaxHeight

        End Function

        Public Shared Function GetMinimumScreenHeight() As Long

            Dim MinHeight As Long

            ' In this case we just get the smallest screen height
            For Each Screen As WinForms.Screen In WinForms.Screen.AllScreens
                If MinHeight = 0 Or MinHeight > Screen.Bounds.Height Then MinHeight = Screen.Bounds.Height
            Next

            Return MinHeight

        End Function

    End Class

End Namespace