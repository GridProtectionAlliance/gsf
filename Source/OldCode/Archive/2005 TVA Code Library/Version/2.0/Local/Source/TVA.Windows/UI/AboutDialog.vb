'*******************************************************************************************************
'  TVA.Windows.UI.AboutDialog.vb - Standard TVA About Dialog
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/26/2006 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Windows.Forms
Imports System.Reflection.Assembly
Imports TVA.Assembly

Namespace UI

    Public Class AboutDialog

#Region " Private Declarations "

        Private m_url As String
        Private m_assemblies As List(Of Assembly)

#End Region
        
#Region " Public Methods "

        ''' <summary>
        ''' Initializes a default instance of the standard TVA About Dialog.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()

            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            ' Set the defaults.
            Dim thisAssembly As Assembly = New Assembly(GetExecutingAssembly())
            SetCompanyUrl("http://www.tva.gov")
            SetCompanyLogo(thisAssembly.GetEmbeddedResource("TVA.Windows.UI.TVALogo.bmp"))
            SetCompanyDisclaimer(thisAssembly.GetEmbeddedResource("TVA.Windows.UI.TVADisclaimer.txt"))

        End Sub

        ''' <summary>
        ''' Conceals the tab where disclaimer text is displayed.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub HideDisclaimerTab()

            TabControlInformation.TabPages.Remove(TabPageDisclaimer)

        End Sub

        ''' <summary>
        ''' Conceals the tab where application information is displayed.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub HideApplicationTab()

            TabControlInformation.TabPages.Remove(TabPageApplication)

        End Sub

        ''' <summary>
        ''' Conceals the tab where assemblies and their information is displayed.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub HideAssembliesTab()

            TabControlInformation.TabPages.Remove(TabPageAssemblies)

        End Sub

        ''' <summary>
        ''' Sets the URL that will be opened when the logo is clicked.
        ''' </summary>
        ''' <param name="url">URL of the company's home page.</param>
        ''' <remarks></remarks>
        Public Sub SetCompanyUrl(ByVal url As String)

            m_url = url

        End Sub

        ''' <summary>
        ''' Sets the logo that is to be displayed in the About Dialog.
        ''' </summary>
        ''' <param name="logoFile">Location of the logo file.</param>
        ''' <remarks></remarks>
        Public Sub SetCompanyLogo(ByVal logoFile As String)

            If File.Exists(logoFile) Then
                ' Logo file exists so load it in memory.
                Dim logoReader As New StreamReader(logoFile)
                SetCompanyLogo(logoReader.BaseStream())
                logoReader.Close()  ' Release all locks on the file.
            Else
                MessageBox.Show("The logo file '" & logoFile & "' does not exist.", "Missing File", _
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

        End Sub

        ''' <summary>
        ''' Sets the logo that is to be displayed in the About Dialog.
        ''' </summary>
        ''' <param name="logoStream">System.IO.Stream of the logo.</param>
        ''' <remarks></remarks>
        Public Sub SetCompanyLogo(ByVal logoStream As Stream)

            If logoStream IsNot Nothing Then
                PictureBoxLogo.Image = New System.Drawing.Bitmap(logoStream)
            End If

        End Sub

        ''' <summary>
        ''' Sets the disclaimer text that is to be displayed in the About Dialog.
        ''' </summary>
        ''' <param name="disclaimerFile">Location of the file that contains the disclaimer text.</param>
        ''' <remarks></remarks>
        Public Sub SetCompanyDisclaimer(ByVal disclaimerFile As String)

            If File.Exists(disclaimerFile) Then
                ' Disclaimer file exists so load it in memory.
                Dim disclaimerReader As New StreamReader(disclaimerFile)
                SetCompanyDisclaimer(disclaimerReader.BaseStream())
                disclaimerReader.Close()    ' Release all locks on the file.
            Else
                MessageBox.Show("The disclaimer file '" & disclaimerFile & "' does not exist.", "Missing File", _
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

        End Sub

        ''' <summary>
        ''' Sets the disclaimer text that is to be displayed in the About Dialog.
        ''' </summary>
        ''' <param name="disclaimerStream">System.IO.Stream of the disclaimer text.</param>
        ''' <remarks></remarks>
        Public Sub SetCompanyDisclaimer(ByVal disclaimerStream As Stream)

            If disclaimerStream IsNot Nothing Then
                RichTextBoxDisclaimer.Text = New StreamReader(disclaimerStream).ReadToEnd()
            End If

        End Sub

#End Region

#Region " Private Methods "

        Private Sub AddListViewItem(ByVal listView As ListView, ByVal text As String, ByVal ParamArray subitems As String())

            'Add a new ListViewItem with the specified data to the specified ListView.
            Dim item As New ListViewItem
            item.Text = text
            For Each subitem As String In subitems
                item.SubItems.Add(subitem)
            Next

            listView.Items.Add(item)

        End Sub

#End Region

#Region " Form Events "

        Private Sub AboutDialog_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

            Me.Text = String.Format(Me.Text(), Assembly.EntryAssembly.Title())
            If Me.Owner() IsNot Nothing Then
                Me.Font = Me.Owner.Font()
            End If

            ' Show information about the application that opened this dialog box.
            ListViewApplicationInfo.Items.Clear()
            AddListViewItem(ListViewApplicationInfo, "Friendly Name", AppDomain.CurrentDomain.FriendlyName())
            AddListViewItem(ListViewApplicationInfo, "Name", EntryAssembly.Name())
            AddListViewItem(ListViewApplicationInfo, "Version", EntryAssembly.Version().ToString())
            AddListViewItem(ListViewApplicationInfo, "Build Date", EntryAssembly.BuildDate.ToString())
            AddListViewItem(ListViewApplicationInfo, "Location", EntryAssembly.Location())
            AddListViewItem(ListViewApplicationInfo, "Title", EntryAssembly.Title())
            AddListViewItem(ListViewApplicationInfo, "Description", EntryAssembly.Description())
            AddListViewItem(ListViewApplicationInfo, "Company", EntryAssembly.Company())
            AddListViewItem(ListViewApplicationInfo, "Product", EntryAssembly.Product())
            AddListViewItem(ListViewApplicationInfo, "Copyright", EntryAssembly.Copyright())
            AddListViewItem(ListViewApplicationInfo, "Trademark", EntryAssembly.Trademark())

            ' Query all the assemblies used by the calling application.
            If m_assemblies Is Nothing Then
                m_assemblies = New List(Of Assembly)
                For Each asm As System.Reflection.Assembly In AppDomain.CurrentDomain.GetAssemblies()
                    Try
                        If File.Exists(asm.Location()) Then
                            ' Discard assemblies that are embedded into the application.
                            m_assemblies.Add(New Assembly(asm))
                        End If
                    Catch ex As Exception
                        ' Accessing Location property on assemblies that are built dynamically will result in an
                        ' exception since such assemblies only exist in-memory, so we'll ignore such assemblies.
                    End Try
                Next
            End If

            ' Show a list of all the queried assemblies.
            ComboBoxAssemblies.DisplayMember = "Name"
            ComboBoxAssemblies.DataSource = m_assemblies
            If ComboBoxAssemblies.Items.Count() > 0 Then
                ComboBoxAssemblies.SelectedIndex = 0
            End If

        End Sub

        Private Sub PictureBoxLogo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBoxLogo.Click

            If Not String.IsNullOrEmpty(m_url) Then
                Process.Start(m_url)
            End If

        End Sub

        Private Sub RichTextBoxDisclaimer_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkClickedEventArgs) Handles RichTextBoxDisclaimer.LinkClicked

            If Not String.IsNullOrEmpty(e.LinkText()) Then
                Process.Start(e.LinkText())
            End If

        End Sub

        Private Sub ComboBoxAssemblies_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxAssemblies.SelectedIndexChanged

            If ComboBoxAssemblies.SelectedItem() IsNot Nothing Then
                ListViewAssemblyInfo.Items.Clear()
                Dim attributes As Specialized.NameValueCollection = DirectCast(ComboBoxAssemblies.SelectedItem(), Assembly).GetAttributes()
                For Each key As String In attributes
                    AddListViewItem(ListViewAssemblyInfo, key, attributes(key))
                Next
            End If

        End Sub

        Private Sub ButtonOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click

            Me.Close()
            Me.Dispose()

        End Sub

#End Region

    End Class

End Namespace