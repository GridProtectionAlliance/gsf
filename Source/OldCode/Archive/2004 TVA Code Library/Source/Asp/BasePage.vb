' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Web.UI
Imports System.Web.SessionState
Imports System.ComponentModel
Imports TVA.Shared.Common

Namespace Asp

    ' Base page for Asp.NET pages
    Public Class BasePage : Inherits Page

        ' Overridden page load - adds common notation to pages written using this utility
        Public Overridable Sub Page_Load(ByVal s As Object, ByVal e As EventArgs)

            Response.Write("<!--" & vbCrLf & vbCrLf)
            Response.Write(vbTab & "This Asp.NET site was developed with the TVA Code Library [" & GetShortAssemblyName(Me.GetType.Assembly) & "]" & vbCrLf)
            Response.Write(vbTab & "Copyright © 2004, TVA.  All rights reserved." & vbCrLf & vbCrLf)
            Response.Write("-->" & vbCrLf)

        End Sub

        ' Argument shortcut function - equivalent to Request(Name)
        Public Function Arg(ByVal Name As String) As String

            Return Request(Name)

        End Function

        ' Argument shortcut function - equivalent to Request.Form(Name)
        Public Function PostArg(ByVal Name As String) As String

            Return Request.Form(Name)

        End Function

        ' Argument shortcut function - equivalent to Request.QueryString(Name)
        Public Function SearchArg(ByVal Name As String) As String

            Return Request.QueryString(Name)

        End Function

        ' Argument shortcut function - equivalent to Request.Form.GetValues(Name)(Index)
        Public Function PostArgItem(ByVal Name As String, ByVal Index As Integer) As String

            Return Request.Form.GetValues(Name)(Index)

        End Function

        ' Argument shortcut function - equivalent to Request.QueryString.GetValues(Name)(Index)
        Public Function SearchArgItem(ByVal Name As String, ByVal Index As Integer) As String

            Return Request.QueryString.GetValues(Name)(Index)

        End Function

        ' Argument shortcut function - equivalent to Request.Form.GetValues(Name).Length
        Public Function PostArgItemCount(ByVal Name As String) As Integer

            Return Request.Form.GetValues(Name).Length

        End Function

        ' Argument shortcut function - equivalent to Request.QueryString.GetValues(Name).Length
        Public Function SearchArgItemCount(ByVal Name As String) As Integer

            Return Request.QueryString.GetValues(Name).Length

        End Function

        ' Required argument shortcut function, response ends if argument not provided - equivalent to Request(Name)
        Public Function RequiredArg(ByVal Name As String) As String

            Dim strValue As String

            strValue = Request(Name)

            If Len(strValue) = 0 Then
                Response.Write("""" & Name & """ is a required argument.  Operation canceled.")
                Response.End()
            Else
                Return strValue
            End If

        End Function

        ' Required argument shortcut function, response ends if argument not provided - equivalent to Request.Form(Name)
        Public Function RequiredPostArg(ByVal Name As String) As String

            Dim strValue As String

            strValue = Request.Form(Name)

            If Len(strValue) = 0 Then
                Response.Write("""" & Name & """ is a required form argument.  Operation canceled.")
                Response.End()
            Else
                Return strValue
            End If

        End Function

        ' Required argument shortcut function, response ends if argument not provided - equivalent to Request.QueryString(Name)
        Public Function RequiredSearchArg(ByVal Name As String) As String

            Dim strValue As String

            strValue = Request.QueryString(Name)

            If Len(strValue) = 0 Then
                Response.Write("""" & Name & """ is a required query string argument.  Operation canceled.")
                Response.End()
            Else
                Return strValue
            End If

        End Function

        ' Required argument shortcut function, response ends if argument not provided - equivalent to Request.Form.GetValues(Name)(Index)
        Public Function RequiredPostArgItem(ByVal Name As String, ByVal Index As Integer) As String

            Dim strValue As String

            strValue = Request.Form.GetValues(Name)(Index)

            If Len(strValue) = 0 Then
                Response.Write("""" & Name & """ is a required form argument.  Operation canceled.")
                Response.End()
            Else
                Return strValue
            End If

        End Function

        ' Required argument shortcut function, response ends if argument not provided - equivalent to Request.QueryString.GetValues(Name)(Index)
        Public Function RequiredSearchArgItem(ByVal Name As String, ByVal Index As Integer) As String

            Dim strValue As String

            strValue = Request.QueryString.GetValues(Name)(Index)

            If Len(strValue) = 0 Then
                Response.Write("""" & Name & """ is a required form argument.  Operation canceled.")
                Response.End()
            Else
                Return strValue
            End If

        End Function

        ' Argument shortcut function - returns True if argument is equal to specified value (uses case insensitive comparsion equivalent to Request(Name) = Value)
        Public Function ArgIs(ByVal Name As String, ByVal Value As String) As Boolean

            Return (StrComp(Request(Name), Value, CompareMethod.Text) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is equal to specified value (uses case insensitive comparsion equivalent to Request.QueryString(Name) = Value)
        Public Function SearchArgIs(ByVal Name As String, ByVal Value As String) As Boolean

            Return (StrComp(Request.QueryString(Name), Value, CompareMethod.Text) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is equal to specified value (uses case insensitive comparsion equivalent to Request.QueryString.GetValues(Name)(Index) = Value)
        Public Function SearchArgItemIs(ByVal Name As String, ByVal Value As String, ByVal Index As Integer) As Boolean

            Return (StrComp(Request.QueryString.GetValues(Name)(Index), Value, CompareMethod.Text) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is equal to specified value (uses case insensitive comparsion equivalent to Request.Form(Name) = Value)
        Public Function PostArgIs(ByVal Name As String, ByVal Value As String) As Boolean

            Return (StrComp(Request.Form(Name), Value, CompareMethod.Text) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is equal to specified value (uses case insensitive comparsion equivalent to Request.Form.GetValues(Name)(Index) = Value)
        Public Function PostArgItemIs(ByVal Name As String, ByVal Value As String, ByVal Index As Integer) As Boolean

            Return (StrComp(Request.Form.GetValues(Name)(Index), Value, CompareMethod.Text) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is empty (equivalent to Len(Request(Name)) = 0)
        Public Function ArgIsEmpty(ByVal Name As String) As Boolean

            Return (Len(Request(Name)) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is empty (equivalent to Len(Request.QueryString(Name)) = 0)
        Public Function SearchArgIsEmpty(ByVal Name As String) As Boolean

            Return (Len(Request.QueryString(Name)) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is empty (equivalent to Len(Request.QueryString.GetValues(Name)(Index)) = 0)
        Public Function SearchArgItemIsEmpty(ByVal Name As String, ByVal Index As Integer) As Boolean

            Return (Len(Request.QueryString.GetValues(Name)(Index)) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is empty (equivalent to Len(Request.Form(Name)) = 0)
        Public Function PostArgIsEmpty(ByVal Name As String) As Boolean

            Return (Len(Request.Form(Name)) = 0)

        End Function

        ' Argument shortcut function - returns True if argument is empty (equivalent to Len(Request.Form.GetValues(Name)(Index)) = 0)
        Public Function PostArgItemIsEmpty(ByVal Name As String, ByVal Index As Integer) As Boolean

            Return (Len(Request.Form.GetValues(Name)(Index)) = 0)

        End Function

        ' Argument shortcut function - insures returned blank numeric arguments come back with a "0" instead of blank
        Public Function NumericArg(ByVal Name As String, Optional ByVal WithDecimal As Boolean = False) As String

            Dim strValue As String

            strValue = Request(Name)

            If Len(strValue) = 0 Then
                If WithDecimal Then
                    strValue = "0.0"
                Else
                    strValue = "0"
                End If
            End If

            Return strValue

        End Function

        ' Argument shortcut function for checkbox field - returns a True or False boolean value for checked/unchecked
        Public Function BoolArg(ByVal Name As String) As Boolean

            ' This function is used for checkbox fields
            If Len(Request.Form(Name)) > 0 Then
                Return True
            Else
                Return False
            End If

        End Function

        ' Argument shortcut function for checkbox field - returns a 1 or 0 integer value for checked/unchecked
        Public Function DBBoolArg(ByVal Name As String) As Integer

            ' This function is used for checkbox fields
            If Len(Request.Form(Name)) > 0 Then
                Return 1
            Else
                Return 0
            End If

        End Function

        ' Argument shortcut function for checkbox field - returns a "Yes" or "No" string value for checked/unchecked
        Public Function YesNoArg(ByVal Name As String) As String

            ' This function is used for checkbox fields
            If Len(Request.Form(Name)) > 0 Then
                Return "Yes"
            Else
                Return "No"
            End If

        End Function

        ' Argument shortcut function for checkbox field - returns a "Y" or "N" string value for checked/unchecked
        Public Function YNArg(ByVal Name As String) As String

            ' This function is used for checkbox fields
            If Len(Request.Form(Name)) > 0 Then
                Return "Y"
            Else
                Return "N"
            End If

        End Function

    End Class

End Namespace