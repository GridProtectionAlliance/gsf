using System.Diagnostics;
using System;
////using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
////using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  ConfigurationCellCollection.vb - PDCstream specific configuration cell collection
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PhasorProtocols
{
    namespace BpaPdcStream
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationCellCollection : PhasorProtocols.ConfigurationCellCollection
        {



            public ConfigurationCellCollection()
                : base(short.MaxValue, true)
            {

                // Although the number of configuration cells are not restricted in the
                // INI file, the data stream limits the maximum number of associated
                // data cells to 32767, so we limit the configurations cells to the same.
                // Also, in PDCstream configuration cells are constant length

            }

            protected ConfigurationCellCollection(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public void Add(ConfigurationCell value)
            {

                base.Add(value);

            }

            public new ConfigurationCell this[int index]
            {
                get
                {
                    return (ConfigurationCell)base[index];
                }
            }

            public bool TryGetBySectionEntry(string sectionEntry, ref ConfigurationCell configurationCell)
            {


                {
                    if (string.Compare(configurationCell.SectionEntry, sectionEntry, true) == 0)
                    {
                        return true;
                    }
                }

                configurationCell = null;
                return false;

            }

        }

    }

}
