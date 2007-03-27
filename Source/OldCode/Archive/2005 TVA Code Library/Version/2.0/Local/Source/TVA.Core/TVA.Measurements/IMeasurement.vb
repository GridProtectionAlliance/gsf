'*******************************************************************************************************
'  TVA.Measurements.IMeasurement.vb - Abstract measurement interface
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This interface abstractly represents a value measured at an exact time interval
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.ComponentModel

Namespace Measurements

    ''' <summary>Abstract measured value interface</summary>
    Public Interface IMeasurement

        Inherits IEquatable(Of IMeasurement), IComparable(Of IMeasurement), IComparable

        ''' <summary>Handy instance reference to self</summary>
        ReadOnly Property This() As IMeasurement

        ''' <summary>Gets or sets the numeric ID of this measurement</summary>
        ''' <remarks>
        ''' <para>In most implementations, this will be a required field</para>
        ''' <para>Note that this field, in addition to Source, typically creates the primary key for a measurement</para>
        ''' </remarks>
        Property ID() As Integer

        ''' <summary>Gets or sets the source of this measurement</summary>
        ''' <remarks>
        ''' <para>In most implementations, this will be a required field</para>
        ''' <para>Note that this field, in addition to ID, typically creates the primary key for a measurement</para>
        ''' <para>This value is typically used to track the archive name in which measurement is stored</para>
        ''' </remarks>
        Property Source() As String

        ''' <summary>Returns the primary key of this measurement</summary>
        ReadOnly Property Key() As MeasurementKey

        ''' <summary>Gets or sets the text based ID of this measurement</summary>
        Property Tag() As String

        ''' <summary>Gets or sets the raw value of this measurement (i.e., the numeric value that is not offset by adder and multiplier)</summary>
        Property Value() As Double

        ''' <summary>Returns the adjusted numeric value of this measurement, taking into account the specified adder and multiplier offsets</summary>
        ''' <remarks>
        ''' <para>Implementors need to account for adder and multiplier in return value, e.g.:</para>
        ''' <code>Return Value * Multiplier + Adder</code>
        ''' </remarks>
        ReadOnly Property AdjustedValue() As Double

        ''' <summary>Defines an offset to add to the measurement value</summary>
        ''' <remarks>Implementors should make sure this value defaults to zero</remarks>
        <DefaultValue(0.0R)> Property Adder() As Double

        ''' <summary>Defines a mulplicative offset to add to the measurement value</summary>
        ''' <remarks>Implementors should make sure this value defaults to one</remarks>
        <DefaultValue(1.0R)> Property Multiplier() As Double

        ''' <summary>Gets or sets exact timestamp of the data represented by this measurement</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Property Ticks() As Long

        ''' <summary>Date representation of ticks of this measurement</summary>
        ReadOnly Property Timestamp() As Date

        ''' <summary>Determines if the quality of the numeric value of this measurement is good</summary>
        Property ValueQualityIsGood() As Boolean

        ''' <summary>Determines if the quality of the timestamp of this measurement is good</summary>
        Property TimestampQualityIsGood() As Boolean

    End Interface

End Namespace
