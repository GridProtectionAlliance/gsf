'*******************************************************************************************************
'  IBinaryDataProvider.vb - Binary data provider interface
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
'  03/01/2007 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

''' <summary>This interface allows any data structure to provide a binary representation of itself.</summary>
Public Interface IBinaryDataProvider

    ReadOnly Property BinaryImage() As Byte()

    ReadOnly Property BinaryLength() As Integer

End Interface