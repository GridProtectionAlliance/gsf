'Author: Pinal Patel
'Created: 04/29/05
'Modified: 05/02/05
'Description: This class reads the assembly attributes from an AssemblyInfo.vb file.



'Namespaces used.
Imports SR = System.Reflection

Namespace [Shared]

    Public Class [Assembly]

        Private Shared m_Assembly As System.Reflection.Assembly

        Sub New()

            'Get the current executing asembly.
            m_Assembly = SR.Assembly.GetExecutingAssembly

        End Sub

        Public Shared ReadOnly Property Company() As String

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the Company attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(SR.AssemblyCompanyAttribute), False)(0), SR.AssemblyCompanyAttribute).Company.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Copyright() As String

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the Copyright attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(SR.AssemblyCopyrightAttribute), False)(0), SR.AssemblyCopyrightAttribute).Copyright.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Description() As String

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the Description attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(SR.AssemblyDescriptionAttribute), False)(0), SR.AssemblyDescriptionAttribute).Description.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Product() As String

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the Product attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(SR.AssemblyProductAttribute), False)(0), SR.AssemblyProductAttribute).Product.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Title() As String

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the Title attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(SR.AssemblyTitleAttribute), False)(0), SR.AssemblyTitleAttribute).Title.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Trademark() As String

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the Trademark attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(SR.AssemblyTrademarkAttribute), False)(0), SR.AssemblyTrademarkAttribute).Trademark.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Version() As Version

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the Version attribute.
                Return m_Assembly.GetName.Version()
            End Get

        End Property

        Public Shared ReadOnly Property BuildDate() As Date

            Get
                'Instantiate a new instance of this class if not instantiated.
                If m_Assembly Is Nothing Then Dim oAssembly As New [Assembly]

                'Return the date and time when the assembly was last built.
                Return System.IO.File.GetLastWriteTime(m_Assembly.Location())
            End Get

        End Property

    End Class

End Namespace
