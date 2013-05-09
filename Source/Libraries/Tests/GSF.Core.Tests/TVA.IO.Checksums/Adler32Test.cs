//******************************************************************************************************
//  Adler32Test.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  04/11/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Text;
using GSF.IO.Checksums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.IO.Checksums
{
    /// <summary>
    /// This is a test class for Adler32 and is intended
    /// to contain all Adler32 Unit Tests
    /// </summary>
    [TestClass]
    public class Adler32Test
    {
        // Eclipse Public License used to test checksum algorithm.
        private const string License = "EclipsePublicLicense-v1.0THEACCOMPANYINGPROGRAMISPROVIDEDUNDERTHETERMSOFTHISECLIPSEPUBLICLICENSE(\"AGREEMENT\").ANYUSE,REPRODUCTIONORDISTRIBUTIONOFTHEPROGRAMCONSTITUTESRECIPIENT'SACCEPTANCEOFTHISAGREEMENT.1.DEFINITIONS\"Contribution\"means:a)inthecaseoftheinitialContributor,theinitialcodeanddocumentationdistributedunderthisAgreement,andb)inthecaseofeachsubsequentContributor:i)changestotheProgram,andii)additionstotheProgram;wheresuchchangesand/oradditionstotheProgramoriginatefromandaredistributedbythatparticularContributor.AContribution'originates'fromaContributorifitwasaddedtotheProgrambysuchContributoritselforanyoneactingonsuchContributor'sbehalf.ContributionsdonotincludeadditionstotheProgramwhich:(i)areseparatemodulesofsoftwaredistributedinconjunctionwiththeProgramundertheirownlicenseagreement,and(ii)arenotderivativeworksoftheProgram.\"Contributor\"meansanypersonorentitythatdistributestheProgram.\"LicensedPatents\"meanpatentclaimslicensablebyaContributorwhicharenecessarilyinfringedbytheuseorsaleofitsContributionaloneorwhencombinedwiththeProgram.\"Program\"meanstheContributionsdistributedinaccordancewiththisAgreement.\"Recipient\"meansanyonewhoreceivestheProgramunderthisAgreement,includingallContributors.2.GRANTOFRIGHTSa)SubjecttothetermsofthisAgreement,eachContributorherebygrantsRecipientanon-exclusive,worldwide,royalty-freecopyrightlicensetoreproduce,preparederivativeworksof,publiclydisplay,publiclyperform,distributeandsublicensetheContributionofsuchContributor,ifany,andsuchderivativeworks,insourcecodeandobjectcodeform.b)SubjecttothetermsofthisAgreement,eachContributorherebygrantsRecipientanon-exclusive,worldwide,royalty-freepatentlicenseunderLicensedPatentstomake,use,sell,offertosell,importandotherwisetransfertheContributionofsuchContributor,ifany,insourcecodeandobjectcodeform.ThispatentlicenseshallapplytothecombinationoftheContributionandtheProgramif,atthetimetheContributionisaddedbytheContributor,suchadditionoftheContributioncausessuchcombinationtobecoveredbytheLicensedPatents.ThepatentlicenseshallnotapplytoanyothercombinationswhichincludetheContribution.Nohardwareperseislicensedhereunder.c)RecipientunderstandsthatalthougheachContributorgrantsthelicensestoitsContributionssetforthherein,noassurancesareprovidedbyanyContributorthattheProgramdoesnotinfringethepatentorotherintellectualpropertyrightsofanyotherentity.EachContributordisclaimsanyliabilitytoRecipientforclaimsbroughtbyanyotherentitybasedoninfringementofintellectualpropertyrightsorotherwise.Asaconditiontoexercisingtherightsandlicensesgrantedhereunder,eachRecipientherebyassumessoleresponsibilitytosecureanyotherintellectualpropertyrightsneeded,ifany.Forexample,ifathirdpartypatentlicenseisrequiredtoallowRecipienttodistributetheProgram,itisRecipient'sresponsibilitytoacquirethatlicensebeforedistributingtheProgram.d)EachContributorrepresentsthattoitsknowledgeithassufficientcopyrightrightsinitsContribution,ifany,tograntthecopyrightlicensesetforthinthisAgreement.3.REQUIREMENTSAContributormaychoosetodistributethePrograminobjectcodeformunderitsownlicenseagreement,providedthat:a)itcomplieswiththetermsandconditionsofthisAgreement;andb)itslicenseagreement:i)effectivelydisclaimsonbehalfofallContributorsallwarrantiesandconditions,expressandimplied,includingwarrantiesorconditionsoftitleandnon-infringement,andimpliedwarrantiesorconditionsofmerchantabilityandfitnessforaparticularpurpose;ii)effectivelyexcludesonbehalfofallContributorsallliabilityfordamages,includingdirect,indirect,special,incidentalandconsequentialdamages,suchaslostprofits;iii)statesthatanyprovisionswhichdifferfromthisAgreementareofferedbythatContributoraloneandnotbyanyotherparty;andiv)statesthatsourcecodefortheProgramisavailablefromsuchContributor,andinformslicenseeshowtoobtainitinareasonablemanneronorthroughamediumcustomarilyusedforsoftwareexchange.WhentheProgramismadeavailableinsourcecodeform:a)itmustbemadeavailableunderthisAgreement;andb)acopyofthisAgreementmustbeincludedwitheachcopyoftheProgram.ContributorsmaynotremoveoralteranycopyrightnoticescontainedwithintheProgram.EachContributormustidentifyitselfastheoriginatorofitsContribution,ifany,inamannerthatreasonablyallowssubsequentRecipientstoidentifytheoriginatoroftheContribution.4.COMMERCIALDISTRIBUTIONCommercialdistributorsofsoftwaremayacceptcertainresponsibilitieswithrespecttoendusers,businesspartnersandthelike.WhilethislicenseisintendedtofacilitatethecommercialuseoftheProgram,theContributorwhoincludesthePrograminacommercialproductofferingshoulddosoinamannerwhichdoesnotcreatepotentialliabilityforotherContributors.Therefore,ifaContributorincludesthePrograminacommercialproductoffering,suchContributor(\"CommercialContributor\")herebyagreestodefendandindemnifyeveryotherContributor(\"IndemnifiedContributor\")againstanylosses,damagesandcosts(collectively\"Losses\")arisingfromclaims,lawsuitsandotherlegalactionsbroughtbyathirdpartyagainsttheIndemnifiedContributortotheextentcausedbytheactsoromissionsofsuchCommercialContributorinconnectionwithitsdistributionofthePrograminacommercialproductoffering.TheobligationsinthissectiondonotapplytoanyclaimsorLossesrelatingtoanyactualorallegedintellectualpropertyinfringement.Inordertoqualify,anIndemnifiedContributormust:a)promptlynotifytheCommercialContributorinwritingofsuchclaim,andb)allowtheCommercialContributortocontrol,andcooperatewiththeCommercialContributorin,thedefenseandanyrelatedsettlementnegotiations.TheIndemnifiedContributormayparticipateinanysuchclaimatitsownexpense.Forexample,aContributormightincludethePrograminacommercialproductoffering,ProductX.ThatContributoristhenaCommercialContributor.IfthatCommercialContributorthenmakesperformanceclaims,orofferswarrantiesrelatedtoProductX,thoseperformanceclaimsandwarrantiesaresuchCommercialContributor'sresponsibilityalone.Underthissection,theCommercialContributorwouldhavetodefendclaimsagainsttheotherContributorsrelatedtothoseperformanceclaimsandwarranties,andifacourtrequiresanyotherContributortopayanydamagesasaresult,theCommercialContributormustpaythosedamages.5.NOWARRANTYEXCEPTASEXPRESSLYSETFORTHINTHISAGREEMENT,THEPROGRAMISPROVIDEDONAN\"ASIS\"BASIS,WITHOUTWARRANTIESORCONDITIONSOFANYKIND,EITHEREXPRESSORIMPLIEDINCLUDING,WITHOUTLIMITATION,ANYWARRANTIESORCONDITIONSOFTITLE,NON-INFRINGEMENT,MERCHANTABILITYORFITNESSFORAPARTICULARPURPOSE.EachRecipientissolelyresponsiblefordeterminingtheappropriatenessofusinganddistributingtheProgramandassumesallrisksassociatedwithitsexerciseofrightsunderthisAgreement,includingbutnotlimitedtotherisksandcostsofprogramerrors,compliancewithapplicablelaws,damagetoorlossofdata,programsorequipment,andunavailabilityorinterruptionofoperations.6.DISCLAIMEROFLIABILITYEXCEPTASEXPRESSLYSETFORTHINTHISAGREEMENT,NEITHERRECIPIENTNORANYCONTRIBUTORSSHALLHAVEANYLIABILITYFORANYDIRECT,INDIRECT,INCIDENTAL,SPECIAL,EXEMPLARY,ORCONSEQUENTIALDAMAGES(INCLUDINGWITHOUTLIMITATIONLOSTPROFITS),HOWEVERCAUSEDANDONANYTHEORYOFLIABILITY,WHETHERINCONTRACT,STRICTLIABILITY,ORTORT(INCLUDINGNEGLIGENCEOROTHERWISE)ARISINGINANYWAYOUTOFTHEUSEORDISTRIBUTIONOFTHEPROGRAMORTHEEXERCISEOFANYRIGHTSGRANTEDHEREUNDER,EVENIFADVISEDOFTHEPOSSIBILITYOFSUCHDAMAGES.7.GENERALIfanyprovisionofthisAgreementisinvalidorunenforceableunderapplicablelaw,itshallnotaffectthevalidityorenforceabilityoftheremainderofthetermsofthisAgreement,andwithoutfurtheractionbythepartieshereto,suchprovisionshallbereformedtotheminimumextentnecessarytomakesuchprovisionvalidandenforceable.IfRecipientinstitutespatentlitigationagainstanyentity(includingacross-claimorcounterclaiminalawsuit)allegingthattheProgramitself(excludingcombinationsoftheProgramwithothersoftwareorhardware)infringessuchRecipient'spatent(s),thensuchRecipient'srightsgrantedunderSection2(b)shallterminateasofthedatesuchlitigationisfiled.AllRecipient'srightsunderthisAgreementshallterminateifitfailstocomplywithanyofthematerialtermsorconditionsofthisAgreementanddoesnotcuresuchfailureinareasonableperiodoftimeafterbecomingawareofsuchnoncompliance.IfallRecipient'srightsunderthisAgreementterminate,RecipientagreestoceaseuseanddistributionoftheProgramassoonasreasonablypracticable.However,Recipient'sobligationsunderthisAgreementandanylicensesgrantedbyRecipientrelatingtotheProgramshallcontinueandsurvive.EveryoneispermittedtocopyanddistributecopiesofthisAgreement,butinordertoavoidinconsistencytheAgreementiscopyrightedandmayonlybemodifiedinthefollowingmanner.TheAgreementStewardreservestherighttopublishnewversions(includingrevisions)ofthisAgreementfromtimetotime.NooneotherthantheAgreementStewardhastherighttomodifythisAgreement.TheEclipseFoundationistheinitialAgreementSteward.TheEclipseFoundationmayassigntheresponsibilitytoserveastheAgreementStewardtoasuitableseparateentity.EachnewversionoftheAgreementwillbegivenadistinguishingversionnumber.TheProgram(includingContributions)mayalwaysbedistributedsubjecttotheversionoftheAgreementunderwhichitwasreceived.Inaddition,afteranewversionoftheAgreementispublished,ContributormayelecttodistributetheProgram(includingitsContributions)underthenewversion.ExceptasexpresslystatedinSections2(a)and2(b)above,RecipientreceivesnorightsorlicensestotheintellectualpropertyofanyContributorunderthisAgreement,whetherexpressly,byimplication,estoppelorotherwise.AllrightsintheProgramnotexpresslygrantedunderthisAgreementarereserved.ThisAgreementisgovernedbythelawsoftheStateofNewYorkandtheintellectualpropertylawsoftheUnitedStatesofAmerica.NopartytothisAgreementwillbringalegalactionunderthisAgreementmorethanoneyearafterthecauseofactionarose.Eachpartywaivesitsrightstoajurytrialinanyresultinglitigation.";

        // UTF-8 encoding of the Eclipse Public License.
        private readonly byte[] LicenseData = Encoding.UTF8.GetBytes(License);

        // Precomputed checksum calculated by a separate checksum calculator.
        private const uint LicenseChecksum = 0xE239CA42u;

        /// <summary>
        /// Tests the extension method which utilizes the most straightforward use of the checksum API.
        /// </summary>
        [TestMethod]
        public void ExtensionMethodTest()
        {
            Assert.AreEqual(LicenseChecksum, LicenseData.Adler32Checksum(0, LicenseData.Length));
        }

        /// <summary>
        /// Tests the Update method that accepts a single byte as a parameter.
        /// </summary>
        [TestMethod]
        public void UpdateByteTest()
        {
            Adler32 checksum = new Adler32();

            foreach (byte d in LicenseData)
                checksum.Update(d);

            Assert.AreEqual(LicenseChecksum, checksum.Value);
        }

        /// <summary>
        /// Tests the use of calls to the two separate implementations of Update in one checksum calculation.
        /// </summary>
        [TestMethod]
        public void MixedUpdateTest()
        {
            Adler32 checksum = new Adler32();
            int i = 0;

            checksum.Reset();

            while (i < LicenseData.Length / 4)
                checksum.Update(LicenseData[i++]);

            for (int j = 0; j < 2; j++)
            {
                checksum.Update(LicenseData, i, LicenseData.Length / 4);
                i += LicenseData.Length / 4;
            }

            while (i < LicenseData.Length)
                checksum.Update(LicenseData[i++]);

            Assert.AreEqual(LicenseChecksum, checksum.Value);
        }
    }
}
