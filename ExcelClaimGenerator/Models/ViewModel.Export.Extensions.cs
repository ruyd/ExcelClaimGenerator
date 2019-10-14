using System;
using System.Collections.Generic;
using System.Linq;
using OopFactory.X12.Parsing.Model;
using OopFactory.X12.Parsing.Model.Typed;
 

namespace ExcelClaimGenerator
{
    public partial class ViewModel
    {
        private DateTime fillerBirthDate = new DateTime(1999, 11, 1);
        public Transaction CreateProfessionalTransaction(ClaimLine claim, FunctionGroup fg, FileConfigModel config, int parentControlNumber = 1)
        {       
            var controlNumber = parentControlNumber.ToString("D9");  // = (fg.Transactions.Count + 1).ToString("D9");

            var t = fg.AddTransaction("837", controlNumber);
            
            try
            {
                if (!string.IsNullOrWhiteSpace(fg.VersionIdentifierCode))
                    t.SetElement(3, fg.VersionIdentifierCode);
            }
            catch
            { 
            
            }

            var bht = t.AddSegment("BHT");
            bht.SetElement(1, "0019");          
            bht.SetElement(2, "00");            
            bht.SetElement(3, t.ControlNumber);
            bht.SetElement(4, DateTime.UtcNow.ToString("yyyyMMdd"));
            bht.SetElement(5, DateTime.UtcNow.ToString("hhmm"));
            bht.SetElement(6, "CH");
            
 
            var submitterProvider =
                new ProviderInfo()
                {
                    FirstName = "",
                    LastName = "SUBMITTER NAME",
                    ProviderId = 1,
                    ProviderTypeId = 2,
                    AlternateId = "ID",
                    WorkPhoneNumber = "3050000000"
                };

            var sender = t.AddLoop(new TypedLoopNM1("41"));
            sender.NM102_EntityTypeQualifier = EntityTypeQualifier.Person;

            if (submitterProvider.ProviderTypeId == 2)
            {
                sender.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;
                sender.NM103_NameLastOrOrganizationName = submitterProvider.FullName.Clean(35); 
            }
            else
            {
                if (submitterProvider?.LastName != null)
                {
                    sender.NM103_NameLastOrOrganizationName = submitterProvider.LastName.Clean(35);
                    sender.NM104_NameFirst = submitterProvider.FirstName.Clean(35);
                }
            }

            //SUBMITTER
            sender.NM108_IdCodeQualifier = "46"; //ETIN 
            if (!String.IsNullOrWhiteSpace(config.SenderIDQualifier) && config.SenderIDQualifier != "30")
                sender.NM108_IdCodeQualifier = config.SenderIDQualifier;

            sender.NM109_IdCode = config.SenderID.Clean();

            var senderPer = sender.AddSegment(new TypedSegmentPER());
            senderPer.PER01_ContactFunctionCode = "IC";

            //INMEDIATA BUSINESS RULE
            var nameTest = (sender.NM104_NameFirst + " " + sender.NM103_NameLastOrOrganizationName).Clean();
            if (nameTest != submitterProvider.FullName.Clean())
                senderPer.PER02_Name = submitterProvider.FullName.Clean();

            if (!string.IsNullOrWhiteSpace(submitterProvider.WorkPhoneNumber))
            {
                senderPer.PER03_CommunicationNumberQualifier = CommunicationNumberQualifer.Telephone;
                senderPer.PER04_CommunicationNumber = submitterProvider.WorkPhoneNumber.StripNonNumeric();
            }
            else
            {
                senderPer.PER03_CommunicationNumberQualifier = CommunicationNumberQualifer.Telephone;
                senderPer.PER04_CommunicationNumber = "3050001234";
            }

            //END SUBMITTER 

            //Loop 1000B 
            var reciever = t.AddLoop(new TypedLoopNM1("40"));
            reciever.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;

            reciever.NM103_NameLastOrOrganizationName = "RECIEVER NAME";
            reciever.NM108_IdCodeQualifier = "46";
            reciever.NM109_IdCode = fg.Interchange.InterchangeReceiverId != null ? fg.Interchange.InterchangeReceiverId.Trim() : "";

            //BILLER 
            var providerLoop = t.AddHLoop("1", "20", true);
            var billingProvider = new ProviderInfo()
            {
                FirstName = "",
                LastName = "LASTNAME",
                ProviderId = 1,
                ProviderTypeId = 2,
                ProviderNPI = 1111111111,
                AlternateId = "660000000",
                WorkAddress1 = "",
                WorkAddress2 = "",
                WorkCity = "",
                WorkPhoneNumber = ""
            };            

            if (!String.IsNullOrWhiteSpace(billingProvider.SpecialtyCode))
            {
                var provSpecialty = providerLoop.AddSegment(new TypedSegmentPRV());
                provSpecialty.PRV01_ProviderCode = "BI";
                provSpecialty.PRV02_ReferenceIdQualifier = "PXC";
                provSpecialty.PRV03_ProviderTaxonomyCode = billingProvider.SpecialtyCode;
            }

            var provName = providerLoop.AddLoop(new TypedLoopNM1("85"));

            provName.NM102_EntityTypeQualifier = EntityTypeQualifier.Person;

            if (billingProvider.ProviderTypeId == 2)
            {
                provName.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;
                provName.NM103_NameLastOrOrganizationName = billingProvider.FullName.Clean(35);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(billingProvider.FirstName))
                {
                    provName.NM103_NameLastOrOrganizationName = billingProvider.LastName.Clean(35);
                    provName.NM104_NameFirst = billingProvider.FirstName.Clean(35);
                }
            }

            provName.NM108_IdCodeQualifier = "XX";//NPI 
            provName.NM109_IdCode = billingProvider.ProviderNPI?.ToString();
            
            //QH
            if (billingProvider.WorkAddress1 != null)
                AddNm1Address(provName, billingProvider.WorkAddress1, billingProvider.WorkAddress2, "", billingProvider.WorkCity, "FL");

            //CONTACT
            var provNameRef = provName.AddSegment(new TypedSegmentREF());
            if (!string.IsNullOrWhiteSpace(billingProvider.AlternateId) && billingProvider.AlternateId.StartsWith("66"))
                provNameRef.REF01_ReferenceIdQualifier = "EI"; 
            else
                provNameRef.REF01_ReferenceIdQualifier = "SY";
            provNameRef.REF02_ReferenceId = billingProvider.AlternateId;  

            //PATIENT 
            var subscriberLoop = providerLoop.AddHLoop("2", "22", false);

            //PLAN
            if (!string.IsNullOrWhiteSpace(claim.ContractNumber))
            {
                var sbr = subscriberLoop.AddSegment(new TypedSegmentSBR());
                sbr.SBR01_PayerResponsibilitySequenceNumberCode = "P";
                sbr.SBR02_IndividualRelationshipCode = "18";
                sbr.SBR09_ClaimFilingIndicatorCode = "CI";
 
            }

            if (!string.IsNullOrWhiteSpace(claim.PatientFirstName))
            {
                var subsName = subscriberLoop.AddLoop(new TypedLoopNM1("IL"));
                subsName.NM102_EntityTypeQualifier = EntityTypeQualifier.Person;
                subsName.NM103_NameLastOrOrganizationName = claim.PatientLastName.Clean();
                subsName.NM104_NameFirst = claim.PatientFirstName.Clean();

                //if (!String.IsNullOrWhiteSpace(claim.PatientName))
                //    subsName.NM105_NameMiddle = TypeEx.Clean(claim.PatientName);

                if (!string.IsNullOrWhiteSpace(claim.ContractNumber))
                {
                    subsName.NM108_IdCodeQualifier = "MI";//Health Plan Member Identification Number 
                    subsName.NM109_IdCode = claim.ContractNumber.Clean();
                }

                //Address
                AddNm1Address(subsName, "ADDRESS1", "ADDRESS2", "33176", "MIAMI", "FL");

                var subsNameDMG = subsName.AddSegment(new TypedSegmentDMG());
                subsNameDMG.DMG01_DateTimePeriodFormatQualifier = $"D{DTPFormatQualifier.MMDDCCYY.ToString("d")}"; //"D8"; 
                subsNameDMG.DMG02_DateOfBirth =  claim.MemberBirthDate ?? fillerBirthDate ;
                subsNameDMG.DMG03_Gender = Gender.Unknown;
            }
 
            //PAYER                 
            var pay = subscriberLoop.AddLoop(new TypedLoopNM1("PR"));
            pay.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;
            pay.NM103_NameLastOrOrganizationName = "PAYER MEDICAL";

            pay.NM108_IdCodeQualifier = "PI";
            pay.NM109_IdCode = "GHP6600000000";

            //CLAIM - LOOP 2300
            var clm = subscriberLoop.AddLoop(new TypedLoopCLM());

                 
            clm.CLM02_TotalClaimChargeAmount = claim.Tariff ?? 0;
            
            clm.CLM05._1_FacilityCodeValue = "11";
            clm.CLM05._2_FacilityCodeQualifier = "B";
            clm.CLM05._3_ClaimFrequencyTypeCode = "1";

            //CLAIM NUMBER 
            if (!string.IsNullOrWhiteSpace(claim.ClaimNumber))
            {
                //CLM Claim Information
                clm.CLM01_PatientControlNumber = claim.ClaimNumber.ToString();
                //clm.CLM05._3_ClaimFrequencyTypeCode = "7";
                //Header
                //bht.SetElement(2, "18");//Reissue
                
                //original claim from reissue
                //var ref = clm.AddSegment(new TypedSegmentREF());
                //ref.REF01_ReferenceIdQualifier = "F8";
                //ref.REF02_ReferenceId = claim.ClaimNumber;
            }

            if (!string.IsNullOrWhiteSpace(claim.PlaceOfService))
            {
                clm.CLM05._1_FacilityCodeValue = claim.PlaceOfService;
            }

            
            clm.CLM06_ProviderOrSupplierSignatureIndicator = true;
            clm.CLM07_ProviderAcceptAssignmentCode = "A";
            clm.CLM08_BenefitsAssignmentCerficationIndicator = "Y";
            clm.CLM09_ReleaseOfInformationCode = "Y";
            clm.CLM10_PatientSignatureSourceCode = "P";
 
            //Add DX
            var dxList = new List<string>();
            if (!string.IsNullOrWhiteSpace(claim.Dx1)) dxList.Add(claim.Dx1.Clean());
            if (!string.IsNullOrWhiteSpace(claim.Dx2)) dxList.Add(claim.Dx2.Clean());
            if (!string.IsNullOrWhiteSpace(claim.Dx3)) dxList.Add(claim.Dx3.Clean());
            if (!string.IsNullOrWhiteSpace(claim.Dx4)) dxList.Add(claim.Dx4.Clean());
            if (!string.IsNullOrWhiteSpace(claim.Dx5)) dxList.Add(claim.Dx5.Clean());
            if (!string.IsNullOrWhiteSpace(claim.Dx6)) dxList.Add(claim.Dx6.Clean());
            if (!string.IsNullOrWhiteSpace(claim.Dx7)) dxList.Add(claim.Dx7.Clean());


            if (dxList.Count > 0)
            {
                var hiSegment = clm.AddSegment(new TypedSegmentHI());

                foreach (var item in dxList)
                {
                    var index = dxList.IndexOf(item) + 1;

                    if (string.IsNullOrWhiteSpace(item)) continue;
                    
                    var prefix = "A";

                    //inmediata requires dotless notation 
                    var code = item.Replace(".", "").ToUpper();

                    if (index == 1) hiSegment.HI01_HealthCareCodeInformation = prefix + "BK:" + code;
                    if (index == 2) hiSegment.HI02_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 3) hiSegment.HI03_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 4) hiSegment.HI04_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 5) hiSegment.HI05_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 6) hiSegment.HI06_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 7) hiSegment.HI07_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 8) hiSegment.HI08_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 9) hiSegment.HI09_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 10) hiSegment.HI10_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 11) hiSegment.HI11_HealthCareCodeInformation = prefix + "BF:" + code;
                    if (index == 12) hiSegment.HI12_HealthCareCodeInformation = prefix + "BF:" + code;
                }
            }

   
 

            var serviceList = new List<string>();
            if (!string.IsNullOrWhiteSpace(claim.CPT))
                serviceList.Add(claim.CPT.Clean());

            int cnt = 1;
            foreach (var serviceCode in serviceList)
            { 
 
                if (!string.IsNullOrWhiteSpace(claim.RenderNPI))
                {
                    var clmProv = AddClmProvider(clm, claim, "82", "PE", "PXC");
                }

                //Service
                var svc1 = clm.AddLoop(new TypedLoopLX("LX"));
                svc1.LX01_AssignedNumber = cnt.ToString();  

                var compositeCodeBuilder = new System.Text.StringBuilder();

                if (!string.IsNullOrWhiteSpace(serviceCode))
                {
           
                    compositeCodeBuilder.Append("HC:" + serviceCode.Clean()); 
                }

                var svc101 = svc1.AddSegment(new TypedSegmentSV1());

                if (!string.IsNullOrWhiteSpace(serviceCode))
                    svc101.SV101_CompositeMedicalProcedure = compositeCodeBuilder.ToString();

                //AMOUNT 
                svc101.SV102_MonetaryAmount = claim.Tariff.toString("F"); 
                svc101.SV103_UnitBasisMeasCode = "UN";                
                svc101.SV104_Quantity = "1";

                //Only when not the same as CLM05
                if (!string.IsNullOrWhiteSpace(claim.PlaceOfService) && clm.CLM05._1_FacilityCodeValue != claim.PlaceOfService)
                    svc101.SV105_FacilityCode = claim.PlaceOfService;

                //DX Pointer
                svc101.SV107_CompDiagCodePoint = "1";

  
                var svc101DTP = svc1.AddSegment(new TypedSegmentDTP());
                svc101DTP.DTP01_DateTimeQualifier = DTPQualifier.Service;

                if (claim.ServiceFrom.HasValue && claim.ServiceTo.HasValue)
                {
                    svc101DTP.DTP02_DateTimePeriodFormatQualifier = DTPFormatQualifier.CCYYMMDD_CCYYMMDD;
                    svc101DTP.DTP03_Date = new DateTimePeriod(claim.ServiceFrom.Value, claim.ServiceTo.Value);
                }
                else if (claim.ServiceFrom.HasValue)
                {
                    svc101DTP.DTP02_DateTimePeriodFormatQualifier = DTPFormatQualifier.CCYYMMDD;
                    svc101DTP.DTP03_Date = new DateTimePeriod(claim.ServiceFrom.Value);
                }

                //End Service Date
 

                cnt++;
            }
 
 
            return t;
        }
 
        private static TypedLoopNM1 AddClmProvider(TypedLoopCLM clm, ClaimLine claim, string entityIdentifier, string providerCode, string providerCodeQualifier = "ZZ")
        {
            
            var loop = clm.AddLoop(new TypedLoopNM1(entityIdentifier));
            loop.NM102_EntityTypeQualifier = EntityTypeQualifier.Person;

            //if (prov.ProviderTypeId == 1)
            //{
            //    loop.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;
            //    loop.NM103_NameLastOrOrganizationName = TypeEx.Clean(prov.FullName, 35);
            //}
            //else
            //{

                loop.NM103_NameLastOrOrganizationName = TypeEx.Clean(claim.RenderProvider);
                loop.NM104_NameFirst = TypeEx.Clean(claim.RenderProvider);
            //}

            loop.NM108_IdCodeQualifier = "XX";
            loop.NM109_IdCode = claim.RenderNPI.Clean();

 
            return loop;
        }

        private static TypedLoopNM1 AddClmProvider(TypedLoopCLM clm, int providerID, string entityIdentifier, string providerCode, List<ProviderInfo> provList, string providerCodeQualifier = "ZZ")
        {
            var prov = provList.FirstOrDefault(a => a.ProviderId == providerID);
            if (prov == null)
                return null;

            var loop = clm.AddLoop(new TypedLoopNM1(entityIdentifier));
            loop.NM102_EntityTypeQualifier = EntityTypeQualifier.Person;

            if (prov.ProviderTypeId == 1)
            {
                loop.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;
                loop.NM103_NameLastOrOrganizationName = TypeEx.Clean(prov.FullName, 35);
            }
            else
            {

                loop.NM103_NameLastOrOrganizationName = TypeEx.Clean(prov.LastName);
                loop.NM104_NameFirst = TypeEx.Clean(prov.FirstName);

            }

            loop.NM108_IdCodeQualifier = "XX";
            loop.NM109_IdCode = prov.ProviderNPI.ToString();

            if (!string.IsNullOrWhiteSpace(prov.SpecialtyCode))
            {
                var clmProvPrv = loop.AddSegment(new TypedSegmentPRV());
                //Some business rules wont allow this segment for certain identifiers, hence null check. 
                if (clmProvPrv != null)
                {
                    clmProvPrv.PRV01_ProviderCode = providerCode;
                    clmProvPrv.PRV02_ReferenceIdQualifier = providerCodeQualifier;
                    clmProvPrv.PRV03_ProviderTaxonomyCode = prov.SpecialtyCode;//system specialty taxonomy code 
                }
            }

            return loop;
        }

        private static void AddNm1Address(TypedLoopNM1 loop, string address, string address2, string zip, string city, string state)
        {
            var subs1 = loop.AddSegment(new TypedSegmentN3());

            if (!String.IsNullOrWhiteSpace(address))
                subs1.N301_AddressInformation = TypeEx.Clean(address, 35);

            if (!String.IsNullOrWhiteSpace(address2))
                subs1.N302_AddressInformation = TypeEx.Clean(address2, 35);

            if (!String.IsNullOrWhiteSpace(zip))
            {
                var subs2 = loop.AddSegment(new TypedSegmentN4());

                if (!String.IsNullOrWhiteSpace(city))
                    subs2.N401_CityName = TypeEx.Clean(city);

                if (!String.IsNullOrWhiteSpace(state))
                    subs2.N402_StateOrProvinceCode = state;
                else
                {
                    subs2.N402_StateOrProvinceCode = "FL"; 

                }

                subs2.N403_PostalCode = zip;
            }
        }

    }
}