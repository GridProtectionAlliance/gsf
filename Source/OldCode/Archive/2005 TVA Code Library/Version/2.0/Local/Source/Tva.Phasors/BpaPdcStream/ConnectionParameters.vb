'*******************************************************************************************************
'  ConnectionParameters.vb - BPA PDCstream specific connection parameters
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
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
        Private m_reloadConfigurationFrameOnChange As Boolean
        Private m_refreshConfigurationFileOnChange As Boolean

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            ' Deserialize connection parameters
            m_configurationFileName = info.GetString("configurationFileName")

        End Sub

        Public Sub New()

            m_reloadConfigurationFrameOnChange = True

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

        <Category("Optional Connection Parameters"), Description("Set to False to always use latest configuration frame parsed from stream, set to True to only use configuration frame from stream if it has changed. BPA PDCstream configuration frame arrives in stream every minute and is typically the same so this property defaults to True so that the configuration frame parsed from stream is only updated if it has changed."), DefaultValue(True)> _
        Public Property ReloadConfigurationFrameOnChange() As Boolean
            Get
                Return m_reloadConfigurationFrameOnChange
            End Get
            Set(ByVal value As Boolean)
                m_reloadConfigurationFrameOnChange = value
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

        End Sub

    End Class

End Namespace
