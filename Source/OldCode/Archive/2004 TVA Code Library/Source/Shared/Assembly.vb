'Author: Pinal Patel
'Created: 04/29/05
'Modified: 05/13/05
'Description: This class reads the assembly attributes from an AssemblyInfo.vb file.



Namespace [Shared]

    Public Class [Assembly]

        Private Shared m_Assembly As System.Reflection.Assembly

        Shared Sub New()

            'Get a reference to the assembly.
            m_Assembly = Reflection.Assembly.GetEntryAssembly

        End Sub

        Sub New(ByVal Asm As Reflection.Assembly)

            'Set the assembly.
            m_Assembly = Asm

        End Sub

        Public Shared ReadOnly Property Company() As String

            Get
                'Return the Company attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(Reflection.AssemblyCompanyAttribute), False)(0), Reflection.AssemblyCompanyAttribute).Company.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Copyright() As String

            Get
                'Return the Copyright attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(Reflection.AssemblyCopyrightAttribute), False)(0), Reflection.AssemblyCopyrightAttribute).Copyright.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Description() As String

            Get
                'Return the Description attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(Reflection.AssemblyDescriptionAttribute), False)(0), Reflection.AssemblyDescriptionAttribute).Description.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Product() As String

            Get
                'Return the Product attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(Reflection.AssemblyProductAttribute), False)(0), Reflection.AssemblyProductAttribute).Product.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Title() As String

            Get
                'Return the Title attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(Reflection.AssemblyTitleAttribute), False)(0), Reflection.AssemblyTitleAttribute).Title.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property Trademark() As String

            Get
                'Return the Trademark attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(Reflection.AssemblyTrademarkAttribute), False)(0), Reflection.AssemblyTrademarkAttribute).Trademark.ToString()
            End Get

        End Property

        Public Shared ReadOnly Property CLSCompliant() As Boolean

            Get
                'Return the CLSCompliant attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(CLSCompliantAttribute), False)(0), CLSCompliantAttribute).IsCompliant()
            End Get

        End Property

        Public Shared ReadOnly Property Debuggable() As Boolean

            Get
                'Return the Debuggable attribute.
                Return DirectCast(m_Assembly.GetCustomAttributes(GetType(Diagnostics.DebuggableAttribute), False)(0), Diagnostics.DebuggableAttribute).IsJITTrackingEnabled()
            End Get

        End Property

        Public Shared ReadOnly Property Location() As String

            Get
                'Return the location of the assembly.
                Return m_Assembly.Location().ToLower()
            End Get

        End Property

        Public Shared ReadOnly Property CodeBase() As String

            Get
                'Return the location of the assembly in codebase format.
                Return m_Assembly.CodeBase().Replace("file:///", "").ToLower()
            End Get

        End Property

        Public Shared ReadOnly Property FullName() As String

            Get
                'Return the full name of the assembly.
                Return m_Assembly.FullName()
            End Get

        End Property

        Public Shared ReadOnly Property Name() As String

            Get
                'Return the name of the assembly.
                Return m_Assembly.GetName().Name()
            End Get

        End Property

        Public Shared ReadOnly Property Version() As Version

            Get
                'Return the Version attribute.
                Return m_Assembly.GetName().Version()
            End Get

        End Property

        Public Shared ReadOnly Property ImageRuntimeVersion() As String

            Get
                'Return the version of Common Language Runtime.
                Return m_Assembly.ImageRuntimeVersion()
            End Get

        End Property

        Public Shared ReadOnly Property GACLoaded() As Boolean

            Get
                'Return whether the assembly was loaded from the GAC.
                Return m_Assembly.GlobalAssemblyCache()
            End Get

        End Property

        Public Shared ReadOnly Property BuildDate() As Date

            Get
                'Return the date and time when the assembly was last built.
                Return IO.File.GetLastWriteTime(m_Assembly.Location())
            End Get

        End Property

    End Class

End Namespace
