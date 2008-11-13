using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  PhasorValue.vb - PDCstream Phasor value
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


namespace PCS.PhasorProtocols
{
    namespace BpaPdcStream
    {

        [CLSCompliant(false), Serializable()]
        public class PhasorValue : PhasorValueBase
        {



            public static PhasorValue CreateFromPolarValues(IDataCell parent, IPhasorDefinition phasorDefinition, float angle, float magnitude)
            {

                return (PhasorValue)PhasorValueBase.CreateFromPolarValues(CreateNewPhasorValue, parent, phasorDefinition, angle, magnitude);

            }

            public static PhasorValue CreateFromRectangularValues(IDataCell parent, IPhasorDefinition phasorDefinition, float real, float imaginary)
            {

                return (PhasorValue)PhasorValueBase.CreateFromRectangularValues(CreateNewPhasorValue, parent, phasorDefinition, real, imaginary);

            }

            public static PhasorValue CreateFromUnscaledRectangularValues(IDataCell parent, IPhasorDefinition phasorDefinition, short real, short imaginary)
            {

                return (PhasorValue)PhasorValueBase.CreateFromUnscaledRectangularValues(CreateNewPhasorValue, parent, phasorDefinition, real, imaginary);

            }

            private static IPhasorValue CreateNewPhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition, float real, float imaginary)
            {

                return new PhasorValue(parent, phasorDefinition, real, imaginary);

            }

            protected PhasorValue()
            {
            }

            protected PhasorValue(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public PhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition, float real, float imaginary)
                : base(parent, phasorDefinition, real, imaginary)
            {


            }

            public PhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition, short unscaledReal, short unscaledImaginary)
                : base(parent, phasorDefinition, unscaledReal, unscaledImaginary)
            {


            }

            public PhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition, byte[] binaryImage, int startIndex)
                : base(parent, phasorDefinition, binaryImage, startIndex)
            {


            }

            public PhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition, IPhasorValue phasorValue)
                : base(parent, phasorDefinition, phasorValue)
            {


            }

            internal static IPhasorValue CreateNewPhasorValue(IDataCell parent, IPhasorDefinition definition, byte[] binaryImage, int startIndex)
            {

                return new PhasorValue(parent, definition, binaryImage, startIndex);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataCell Parent
            {
                get
                {
                    return (DataCell)base.Parent;
                }
            }

            public new PhasorDefinition Definition
            {
                get
                {
                    return (PhasorDefinition)base.Definition;
                }
                set
                {
                    base.Definition = value;
                }
            }

            // BPA PDCstream defines an angle offset in the configuration file
            public override float Angle
            {
                get
                {
                    return base.Angle + Definition.Offset;
                }
                set
                {
                    base.Angle = value - Definition.Offset;
                }
            }

            // Phasor magnitudes in BPA PDCstream are calculated use a custom conversion factor
            public override float Magnitude
            {
                get
                {
                    return base.Magnitude * PhasorDefinition.CustomConversionFactor(Definition);
                }
                set
                {
                    base.Magnitude = value / PhasorDefinition.CustomConversionFactor(Definition);
                }
            }

            public static ushort CalculateBinaryLength(IPhasorDefinition definition)
            {

                // The phasor definition will determine the binary length based on data format
                return (new PhasorValue(null, definition, 0, 0)).BinaryLength;

            }

        }

    }
}
