'*******************************************************************************************************
'  Tva.Interop.Windows.IniFile.vb - Old style Windows INI file manipulation class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'  01/05/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Interop.Windows.IniFile)
'
'*******************************************************************************************************

Imports System.Text

Namespace Interop.Windows

    ''' <summary>
    ''' <para>Old style Windows INI file manipulation class</para>
    ''' </summary>
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

        Private Const BufferSize As Integer = 4096

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

        ''' <summary>
        ''' Gets the value of the specified key
        ''' </summary>
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
                GetPrivateProfileString(section, entry, defaultValue, buffer, BufferSize, m_iniFileName)

                ' Remove any trailing comments from key value
                value = buffer.ToString.Trim()
                commentIndex = value.IndexOf(";"c)
                If commentIndex > -1 Then value = value.Substring(0, commentIndex).Trim()

                Return value
            End Get
        End Property

        ''' <summary>
        ''' Sets the value of the specified key
        ''' </summary>
        ''' <param name="section">Section key exists in</param>
        ''' <param name="entry">Name of key</param>
        ''' <value>The new key value to store in the INI file</value>
        ''' <remarks>This is the default member of this class</remarks>
        Default Public WriteOnly Property KeyValue(ByVal section As String, ByVal entry As String) As String
            Set(ByVal value As String)
                WritePrivateProfileString(section, entry, value, m_iniFileName)
            End Set
        End Property

        ''' <summary>
        ''' Returns a string array of section names in the INI file
        ''' </summary>
        Public ReadOnly Property SectionNames() As String()
            Get
                Const BufferSize As Integer = 32768
                Dim sections As New ArrayList
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
                Dim readLength, nullIndex, startIndex As Integer

                readLength = GetPrivateProfileSectionNames(buffer, BufferSize, m_iniFileName)

                If readLength > 0 Then
                    Do While startIndex < readLength
                        nullIndex = Array.IndexOf(buffer, System.Convert.ToByte(0), startIndex)

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
