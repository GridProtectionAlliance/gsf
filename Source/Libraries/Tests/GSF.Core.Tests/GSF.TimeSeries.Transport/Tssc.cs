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
using System.Threading;
using GSF.Collections;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport.TSSC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
