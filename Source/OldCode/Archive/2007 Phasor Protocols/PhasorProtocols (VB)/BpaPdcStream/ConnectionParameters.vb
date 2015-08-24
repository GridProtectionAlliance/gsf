'*******************************************************************************************************
'  ConnectionParameters.vb - BPA PDCstream specific connection parameters
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/26/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.ComponentModel
Imports System.Runtime.Serialization
Imports System.IO

Namespace BpaPdcStream

#Region " INI File Name Editor for Property Grid "

    Public Class IniFileNameEditor

        Inherits System.Windows.Forms.Design.FileNameEditor

        Protected Overrides Sub InitializeDialog(ByVal openFileDialog As System.Windows.Forms.OpenFileDialog)

            MyBase.InitializeDialog(openFileDialog)

            ' We override this function to customize file dialog...
            With openFileDialog
                .Title = "Load BPA PDCstream INI Configuration File"
                .Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*"
            End With

        End Sub

    End Class

#End Region

    <Serializable()> _
    Public Class ConnectionParameters

        Inherits ConnectionParametersBase

        Private m_configurationFileName As String
        Private m_refreshConfigurationFileOnChange As Boolean
        Private m_parseWordCountFromByte As Boolean

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            ' Deserialize connection parameters
            m_configurationFileName = info.GetString("configurationFileName")
            m_refreshConfigurationFileOnChange = info.GetBoolean("refreshConfigurationFileOnChange")
            m_parseWordCountFromByte = info.GetBoolean("parseWordCountFromByte")

        End Sub

        Public Sub New()

            m_refreshConfigurationFileOnChange = True

        End Sub

        <Category("Required Connection Parameters"), Description("Defines required external BPA PDCstream INI based configuration file."), _
         Editor(GetType(IniFileNameEditor), GetType(System.Drawing.Design.UITypeEditor))> _
        Public Property ConfigurationFileName() As String
            Get
                Return m_configurationFileName
            End Get
            Set(ByVal value As String)
                m_configurationFileName = value
            End Set
        End Property

        <Category("Required Connection Parameters"), Description("Set to True to interpret word count in packet header from a byte instead of a word - if the sync byte (0xAA) is at position one, then the word count would be interpreted from byte four.  Some older BPA PDC stream implementations have a 0x01 in byte three where there should be a 0x00 and this throws off the frame length, setting this property to True will correctly interpret the word count."), DefaultValue(False)> _
        Public Property ParseWordCountFromByte() As Boolean
            Get
                Return m_parseWordCountFromByte
            End Get
            Set(ByVal value As Boolean)
                m_parseWordCountFromByte = value
            End Set
        End Property

        <Category("Optional Connection Parameters"), Description("Set to True to automatically reload configuration file when it has changed on disk."), DefaultValue(True)> _
        Public Property RefreshConfigurationFileOnChange() As Boolean
            Get
                Return m_refreshConfigurationFileOnChange
            End Get
            Set(ByVal value As Boolean)
                m_refreshConfigurationFileOnChange = value
            End Set
        End Property

        Public Overrides ReadOnly Property ValuesAreValid() As Boolean
            Get
                Return File.Exists(m_configurationFileName)
            End Get
        End Property

        Public Overrides Sub GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            ' Serialize connection parameters
            info.AddValue("configurationFileName", m_configurationFileName)
            info.AddValue("refreshConfigurationFileOnChange", m_refreshConfigurationFileOnChange)
            info.AddValue("parseWordCountFromByte", m_parseWordCountFromByte)

        End Sub

    End Class

End Namespace
