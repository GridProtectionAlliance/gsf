'*******************************************************************************************************
'  DataSample.vb - PDCstream data sample
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.DateTime

Namespace BpaPdcStream

    ' This class represents a complete sample of data for a given second - a time indexed sub-second set of PMU data rows.
    ' Don't confuse this with a DataPacket even though the config file defines a "sample rate".  The sample rate actually
    ' defines how many DataPacket's (i.e., rows) there will be in each sample.  The sample rate defined in the config file
    ' is really the "DataPacket" rate.  Note also that the nomenclature I used for the class names here match what is in
    ' the PDC stream specification to help make things easier to understand.
    Public Class DataSample

        Inherits Phasors.DataSample

        Public Sub New(ByVal sampleRate As Integer, ByVal timeStamp As Date)

            MyBase.New(sampleRate, timeStamp)

            For x As Integer = 0 To sampleRate - 1
                'DataFrames.Add(   'Rows(x) = New DataPacket(m_configFile, timeStamp, x)
            Next

        End Sub

    End Class

End Namespace