'*******************************************************************************************************
'  AnalogValue.vb - PDCstream Analog value
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

Namespace BpaPdcStream

    ''' <summary>
    ''' BPA PDCstream Analog Value Class
    ''' </summary>
    <CLSCompliant(False), Serializable()> _
    Public Class AnalogValue

        Inherits AnalogValueBase

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal value As Single)

            MyBase.New(parent, analogDefinition, value)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal unscaledValue As Int16)

            MyBase.New(parent, analogDefinition, unscaledValue)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, analogDefinition, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal analogValue As IAnalogValue)

            MyBase.New(parent, analogDefinition, analogValue)

        End Sub

        Friend Shared Function CreateNewAnalogValue(ByVal parent As IDataCell, ByVal definition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IAnalogValue

            Return New AnalogValue(parent, definition, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As DataCell
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Shadows Property Definition() As AnalogDefinition
            Get
                Return MyBase.Definition
            End Get
            Set(ByVal value As AnalogDefinition)
                MyBase.Definition = value
            End Set
        End Property

    End Class

End Namespace