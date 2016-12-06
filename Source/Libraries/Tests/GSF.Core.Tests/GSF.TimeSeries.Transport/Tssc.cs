//******************************************************************************************************
//  Tssc.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/04/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GSF.Collections;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport.TSSC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OGE.MeasurementStream;

namespace GSF.Core.Tests.GSF.Threading
{
    [TestClass]
    public class TsscTest
    {
        [TestMethod]
        public void TestReading()
        {
            int pointCount = 0;
            var reading = new TsscDecoder();
            reading.SetBuffer(new byte[10], 5, 0);

            ushort id;
            long timestamp;
            uint quality;
            float value;

            while (reading.TryGetMeasurement(out id, out timestamp, out quality, out value))
            {
                throw new ArgumentOutOfRangeException();
            }

            System.Console.WriteLine(pointCount);
        }


        [TestMethod]
        public void TestReading1()
        {
            //Just adds metadata
            var r = new Randomizer(3);

            byte[] buffer = new byte[65536];
            int endPosition;
            {
                var comp = new TsscEncoder();
                comp.SetBuffer(buffer, 0, 65530);
                for (int x = 0; x < 1000; x++)
                {
                    if (!comp.TryAddMeasurement(r.GetUInt16(), r.GetDate(), r.GetUInt16(), r.GetUInt16()))
                        throw new Exception();
                }
                endPosition = comp.FinishBlock();
            }

            r.Reset();

            {
                ushort id;
                long timestamp;
                uint quality;
                float value;

                var reading = new TsscDecoder();
                reading.SetBuffer(buffer, 0, endPosition);

                for (int x = 0; x < 1000; x++)
                {
                    if (!reading.TryGetMeasurement(out id, out timestamp, out quality, out value))
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    if (id != r.GetUInt16())
                        throw new Exception();
                    if (timestamp != r.GetDate())
                        throw new Exception();
                    if (quality != r.GetUInt16())
                        throw new Exception();
                    if (value != r.GetUInt16())
                        throw new Exception();
                }

                if (reading.TryGetMeasurement(out id, out timestamp, out quality, out value))
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (reading.TryGetMeasurement(out id, out timestamp, out quality, out value))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [TestMethod]
        public unsafe void TestReading2()
        {
            var comp = new TsscEncoder();
            var reading = new TsscDecoder();

            for (int p = 0; p < 32; p++)
            {
                //Just adds metadata
                var r = new Randomizer(3);

                byte[] buffer = new byte[65536];
                int endPosition;
                {
                    comp.SetBuffer(buffer, 0, 65530);
                    for (int x = 0; x < 1000; x++)
                    {
                        if (!comp.TryAddMeasurement(r.GetUInt16(), r.GetDate(), r.GetUInt16(), r.Getfloat(p)))
                            throw new Exception();
                    }
                    endPosition = comp.FinishBlock();
                }

                r.Reset();

                {
                    ushort id;
                    long timestamp;
                    uint quality;
                    float value;

                    reading.SetBuffer(buffer, 0, endPosition);

                    for (int x = 0; x < 1000; x++)
                    {
                        if (!reading.TryGetMeasurement(out id, out timestamp, out quality, out value))
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                        if (id != r.GetUInt16())
                            throw new Exception();
                        if (timestamp != r.GetDate())
                            throw new Exception();
                        if (quality != r.GetUInt16())
                            throw new Exception();
                        float v = r.Getfloat(p);
                        if (*(int*)&value != *(int*)&v)
                            throw new Exception();
                    }

                    if (reading.TryGetMeasurement(out id, out timestamp, out quality, out value))
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    if (reading.TryGetMeasurement(out id, out timestamp, out quality, out value))
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }

        }

        private class OrigReader
        {
            private readonly Dictionary<Guid, ushort> m_lookup;
            private MeasurementStreamFileReading m_reader = new MeasurementStreamFileReading();
            private List<ushort> m_measurements = new List<ushort>();

            public OrigReader(byte[] data, Dictionary<Guid, ushort> lookup)
            {
                m_lookup = lookup;
                m_reader.Load(data);
            }

            public bool ReadNextMeasurement(out ushort id, out long timestamp, out uint quality, out float value)
            {
                while (true)
                {
                    switch (m_reader.GetMeasurement())
                    {
                        case DecompressionExitCode.NewMeasurementRegistered:
                            if (m_reader.NewMeasurementRegisteredId != m_measurements.Count)
                            {
                                throw new NotSupportedException();
                            }
                            ushort newID;
                            Guid guid = GuidExtensions.ToLittleEndianGuid(m_reader.NewMeasurementRegisteredMetadata);
                            if (!m_lookup.TryGetValue(guid, out newID))
                            {
                                newID = (ushort)m_lookup.Count;
                                m_lookup.Add(guid, newID);
                            }
                            m_measurements.Add(newID);
                            break;
                        case DecompressionExitCode.UserData:
                        case DecompressionExitCode.UserDataWithValue:
                            break;
                        case DecompressionExitCode.EndOfStreamOccured:
                            id = 0;
                            timestamp = 0;
                            quality = 0;
                            value = 0;
                            return false;
                        case DecompressionExitCode.MeasurementRead:
                            id = m_measurements[m_reader.ID];
                            timestamp = m_reader.Timestamp;
                            quality = m_reader.Quality;
                            value = m_reader.Value.ValueSingle;
                            return true;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

            }
        }



        [TestMethod]
        public unsafe void TestActualData()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long totalSize = 0;
            long measurementsProcessed = 0;
            int quitAfterCount = 0;
            Dictionary<Guid, ushort> idLookup = new Dictionary<Guid, ushort>();

            var encoder = new TsscEncoder();
            var encoder2 = new TsscEncoder();
            var decoder = new TsscDecoder();

            foreach (var file in Directory.GetFiles(@"D:\TsscTest", "*.PhasorStream"))
            {
                if (quitAfterCount == 10)
                    return;
                quitAfterCount++;

                byte[] data = File.ReadAllBytes(file);
                var reader1 = new OrigReader(data, idLookup);
                var reader2 = new OrigReader(data, idLookup);

                byte randomValue = Security.Cryptography.Random.ByteBetween(1, 200);
                byte[] writeBuffer = new byte[randomValue * 1024];
                encoder.SetBuffer(writeBuffer, randomValue, writeBuffer.Length - randomValue);

                ushort id;
                long timestamp;
                uint quality;
                float value;

                ushort idOld = 0;
                long timestampOld = 0;
                uint qualityOld = 0;
                float valueOld = 0;

                while (reader1.ReadNextMeasurement(out id, out timestamp, out quality, out value))
                {
                    measurementsProcessed++;
                    if (!encoder.TryAddMeasurement(id, timestamp, quality, value))
                    {
                        totalSize += encoder.FinishBlock();
                        TestEncoding(encoder, decoder, encoder2, writeBuffer, randomValue, reader2, idOld, timestampOld, qualityOld, valueOld);

                        randomValue = Security.Cryptography.Random.ByteBetween(1, 200);
                        writeBuffer = new byte[randomValue * 1024];
                        encoder.SetBuffer(writeBuffer, randomValue, writeBuffer.Length - randomValue);
                        if (!encoder.TryAddMeasurement(id, timestamp, quality, value))
                        {
                            throw new Exception();
                        }
                    }

                    idOld = id;
                    timestampOld = timestamp;
                    qualityOld = quality;
                    valueOld = value;
                }

                totalSize += encoder.FinishBlock();
                TestEncoding(encoder, decoder, encoder2, writeBuffer, randomValue, reader2, idOld, timestampOld, qualityOld, valueOld);

                System.Console.WriteLine(measurementsProcessed.ToString("N0") + " " + file + " " + sw.Elapsed.TotalSeconds.ToString("N1") + " " + (totalSize * 8 / (double)measurementsProcessed).ToString("N2"));

            }

        }

        [TestMethod]
        public unsafe void TestSmallerSegments()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long totalSize = 0;
            long measurementsProcessed = 0;
            int quitAfterCount = 0;
            Dictionary<Guid, ushort> idLookup = new Dictionary<Guid, ushort>();

            var encoder = new TsscEncoder();
            var encoder2 = new TsscEncoder();
            var decoder = new TsscDecoder();

            foreach (var file in Directory.GetFiles(@"D:\TsscTest", "*.PhasorStream"))
            {
                if (quitAfterCount == 1)
                    return;
                quitAfterCount++;

                byte[] data = File.ReadAllBytes(file);
                var reader1 = new OrigReader(data, idLookup);
                var reader2 = new OrigReader(data, idLookup);

                byte randomValue = Security.Cryptography.Random.ByteBetween(1, 200);
                byte[] writeBuffer = new byte[randomValue * 1024];
                encoder.SetBuffer(writeBuffer, randomValue, writeBuffer.Length - randomValue);

                ushort id;
                long timestamp;
                uint quality;
                float value;

                ushort idOld = 0;
                long timestampOld = 0;
                uint qualityOld = 0;
                float valueOld = 0;

                int quitAfter = Security.Cryptography.Random.ByteBetween(0, 40);
                while (reader1.ReadNextMeasurement(out id, out timestamp, out quality, out value))
                {
                    measurementsProcessed++;
                    if (quitAfter <= 0 || !encoder.TryAddMeasurement(id, timestamp, quality, value))
                    {
                        totalSize += encoder.FinishBlock();
                        TestEncoding(encoder, decoder, encoder2, writeBuffer, randomValue, reader2, idOld, timestampOld, qualityOld, valueOld);

                        randomValue = Security.Cryptography.Random.ByteBetween(1, 200);
                        writeBuffer = new byte[randomValue * 1024];
                        encoder.SetBuffer(writeBuffer, randomValue, writeBuffer.Length - randomValue);
                        if (!encoder.TryAddMeasurement(id, timestamp, quality, value))
                        {
                            throw new Exception();
                        }

                        quitAfter = Security.Cryptography.Random.ByteBetween(0, 40);

                    }

                    quitAfter--;

                    idOld = id;
                    timestampOld = timestamp;
                    qualityOld = quality;
                    valueOld = value;
                }

                totalSize += encoder.FinishBlock();
                TestEncoding(encoder, decoder, encoder2, writeBuffer, randomValue, reader2, idOld, timestampOld, qualityOld, valueOld);

                System.Console.WriteLine(measurementsProcessed.ToString("N0") + " " + file + " " + sw.Elapsed.TotalSeconds.ToString("N1") + " " + (totalSize * 8 / (double)measurementsProcessed).ToString("N2"));
            }

        }

        private static void TestEncoding(TsscEncoder encoder, TsscDecoder decoder, TsscEncoder encoder2, byte[] writeBuffer, byte randomValue, OrigReader reader2, ushort idOld, long timestampOld, uint qualityOld, float valueOld)
        {
            ushort id2;
            long timestamp2;
            uint quality2;
            float value2;

            ushort id3 = 0;
            long timestamp3 = 0;
            uint quality3 = 0;
            float value3 = 0;
            int flushPosition;

            flushPosition = encoder.FinishBlock();
            Array.Clear(writeBuffer, flushPosition, writeBuffer.Length - flushPosition);

            byte[] writeBuffer2 = new byte[writeBuffer.Length];
            encoder2.SetBuffer(writeBuffer2, randomValue, writeBuffer2.Length - randomValue);

            decoder.SetBuffer(writeBuffer, randomValue, flushPosition - randomValue);

            while (decoder.TryGetMeasurement(out id2, out timestamp2, out quality2, out value2))
            {
                if (!encoder2.TryAddMeasurement(id2, timestamp2, quality2, value2))
                {
                    throw new Exception();
                }
                if (!reader2.ReadNextMeasurement(out id3, out timestamp3, out quality3, out value3))
                {
                    throw new Exception();
                }
                if (id2 != id3)
                    throw new Exception();
                if (timestamp2 != timestamp3)
                    throw new Exception();
                if (quality2 != quality3)
                    throw new Exception();
                if (value2 != value3)
                    throw new Exception();
            }

            if (idOld != id3)
                throw new Exception();
            if (timestampOld != timestamp3)
                throw new Exception();
            if (qualityOld != quality3)
                throw new Exception();
            if (valueOld != value3)
                throw new Exception();

            if (flushPosition != encoder2.FinishBlock())
                throw new Exception();

            for (int x = 0; x < writeBuffer.Length; x++)
            {
                if (writeBuffer[x] != writeBuffer2[x])
                    throw new Exception();
            }

        }

        public class Randomizer
        {
            private Random m_r;
            private int m_seed;
            private DateTime m_time;

            public void Reset()
            {
                m_r = new Random(m_seed);
                m_time = new DateTime(2016, 1, 1).AddTicks(m_r.Next());
            }

            public Randomizer(int seed)
            {
                m_seed = seed;
                Reset();
            }

            public byte[] GetBytes(int length)
            {
                byte[] data = new byte[length];
                m_r.NextBytes(data);
                return data;
            }

            public ushort GetUInt16()
            {
                return (ushort)m_r.Next(0, ushort.MaxValue + 1);
            }

            public int GetInt(int minValue, int maxValue)
            {
                return m_r.Next(minValue, maxValue);
            }

            public unsafe float Getfloat(int precision)
            {
                uint value = (uint)m_r.Next() >> precision;
                return *(float*)&value;
            }

            public long GetIntX(int bits)
            {
                byte[] data = new byte[8];
                m_r.NextBytes(data);
                return LittleEndian.ToInt64(data, 0) & ((1L << bits) - 1);
            }

            public long GetDate()
            {
                return m_time.Ticks + m_r.Next();
            }

        }
    }
}
