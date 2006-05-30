' 05-26-06

Imports System.IO
Imports System.Windows.Forms
Imports System.Reflection.Assembly
Imports Tva.Assembly

Namespace UI

    Public Class AboutDialog

        Private m_url As String
        Private m_assemblies As List(Of Assembly)

#Region " Public "

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
            SetCompanyHomePage("http://www.tva.gov")
            SetCompanyLogo(thisAssembly.GetEmbeddedResource("TVALogo.bmp"))
            SetCompanyDisclaimer(thisAssembly.GetEmbeddedResource("TVADisclaimer.txt"))

        End Sub

        ''' <summary>
        ''' Sets the URL that will be opened when the Home Page link is clicked.
        ''' </summary>
        ''' <param name="url">URL of the home page.</param>
        ''' <remarks></remarks>
        Public Sub SetCompanyHomePage(ByVal url As String)

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
                PictureBoxLogo.Image = New Drawing.Bitmap(logoStream)
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

#Region " Private "

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
                    If File.Exists(asm.Location()) Then
                        ' Discard assemblies that are embedded into the application.
                        m_assemblies.Add(New Assembly(asm))
                    End If
                Next
            End If

            ' Show a list of all the queried assemblies.
            ComboBoxAssemblies.DisplayMember = "Name"
            ComboBoxAssemblies.DataSource = m_assemblies
            If ComboBoxAssemblies.Items.Count() > 0 Then
                ComboBoxAssemblies.SelectedIndex = 0
            End If

        End Sub

        Private Sub LinkLabelHomePage_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabelHomePage.LinkClicked

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