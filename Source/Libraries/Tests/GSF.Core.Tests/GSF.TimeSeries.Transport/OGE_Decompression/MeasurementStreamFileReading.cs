using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using GSF;
using GSF.IO;

namespace OGE.MeasurementStream
{
    internal enum DecompressionExitCode
    {
        NewMeasurementRegistered,
        UserData,
        UserDataWithValue,
        EndOfStreamOccured,
        MeasurementRead
    }

    internal partial class MeasurementStreamFileReading
    {
        private ByteBuffer m_buffer;
        private int m_length;

        private long m_prevTimestamp1;
        private long m_prevTimestamp2;

        private long m_prevTimeDelta1;
        private long m_prevTimeDelta2;
        private long m_prevTimeDelta3;
        private long m_prevTimeDelta4;

        private PointMetaData m_lastPoint;
        private List<PointMetaData> m_points;

        #region [ Returned Value ]

        public int ID;
        public long Timestamp;
        public uint Quality;
        public readonly UnionValues Value;

        public long UserCommand;
        public byte[] UserCommandData;
        public int UserCommandDataLength;

        public int NewMeasurementRegisteredId;
        public MeasurementTypeCode NewMeasurementRegisteredTypeCode;
        public byte[] NewMeasurementRegisteredMetadata;
        public int NewMeasurementRegisteredMetadataLength;

        private BitStreamReader m_bitStream;
        #endregion

        public MeasurementStreamFileReading()
        {
            Value = new UnionValues();
            m_points = new List<PointMetaData>();
            m_buffer = new ByteBuffer(4096);
            m_bitStream = new BitStreamReader(this);
            NewMeasurementRegisteredMetadata = new byte[16];
            UserCommandData = new byte[16];
        }

        public void LoadFromStream(Stream stream)
        {
            int lengthToFill = (int)(stream.Length - stream.Position);

            while (m_buffer.Data.Length < lengthToFill)
               m_buffer.Grow();

            m_buffer.Position = 0;
            m_length = lengthToFill;
            stream.ReadAll(m_buffer.Data, 0, lengthToFill);

            Reset();
        }

        private void Reset()
        {
            m_lastPoint = new PointMetaDataInt32(m_buffer, m_bitStream, MeasurementTypeCode.UInt32, -1);
            m_points.Clear();
            m_prevTimeDelta1 = long.MaxValue;
            m_prevTimeDelta2 = long.MaxValue;
            m_prevTimeDelta3 = long.MaxValue;
            m_prevTimeDelta4 = long.MaxValue;
            m_prevTimestamp1 = 0;
            m_prevTimestamp2 = 0;
        }

        public void Load(byte[] data)
        {
            while (m_buffer.Data.Length < data.Length)
                m_buffer.Grow();

            m_buffer.Position = 0;
            m_length = data.Length;
            data.CopyTo(m_buffer.Data, 0);

            Reset();
        }

        public DecompressionExitCode GetMeasurement()
        {
            TryAgain:

            PointMetaData nextPoint = null;

            if (m_buffer.Position == m_length && m_bitStream.IsEmpty)
                return DecompressionExitCode.EndOfStreamOccured;

            int code = m_lastPoint.ReadCode(m_bitStream);

            if (code >= MeasurementStreamCodes.NewPointId && code <= MeasurementStreamCodes.FlushBits)
            {
                if (code == MeasurementStreamCodes.NewPointId)
                {
                    MeasurementTypeCode type = (MeasurementTypeCode)m_buffer.Data[m_buffer.Position++];

                    PointMetaData point;
                    switch (type)
                    {
                        case MeasurementTypeCode.Int32:
                        case MeasurementTypeCode.UInt32:
                        case MeasurementTypeCode.Single:
                            point = new PointMetaDataInt32(m_buffer, m_bitStream, type, m_points.Count);
                            break;
                        case MeasurementTypeCode.UInt64:
                        case MeasurementTypeCode.Int64:
                        case MeasurementTypeCode.Double:
                            point = new PointMetaDataInt64(m_buffer, m_bitStream, type, m_points.Count);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }

                    NewMeasurementRegisteredTypeCode = type;
                    NewMeasurementRegisteredId = point.ReferenceId;
                    m_lastPoint.PrevNextPointId1 = point.ReferenceId;
                    m_points.Add(point);

                    NewMeasurementRegisteredMetadataLength = (int)Encoding7Bit.ReadUInt32(m_buffer.Data, ref m_buffer.Position);

                    while (NewMeasurementRegisteredMetadata.Length < NewMeasurementRegisteredMetadataLength)
                        NewMeasurementRegisteredMetadata = new byte[NewMeasurementRegisteredMetadata.Length * 2];

                    Array.Copy(m_buffer.Data, m_buffer.Position, NewMeasurementRegisteredMetadata, 0, NewMeasurementRegisteredMetadataLength);
                    m_buffer.Position += NewMeasurementRegisteredMetadataLength;

                    return DecompressionExitCode.NewMeasurementRegistered;
                }
                else if (code == MeasurementStreamCodes.UserCommand)
                {
                    UserCommand = (long)Encoding7Bit.ReadUInt64(m_buffer.Data, ref m_buffer.Position);
                    return DecompressionExitCode.UserData;
                }
                else if (code == MeasurementStreamCodes.UserCommandWithData)
                {
                    UserCommand = (long)Encoding7Bit.ReadUInt64(m_buffer.Data, ref m_buffer.Position);
                    UserCommandDataLength = (int)Encoding7Bit.ReadUInt32(m_buffer.Data, ref m_buffer.Position);
                    while (UserCommandData.Length < UserCommandDataLength)
                        UserCommandData = new byte[UserCommandData.Length * 2];

                    Array.Copy(m_buffer.Data, m_buffer.Position, UserCommandData, 0, UserCommandDataLength);
                    m_buffer.Position += UserCommandDataLength;

                    return DecompressionExitCode.UserDataWithValue;
                }
                else if (code == MeasurementStreamCodes.FlushBits)
                {
                    m_bitStream.Clear();
                    goto TryAgain;
                }
                else
                {
                    throw new Exception("Programming Error.");
                }
            }

            if (code < MeasurementStreamCodes.PointIDXOR4)
                throw new Exception("Expecting higher code");

            if (code <= MeasurementStreamCodes.PointIDXOR32)
            {
                if (code == MeasurementStreamCodes.PointIDXOR4)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_bitStream.ReadBits4();
                }
                else if (code == MeasurementStreamCodes.PointIDXOR8)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                }
                else if (code == MeasurementStreamCodes.PointIDXOR12)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_bitStream.ReadBits4();
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 4;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR16)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 8;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR20)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_bitStream.ReadBits4();
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 4;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 12;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR24)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 8;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 16;
                }
                else if (code == MeasurementStreamCodes.PointIDXOR32)
                {
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++];
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 8;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 16;
                    m_lastPoint.PrevNextPointId1 ^= m_buffer.Data[m_buffer.Position++] << 24;
                }
                else
                {
                    throw new Exception("Programming Error.");
                }

                code = m_lastPoint.ReadCode(m_bitStream);
            }

            if (code < MeasurementStreamCodes.TimeDelta1Forward)
                throw new Exception("Expecting higher code");

            ID = m_lastPoint.PrevNextPointId1;
            nextPoint = m_points[m_lastPoint.PrevNextPointId1];
            Quality = nextPoint.PrevQuality1;

            if (code <= MeasurementStreamCodes.TimeXOR7Bit)
            {
                if (code == MeasurementStreamCodes.TimeDelta1Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta1;
                }
                else if (code == MeasurementStreamCodes.TimeDelta2Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta2;
                }
                else if (code == MeasurementStreamCodes.TimeDelta3Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta3;
                }
                else if (code == MeasurementStreamCodes.TimeDelta4Forward)
                {
                    Timestamp = m_prevTimestamp1 + m_prevTimeDelta4;
                }
                else if (code == MeasurementStreamCodes.TimeDelta1Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta1;
                }
                else if (code == MeasurementStreamCodes.TimeDelta2Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta2;
                }
                else if (code == MeasurementStreamCodes.TimeDelta3Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta3;
                }
                else if (code == MeasurementStreamCodes.TimeDelta4Reverse)
                {
                    Timestamp = m_prevTimestamp1 - m_prevTimeDelta4;
                }
                else if (code == MeasurementStreamCodes.Timestamp2)
                {
                    Timestamp = m_prevTimestamp2;
                }
                else if (code == MeasurementStreamCodes.TimeXOR7Bit)
                {
                    Timestamp = m_prevTimestamp1 ^ (long)Encoding7Bit.ReadUInt64(m_buffer.Data, ref m_buffer.Position);
                }
                else
                {
                    throw new Exception("Programming Error.");
                }

                //Save the smallest delta time
                long minDelta = Math.Abs(m_prevTimestamp1 - Timestamp);

                if (minDelta < m_prevTimeDelta4 && minDelta != m_prevTimeDelta1 && minDelta != m_prevTimeDelta2 && minDelta != m_prevTimeDelta3)
                {
                    if (minDelta < m_prevTimeDelta1)
                    {
                        m_prevTimeDelta4 = m_prevTimeDelta3;
                        m_prevTimeDelta3 = m_prevTimeDelta2;
                        m_prevTimeDelta2 = m_prevTimeDelta1;
                        m_prevTimeDelta1 = minDelta;
                    }
                    else if (minDelta < m_prevTimeDelta2)
                    {
                        m_prevTimeDelta4 = m_prevTimeDelta3;
                        m_prevTimeDelta3 = m_prevTimeDelta2;
                        m_prevTimeDelta2 = minDelta;
                    }
                    else if (minDelta < m_prevTimeDelta3)
                    {
                        m_prevTimeDelta4 = m_prevTimeDelta3;
                        m_prevTimeDelta3 = minDelta;
                    }
                    else
                    {
                        m_prevTimeDelta4 = minDelta;
                    }
                }

                m_prevTimestamp2 = m_prevTimestamp1;
                m_prevTimestamp1 = Timestamp;
                code = m_lastPoint.ReadCode(m_bitStream);
            }
            else
            {
                Timestamp = m_prevTimestamp1;
            }

            if (code < MeasurementStreamCodes.Quality2)
                throw new Exception("Expecting higher code");

            if (code <= MeasurementStreamCodes.Quality7Bit32)
            {
                if (code == MeasurementStreamCodes.Quality2)
                {
                    Quality = nextPoint.PrevQuality2;
                }
                else if (code == MeasurementStreamCodes.Quality7Bit32)
                {
                    Quality = Encoding7Bit.ReadUInt32(m_buffer.Data, ref m_buffer.Position);
                }
                nextPoint.PrevQuality2 = nextPoint.PrevQuality1;
                nextPoint.PrevQuality1 = Quality;
                code = m_lastPoint.ReadCode(m_bitStream);
            }
            else
            {
                Quality = nextPoint.PrevQuality1;
            }

            if (code < 32)
                throw new Exception("Programming Error. Expecting a value quality code.");

            nextPoint.ReadValue(code, Value);
            Value.Code = nextPoint.Code;
            m_lastPoint = nextPoint;

            return DecompressionExitCode.MeasurementRead;
        }

    }

}
