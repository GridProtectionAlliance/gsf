'*******************************************************************************************************
'  Tva.Math.CompositeValues.vb - Composite values class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
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
'       Original version of source code generated
'  12/29/2005 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Math)
'
'*******************************************************************************************************

Namespace Math

    ''' <summary>
    ''' <para>Class to temporarily cache composite values until all values been received so that a compound value can be created.</para>
    ''' </summary>
    Public Class CompositeValues

        Private Structure CompositeValue

            Public Value As Double
            Public Received As Boolean

        End Structure

        Private m_compositeValues As CompositeValue()
        Private m_allReceived As Boolean

        ''' <summary>
        ''' <para>This constructor creates a new instance of the CompositeValues class specifing the total number of composite values to track.</para>
        ''' </summary>
        ''' <param name="count">Total number of composite values to track</param>
        Public Sub New(ByVal count As Integer)

            m_compositeValues = Array.CreateInstance(GetType(CompositeValue), count)

        End Sub

        ''' <summary>
        ''' <para>Gets or sets the composite value at the specified index in composite value collection.</para>
        ''' </summary>
        ''' <param name="index">The zero-based index of the composite value to get or set</param>
        ''' <returns>The composite value at the specified index in composite value collection</returns>
        Default Public Property Value(ByVal index As Integer) As Double
            Get
                Return m_compositeValues(index).Value
            End Get
            Set(ByVal value As Double)
                With m_compositeValues(index)
                    .Value = value
                    .Received = True
                End With
            End Set
        End Property

        ''' <summary>
        ''' <para>Gets a boolean value indicating if composite value at the specified index is received.</para>
        ''' </summary>
        ''' <param name="index">The zero-based index of the composite value.</param>
        ''' <returns>True if composite value at the specified index is received; otherwise, False.</returns>
        Public ReadOnly Property Received(ByVal index As Integer) As Boolean
            Get
                Return m_compositeValues(index).Received
            End Get
        End Property

        ''' <summary>
        ''' <para>Gets the number of compisite values in the composite value collection.</para>
        ''' </summary>
        ''' <returns>To be provided.</returns>
        Public ReadOnly Property Count() As Integer
            Get
                Return m_compositeValues.Length
            End Get
        End Property

        ''' <summary>
        ''' <para>Gets a boolean value indicating if all composite values are received.</para>
        ''' </summary>
        ''' <returns>True if all composite values are received; otherwise, False.</returns>
        Public ReadOnly Property AllReceived() As Boolean
            Get
                If m_allReceived Then
                    Return True
                Else
                    Dim allValuesReceived As Boolean = True

                    For x As Integer = 0 To m_compositeValues.Length - 1
                        If Not m_compositeValues(x).Received Then
                            allValuesReceived = False
                            Exit For
                        End If
                    Next

                    If allValuesReceived Then m_allReceived = True
                    Return allValuesReceived
                End If
            End Get
        End Property

    End Class

End Namespace
