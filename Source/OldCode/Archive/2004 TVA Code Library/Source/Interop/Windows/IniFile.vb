'***********************************************************************
'  IniFile.vb - Older Windows Versions Interoperability Classes
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Text

Namespace Interop.Windows

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

        Private Declare Ansi Function GetPrivateProfileSectionNames Lib "kernel32.DLL" Alias "GetPrivateProfileSectionNamesA" ( _
            ByVal lpszReturnBuffer As Byte(), _
            ByVal nSize As Integer, _
            ByVal lpFileName As String) _
        As Integer

        Private m_iniFileName As String

        Public Sub New()

            m_iniFileName = "Win.ini"

        End Sub

        Public Sub New(ByVal iniFileName As String)

            m_iniFileName = iniFileName

        End Sub

        Public Property IniFileName() As String
            Get
                Return m_iniFileName
            End Get
            Set(ByVal Value As String)
                m_iniFileName = Value
            End Set
        End Property

        Default Public Property KeyValue(ByVal section As String, ByVal entry As String, Optional ByVal defaultValue As String = "") As String
            Get
                Const BufferSize As Integer = 4096
                Dim buffer As New StringBuilder(BufferSize)
                Dim commentIndex As Integer
                Dim value As String

                GetPrivateProfileString(section, entry, defaultValue, buffer, BufferSize, m_iniFileName)

                ' Remove any trailing comments from key value
                value = buffer.ToString.Trim()
                commentIndex = value.IndexOf(";"c)
                If commentIndex > -1 Then value = value.Substring(0, commentIndex).Trim()

                Return value
            End Get
            Set(ByVal value As String)
                WritePrivateProfileString(section, entry, value, m_iniFileName)
            End Set
        End Property

        Public ReadOnly Property SectionNames() As String()
            Get
                Const BufferSize As Integer = 32768
                Dim sections As New ArrayList
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
                Dim readLength, nullIndex, startIndex As Integer

                readLength = GetPrivateProfileSectionNames(buffer, BufferSize, m_iniFileName)

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

                Return sections.ToArray(GetType(String))
            End Get
        End Property

    End Class

End Namespace
