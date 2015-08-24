'*******************************************************************************************************
'  TVA.Interop.IniFile.vb - Old style Windows INI file manipulation class
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'  01/05/2006 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Interop.Windows.IniFile)
'  01/05/2007 - J. Ritchie Carroll
'       Breaking change: Renamed "IniFileName" property to "FileName"
'       Updated "SectionNames" to use List(Of String) instead of ArrayList
'
'*******************************************************************************************************

Imports System.Text
Imports TVA.Common

Namespace Interop

    ''' <summary>Old style Windows INI file manipulation class</summary>
    Public Class IniFile

        Private Declare Ansi Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" ( _
            ByVal lpAppName As String, _
            ByVal lpKeyName As String, _
            ByVal lpDefault As String, _
            ByVal lpReturnedString As StringBuilder, _
            ByVal nSize As Integer, _
            ByVal lpFileName As String) _
        As Integer

        Private Declare Ansi Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" ( _
            ByVal lpAppName As String, _
            ByVal lpKeyName As String, _
            ByVal lpString As String, _
            ByVal lpFileName As String) _
        As Integer

        Private Declare Ansi Function GetPrivateProfileSectionNames Lib "kernel32" Alias "GetPrivateProfileSectionNamesA" ( _
            ByVal lpszReturnBuffer As Byte(), _
            ByVal nSize As Integer, _
            ByVal lpFileName As String) _
        As Integer

        Private Const BufferSize As Integer = 4096

        Private m_fileName As String

        ''' <summary>Creates a new instance of IniFile class</summary>
        ''' <remarks>Ini file name defaults to "Win.ini" - change using FileName property</remarks>
        Public Sub New()

            m_fileName = "Win.ini"

        End Sub

        ''' <summary>Creates a new instance of IniFile class using the specified INI file name</summary>
        Public Sub New(ByVal fileName As String)

            m_fileName = fileName

        End Sub

        ''' <summary>File name of the INI file</summary>
        Public Property FileName() As String
            Get
                Return m_fileName
            End Get
            Set(ByVal value As String)
                m_fileName = value
            End Set
        End Property

        ''' <summary>Gets the value of the specified key</summary>
        ''' <param name="section">Section key exists in</param>
        ''' <param name="entry">Name of key</param>
        ''' <param name="defaultValue">Default value of key</param>
        ''' <returns>Value of key</returns>
        ''' <remarks>This is the default member of this class</remarks>
        Default Public ReadOnly Property KeyValue(ByVal section As String, ByVal entry As String, ByVal defaultValue As String) As String
            Get
                Dim buffer As New StringBuilder(BufferSize)
                Dim commentIndex As Integer
                Dim value As String

                If defaultValue Is Nothing Then defaultValue = ""
                GetPrivateProfileString(section, entry, defaultValue, buffer, BufferSize, m_fileName)

                ' Remove any trailing comments from key value
                value = buffer.ToString.Trim()
                commentIndex = value.IndexOf(";"c)
                If commentIndex > -1 Then value = value.Substring(0, commentIndex).Trim()

                Return value
            End Get
        End Property

        ''' <summary>Sets the value of the specified key</summary>
        ''' <param name="section">Section key exists in</param>
        ''' <param name="entry">Name of key</param>
        ''' <value>The new key value to store in the INI file</value>
        ''' <remarks>This is the default member of this class</remarks>
        Default Public WriteOnly Property KeyValue(ByVal section As String, ByVal entry As String) As String
            Set(ByVal value As String)
                WritePrivateProfileString(section, entry, value, m_fileName)
            End Set
        End Property

        ''' <summary>Returns a string array of section names in the INI file</summary>
        Public ReadOnly Property SectionNames() As String()
            Get
                Const BufferSize As Integer = 32768
                Dim sections As New List(Of String)
                Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
                Dim readLength, nullIndex, startIndex As Integer

                readLength = GetPrivateProfileSectionNames(buffer, BufferSize, m_fileName)

                If readLength > 0 Then
                    Do While startIndex < readLength
                        nullIndex = Array.IndexOf(buffer, Convert.ToByte(0), startIndex)

                        If nullIndex > -1 Then
                            If buffer(startIndex) > 0 Then sections.Add(Encoding.Default.GetString(buffer, startIndex, nullIndex - startIndex).Trim())
                            startIndex = nullIndex + 1
                        Else
                            Exit Do
                        End If
                    Loop
                End If

                Return sections.ToArray()
            End Get
        End Property

    End Class

End Namespace
