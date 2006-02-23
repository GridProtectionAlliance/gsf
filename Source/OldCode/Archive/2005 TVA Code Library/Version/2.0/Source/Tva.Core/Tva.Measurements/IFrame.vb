'*******************************************************************************************************
'  Tva.Measurements.IFrame.vb - Abstract frame interface
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a keyed collection of measurements for a given timestamp
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    Public Interface IFrame

        Inherits IComparable

        ''' <summary>Handy instance reference to self</summary>
        ReadOnly Property This() As IFrame

        ''' <summary>Keyed measurements in this frame</summary>
        ReadOnly Property Measurements() As IDictionary(Of Integer, IMeasurement)

        ''' <summary>Exact timestamp of the data represented in this frame</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Property Ticks() As Long

        ''' <summary>Closest date representation of ticks of this frame</summary>
        ReadOnly Property Timestamp() As Date

    End Interface

End Namespace
