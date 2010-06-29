'*******************************************************************************************************
'  DataMigrationUtility.vb - Gbtc
'
'  Tennessee Valley Authority, 2010
'  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
'
'  This software is made freely available under the TVA Open Source Agreement (see below).
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/28/2010 - J. Ritchie Carroll
'       Generated original version of source code.
'
'*******************************************************************************************************

#Region " TVA Open Source Agreement "

' THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
' MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
' TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
' ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
' DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
' MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
' ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

' Original Software Designation: openPDC
' Original Software Title: The TVA Open Source Phasor Data Concentrator
' User Registration Requested. Please Visit https://naspi.tva.com/Registration/
' Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

' 1. DEFINITIONS

' A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
' that makes a Modification.

' B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
' the use or sale of its Modification alone or when combined with the Subject Software.

' C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
' image, or any other device.

' D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
' another.

' E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
' software separate from the Subject Software that is not governed by the terms of this Agreement.

' F. "Modification" means any alteration of, including addition to or deletion from, the substance or
' structure of either the Original Software or Subject Software, and includes derivative works, as that
' term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
' as part of a Larger Work does not in and of itself constitute a Modification.

' G. "Original Software" means the computer software first released under this Agreement by Government
' Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

' H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
' Contributors.

' I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

' J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

' K. "Sale" means the exchange of the Subject Software for money or equivalent value.

' L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

' M. "Use" means the application or employment of the Subject Software for any purpose.

' 2. GRANT OF RIGHTS

' A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
' with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
' non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
' the Subject Software:

' 1. Use

' 2. Distribution

' 3. Reproduction

' 4. Modification

' 5. Redistribution

' 6. Display

' B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
' respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
' Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
' pertaining to the Subject Software:

' 1. Use

' 2. Distribution

' 3. Reproduction

' 4. Sale

' 5. Offer for Sale

' C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
' and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
' such Modification causes the combination to be covered by the Covered Patents. It does not apply to
' any other combinations that include a Modification. 

' D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
' Such sublicense must be under the same terms and conditions of this Agreement.

' 3. OBLIGATIONS OF RECIPIENT

' A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
' additions covered under paragraph 3H. 

' 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
' must be included with each copy of the Subject Software; and

' 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
' Recipient must also make the source code freely available, and must provide with each copy of the
' Subject Software information on how to obtain the source code in a reasonable manner on or through a
' medium customarily used for software exchange.

' B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
' Software:

'          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

' C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
' must identify itself as the originator of its Modification in a manner that reasonably allows
' subsequent Recipients to identify the originator of the Modification. In fulfillment of these
' requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
' made and the date of the alterations, identifies Contributor as originator of the alterations, and
' consents to characterization of the alterations as a Modification, for example, by including a
' statement that the Modification is derived, directly or indirectly, from Original Software provided by
' Government Agency. Once consent is granted, it may not thereafter be revoked.

' D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
' been added to the Subject Software, a Recipient may not remove it without the express permission of
' the Contributor who added the notice.

' E. A Recipient may not make any representation in the Subject Software or in any promotional,
' advertising or other material that may be construed as an endorsement by Government Agency or by any
' prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
' advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

' F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
' upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
' following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
' shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
' requested that the Recipient inform Government Agency at the web site provided above how to access the
' Modification.

' G. Each Contributor represents that that its Modification does not violate any existing agreements,
' regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
' conveyed by this Agreement.

' H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
' liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
' however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
' Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
' obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
' Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
' indemnity and/or liability offered by such Recipient.

' I. A Recipient may create a Larger Work by combining Subject Software with separate software not
' governed by the terms of this agreement and distribute the Larger Work as a single product. In such
' case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
' is subject to this Agreement.

' J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
' any goods or technical data from the United States may require some form of export license from the
' U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
' U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
' required, it shall be issued. Nothing granted herein provides any such export license.

' 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

' A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
' EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
' SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
' PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
' FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
' AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
' RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
' RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
' LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
' "AS IS."

' B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
' AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
' OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
' SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
' SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
' EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
' LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
' EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
' GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
' IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

' 5. GENERAL TERMS

' A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
' Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
' thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
' immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
' Software properly granted by the breaching Recipient shall survive any such termination of this
' Agreement.

' B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
' it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

' C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
' including, but not limited to, determining the validity of this Agreement, the meaning of its
' provisions and the rights, obligations and remedies of the parties.

' D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
' parties relating to release of the Subject Software and may not be superseded, modified or amended
' except by further written agreement duly executed by the parties.

' E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
' affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
' Recipient hereby agrees to all terms and conditions herein.

' F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
' representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

#End Region

Option Strict Off
Option Explicit On

Imports System.Data.OleDb
Imports TVA
Imports TVA.Configuration
Imports TVA.Reflection
Imports TVA.Reflection.AssemblyInfo
Imports TVA.Database
Imports TVA.Units
Imports TVA.Windows
Imports TVA.Windows.Forms
Imports VB = Microsoft.VisualBasic

Friend Class DataMigrationUtility

    Inherits System.Windows.Forms.Form

    Friend AppName As String = AssemblyInfo.ExecutingAssembly.Name

    Private m_applicationSettings As ApplicationSettings

#Region "Windows Form Designer generated code "
    Public Sub New()
        MyBase.New()
        'This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub
    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal Disposing As Boolean)
        If Disposing Then
            If Not components Is Nothing Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(Disposing)
    End Sub
    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Public WithEvents ProgressLabel As System.Windows.Forms.Label
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    Friend WithEvents ProgressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents OverallProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents ToConnectString As System.Windows.Forms.TextBox
    Friend WithEvents FromConnectString As System.Windows.Forms.TextBox
    Friend WithEvents Cancel As System.Windows.Forms.Button
    Friend WithEvents Import As System.Windows.Forms.Button
    Friend WithEvents Version As System.Windows.Forms.Label
    Friend WithEvents GroupBox As System.Windows.Forms.GroupBox
    Friend WithEvents UseFromForRI As System.Windows.Forms.RadioButton
    Friend WithEvents UseToForRI As System.Windows.Forms.RadioButton
    Friend WithEvents WarningLabel As System.Windows.Forms.Label
    Friend WithEvents ToConnectStringLabel As System.Windows.Forms.Label
    Friend WithEvents FromConnectStringLabel As System.Windows.Forms.Label
    Friend WithEvents WarningLabelBold As System.Windows.Forms.Label
    Friend WithEvents LinkFromTest As System.Windows.Forms.LinkLabel
    Friend WithEvents LinkToTest As System.Windows.Forms.LinkLabel
    Friend WithEvents Messages As System.Windows.Forms.TextBox
    Friend WithEvents FromSchema As TVA.Database.Schema
    Friend WithEvents ToSchema As TVA.Database.Schema
    Friend WithEvents DataInserter As TVA.Database.DataInserter
    Friend WithEvents FromDataType As System.Windows.Forms.ComboBox
    Friend WithEvents FromDataTypeLabel As System.Windows.Forms.Label
    Friend WithEvents ToDataType As System.Windows.Forms.ComboBox
    Friend WithEvents ToDataTypeLabel As System.Windows.Forms.Label
    Friend WithEvents ExampleConnectionStringLinkLabel As System.Windows.Forms.LinkLabel
    Friend WithEvents ExcludedTablesTextBox As System.Windows.Forms.TextBox
    Friend WithEvents ExcludeTablesLabel As System.Windows.Forms.Label
    Friend WithEvents CommaSeparateValuesLabel As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DataMigrationUtility))
        Me.ProgressLabel = New System.Windows.Forms.Label
        Me.ProgressBar = New System.Windows.Forms.ProgressBar
        Me.OverallProgress = New System.Windows.Forms.ProgressBar
        Me.Messages = New System.Windows.Forms.TextBox
        Me.GroupBox = New System.Windows.Forms.GroupBox
        Me.ExampleConnectionStringLinkLabel = New System.Windows.Forms.LinkLabel
        Me.ToConnectString = New System.Windows.Forms.TextBox
        Me.ToDataType = New System.Windows.Forms.ComboBox
        Me.ToDataTypeLabel = New System.Windows.Forms.Label
        Me.FromConnectString = New System.Windows.Forms.TextBox
        Me.FromDataType = New System.Windows.Forms.ComboBox
        Me.FromDataTypeLabel = New System.Windows.Forms.Label
        Me.LinkToTest = New System.Windows.Forms.LinkLabel
        Me.LinkFromTest = New System.Windows.Forms.LinkLabel
        Me.WarningLabelBold = New System.Windows.Forms.Label
        Me.WarningLabel = New System.Windows.Forms.Label
        Me.UseToForRI = New System.Windows.Forms.RadioButton
        Me.UseFromForRI = New System.Windows.Forms.RadioButton
        Me.Cancel = New System.Windows.Forms.Button
        Me.Import = New System.Windows.Forms.Button
        Me.Version = New System.Windows.Forms.Label
        Me.ToConnectStringLabel = New System.Windows.Forms.Label
        Me.FromConnectStringLabel = New System.Windows.Forms.Label
        Me.ExcludedTablesTextBox = New System.Windows.Forms.TextBox
        Me.ExcludeTablesLabel = New System.Windows.Forms.Label
        Me.CommaSeparateValuesLabel = New System.Windows.Forms.Label
        Me.FromSchema = New TVA.Database.Schema
        Me.ToSchema = New TVA.Database.Schema
        Me.DataInserter = New TVA.Database.DataInserter
        Me.GroupBox.SuspendLayout()
        Me.SuspendLayout()
        '
        'ProgressLabel
        '
        Me.ProgressLabel.BackColor = System.Drawing.SystemColors.Control
        Me.ProgressLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.ProgressLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ProgressLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ProgressLabel.Location = New System.Drawing.Point(8, 240)
        Me.ProgressLabel.Name = "ProgressLabel"
        Me.ProgressLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ProgressLabel.Size = New System.Drawing.Size(601, 32)
        Me.ProgressLabel.TabIndex = 23
        Me.ProgressLabel.Text = "Progress:"
        '
        'ProgressBar
        '
        Me.ProgressBar.Location = New System.Drawing.Point(8, 272)
        Me.ProgressBar.Name = "ProgressBar"
        Me.ProgressBar.Size = New System.Drawing.Size(601, 24)
        Me.ProgressBar.TabIndex = 24
        '
        'OverallProgress
        '
        Me.OverallProgress.Location = New System.Drawing.Point(8, 304)
        Me.OverallProgress.Name = "OverallProgress"
        Me.OverallProgress.Size = New System.Drawing.Size(601, 24)
        Me.OverallProgress.TabIndex = 25
        '
        'Messages
        '
        Me.Messages.Location = New System.Drawing.Point(8, 336)
        Me.Messages.Multiline = True
        Me.Messages.Name = "Messages"
        Me.Messages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.Messages.Size = New System.Drawing.Size(601, 120)
        Me.Messages.TabIndex = 26
        Me.Messages.Text = "Messages:"
        '
        'GroupBox
        '
        Me.GroupBox.Controls.Add(Me.ExampleConnectionStringLinkLabel)
        Me.GroupBox.Controls.Add(Me.ToConnectString)
        Me.GroupBox.Controls.Add(Me.ToDataType)
        Me.GroupBox.Controls.Add(Me.ToDataTypeLabel)
        Me.GroupBox.Controls.Add(Me.FromConnectString)
        Me.GroupBox.Controls.Add(Me.FromDataType)
        Me.GroupBox.Controls.Add(Me.FromDataTypeLabel)
        Me.GroupBox.Controls.Add(Me.LinkToTest)
        Me.GroupBox.Controls.Add(Me.LinkFromTest)
        Me.GroupBox.Controls.Add(Me.WarningLabelBold)
        Me.GroupBox.Controls.Add(Me.WarningLabel)
        Me.GroupBox.Controls.Add(Me.UseToForRI)
        Me.GroupBox.Controls.Add(Me.UseFromForRI)
        Me.GroupBox.Controls.Add(Me.Cancel)
        Me.GroupBox.Controls.Add(Me.Import)
        Me.GroupBox.Controls.Add(Me.Version)
        Me.GroupBox.Controls.Add(Me.ToConnectStringLabel)
        Me.GroupBox.Controls.Add(Me.FromConnectStringLabel)
        Me.GroupBox.Location = New System.Drawing.Point(8, 0)
        Me.GroupBox.Name = "GroupBox"
        Me.GroupBox.Size = New System.Drawing.Size(601, 202)
        Me.GroupBox.TabIndex = 1
        Me.GroupBox.TabStop = False
        '
        'ExampleConnectionStringLinkLabel
        '
        Me.ExampleConnectionStringLinkLabel.AutoSize = True
        Me.ExampleConnectionStringLinkLabel.Location = New System.Drawing.Point(16, 181)
        Me.ExampleConnectionStringLinkLabel.Name = "ExampleConnectionStringLinkLabel"
        Me.ExampleConnectionStringLinkLabel.Size = New System.Drawing.Size(150, 14)
        Me.ExampleConnectionStringLinkLabel.TabIndex = 27
        Me.ExampleConnectionStringLinkLabel.TabStop = True
        Me.ExampleConnectionStringLinkLabel.Text = "Example Connection Strings..."
        '
        'ToConnectString
        '
        Me.ToConnectString.Location = New System.Drawing.Point(16, 122)
        Me.ToConnectString.Multiline = True
        Me.ToConnectString.Name = "ToConnectString"
        Me.ToConnectString.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.ToConnectString.Size = New System.Drawing.Size(450, 56)
        Me.ToConnectString.TabIndex = 10
        '
        'ToDataType
        '
        Me.ToDataType.FormattingEnabled = True
        Me.ToDataType.Location = New System.Drawing.Point(314, 96)
        Me.ToDataType.Name = "ToDataType"
        Me.ToDataType.Size = New System.Drawing.Size(124, 22)
        Me.ToDataType.TabIndex = 29
        '
        'ToDataTypeLabel
        '
        Me.ToDataTypeLabel.BackColor = System.Drawing.SystemColors.Control
        Me.ToDataTypeLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.ToDataTypeLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToDataTypeLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ToDataTypeLabel.Location = New System.Drawing.Point(249, 98)
        Me.ToDataTypeLabel.Name = "ToDataTypeLabel"
        Me.ToDataTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ToDataTypeLabel.Size = New System.Drawing.Size(62, 18)
        Me.ToDataTypeLabel.TabIndex = 30
        Me.ToDataTypeLabel.Text = "Data Type:"
        Me.ToDataTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'FromConnectString
        '
        Me.FromConnectString.Location = New System.Drawing.Point(16, 37)
        Me.FromConnectString.Multiline = True
        Me.FromConnectString.Name = "FromConnectString"
        Me.FromConnectString.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.FromConnectString.Size = New System.Drawing.Size(450, 56)
        Me.FromConnectString.TabIndex = 3
        '
        'FromDataType
        '
        Me.FromDataType.FormattingEnabled = True
        Me.FromDataType.Location = New System.Drawing.Point(314, 11)
        Me.FromDataType.Name = "FromDataType"
        Me.FromDataType.Size = New System.Drawing.Size(124, 22)
        Me.FromDataType.TabIndex = 27
        '
        'FromDataTypeLabel
        '
        Me.FromDataTypeLabel.BackColor = System.Drawing.SystemColors.Control
        Me.FromDataTypeLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.FromDataTypeLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FromDataTypeLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.FromDataTypeLabel.Location = New System.Drawing.Point(249, 13)
        Me.FromDataTypeLabel.Name = "FromDataTypeLabel"
        Me.FromDataTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.FromDataTypeLabel.Size = New System.Drawing.Size(62, 18)
        Me.FromDataTypeLabel.TabIndex = 28
        Me.FromDataTypeLabel.Text = "Data Type:"
        Me.FromDataTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LinkToTest
        '
        Me.LinkToTest.Location = New System.Drawing.Point(441, 99)
        Me.LinkToTest.Name = "LinkToTest"
        Me.LinkToTest.Size = New System.Drawing.Size(32, 16)
        Me.LinkToTest.TabIndex = 9
        Me.LinkToTest.TabStop = True
        Me.LinkToTest.Text = "Test"
        '
        'LinkFromTest
        '
        Me.LinkFromTest.Location = New System.Drawing.Point(441, 14)
        Me.LinkFromTest.Name = "LinkFromTest"
        Me.LinkFromTest.Size = New System.Drawing.Size(32, 16)
        Me.LinkFromTest.TabIndex = 5
        Me.LinkFromTest.TabStop = True
        Me.LinkFromTest.Text = "Test"
        '
        'WarningLabelBold
        '
        Me.WarningLabelBold.AutoSize = True
        Me.WarningLabelBold.Font = New System.Drawing.Font("Arial", 7.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.WarningLabelBold.ForeColor = System.Drawing.Color.DarkRed
        Me.WarningLabelBold.Location = New System.Drawing.Point(477, 104)
        Me.WarningLabelBold.Name = "WarningLabelBold"
        Me.WarningLabelBold.Size = New System.Drawing.Size(55, 12)
        Me.WarningLabelBold.TabIndex = 16
        Me.WarningLabelBold.Text = "WARNING:"
        '
        'WarningLabel
        '
        Me.WarningLabel.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.WarningLabel.Location = New System.Drawing.Point(477, 104)
        Me.WarningLabel.Name = "WarningLabel"
        Me.WarningLabel.Size = New System.Drawing.Size(120, 74)
        Me.WarningLabel.TabIndex = 17
        Me.WarningLabel.Text = "                  To maintain data integrity, ensure you have selected the correc" & _
            "t source and destination databases."
        '
        'UseToForRI
        '
        Me.UseToForRI.Checked = True
        Me.UseToForRI.Location = New System.Drawing.Point(176, 98)
        Me.UseToForRI.Name = "UseToForRI"
        Me.UseToForRI.Size = New System.Drawing.Size(80, 16)
        Me.UseToForRI.TabIndex = 7
        Me.UseToForRI.TabStop = True
        Me.UseToForRI.Text = "Use for RI"
        '
        'UseFromForRI
        '
        Me.UseFromForRI.Location = New System.Drawing.Point(176, 13)
        Me.UseFromForRI.Name = "UseFromForRI"
        Me.UseFromForRI.Size = New System.Drawing.Size(80, 17)
        Me.UseFromForRI.TabIndex = 3
        Me.UseFromForRI.TabStop = True
        Me.UseFromForRI.Text = "Use for RI"
        '
        'Cancel
        '
        Me.Cancel.BackColor = System.Drawing.SystemColors.Control
        Me.Cancel.Cursor = System.Windows.Forms.Cursors.Default
        Me.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Cancel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Cancel.Location = New System.Drawing.Point(480, 48)
        Me.Cancel.Name = "Cancel"
        Me.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Cancel.Size = New System.Drawing.Size(105, 25)
        Me.Cancel.TabIndex = 21
        Me.Cancel.Text = "E&xit"
        Me.Cancel.UseVisualStyleBackColor = False
        '
        'Import
        '
        Me.Import.BackColor = System.Drawing.SystemColors.Control
        Me.Import.Cursor = System.Windows.Forms.Cursors.Default
        Me.Import.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Import.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Import.Location = New System.Drawing.Point(481, 17)
        Me.Import.Name = "Import"
        Me.Import.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Import.Size = New System.Drawing.Size(105, 25)
        Me.Import.TabIndex = 20
        Me.Import.Text = "&Migrate"
        Me.Import.UseVisualStyleBackColor = False
        '
        'Version
        '
        Me.Version.BackColor = System.Drawing.SystemColors.Control
        Me.Version.Cursor = System.Windows.Forms.Cursors.Default
        Me.Version.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Version.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Version.Location = New System.Drawing.Point(472, 78)
        Me.Version.Name = "Version"
        Me.Version.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Version.Size = New System.Drawing.Size(123, 13)
        Me.Version.TabIndex = 22
        Me.Version.Text = "Version: x.x.x"
        Me.Version.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'ToConnectStringLabel
        '
        Me.ToConnectStringLabel.BackColor = System.Drawing.SystemColors.Control
        Me.ToConnectStringLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.ToConnectStringLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToConnectStringLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ToConnectStringLabel.Location = New System.Drawing.Point(16, 99)
        Me.ToConnectStringLabel.Name = "ToConnectStringLabel"
        Me.ToConnectStringLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ToConnectStringLabel.Size = New System.Drawing.Size(168, 17)
        Me.ToConnectStringLabel.TabIndex = 6
        Me.ToConnectStringLabel.Text = "&To Connect String (OLEDB):"
        Me.ToConnectStringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'FromConnectStringLabel
        '
        Me.FromConnectStringLabel.BackColor = System.Drawing.SystemColors.Control
        Me.FromConnectStringLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.FromConnectStringLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FromConnectStringLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.FromConnectStringLabel.Location = New System.Drawing.Point(16, 14)
        Me.FromConnectStringLabel.Name = "FromConnectStringLabel"
        Me.FromConnectStringLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.FromConnectStringLabel.Size = New System.Drawing.Size(176, 17)
        Me.FromConnectStringLabel.TabIndex = 2
        Me.FromConnectStringLabel.Text = "&From Connect String (OLEDB):"
        Me.FromConnectStringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ExcludedTablesTextBox
        '
        Me.ExcludedTablesTextBox.Location = New System.Drawing.Point(94, 208)
        Me.ExcludedTablesTextBox.Name = "ExcludedTablesTextBox"
        Me.ExcludedTablesTextBox.Size = New System.Drawing.Size(515, 20)
        Me.ExcludedTablesTextBox.TabIndex = 27
        Me.ExcludedTablesTextBox.Text = "Runtime"
        '
        'ExcludeTablesLabel
        '
        Me.ExcludeTablesLabel.BackColor = System.Drawing.SystemColors.Control
        Me.ExcludeTablesLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.ExcludeTablesLabel.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ExcludeTablesLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ExcludeTablesLabel.Location = New System.Drawing.Point(8, 209)
        Me.ExcludeTablesLabel.Name = "ExcludeTablesLabel"
        Me.ExcludeTablesLabel.Size = New System.Drawing.Size(83, 18)
        Me.ExcludeTablesLabel.TabIndex = 29
        Me.ExcludeTablesLabel.Text = "Exclude Tables:"
        Me.ExcludeTablesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'CommaSeparateValuesLabel
        '
        Me.CommaSeparateValuesLabel.BackColor = System.Drawing.SystemColors.Control
        Me.CommaSeparateValuesLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me.CommaSeparateValuesLabel.Font = New System.Drawing.Font("Arial", 7.0!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CommaSeparateValuesLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.CommaSeparateValuesLabel.Location = New System.Drawing.Point(91, 224)
        Me.CommaSeparateValuesLabel.Name = "CommaSeparateValuesLabel"
        Me.CommaSeparateValuesLabel.Size = New System.Drawing.Size(518, 18)
        Me.CommaSeparateValuesLabel.TabIndex = 30
        Me.CommaSeparateValuesLabel.Text = "Comma separate table names."
        Me.CommaSeparateValuesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'FromSchema
        '
        Me.FromSchema.ConnectString = ""
        Me.FromSchema.DataSourceType = TVA.Database.DatabaseType.Unspecified
        Me.FromSchema.ImmediateClose = False
        Me.FromSchema.TableTypeRestriction = TVA.Database.TableType.Table
        '
        'ToSchema
        '
        Me.ToSchema.ConnectString = ""
        Me.ToSchema.DataSourceType = TVA.Database.DatabaseType.Unspecified
        Me.ToSchema.ImmediateClose = False
        Me.ToSchema.TableTypeRestriction = TVA.Database.TableType.Table
        '
        'DataInserter
        '
        Me.DataInserter.BulkInsertFilePath = ""
        Me.DataInserter.BulkInsertSettings = "FIELDTERMINATOR = '\t', ROWTERMINATOR = '\n', CODEPAGE = 'OEM', FIRE_TRIGGERS, KE" & _
            "EPNULLS"
        Me.DataInserter.DelimeterReplacement = " - "
        Me.DataInserter.FromSchema = Me.FromSchema
        Me.DataInserter.RowReportInterval = 500
        Me.DataInserter.ToSchema = Me.ToSchema
        '
        'DataMigrationUtility
        '
        Me.AcceptButton = Me.Import
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.CancelButton = Me.Cancel
        Me.ClientSize = New System.Drawing.Size(617, 468)
        Me.Controls.Add(Me.ExcludedTablesTextBox)
        Me.Controls.Add(Me.CommaSeparateValuesLabel)
        Me.Controls.Add(Me.ExcludeTablesLabel)
        Me.Controls.Add(Me.GroupBox)
        Me.Controls.Add(Me.Messages)
        Me.Controls.Add(Me.OverallProgress)
        Me.Controls.Add(Me.ProgressBar)
        Me.Controls.Add(Me.ProgressLabel)
        Me.Font = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(140, 145)
        Me.MaximizeBox = False
        Me.Name = "DataMigrationUtility"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Data Migration Utility"
        Me.GroupBox.ResumeLayout(False)
        Me.GroupBox.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region

    Private Sub DataImporter_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load

        m_applicationSettings = New ApplicationSettings

        With EntryAssembly.Version
            Version.Text = "Version " & .Major & "." & .Minor & "." & .Build
        End With

        ' Load database types from enumeration values
        For Each databaseType As String In [Enum].GetNames(GetType(DatabaseType))
            ToDataType.Items.Add(databaseType)
            FromDataType.Items.Add(databaseType)
        Next

        ' Restore last settings
        FromDataType.SelectedIndex = m_applicationSettings.FromDataType
        ToDataType.SelectedIndex = m_applicationSettings.ToDataType
        FromConnectString.Text = m_applicationSettings.FromConnectionString
        ToConnectString.Text = m_applicationSettings.ToConnectionString

        If m_applicationSettings.UseFromConnectionForRI Then
            UseFromForRI.Checked = True
        Else
            UseToForRI.Checked = True
        End If

        ' Restore last window location
        Me.RestoreLocation()

        Show()
        MessageBox.Show(Me, "IMPORTANT: Always backup database before any mass database update and remember to select the proper data source to use for referential integrity BEFORE you begin your data operation!", AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        BringToFront()

    End Sub

    Private Sub DataImporter_Closed(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Closed

        Me.Hide()
        Application.DoEvents()

        Try
            m_applicationSettings.FromDataType = FromDataType.SelectedIndex
            m_applicationSettings.ToDataType = ToDataType.SelectedIndex
            m_applicationSettings.FromConnectionString = FromConnectString.Text
            m_applicationSettings.ToConnectionString = ToConnectString.Text
            m_applicationSettings.UseFromConnectionForRI = UseFromForRI.Checked

            ' Save application settings
            m_applicationSettings.Save()

            ' Save current window location (size is fixed)
            Me.SaveLocation()
        Catch
            ' Don't want any possible failures during this event to prevent shudown :)
        End Try

        End

    End Sub

    Private Sub Cancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Cancel.Click

        Me.Close()

    End Sub

    Private Sub Import_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Import.Click

        ' Validate inputs
        If Len(FromConnectString.Text) = 0 Then
            MsgBox("Cannot perform migration until source DSN is selected.", MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical, AppName)
            FromConnectString.Focus()
            Exit Sub
        End If

        If Len(ToConnectString.Text) = 0 Then
            MsgBox("Cannot perform migration until destination DSN is selected.", MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical, AppName)
            ToConnectString.Focus()
            Exit Sub
        End If

        If FromConnectString.Text.Replace(" ", "").ToLower() = ToConnectString.Text.Replace(" ", "").ToLower() Then
            MsgBox("Cannot perform migration when source and destination connection strings are identical.", MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical, AppName)
            ToConnectString.Focus()
            Exit Sub
        End If

        Try

            Cursor.Current = Cursors.WaitCursor

            ' Don't allow user to inadvertently click the import button again
            Import.Enabled = False

            UpdateProgress("Analyzing data structures, please wait...")

            With FromSchema
                .ConnectString = FromConnectString.Text
                .DataSourceType = FromDataType.SelectedIndex ' IIf(FromIsSQLServer.Checked, DatabaseType.SqlServer, DatabaseType.Unspecified)
            End With

            With ToSchema
                .ConnectString = ToConnectString.Text
                .DataSourceType = ToDataType.SelectedIndex ' IIf(ToIsSQLServer.Checked, DatabaseType.SqlServer, DatabaseType.Unspecified)
            End With

            With DataInserter
                .UseFromSchemaReferentialIntegrity = UseFromForRI.Checked

                If Not String.IsNullOrEmpty(ExcludedTablesTextBox.Text) Then
                    .ExcludedTables.AddRange(ExcludedTablesTextBox.Text.Split(","c))
                End If

                .Execute()
            End With
        Catch ex As Exception
            UpdateProgress("Exception - " & ex.Message)
            MsgBox("Exception occured during insert: " & ex.Message, MsgBoxStyle.OkOnly + MsgBoxStyle.Critical, "Data Insert Error")
        Finally
            Import.Enabled = True
            Cursor.Current = Cursors.Default
        End Try

    End Sub

    Private Sub UpdateProgress(ByVal LabelText As String)

        ProgressLabel.Text = "Progress: " & LabelText
        ProgressLabel.Refresh()

    End Sub

    Private Sub AddMessage(ByVal MessageText As String)

        Messages.Text &= vbCrLf & vbCrLf & MessageText
        Messages.SelectionStart = Len(Messages.Text)
        Messages.ScrollToCaret()

    End Sub

    Private Sub AddSQLErrorMessage(ByVal SQL As String, ByVal ErrorText As String)

        AddMessage(ErrorText & vbCrLf & "Caused by: " & SQL)

    End Sub

    Private Sub DataHandler_OverallProgress(ByVal Current As Integer, ByVal Total As Integer) Handles DataInserter.OverallProgress

        With OverallProgress
            If .Minimum <> 0 Then .Minimum = 0
            If .Maximum <> Total Then .Maximum = Total
            .Value = IIf(Current < Total, Current, Total)
        End With

        If Current = Total Then OverallProgress.Refresh()
        Application.DoEvents()

    End Sub

    Private Sub DataHandler_RowProgress(ByVal TableName As String, ByVal CurrentRow As Integer, ByVal TotalRows As Integer) Handles DataInserter.RowProgress

        With ProgressBar
            If .Minimum <> 0 Then .Minimum = 0
            If .Maximum <> TotalRows Then .Maximum = TotalRows
            .Value = IIf(CurrentRow < TotalRows, CurrentRow, TotalRows)
        End With

        If CurrentRow = TotalRows Then ProgressBar.Refresh()
        Application.DoEvents()

    End Sub

    Private Sub DataHandler_TableProgress(ByVal TableName As String, ByVal FoundInDest As Boolean, ByVal CurrentTable As Integer, ByVal TotalTables As Integer) Handles DataInserter.TableProgress

        If CurrentTable = TotalTables And Len(TableName) = 0 Then
            UpdateProgress("Processing complete. ( " & CurrentTable & " / " & TotalTables & " )")
        Else
            If FoundInDest Then
                System.Threading.Thread.Sleep(250)
                UpdateProgress("Processing """ & TableName & """... ( " & CurrentTable & " / " & TotalTables & " )")
            Else
                UpdateProgress("Skipped """ & TableName & """... ( " & CurrentTable & " / " & TotalTables & " )")
            End If
        End If

        Application.DoEvents()

    End Sub

    Private Sub DataInserter_TableCleared(ByVal TableName As String) Handles DataInserter.TableCleared

        UpdateProgress("Cleared data from """ & TableName & """...")

    End Sub

    Private Sub DataInserter_BulkInsertCompleted(ByVal TableName As String, ByVal TotalRows As Integer, ByVal TotalSeconds As Integer) Handles DataInserter.BulkInsertCompleted

        AddMessage("Bulk insert of " & TotalRows & " rows into table """ & TableName & """ completed in " & Ticks.FromSeconds(TotalSeconds).ToElapsedTimeString().ToLower())

    End Sub

    Private Sub DataInserter_BulkInsertException(ByVal TableName As String, ByVal SQL As String, ByVal ex As System.Exception) Handles DataInserter.BulkInsertException

        AddMessage("Exception occurred during bulk insert into table """ & TableName & """: " & ex.Message)
        AddMessage("    Bulk Insert SQL: " & SQL)

    End Sub

    Private Sub DataInserter_BulkInsertExecuting(ByVal TableName As String) Handles DataInserter.BulkInsertExecuting

        AddMessage("Executing bulk insert into table """ & TableName & """...")

    End Sub

    Private Sub DataInserter_SQLFailure(ByVal SQL As String, ByVal ex As System.Exception) Handles DataInserter.SqlFailure

        AddSQLErrorMessage(SQL, ex.Message)

    End Sub

    Private Sub LinkFromTest_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkFromTest.LinkClicked

        TestConnection(FromConnectString.Text, "From Connection Test Status")

    End Sub

    Private Sub LinkToTest_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkToTest.LinkClicked

        TestConnection(ToConnectString.Text, "To Connection Test Status")

    End Sub

    Private Sub ExampleConnectionStringLinkLabel_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles ExampleConnectionStringLinkLabel.LinkClicked

        Process.Start("http://www.connectionstrings.com/")

    End Sub

    Private Sub TestConnection(ByVal ConnectString As String, ByVal Title As String)

        Try
            Dim cnn As New OleDbConnection(ConnectString)
            cnn.Open()
            cnn.Close()
            MsgBox("Connection Succeeded!", MsgBoxStyle.OkOnly + MsgBoxStyle.Information, Title)
        Catch ex As Exception
            MsgBox("Connection Failed: " & ex.Message, MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, Title)
        End Try

    End Sub

    Private Sub DataType_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FromDataType.SelectedIndexChanged, ToDataType.SelectedIndexChanged

        Dim source As ComboBox = DirectCast(sender, ComboBox)
        Dim destination As TextBox
        Dim index As Integer = source.SelectedIndex

        If source.Name = "FromDataType" Then
            destination = FromConnectString
        Else
            destination = ToConnectString
        End If

        Select Case index
            Case DatabaseType.Access
                destination.Text = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=C:\Program Files\openPDC\openPDC.mdb"
            Case DatabaseType.SqlServer
                destination.Text = "Provider=SQLOLEDB; Data Source=localhost; Initial Catalog=openPDC; User Id=myUsername; Password=myPassword;"
            Case DatabaseType.MySQL
                destination.Text = "Provider=MySQLProv; Data Source=localhost; Initial Catalog=openPDC; User Id=myUsername; Password=myPassword;"
            Case DatabaseType.Oracle
                destination.Text = "Provider=msdaora; Data Source=openPDC; User Id=myUsername; Password=myPassword;"
            Case Else
                destination.Text = ""
        End Select

    End Sub

#Region "[ Old Code ]"

    'Imports TVA.Shared.Common
    'Imports TVA.Shared.DateTime

    'FromIsSQLServer.Checked = CBool(GetSetting(AppName, "General", "FromIsSQLServer", False))
    'ToIsSQLServer.Checked = CBool(GetSetting(AppName, "General", "ToIsSQLServer", False))
    'UseBulkInsert.Checked = CBool(GetSetting(AppName, "General", "UseBulkInsert", False))
    'UseTruncateTable.Checked = CBool(GetSetting(AppName, "General", "UseTruncateTable", False))
    'BulkInsertFilePath.Text = MakePathSlashSuffixed(GetSetting(AppName, "General", "BulkInsertFilePath", BulkInsertFilePath.Text))

    'SaveSetting(AppName, "General", "FromIsSQLServer", CStr(FromIsSQLServer.Checked))
    'SaveSetting(AppName, "General", "ToIsSQLServer", CStr(ToIsSQLServer.Checked))
    'SaveSetting(AppName, "General", "UseBulkInsert", CStr(UseBulkInsert.Checked))
    'SaveSetting(AppName, "General", "UseTruncateTable", CStr(UseTruncateTable.Checked))
    'SaveSetting(AppName, "General", "BulkInsertFilePath", BulkInsertFilePath.Text)

    'If Not ForInsert.Checked And Not ForUpdate.Checked And Not ForDelete.Checked Then
    '    MsgBox("You must select if this data migration is for ""Import"", ""Update"", or ""Delete""!", MsgBoxStyle.OkOnly, AppName)
    '    ForInsert.Focus()
    '    Exit Sub
    'End If

    'If ForInsert.Checked Then
    'ElseIf ForUpdate.Checked Then
    '    Try
    '        With DataUpdater
    '            .UseFromSchemaReferentialIntegrity = UseFromForRI.Checked
    '            .RowReportInterval = 5
    '            .Execute()
    '        End With
    '    Catch ex As Exception
    '        UpdateProgress("Exception - " & ex.Message)
    '        MsgBox("Exception occured during update: " & ex.Message, MsgBoxStyle.OkOnly + MsgBoxStyle.Critical, "Data Update Error")
    '    End Try
    'ElseIf ForDelete.Checked Then
    '    Try
    '        With DataDeleter
    '            .UseFromSchemaReferentialIntegrity = UseFromForRI.Checked
    '            .RowReportInterval = 5
    '            .Execute()
    '        End With
    '    Catch ex As Exception
    '        UpdateProgress("Exception - " & ex.Message)
    '        MsgBox("Exception occured during delete: " & ex.Message, MsgBoxStyle.OkOnly + MsgBoxStyle.Critical, "Data Delete Error")
    '    End Try
    'End If

    'Private Sub BulkInsertFilePath_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs)

    '    BulkInsertFilePath.Text = MakePathSlashSuffixed(BulkInsertFilePath.Text)

    'End Sub

    'Private Sub ImportType_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    '    Dim flgIsInsertType As Boolean = ForInsert.Checked
    '    Dim flgIsSQLServer As Boolean = ToIsSQLServer.Checked
    '    Dim flgUseBulkInsert As Boolean = UseBulkInsert.Checked
    '    Dim flgClearingTables As Boolean = ClearDestinationTables.Checked

    '    ClearDestinationTables.Enabled = flgIsInsertType
    '    UseBulkInsert.Enabled = (flgIsInsertType And flgIsSQLServer)
    '    BulkInsertFilePathLabel.Enabled = (flgIsInsertType And flgIsSQLServer And flgUseBulkInsert)
    '    BulkInsertFilePath.Enabled = (flgIsInsertType And flgIsSQLServer And flgUseBulkInsert)
    '    UseTruncateTable.Enabled = (flgIsInsertType And flgIsSQLServer And flgClearingTables)

    'End Sub

    'Private Sub ClearDestinationTables_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    '    If ClearDestinationTables.Checked Then
    '        MsgBox("Warning: This will delete all of the records from tables in the destination connection where records exist in the source connection before inserting any new records!" & vbCrLf & "Please verify this is the desired action...", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Database Update Warning")
    '    End If

    '    ImportType_CheckedChanged(sender, e)

    'End Sub

    'Private Sub UseBulkInsert_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    '    If UseBulkInsert.Checked Then
    '        MsgBox("Warning: Using Bulk Inserts will dramtically increase insert speed, however the system will not be able to perform automatic auto-inc value translations for foreign key fields when using this feature.  Also note that you must have sufficient table rights to perform this operation.", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Database Update Warning")
    '    End If

    '    ImportType_CheckedChanged(sender, e)

    'End Sub

    'Private Sub UseTruncateTable_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    '    If UseTruncateTable.Checked Then
    '        MsgBox("Warning: Using Truncate Table will dramtically increase the speed at which tables are cleared of data, however it will not be used on a table referenced by foreign key constraints.  Also note that you must have sufficient table rights to perform this operation.", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Database Update Warning")
    '    End If

    'End Sub

    'Private Function MakePathSlashSuffixed(ByVal FilePath As String) As String

    '    FilePath = Trim(FilePath)
    '    If VB.Right(FilePath, 1) <> "\" Then FilePath &= "\"
    '    Return FilePath

    'End Function

#End Region

End Class