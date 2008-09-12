//*******************************************************************************************************
//  GenericEventArgs.vb - Generic event arguments class
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
//  11/24/2006 - Pinal C. Patel
//      Generated original version of source code.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Generic event arguments class.
    /// </summary>
    /// <typeparam name="T">Type of argument for this event arguments instance.</typeparam>
    public class GenericEventArgs<T> : EventArgs
    {
        private T m_argument;

        public GenericEventArgs(T argument)
        {
            m_argument = argument;
        }

        public T Argument
        {
            get
            {
                return m_argument;
            }
            set
            {
                m_argument = value;
            }
        }
    }
}