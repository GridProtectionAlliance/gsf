'*******************************************************************************************************
'  TVA.IO.Enumerations.vb - Common Configuration Functions
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  ??/??/???? - Pinal C. Patel
'       Generated original version of source code.
'  08/22/2007 - Darrell Zuercher
'       Edited code comments.
'
'*******************************************************************************************************
Namespace IO

    ''' <summary>
    ''' Specifies the operation to be performed on the log file when it is full.
    ''' </summary>
    Public Enum LogFileFullOperation As Integer
        ''' <summary>
        ''' Truncates the existing entries in the log file to make space for new entries.
        ''' </summary>
        Truncate
        ''' <summary>
        ''' Rolls over to a new log file, and keeps the full log file for reference.
        ''' </summary>
        Rollover
    End Enum

End Namespace