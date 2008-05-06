'*******************************************************************************************************
'  TVA.Security.Application.Controls.ControlContainer.vb - Container control for user input controls
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
'  04/21/2008 - Pinal C. Patel
'       Original version of source code generated.
'
'*******************************************************************************************************

Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Reflection

Namespace Application.Controls

    ''' <summary>
    ''' Providers user interface (UI) that hosts user input controls.
    ''' </summary>
    Public Class ControlContainer
        Inherits CompositeControl

#Region " Member Declaration "

        Private m_table As Table
        Private m_controls As Dictionary(Of String, Control)

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Creates an instance of the container control.
        ''' </summary>
        ''' <param name="securityProvider">Current security control.</param>
        ''' <param name="activeControl">Link text of the default active control.</param>
        Public Sub New(ByVal securityProvider As WebSecurityProvider, ByVal activeControl As String)

            MyBase.New()
            m_controls = New Dictionary(Of String, Control)(StringComparer.CurrentCultureIgnoreCase)

            ' Add the default controls.
            AddControl("Login", New Login(Me, securityProvider))
            AddControl("Change Password", New ChangePassword(Me, securityProvider))

            ' Set the default property values.
            Me.Width = Unit.Parse("75px")
            Me.HelpText = "For immediate assistance, please contact the Operations Duty Specialist at 423-751-1700."
            Me.CompanyText = "TENNESSEE VALLEY AUTHORITY"
            Me.ActiveControl = activeControl

        End Sub

        ''' <summary>
        ''' Gets or sets the text to be displayed for help.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Text to be displayed for help.</returns>
        Public Property HelpText() As String
            Get
                Dim value As Object = ViewState("HelpText")
                If value IsNot Nothing Then
                    Return value.ToString()
                Else
                    Return String.Empty
                End If
            End Get
            Set(ByVal value As String)
                ViewState("HelpText") = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the company specific information to be displayed.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Company specific information to be displayed.</returns>
        Public Property CompanyText() As String
            Get
                Dim value As Object = ViewState("CompanyText")
                If value IsNot Nothing Then
                    Return value.ToString()
                Else
                    Return String.Empty
                End If
            End Get
            Set(ByVal value As String)
                ViewState("CompanyText") = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the any annoucement message to be displayed.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Annoucement message to be displayed.</returns>
        Public Property MessageText() As String
            Get
                Dim value As Object = ViewState("MessageText")
                If value IsNot Nothing Then
                    Return value.ToString()
                Else
                    Return String.Empty
                End If
            End Get
            Set(ByVal value As String)
                ViewState("MessageText") = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the link text of the active control.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Link text of the active control.</returns>
        Public Property ActiveControl() As String
            Get
                Dim value As Object = ViewState("ActiveControl")
                If value IsNot Nothing Then
                    Return value.ToString()
                Else
                    Return String.Empty
                End If
            End Get
            Set(ByVal value As String)
                If String.IsNullOrEmpty(value) OrElse m_controls.ContainsKey(value) Then
                    ' Either null value is specified for the active control link text or valid link text value
                    ' is specified. Null value is a valid value because then no control is an active control.
                    ViewState("ActiveControl") = value
                Else
                    Throw New ArgumentException("No such control.")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Adds the specified control.
        ''' </summary>
        ''' <param name="linkText">Link text for the control.</param>
        ''' <param name="control">The control to be added.</param>
        Public Sub AddControl(ByVal linkText As String, ByVal control As Control)

            If Not m_controls.ContainsKey(linkText) Then
                ' A control doesn't already exist with the specified link text.
                m_controls.Add(linkText, control)
            End If

        End Sub

        ''' <summary>
        ''' Performs a redirect to the specified URL or to the current URL if none specified.
        ''' </summary>
        ''' <param name="url">URL to redirect to.</param>
        Public Sub Redirect(ByVal url As String)

            If Not String.IsNullOrEmpty(url) Then
                Page.Response.Redirect(url, False)
            Else
                Page.Response.Redirect(Page.Request.Url.AbsoluteUri, False)
            End If

        End Sub

        ''' <summary>
        ''' Causes the control with the specified link text to be the active control and re-renders this control.
        ''' </summary>
        ''' <param name="linkText">Link text of the control to be made the active control.</param>
        Public Sub UpdateActiveControl(ByVal linkText As String)

            Me.ActiveControl = linkText
            If m_table IsNot Nothing Then
                ' Instead of just updating the cells for active control, we will re-render this control again
                ' so that the controls (this control and the active control) are created in the right order.
                ' This is required for events inside of the active control to be handled correctly on postbacks
                ' as events are handled based on the UniqueID of the control causing the postback and this 
                ' UniqueID is dependent on the order in which the controls are created/rendered.
                ' Refer: http://msdn2.microsoft.com/en-us/library/aa720472(VS.71).aspx
                CreateChildControls()
            End If

            ' When switching the active control, we reset the message text that might be present.
            UpdateMessageText(String.Empty, MessageType.Error)

        End Sub

        ''' <summary>
        ''' Causes the message text to be set to the specified text and updates the control to reflect the change.
        ''' </summary>
        ''' <param name="message">Text to set as the message text.</param>
        ''' <param name="type">Indicates the type of message.</param>
        Public Sub UpdateMessageText(ByVal message As String, ByVal type As MessageType)

            Me.MessageText = message
            If m_table IsNot Nothing Then
                ' Control has been rendered, so update the message text in the cell designated for message text.
                m_table.Rows(4).Cells(0).Text = message
                m_table.Rows(4).Cells(0).CssClass = type.ToString() & "Message"
            End If

        End Sub

#Region " Shared "

        ''' <summary>
        ''' Creates a server table.
        ''' </summary>
        ''' <param name="rowCount">Number of rows for the table.</param>
        ''' <param name="columnCount">Number of columns for the table.</param>
        ''' <returns>A server table.</returns>
        Public Shared Function NewTable(ByVal rowCount As Integer, ByVal columnCount As Integer) As Table

            Dim table As New Table()
            For i As Integer = 1 To rowCount
                Dim row As New TableRow()
                For j As Integer = 1 To columnCount
                    row.Cells.Add(New TableCell())
                Next
                table.Rows.Add(row)
            Next

            Return table

        End Function

#End Region

#End Region

#Region " Code Scope: Protected "

        ''' <summary>
        ''' Performs layout of the control.
        ''' </summary>
        Protected Overrides Sub CreateChildControls()

            ' Apply style-sheet.
            Dim includeTemplate As String = "<link rel='stylesheet' text='text/css' href='{0}' />"
            Dim includeLocation As String = Page.ClientScript.GetWebResourceUrl(GetType(ControlContainer), _
                                                                               "TVA.Security.Application.Controls.StyleSheet.css")
            Page.Header.Controls.Add(New LiteralControl(String.Format(includeTemplate, includeLocation)))

            ' ----------------------------------------------------------------------------------
            ' |                                                                                |
            ' | Header Image                                                                   |
            ' |                                                                                |
            ' ----------------------------------------------------------------------------------
            ' | :: Control Link  :: Control Link                                               |
            ' ----------------------------------------------------------------------------------
            ' |                             Active Control Link Text                           |
            ' ----------------------------------------------------------------------------------
            ' |                                                                                |
            ' |                                                                                |
            ' |                                                                                |
            ' |                              Active Control Content                            |
            ' |                                                                                |
            ' |                                                                                |
            ' |                                                                                |
            ' ----------------------------------------------------------------------------------
            ' | Message Text                                                                   |
            ' ----------------------------------------------------------------------------------
            ' | Help Text                                                                      |
            ' ----------------------------------------------------------------------------------
            ' | Company Text                                                               | ? |
            ' ----------------------------------------------------------------------------------

            ' Layout the control.
            m_table = NewTable(7, 1)
            m_table.CssClass = "Container"

            ' Row #1
            Dim headerImage As New Image()
            headerImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(ControlContainer), _
                                                                       "TVA.Security.Application.Controls.LogoExternal.jpg")
            m_table.Rows(0).Cells(0).CssClass = "HeaderSection"
            m_table.Rows(0).Cells(0).Controls.Add(headerImage)

            ' Row #2, #3, #4
            Dim linksTable As Table = NewTable(1, m_controls.Count)
            For i As Integer = 0 To m_controls.Count - 1
                Dim text As New Label()
                text.Text = "&nbsp;&nbsp;::&nbsp;"
                text.Font.Bold = True
                Dim link As New LinkButton()
                link.Text = m_controls.Keys(i)
                link.CausesValidation = False
                AddHandler link.Click, AddressOf Link_Click
                linksTable.Rows(0).Cells(i).Controls.Add(text)
                linksTable.Rows(0).Cells(i).Controls.Add(link)

                If String.Compare(ActiveControl, m_controls.Keys(i), True) = 0 Then
                    ' Active control caption
                    m_table.Rows(2).Cells(0).Text = m_controls.Keys(i)
                    ' Active control content
                    m_table.Rows(3).Cells(0).Controls.Add(m_controls.Values(i))
                End If
            Next
            m_table.Rows(1).Cells(0).CssClass = "ControlLinks"
            m_table.Rows(2).Cells(0).CssClass = "ActiveControlCaption"
            m_table.Rows(3).Cells(0).CssClass = "ActiveControlContent"
            m_table.Rows(1).Cells(0).Controls.Add(linksTable)

            ' Row #5
            m_table.Rows(4).Cells(0).Text = MessageText
            m_table.Rows(4).Cells(0).CssClass = "ErrorMessage"

            ' Row #6
            m_table.Rows(5).Cells(0).Text = HelpText
            m_table.Rows(5).Cells(0).CssClass = "HelpText"

            ' Row #7
            Dim footerTable As Table = NewTable(1, 2)
            Dim imageLinkText As String = "<a href=""{0}"" target=""_blank""><img src=""{1}"" style=""height:16px;width:16px;border-width:0px;"" /></a>"
            imageLinkText = String.Format(imageLinkText, _
                                          Page.ClientScript.GetWebResourceUrl(GetType(ControlContainer), "TVA.Security.Application.Controls.Help.pdf"), _
                                          Page.ClientScript.GetWebResourceUrl(GetType(ControlContainer), "TVA.Security.Application.Controls.Help.gif"))
            footerTable.Width = Unit.Parse("100%")
            footerTable.Rows(0).Cells(0).Text = "&nbsp;&nbsp;" & CompanyText
            footerTable.Rows(0).Cells(0).Width = Unit.Parse("95%")
            footerTable.Rows(0).Cells(1).Text = imageLinkText
            m_table.Rows(6).Cells(0).CssClass = "FooterSection"
            m_table.Rows(6).Cells(0).Controls.Add(footerTable)

            Me.Controls.Clear()
            Me.Controls.Add(m_table)

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub Link_Click(ByVal sender As Object, ByVal e As System.EventArgs)

            ' Set the active control
            UpdateActiveControl(CType(sender, LinkButton).Text)

        End Sub

#End Region

    End Class


End Namespace
