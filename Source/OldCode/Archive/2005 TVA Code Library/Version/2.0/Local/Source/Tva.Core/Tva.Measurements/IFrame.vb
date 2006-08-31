'*******************************************************************************************************
'  Tva.Measurements.IFrame.vb - Abstract frame interface
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This interface represents a keyed collection of measurements for a given timestamp
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    Public Interface IFrame

        Inherits IComparable

        ''' <summary>Handy instance reference to self</summary>
        ReadOnly Property This() As IFrame

        ''' <summary>Keyed measurements in this frame</summary>
        ''' <remarks>Represents a dictionary of measurements, keyed by an integer ID</remarks>
        ReadOnly Property Measurements() As Dictionary(Of MeasurementKey, IMeasurement)

        ''' <summary>Gets or sets published state of this frame</summary>
        Property Published() As Boolean

        ''' <summary>Exact timestamp of the data represented in this frame</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Property Ticks() As Long

        ''' <summary>Date representation of ticks of this frame</summary>
        ReadOnly Property Timestamp() As Date

    End Interface

End Namespace
