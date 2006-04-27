'*******************************************************************************************************
'  Tva.Measurements.IMeasurement.vb - Abstract measurement interface
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This interface represents a value measured at an exact time interval
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    Public Interface IMeasurement

        Inherits IComparable

        ''' <summary>Handy instance reference to self</summary>
        ReadOnly Property This() As IMeasurement

        ''' <summary>Gets or sets index or ID of this measurement</summary>
        Property ID() As Integer

        ''' <summary>Gets or sets numeric value of this measurement</summary>
        Property Value() As Double

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
