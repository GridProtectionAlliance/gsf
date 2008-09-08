//*******************************************************************************************************
//  IdentifiableItem.vb - Generic identifiable item class
//  Copyright © 2005 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/12/2007 - Pinal C. Patel
//      Generated original version of source code.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Generic identifiable item class.
    /// </summary>
    /// <remarks>
    /// This class is used to abstractly track items along with their source.
    /// </remarks>
    /// <typeparam name="TIdentifier">Type of data item source.</typeparam>
    /// <typeparam name="TItem">Type of data item.</typeparam>
    public class IdentifiableItem<TIdentifier, TItem>
    {
        private TIdentifier m_source;
        private TItem m_item;

        public IdentifiableItem(TIdentifier source, TItem item)
        {
            m_source = source;
            m_item = item;
        }

        public TIdentifier Source
        {
            get
            {
                return m_source;
            }
            set
            {
                m_source = value;
            }
        }

        public TItem Item
        {
            get
            {
                return m_item;
            }
            set
            {
                m_item = value;
            }
        }
    }
}