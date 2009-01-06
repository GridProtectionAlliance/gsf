//*******************************************************************************************************
//  PhasorValueBase.vb - Phasor value base class
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Note: Phasors are stored in rectangular format internally
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PCS;
using PCS.NumericalAnalysis;

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent representation of a phasor value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class PhasorValueBase : ChannelValueBase<IPhasorDefinition>, IPhasorValue
    {
        protected delegate IPhasorValue CreateNewPhasorValueFunctionSignature(IDataCell parent, IPhasorDefinition phasorDefinition, float real, float imaginary);

        private const float DegreesToRadians = (float)(System.Math.PI / 180.0D);
        private const float RadiansToDegrees = (float)(180.0D / System.Math.PI);

        private float m_real;
        private float m_imaginary;
        private bool m_realAssigned;
        private bool m_imaginaryAssigned;
        private CompositeValues m_compositeValues = new CompositeValues(2);

        /// <summary>Create phasor from polar coordinates (angle expected in Degrees)</summary>
        /// <remarks>Note: This method is expected to be implemented as a public shared method in derived class automatically passing in createNewPhasorValueFunction</remarks>
        protected static IPhasorValue CreateFromPolarValues(CreateNewPhasorValueFunctionSignature createNewPhasorValueFunction, IDataCell parent, IPhasorDefinition phasorDefinition, float angle, float magnitude)
        {

            return CreateFromRectangularValues(createNewPhasorValueFunction, parent, phasorDefinition, CalculateRealComponent(angle, magnitude), CalculateImaginaryComponent(angle, magnitude));

        }

        /// <summary>Create phasor from rectangular coordinates</summary>
        /// <remarks>Note: This method is expected to be implemented as a public shared method in derived class automatically passing in createNewPhasorValueFunction</remarks>
        protected static IPhasorValue CreateFromRectangularValues(CreateNewPhasorValueFunctionSignature createNewPhasorValueFunction, IDataCell parent, IPhasorDefinition phasorDefinition, float real, float imaginary)
        {

            if (phasorDefinition == null) throw (new ArgumentNullException("phasorDefinition", "No phasor definition specified"));
            return createNewPhasorValueFunction.Invoke(parent, phasorDefinition, real, imaginary);

        }

        /// <summary>Create phasor from unscaled rectangular coordinates</summary>
        /// <remarks>Note: This method is expected to be implemented as a public shared method in derived class automatically passing in createNewPhasorValueFunction</remarks>
        protected static IPhasorValue CreateFromUnscaledRectangularValues(CreateNewPhasorValueFunctionSignature createNewPhasorValueFunction, IDataCell parent, IPhasorDefinition phasorDefinition, short real, short imaginary)
        {

            float factor = phasorDefinition.ConversionFactor;
            return CreateFromRectangularValues(createNewPhasorValueFunction, parent, phasorDefinition, (float)real * factor, (float)imaginary * factor);

        }

        /// <summary>Gets real component from angle (in Degrees) and magnitude</summary>
        public static float CalculateRealComponent(float angle, float magnitude)
        {

            return magnitude * (float)System.Math.Cos(angle * DegreesToRadians);

        }

        /// <summary>Gets imaginary component from angle (in Degrees) and magnitude</summary>
        public static float CalculateImaginaryComponent(float angle, float magnitude)
        {

            return magnitude * (float)System.Math.Sin(angle * DegreesToRadians);

        }

        /// <summary>Calculate watts from imaginary and real components of two phasors</summary>
        public static float CalculatePower(IPhasorValue voltage, IPhasorValue current)
        {
            if (voltage == null) throw (new ArgumentNullException("voltage", "No voltage specified"));
            if (current == null) throw (new ArgumentNullException("current", "No current specified"));

            return 3 * (voltage.Real * current.Real + voltage.Imaginary * current.Imaginary);
            //Return 3 * voltage.Magnitude * current.Magnitude * System.Math.Cos((voltage.Angle - current.Angle) * DegreesToRadians)
        }

        /// <summary>Calculate vars from imaginary and real components of two phasors</summary>
        public static float CalculateVars(IPhasorValue voltage, IPhasorValue current)
        {
            if (voltage == null) throw (new ArgumentNullException("voltage", "No voltage specified"));
            if (current == null) throw (new ArgumentNullException("current", "No current specified"));

            return 3 * (voltage.Imaginary * current.Real - voltage.Real * current.Imaginary);
            //Return 3 * voltage.Magnitude * current.Magnitude * System.Math.Sin((voltage.Angle - current.Angle) * DegreesToRadians)
        }

        protected PhasorValueBase()
        {
        }

        protected PhasorValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize phasor value
            m_real = info.GetSingle("real");
            m_imaginary = info.GetSingle("imaginary");

            m_realAssigned = true;
            m_imaginaryAssigned = true;

        }

        protected PhasorValueBase(IDataCell parent)
            : base(parent)
        {


        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal real As Single, ByVal imaginary As Single)
        protected PhasorValueBase(IDataCell parent, IPhasorDefinition phasorDefinition, float real, float imaginary)
            : base(parent, phasorDefinition)
        {


            m_real = real;
            m_imaginary = imaginary;

            m_realAssigned = !float.IsNaN(real);
            m_imaginaryAssigned = !float.IsNaN(imaginary);

        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal unscaledReal As short, ByVal unscaledImaginary As short)
        protected PhasorValueBase(IDataCell parent, IPhasorDefinition phasorDefinition, short unscaledReal, short unscaledImaginary)
            : this(parent, phasorDefinition, (float)unscaledReal * phasorDefinition.ConversionFactor, (float)unscaledImaginary * phasorDefinition.ConversionFactor)
        {


        }

        // Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal phasorDefinition As IPhasorDefinition, ByVal binaryImage As Byte(), ByVal startIndex As int)
        protected PhasorValueBase(IDataCell parent, IPhasorDefinition phasorDefinition, byte[] binaryImage, int startIndex)
            : base(parent, phasorDefinition)
        {

            ParseBinaryImage(null, binaryImage, startIndex);

        }

        // Derived classes are expected to expose a Public Sub New(ByVal phasorValue As IPhasorValue)
        protected PhasorValueBase(IDataCell parent, IPhasorDefinition phasorDefinition, IPhasorValue phasorValue)
            : this(parent, phasorDefinition, phasorValue.Real, phasorValue.Imaginary)
        {


        }

        public virtual CoordinateFormat CoordinateFormat
        {
            get
            {
                return Definition.CoordinateFormat;
            }
        }

        public virtual PhasorType Type
        {
            get
            {
                return Definition.Type;
            }
        }

        public virtual float AngleInRadians
        {
            get
            {
                return (float)System.Math.Atan2(m_imaginary, m_real);
            }
        }

        public virtual float Angle
        {
            get
            {
                return AngleInRadians * RadiansToDegrees;
            }
            set
            {
                // We store angle as one of our required composite values
                m_compositeValues[CompositePhasorValue.Angle] = value;

                // If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValueFromComposites();
            }
        }

        public virtual bool AngleAssigned
        {
            get
            {
                return m_compositeValues.Received(CompositePhasorValue.Angle);
            }
        }

        public virtual float Magnitude
        {
            get
            {
                return (float)System.Math.Sqrt(m_real * m_real + m_imaginary * m_imaginary);
            }
            set
            {
                m_compositeValues[CompositePhasorValue.Magnitude] = value;

                // If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValueFromComposites();
            }
        }

        public virtual bool MagnitudeAssigned
        {
            get
            {
                return m_compositeValues.Received(CompositePhasorValue.Magnitude);
            }
        }

        private void CalculatePhasorValueFromComposites()
        {

            if (m_compositeValues.AllReceived)
            {
                float angle;
                float magnitude;

                // All values received, create a new phasor value from composite values
                angle = (float)m_compositeValues[CompositePhasorValue.Angle];
                magnitude = (float)m_compositeValues[CompositePhasorValue.Magnitude];

                m_real = CalculateRealComponent(angle, magnitude);
                m_imaginary = CalculateImaginaryComponent(angle, magnitude);

                m_realAssigned = true;
                m_imaginaryAssigned = true;
            }

        }

        public virtual float Real
        {
            get
            {
                return m_real;
            }
            set
            {
                m_real = value;
                m_realAssigned = true;
            }
        }

        public virtual float Imaginary
        {
            get
            {
                return m_imaginary;
            }
            set
            {
                m_imaginary = value;
                m_imaginaryAssigned = true;
            }
        }

        public virtual short UnscaledReal
        {
            get
            {
                return (short)(m_real / Definition.ConversionFactor);
            }
            set
            {
                m_real = (float)value * Definition.ConversionFactor;
                m_realAssigned = true;
            }
        }

        public virtual short UnscaledImaginary
        {
            get
            {
                return (short)(m_imaginary / Definition.ConversionFactor);
            }
            set
            {
                m_imaginary = (float)value * Definition.ConversionFactor;
                m_imaginaryAssigned = true;
            }
        }

        public override float this[int index]
        {
            get
            {
                switch (index)
                {
                    case CompositePhasorValue.Angle:
                        return Angle;
                    case CompositePhasorValue.Magnitude:
                        return Magnitude;
                    default:
                        throw (new IndexOutOfRangeException("Specified phasor value composite index, " + index + ", is out of range - there are only two composite values for a phasor value: angle (0) and magnitude (1)"));
                }
            }
            set
            {
                switch (index)
                {
                    case CompositePhasorValue.Angle:
                        Angle = value;
                        break;
                    case CompositePhasorValue.Magnitude:
                        Magnitude = value;
                        break;
                    default:
                        throw (new IndexOutOfRangeException("Specified phasor value composite index, " + index + ", is out of range - there are only two composite values for a phasor value: angle (0) and magnitude (1)"));
                }
            }
        }

        public override int CompositeValueCount
        {
            get
            {
                return 2;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (!m_realAssigned || !m_imaginaryAssigned);
            }
        }

        protected override int BodyLength
        {
            get
            {
                if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                    return 4;
                else
                    return 8;
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                if (CoordinateFormat == PhasorProtocols.CoordinateFormat.Rectangular)
                {
                    if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                    {
                        EndianOrder.BigEndian.CopyBytes(UnscaledReal, buffer, 0);
                        EndianOrder.BigEndian.CopyBytes(UnscaledImaginary, buffer, 2);
                    }
                    else
                    {
                        EndianOrder.BigEndian.CopyBytes(m_real, buffer, 0);
                        EndianOrder.BigEndian.CopyBytes(m_imaginary, buffer, 4);
                    }
                }
                else
                {
                    if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                    {
                        EndianOrder.BigEndian.CopyBytes((ushort)Magnitude, buffer, 0);
                        EndianOrder.BigEndian.CopyBytes((short)(Angle * DegreesToRadians * 10000.0F), buffer, 2);
                    }
                    else
                    {
                        EndianOrder.BigEndian.CopyBytes(Magnitude, buffer, 0);
                        EndianOrder.BigEndian.CopyBytes(AngleInRadians, buffer, 4);
                    }
                }

                return buffer;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            if (CoordinateFormat == PhasorProtocols.CoordinateFormat.Rectangular)
            {
                if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                {
                    UnscaledReal = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                    UnscaledImaginary = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2);
                }
                else
                {
                    m_real = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
                    m_imaginary = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4);

                    m_realAssigned = true;
                    m_imaginaryAssigned = true;
                }
            }
            else
            {
                float magnitude;
                float angle;

                if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                {
                    magnitude = (float)EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex);
                    angle = (float)EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2) * RadiansToDegrees / 10000.0F;
                }
                else
                {
                    magnitude = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
                    angle = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4) * RadiansToDegrees;
                }

                m_real = CalculateRealComponent(angle, magnitude);
                m_imaginary = CalculateImaginaryComponent(angle, magnitude);

                m_realAssigned = true;
                m_imaginaryAssigned = true;
            }

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize phasor value
            info.AddValue("real", m_real);
            info.AddValue("imaginary", m_imaginary);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Phasor Type", (int)Type + ": " + Type);
                baseAttributes.Add("Angle Value", Angle + "°");
                baseAttributes.Add("Magnitude Value", Magnitude.ToString());
                baseAttributes.Add("Real Value", Real.ToString());
                baseAttributes.Add("Imaginary Value", Imaginary.ToString());
                baseAttributes.Add("Unscaled Real Value", UnscaledReal.ToString());
                baseAttributes.Add("Unscaled Imaginary Value", UnscaledImaginary.ToString());
                baseAttributes.Add("Angle Value was Assigned", AngleAssigned.ToString());
                baseAttributes.Add("Magnitude Value was Assigned", MagnitudeAssigned.ToString());

                return baseAttributes;
            }
        }
    }
}
