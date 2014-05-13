//******************************************************************************************************
//  Equipment.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/08/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Power quality equipment and software.
    /// </summary>
    public static class Equipment
    {
        /// <summary>
        /// The ID of the WPT 5530 device.
        /// </summary>
        public static readonly Guid WPT5530 = new Guid("e2da5083-7fdb-11d3-9b39-0040052c2d28");

        /// <summary>
        /// The ID of the WPT 5540 device.
        /// </summary>
        public static readonly Guid WPT5540 = new Guid("e2da5084-7fdb-11d3-9b39-0040052c2d28");

        /// <summary>
        /// The ID of the BMI 3100 device.
        /// </summary>
        public static readonly Guid BMI3100 = new Guid("f1c04780-50fb-11d3-ac3e-444553540000");

        /// <summary>
        /// The ID of the BMI 7100 device.
        /// </summary>
        public static readonly Guid BMI7100 = new Guid("e6b51717-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the BMI 7100 device.
        /// </summary>
        public static readonly Guid BMI8010 = new Guid("e6b51718-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the BMI 7100 device.
        /// </summary>
        public static readonly Guid BMI8020 = new Guid("e6b51719-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the BMI 7100 device.
        /// </summary>
        public static readonly Guid BMI9010 = new Guid("e6b5171a-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Cooper V-Harm device.
        /// </summary>
        public static readonly Guid CooperVHarm = new Guid("e6b5171b-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Cooper V-Flicker device.
        /// </summary>
        public static readonly Guid CooperVFlicker = new Guid("e6b5171c-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the DCG EMTP device.
        /// </summary>
        public static readonly Guid DCGEMTP = new Guid("e6b5171d-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Dranetz 656 device.
        /// </summary>
        public static readonly Guid Dranetz656 = new Guid("e6b5171e-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Dranetz 658 device.
        /// </summary>
        public static readonly Guid Dranetz658 = new Guid("e6b5171f-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Electrotek test program.
        /// </summary>
        public static readonly Guid ETKTestProgram = new Guid("e6b51721-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Dranetz 8000 device.
        /// </summary>
        public static readonly Guid Dranetz8000 = new Guid("e6b51720-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Electrotek PQDIF editor.
        /// </summary>
        public static readonly Guid ETKPQDIFEditor = new Guid("e6b51722-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of Electrotek PASS.
        /// </summary>
        public static readonly Guid ETKPASS = new Guid("e6b51723-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of Electrotek Super-Harm.
        /// </summary>
        public static readonly Guid ETKSuperHarm = new Guid("e6b51724-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of Electrotek Super-Tran.
        /// </summary>
        public static readonly Guid ETKSuperTran = new Guid("e6b51725-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of Electrotek TOP.
        /// </summary>
        public static readonly Guid ETKTOP = new Guid("e6b51726-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of Electrotek PQView.
        /// </summary>
        public static readonly Guid ETKPQView = new Guid("e6b51727-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of Electrotek Harmoni.
        /// </summary>
        public static readonly Guid ETKHarmoni = new Guid("e6b51728-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Fluke CUR device.
        /// </summary>
        public static readonly Guid FlukeCUR = new Guid("e6b51729-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of IEEE COMTRADE.
        /// </summary>
        public static readonly Guid IEEECOMTRADE = new Guid("e6b5172b-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Fluke F41 device.
        /// </summary>
        public static readonly Guid FlukeF41 = new Guid("e6b5172a-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of public ATP.
        /// </summary>
        public static readonly Guid PublicATP = new Guid("e6b5172c-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Metrosonic M1 device.
        /// </summary>
        public static readonly Guid MetrosonicM1 = new Guid("e6b5172d-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Square D PowerLogic SMS device.
        /// </summary>
        public static readonly Guid SQDSMS = new Guid("e6b5172e-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Telog M1 device.
        /// </summary>
        public static readonly Guid TelogM1 = new Guid("e6b5172f-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the PML 3710 device.
        /// </summary>
        public static readonly Guid PML3710 = new Guid("085726d0-1dc0-11d0-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the PML 3720 device.
        /// </summary>
        public static readonly Guid PML3720 = new Guid("085726d1-1dc0-11d0-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the PML 3800 device.
        /// </summary>
        public static readonly Guid PML3800 = new Guid("085726d2-1dc0-11d0-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the PML 7300 device.
        /// </summary>
        public static readonly Guid PML7300 = new Guid("085726d3-1dc0-11d0-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the PML 7700 device.
        /// </summary>
        public static readonly Guid PML7700 = new Guid("085726d4-1dc0-11d0-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the PML VIP device.
        /// </summary>
        public static readonly Guid PMLVIP = new Guid("085726d5-1dc0-11d0-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the PML Log Server.
        /// </summary>
        public static readonly Guid PMLLogServer = new Guid("085726d6-1dc0-11d0-9d89-0080c72e70a3");

        /// <summary>
        /// The ID of the Met One ELT15 device.
        /// </summary>
        public static readonly Guid MetOneELT15 = new Guid("b5b5da62-e2e1-11d4-a4b3-444553540000");

        /// <summary>
        /// The ID of the PMI scanner.
        /// </summary>
        public static readonly Guid PMIScanner = new Guid("609acec1-993d-11d4-a4b3-444553540000");

        /// <summary>
        /// The ID of the AdvanTech ADAM 4017 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM4017 = new Guid("92b7977b-0c02-4766-95cf-dd379caeb417");

        /// <summary>
        /// The ID of ETK DSS.
        /// </summary>
        public static readonly Guid ETKDSS = new Guid("d347ba66-e34c-11d4-82d9-00e09872a094");

        /// <summary>
        /// The ID of the AdvanTech ADAM 4018 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM4018 = new Guid("3008151e-2317-4405-a59e-e7b3b20667a9");

        /// <summary>
        /// The ID of the AdvanTech ADAM 4018M device.
        /// </summary>
        public static readonly Guid AdvanTechADAM4018M = new Guid("3a1af807-1347-45f8-966a-f481c6ae208e");

        /// <summary>
        /// The ID of the AdvanTech ADAM 4052 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM4052 = new Guid("8bba416b-a7ec-4616-8b8f-59fed749323d");

        /// <summary>
        /// The ID of the BMI 8800 device.
        /// </summary>
        public static readonly Guid BMI8800 = new Guid("e77d1a81-1235-11d5-a390-0010a4924ecc");

        /// <summary>
        /// The ID of the Trinergi Power Quality Meter.
        /// </summary>
        public static readonly Guid TrinergiPQM = new Guid("0fd5a3aa-d73a-11d2-ac3e-444553540000");

        /// <summary>
        /// The ID of the Medcal device.
        /// </summary>
        public static readonly Guid Medcal = new Guid("f3bfa0a1-eb87-11d2-ac3e-444553540000");

        /// <summary>
        /// The ID of the GE kV Energy Meter.
        /// </summary>
        public static readonly Guid GEKV = new Guid("5202bd01-245c-11d5-a4b3-444553540000");

        /// <summary>
        /// The ID of the GE kV2 Energy Meter.
        /// </summary>
        public static readonly Guid GEKV2 = new Guid("5202bd03-245c-11d5-a4b3-444553540000");

        /// <summary>
        /// The ID of the Acumentrics Control device.
        /// </summary>
        public static readonly Guid AcumentricsControl = new Guid("5202bd04-245c-11d5-a4b3-444553540000");

        /// <summary>
        /// The ID of Electrotek Text PQDIF.
        /// </summary>
        public static readonly Guid ETKTextPQDIF = new Guid("5202bd05-245c-11d5-a4b3-444553540000");

        /// <summary>
        /// The ID of Electrotek PQWeb.
        /// </summary>
        public static readonly Guid ETKPQWeb = new Guid("5202bd06-245c-11d5-a4b3-444553540000");

        /// <summary>
        /// The ID of the QWave Power Distribution device.
        /// </summary>
        public static readonly Guid QWavePowerDistribution = new Guid("80c4a723-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the QWave Power Transmission device.
        /// </summary>
        public static readonly Guid QWavePowerTransmission = new Guid("80c4a725-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the QWave Micro device.
        /// </summary>
        public static readonly Guid QWaveMicro = new Guid("80c4a727-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the QWave TWin device.
        /// </summary>
        public static readonly Guid QWaveTWin = new Guid("80c4a728-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the QWave Premium device.
        /// </summary>
        public static readonly Guid QWavePremium = new Guid("80c4a729-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the QWave Light device.
        /// </summary>
        public static readonly Guid QWaveLight = new Guid("80c4a72a-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the QWave Nomad device.
        /// </summary>
        public static readonly Guid QWaveNomad = new Guid("80c4a72b-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the EWON 4000 device.
        /// </summary>
        public static readonly Guid EWON4000 = new Guid("80c4a762-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the Qualimetre device.
        /// </summary>
        public static readonly Guid Qualimetre = new Guid("80c4a764-2816-11d4-8ab4-004005698d26");

        /// <summary>
        /// The ID of the LEM Analyst 3Q device.
        /// </summary>
        public static readonly Guid LEMAnalyst3Q = new Guid("d567cb71-bcc0-41ee-8e8c-35851553f655");

        /// <summary>
        /// The ID of the LEM Analyst 1Q device.
        /// </summary>
        public static readonly Guid LEMAnalyst1Q = new Guid("477ecb3b-917f-4915-af99-a6c29ac18764");

        /// <summary>
        /// The ID of the LEM Analyst 2050 device.
        /// </summary>
        public static readonly Guid LEMAnalyst2050 = new Guid("9878ccab-a842-4cac-950f-6d47215bffdf");

        /// <summary>
        /// The ID of the LEM Analyst 2060 device.
        /// </summary>
        public static readonly Guid LEMAnalyst2060 = new Guid("312471a2-b586-491c-855a-ca05459a7e20");

        /// <summary>
        /// The ID of the LEM Midget 200 device.
        /// </summary>
        public static readonly Guid LEMMidget200 = new Guid("8449f6b9-10f4-40a7-a1c3-be338eb97422");

        /// <summary>
        /// The ID of the LEM MBX 300 device.
        /// </summary>
        public static readonly Guid LEMMBX300 = new Guid("d4578d61-df2b-4218-a7b1-5ef1a9bb85fa");

        /// <summary>
        /// The ID of the LEM MBX 800 device.
        /// </summary>
        public static readonly Guid LEMMBX800 = new Guid("1c14b57a-ba25-47fb-88fa-5fe5cec99e6a");

        /// <summary>
        /// The ID of the LEM MBX 601 device.
        /// </summary>
        public static readonly Guid LEMMBX601 = new Guid("1f3cda7b-2ce1-4030-a390-e3d49c5615d2");

        /// <summary>
        /// The ID of the LEM MBX 602 device.
        /// </summary>
        public static readonly Guid LEMMBX602 = new Guid("4a157756-414a-427b-9932-55760ed5f707");

        /// <summary>
        /// The ID of the LEM MBX 603 device.
        /// </summary>
        public static readonly Guid LEMMBX603 = new Guid("f7b4677b-b277-45b5-aaae-5fb39341b390");

        /// <summary>
        /// The ID of the LEM MBX 686 device.
        /// </summary>
        public static readonly Guid LEMMBX686 = new Guid("40004266-a978-4991-9ed6-c1cd730f5bf5");

        /// <summary>
        /// The ID of the LEM Perma 701 device.
        /// </summary>
        public static readonly Guid LEMPerma701 = new Guid("9b0dfd9d-d4e9-419d-ba10-c1cee6cf8f93");

        /// <summary>
        /// The ID of the LEM Perma 702 device.
        /// </summary>
        public static readonly Guid LEMPerma702 = new Guid("7f5d62ac-9fab-400f-b51a-f0f3941fb5aa");

        /// <summary>
        /// The ID of the LEM Perma 705 device.
        /// </summary>
        public static readonly Guid LEMPerma705 = new Guid("d85fea9c-14d5-45eb-831f-e03973092bd8");

        /// <summary>
        /// The ID of the LEM Perma 706 device.
        /// </summary>
        public static readonly Guid LEMPerma706 = new Guid("16d6bbfc-0b5a-4cf0-81cf-48a3105eff4f");

        /// <summary>
        /// The ID of the LEM QWave Micro device.
        /// </summary>
        public static readonly Guid LEMQWaveMicro = new Guid("e0380e52-c205-43a0-9ff4-76fbd6765f37");

        /// <summary>
        /// The ID of the LEM QWave Nomad device.
        /// </summary>
        public static readonly Guid LEMQWaveNomad = new Guid("165f145d-90c3-4591-959a-33b101d4bf8b");

        /// <summary>
        /// The ID of the LEM QWave Light device.
        /// </summary>
        public static readonly Guid LEMQWaveLight = new Guid("5198ceb9-4b4e-439c-a1c0-218c963d6a9c");

        /// <summary>
        /// The ID of the LEM QWave TWin device.
        /// </summary>
        public static readonly Guid LEMQWaveTWin = new Guid("67a42a2d-b831-4222-805e-d5fdebdd3a46");

        /// <summary>
        /// The ID of the LEM QWave Power Distribution device.
        /// </summary>
        public static readonly Guid LEMQWavePowerDistribution = new Guid("2401bf48-9db2-46ec-acde-5dedde25e54e");

        /// <summary>
        /// The ID of the LEM QWave Premium device.
        /// </summary>
        public static readonly Guid LEMQWavePremium = new Guid("6b609a29-4a64-4d1c-16e3-caef94fa56a0");

        /// <summary>
        /// The ID of the LEM QWave Power Transport device.
        /// </summary>
        public static readonly Guid LEMQWavePowerTransport = new Guid("d4422eeb-b1cd-4ba9-a7c8-5d141df40518");

        /// <summary>
        /// The ID of the LEM TOPAS LT device.
        /// </summary>
        public static readonly Guid LEMTOPASLT = new Guid("9c46483a-541e-4d66-9c10-f943abfc348a");

        /// <summary>
        /// The ID of the LEM TOPAS 1000 device.
        /// </summary>
        public static readonly Guid LEMTOPAS1000 = new Guid("459b8614-6724-48fb-b5d4-f149ed0c62f5");

        /// <summary>
        /// The ID of the LEM TOPAS 1019 device.
        /// </summary>
        public static readonly Guid LEMTOPAS1019 = new Guid("7b11408b-9d2c-407c-84a5-89440218dcf8");

        /// <summary>
        /// The ID of the LEM TOPAS 1020 device.
        /// </summary>
        public static readonly Guid LEMTOPAS1020 = new Guid("d1def77d-990f-484e-a166-f7921170a64b");

        /// <summary>
        /// The ID of the LEM TOPAS 1040 device.
        /// </summary>
        public static readonly Guid LEMTOPAS1040 = new Guid("d3cc1de2-6e6b-4b6e-ad90-10d6585f8fa2");

        /// <summary>
        /// The ID of the LEM BEN 5000 device.
        /// </summary>
        public static readonly Guid LEMBEN5000 = new Guid("a70e32b1-2f1a-4543-a684-78a4b5be34bb");

        /// <summary>
        /// The ID of the LEM BEN 6000 device.
        /// </summary>
        public static readonly Guid LEMBEN6000 = new Guid("05a4c1b5-6681-47e6-9f64-8da125dbec32");

        /// <summary>
        /// The ID of the LEM EWave device.
        /// </summary>
        public static readonly Guid LEMEWave = new Guid("e46981d5-708d-4822-97aa-fdb6f73b3af2");

        /// <summary>
        /// The ID of the LEM EWON 4000 device.
        /// </summary>
        public static readonly Guid LEMEWON4000 = new Guid("d4c0895c-fd48-498a-997c-9e70d80efb06");

        /// <summary>
        /// The ID of the LEM WPT 5510 device.
        /// </summary>
        public static readonly Guid WPT5510 = new Guid("752871de-0583-4d44-a9ae-c5fadc0144ac");

        /// <summary>
        /// The ID of the LEM WPT 5520 device.
        /// </summary>
        public static readonly Guid WPT5520 = new Guid("0b72d289-7645-40b8-946e-c3ce4f1bcd37");

        /// <summary>
        /// The ID of the LEM WPT 5530T device.
        /// </summary>
        public static readonly Guid WPT5530T = new Guid("8f88ea9e-1007-4569-ab47-756f292a23ed");

        /// <summary>
        /// The ID of the LEM WPT 5560 device.
        /// </summary>
        public static readonly Guid WPT5560 = new Guid("5fd9c0ff-4432-41b5-9a9e-9a32ba2cf005");

        /// <summary>
        /// The ID of the LEM WPT 5590 device.
        /// </summary>
        public static readonly Guid WPT5590 = new Guid("2861d5ca-23ac-4a51-a5a0-498da61d26dd");

        /// <summary>
        /// The ID of Electrotek Node Center.
        /// </summary>
        public static readonly Guid ETKNodeCenter = new Guid("c52e8460-58b4-4f1a-8469-69ca3fef9ff1");

        /// <summary>
        /// The ID of WPT Dran View.
        /// </summary>
        public static readonly Guid WPTDranView = new Guid("08d97aa1-1719-11d6-a4b3-444553540000");

        /// <summary>
        /// The ID of the AdvanTech ADAM 5017 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM5017 = new Guid("2f46263c-92ac-4717-8a08-a6177df3f611");

        /// <summary>
        /// The ID of the AdvanTech ADAM 5018 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM5018 = new Guid("cc2d3247-fe65-4db6-8206-500a23151bb2");

        /// <summary>
        /// The ID of the AdvanTech ADAM 5080 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM5080 = new Guid("6c37b63c-e770-4b85-bd32-4739d6eb9846");

        /// <summary>
        /// The ID of the AdvanTech ADAM 5052 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM5052 = new Guid("e9261dfe-3d44-47e3-ac36-3b097faa8cda");

        /// <summary>
        /// The ID of the AdvanTech ADAM 4050 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM4050 = new Guid("9212066d-ea65-477e-bf95-e4a0066d25ce");

        /// <summary>
        /// The ID of the AdvanTech ADAM 4053 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM4053 = new Guid("dc29b83f-bebe-4cf3-b3fb-00dc63626dd9");

        /// <summary>
        /// The ID of the AdvanTech ADAM 4080 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM4080 = new Guid("64fc42c6-3c90-4633-99df-2c6058214b72");

        /// <summary>
        /// The ID of the AdvanTech ADAM 5050 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM5050 = new Guid("c950a2e3-7a35-440c-8660-63f611972519");

        /// <summary>
        /// The ID of the AdvanTech ADAM 5051 device.
        /// </summary>
        public static readonly Guid AdvanTechADAM5051 = new Guid("c8f92334-a69b-4856-b253-ec2471d137d6");

        /// <summary>
        /// The ID of the ELCOM BK 550 device.
        /// </summary>
        public static readonly Guid ELCOMBK550 = new Guid("f4380a60-6f1d-11d6-9cb3-0020e010453b");

        /// <summary>
        /// Converts the given equpiment ID to a string containing the name of the equipment.
        /// </summary>
        /// <param name="equipmentID">The ID of the equipment to be converted to a string.</param>
        /// <returns>A string containing the name of the equipment with the given ID.</returns>
        public static string ToString(Guid equipmentID)
        {
            if (equipmentID == WPT5530)
                return "WPT 5530";

            if (equipmentID == WPT5540)
                return "WPT 5540";

            if (equipmentID == BMI3100)
                return "BMI 3100";

            if (equipmentID == BMI7100)
                return "BMI 7100";

            if (equipmentID == BMI8010)
                return "BMI 8010";

            if (equipmentID == BMI8020)
                return "BMI 8020";

            if (equipmentID == BMI9010)
                return "BMI 9010";

            if (equipmentID == CooperVHarm)
                return "Cooper V-Harm";

            if (equipmentID == CooperVFlicker)
                return "Cooper V-Flicker";

            if (equipmentID == DCGEMTP)
                return "DCG EMTP";

            if (equipmentID == Dranetz656)
                return "Dranetz 656";

            if (equipmentID == Dranetz658)
                return "Dranetz 658";

            if (equipmentID == ETKTestProgram)
                return "ETK Test Program";

            if (equipmentID == Dranetz8000)
                return "Dranetz 8000";

            if (equipmentID == ETKPQDIFEditor)
                return "ETK PQDIF editor";

            if (equipmentID == ETKPASS)
                return "ETK PASS";

            if (equipmentID == ETKSuperHarm)
                return "ETK Super-Harm";

            if (equipmentID == ETKSuperTran)
                return "ETK Super-Tran";

            if (equipmentID == ETKTOP)
                return "TOP";

            if (equipmentID == ETKPQView)
                return "PQView";

            if (equipmentID == ETKHarmoni)
                return "ETK Harmoni";

            if (equipmentID == FlukeCUR)
                return "Fluke CUR";

            if (equipmentID == IEEECOMTRADE)
                return "IEEE COMTRADE";

            if (equipmentID == FlukeF41)
                return "Fluke F41";

            if (equipmentID == PublicATP)
                return "Public ATP";

            if (equipmentID == MetrosonicM1)
                return "Metrosonic M1";

            if (equipmentID == SQDSMS)
                return "SMS";

            if (equipmentID == TelogM1)
                return "Telog M1";

            if (equipmentID == PML3710)
                return "PML 3710";

            if (equipmentID == PML3720)
                return "PML 3720";

            if (equipmentID == PML3800)
                return "PML 3800";

            if (equipmentID == PML7300)
                return "PML 7300";

            if (equipmentID == PML7700)
                return "PML 7700";

            if (equipmentID == PMLVIP)
                return "PML VIP";

            if (equipmentID == PMLLogServer)
                return "PML Log Server";

            if (equipmentID == MetOneELT15)
                return "ELT15";

            if (equipmentID == PMIScanner)
                return "PMI Scanner";

            if (equipmentID == AdvanTechADAM4017)
                return "ADAM 4017";

            if (equipmentID == ETKDSS)
                return "ETK DSS";

            if (equipmentID == AdvanTechADAM4018)
                return "ADAM 4018";

            if (equipmentID == AdvanTechADAM4018M)
                return "ADAM 4018M";

            if (equipmentID == AdvanTechADAM4052)
                return "ADAM 4052";

            if (equipmentID == BMI8800)
                return "BMI 8800";

            if (equipmentID == TrinergiPQM)
                return "Trinergi PQM";

            if (equipmentID == Medcal)
                return "Medcal";

            if (equipmentID == GEKV)
                return "GE kV Energy Meter";

            if (equipmentID == GEKV2)
                return "GE kV2 Energy Meter";

            if (equipmentID == AcumentricsControl)
                return "Acumentrics Control";

            if (equipmentID == ETKPQWeb)
                return "PQWeb";

            if (equipmentID == QWavePowerDistribution)
                return "QWave Power Distribution";

            if (equipmentID == QWavePowerTransmission)
                return "QWave Power Transmission";

            if (equipmentID == QWaveMicro)
                return "QWave Micro";

            if (equipmentID == QWaveTWin)
                return "QWave TWin";

            if (equipmentID == QWavePremium)
                return "QWave Premium";

            if (equipmentID == QWaveLight)
                return "QWave Light";

            if (equipmentID == QWaveNomad)
                return "QWave Nomad";

            if (equipmentID == EWON4000)
                return "EWON 4000";

            if (equipmentID == Qualimetre)
                return "Qualimetre";

            if (equipmentID == LEMAnalyst3Q)
                return "LEM Analyst 3Q";

            if (equipmentID == LEMAnalyst1Q)
                return "LEM Analyst 1Q";

            if (equipmentID == LEMAnalyst2050)
                return "LEM Analyst 2050";

            if (equipmentID == LEMAnalyst2060)
                return "LEM Analyst 2060";

            if (equipmentID == LEMMidget200)
                return "LEM Midget 200";

            if (equipmentID == LEMMBX300)
                return "MBX 300";

            if (equipmentID == LEMMBX800)
                return "MBX 800";

            if (equipmentID == LEMMBX601)
                return "MBX 601";

            if (equipmentID == LEMMBX602)
                return "MBX 602";

            if (equipmentID == LEMMBX603)
                return "MBX 603";

            if (equipmentID == LEMMBX686)
                return "MBX 686";

            if (equipmentID == LEMPerma701)
                return "Perma 701";

            if (equipmentID == LEMPerma702)
                return "Perma 702";

            if (equipmentID == LEMPerma705)
                return "Perma 705";

            if (equipmentID == LEMPerma706)
                return "Perma 706";

            if (equipmentID == LEMQWaveMicro)
                return "QWave Micro";

            if (equipmentID == LEMQWaveNomad)
                return "QWave Nomad";

            if (equipmentID == LEMQWaveLight)
                return "QWave Light";

            if (equipmentID == LEMQWaveTWin)
                return "QWave TWin";

            if (equipmentID == LEMQWavePowerDistribution)
                return "QWave Power Distribution";

            if (equipmentID == LEMQWavePremium)
                return "QWave Premium";

            if (equipmentID == LEMQWavePowerTransport)
                return "QWave Power Transport";

            if (equipmentID == LEMTOPASLT)
                return "TOPAS LT";

            if (equipmentID == LEMTOPAS1000)
                return "TOPAS 1000";

            if (equipmentID == LEMTOPAS1019)
                return "TOPAS 1019";

            if (equipmentID == LEMTOPAS1020)
                return "TOPAS 1020";

            if (equipmentID == LEMTOPAS1040)
                return "TOPAS 1040";

            if (equipmentID == LEMBEN5000)
                return "BEN 5000";

            if (equipmentID == LEMBEN6000)
                return "BEN 6000";

            if (equipmentID == LEMEWave)
                return "LEM EWave";

            if (equipmentID == LEMEWON4000)
                return "EWON 4000";

            if (equipmentID == WPT5510)
                return "WPT 5510";

            if (equipmentID == WPT5520)
                return "WPT 5520";

            if (equipmentID == WPT5530T)
                return "WPT 5530T";

            if (equipmentID == WPT5560)
                return "WPT 5560";

            if (equipmentID == WPT5590)
                return "WPT 5590";

            if (equipmentID == ETKNodeCenter)
                return "ETK Node Center";

            if (equipmentID == WPTDranView)
                return "WPT Dran View";

            if (equipmentID == AdvanTechADAM5017)
                return "ADAM 5017";

            if (equipmentID == AdvanTechADAM5018)
                return "ADAM 5018";

            if (equipmentID == AdvanTechADAM5080)
                return "ADAM 5080";

            if (equipmentID == AdvanTechADAM5052)
                return "ADAM 5052";

            if (equipmentID == AdvanTechADAM4050)
                return "ADAM 4050";

            if (equipmentID == AdvanTechADAM4053)
                return "ADAM 4053";

            if (equipmentID == AdvanTechADAM4080)
                return "ADAM 4080";

            if (equipmentID == AdvanTechADAM5050)
                return "ADAM 5050";

            if (equipmentID == AdvanTechADAM5051)
                return "ADAM 5051";

            if (equipmentID == ELCOMBK550)
                return "BK 550";

            return equipmentID.ToString();
        }
    }
}
