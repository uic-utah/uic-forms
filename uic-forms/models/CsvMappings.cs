using CsvHelper.Configuration;

namespace uic_forms.models
{
    internal sealed class Forms1Through3Mapping : ClassMap<Forms1Through3>
    {
        public Forms1Through3Mapping()
        {
            Map(x => x.PrimaryAgencyCode).Name("PRI_AGENCY_CODE").Constant("UTEQ").NameIndex(0);
            Map(x => x.FedFiscalYr).Name("FedFiscalYr").NameIndex(1);
            Map(x => x.FyQuarter).Name("FYQuarter").NameIndex(2);
            Map(x => x.Region).Constant("08").NameIndex(3);
            Map(x => x.State).Constant("Utah").NameIndex(4);
            Map(x => x.Tribe).Constant(null).NameIndex(5);
            Map(x => x.WCT).NameIndex(6);
            Map(x => x.V).Name("PermitApps").NameIndex(7);
            Map(x => x.Vian).Name("IndPermitNew").NameIndex(8);
            Map(x => x.Viae).Name("IndPermitExist").NameIndex(9);
            Map(x => x.Vibn).Name("AreaPermitNew").NameIndex(10);
            Map(x => x.Vibe).Name("AreaPermitExist").NameIndex(11);
            Map(x => x.Vicn).Name("AreaWellNew").NameIndex(12);
            Map(x => x.Vice).Name("AreaWellExist").NameIndex(13);
            Map(x => x.Vid).Name("PermitDenied").NameIndex(14);
            Map(x => x.RAIIReview).Name("RAIIReview").Constant("0").NameIndex(15);
            Map(x => x.RAIIDeficient).Name("RAIIDeficient").Constant("0").NameIndex(16);
            Map(x => x.Viiiaa).Name("AORWellAbandon").NameIndex(17);
            Map(x => x.Viiiao).Name("AORWellOther").NameIndex(18);
            Map(x => x.Viiiba).Name("AORWellCAAbandon").NameIndex(19);
            Map(x => x.Viiibo).Name("AORWellCAOther").NameIndex(20);
            Map(x => x.Viiic1).Name("AORWellCasing").NameIndex(21);
            Map(x => x.Viiic2).Name("AORWellPlug").NameIndex(22);
            Map(x => x.Viiic3).Name("AORWellReplug").NameIndex(23);
            Map(x => x.Viiic4).Name("AORWellOtherCA").NameIndex(24);
            Map(x => x.Remark75201).Constant(null).NameIndex(25);
            Map(x => x.Va).Name("WellsViolation").NameIndex(26);
            Map(x => x.Vb1).Name("UAViolations").NameIndex(27);
            Map(x => x.Vb2).Name("MIViolations").NameIndex(28);
            Map(x => x.Vb3).Name("OMViolations").NameIndex(29);
            Map(x => x.Vb4).Name("PAViolations").NameIndex(30);
            Map(x => x.Vb5).Name("MRViolations").NameIndex(31);
            Map(x => x.Vb6).Name("OtherViolations").NameIndex(32);
            Map(x => x.OtherViolationsSpec).Constant(null).NameIndex(33);
            Map(x => x.Via).Name("WellsEA").NameIndex(34);
            Map(x => x.Vib1).Name("NOV_EA").NameIndex(35);
            Map(x => x.Vib2).Name("ConsentAgree_EA").NameIndex(36);
            Map(x => x.Vib3).Name("AdminOrder_EA").NameIndex(37);
            Map(x => x.Vib4).Name("CivilReferral_EA").NameIndex(38);
            Map(x => x.Vib5).Name("CrimReferral_EA").NameIndex(39);
            Map(x => x.Vib6).Name("ShutIn_EA").NameIndex(40);
            Map(x => x.Vib7).Name("PipeSev_EA").Constant("0").NameIndex(41);
            Map(x => x.Vib8).Name("Other_EA").NameIndex(42);
            Map(x => x.Other_EA_Spec).Constant(null).NameIndex(43);
            Map(x => x.Viia).Name("WellRTC").NameIndex(44);
            Map(x => x.Viii).Name("Contamination").NameIndex(45);
            Map(x => x.Ix).Name("MITResolved").NameIndex(46);
            Map(x => x.Remark75202A).Constant(null).NameIndex(47);
            Map(x => x.Va_2b).Name("WellSNC").NameIndex(48);
            Map(x => x.Vb1_2b).Name("UAViolations_SNC").NameIndex(49);
            Map(x => x.Vb2_2b).Name("MIViolation_SNC").NameIndex(50);
            Map(x => x.Vb3_2b).Name("Pressure_SNC").NameIndex(51);
            Map(x => x.Vb4_2b).Name("PAViolations_SNC").NameIndex(52);
            Map(x => x.Vb5_2b).Name("Orders_SNC").NameIndex(53);
            Map(x => x.Vb6_2b).Name("Falsification_SNC").NameIndex(54);
            Map(x => x.Vb7_2b).Name("Other_SNC").NameIndex(55);
            Map(x => x.Other_SNC_Spec).Constant(null).NameIndex(56);
            Map(x => x.Via_2b).Name("WellsEA_SNC").NameIndex(57);
            Map(x => x.Vib1_2b).Name("NOV_SNC").NameIndex(58);
            Map(x => x.Vib2_2b).Name("ConsentAgree_SNC").NameIndex(59);
            Map(x => x.Vib3_2b).Name("AdminOrder_SNC").NameIndex(60);
            Map(x => x.Vib4_2b).Name("CivilReferral_SNC").NameIndex(61);
            Map(x => x.Vib5_2b).Name("CrimReferral_SNC").NameIndex(62);
            Map(x => x.Vib6_2b).Name("ShutIn_SNC").NameIndex(63);
            Map(x => x.PipeSev_SNC).Constant("0").NameIndex(64);
            Map(x => x.Vib8_2b).Name("Other_SNC_EA").NameIndex(65);
            Map(x => x.Other_SNC_EA_Spec).Constant(null).NameIndex(66);
            Map(x => x.Viia_2b).Name("WellRTC_SNC").NameIndex(67);
            Map(x => x.Viii_2b).Name("Contamination_SNC").NameIndex(68);
            Map(x => x.Ix_2b).Name("InvoluntaryClosure").NameIndex(69);
            Map(x => x.Ixv_2b).Name("VoluntaryClosure").NameIndex(70);
            Map(x => x.Va_3b).Name("WellsInspected").NameIndex(71);
            Map(x => x.Vb1_3b).Name("MITInspections").NameIndex(72);
            Map(x => x.Vb2_3b).Name("ERInspections").NameIndex(73);
            Map(x => x.Vb3_3b).Name("Construct_Inspections").NameIndex(74);
            Map(x => x.Vb4_3b).Name("PlugInspections").NameIndex(75);
            Map(x => x.Vb5_3b).Name("RoutineInspections").NameIndex(76);
            Map(x => x.Via_3b).Name("WellsMIT").NameIndex(77);
            Map(x => x.RAWellsMIT_Pass).Constant("0").NameIndex(78);
            Map(x => x.RAWellsMIT_Fail).Constant("0").NameIndex(79);
            Map(x => x.Vic1p_3b).Name("APEval_Pass").NameIndex(80);
            Map(x => x.Vic1f_3b).Name("APEval_Fail").NameIndex(81);
            Map(x => x.Vic2p_3b).Name("CasingPressure_Pass").NameIndex(82);
            Map(x => x.Vic2f_3b).Name("CasingPressure_Fail").NameIndex(83);
            Map(x => x.Vic3p_3b).Name("MonitorEval_Pass").NameIndex(84);
            Map(x => x.Vic3f_3b).Name("MonitorEval_Fail").NameIndex(85);
            Map(x => x.Vic4p_3b).Name("OtherLeakEval_Pass").NameIndex(86);
            Map(x => x.Vic4f_3b).Name("OtherLeakEval_Fail").NameIndex(87);
            Map(x => x.Vid1p_3b).Name("CementEval_Pass").NameIndex(88);
            Map(x => x.Vid1f_3b).Name("CementEval_Fail").NameIndex(89);
            Map(x => x.Vid2p_3b).Name("TempTest_Pass").NameIndex(90);
            Map(x => x.Vid2f_3b).Name("TempTest_Fail").NameIndex(91);
            Map(x => x.Vid3p_3b).Name("RadTest_Pass").NameIndex(92);
            Map(x => x.Vid3f_3b).Name("RadTest_Fail").NameIndex(93);
            Map(x => x.Vid4p_3b).Name("OtherFluidTest_Pass").NameIndex(94);
            Map(x => x.Vid4f_3b).Name("OtherFluidTest_Fail").NameIndex(95);
            Map(x => x.Viia_3b).Name("WellsRemedialAction").NameIndex(96);
            Map(x => x.Viib1_3b).Name("Remedial_Casing").NameIndex(97);
            Map(x => x.Viib2_3b).Name("Remedial_Tubing").NameIndex(98);
            Map(x => x.Viib3_3b).Name("Remedial_Plug").NameIndex(99);
            Map(x => x.Viib4_3b).Name("Remedial_Other").NameIndex(100);
            Map(x => x.Remark75203).Constant(null).NameIndex(101);
        }
    }

    internal sealed class Form4Mapping : ClassMap<Form4>
    {
        public Form4Mapping()
        {
            Map(x => x.PrimaryAgencyCode).Name("PRI_AGENCY_CODE").Constant("UTEQ").NameIndex(0);
            Map(x => x.FedFiscalYr).Name("FedFiscalYr").NameIndex(1);
            Map(x => x.FyQuarter).Name("FYQuarter").NameIndex(2);
            Map(x => x.Region).Constant("08").NameIndex(3);
            Map(x => x.State).Constant("Utah").NameIndex(4);
            Map(x => x.Tribe).Constant(null).NameIndex(5);
            Map(x => x.WCT).Name("WellClass").NameIndex(6);
            Map(x => x.OperatorName).Name("OperatorName").NameIndex(7);
            Map(x => x.OperatorStreet).Name("OperatorStreet").NameIndex(8);
            Map(x => x.OperatorCity).Name("OperatorCity").NameIndex(9);
            Map(x => x.OperatorState).Name("OperatorState").NameIndex(10);
            Map(x => x.OperatorZIP).Name("OperatorZIP").NameIndex(11);
            Map(x => x.WID).Name("WellNumber").NameIndex(12);
            Map(x => x.DOV).Name("ViolationDate").NameIndex(13);
            Map(x => x.UI).Name("UAViolations_QEL").NameIndex(14);
            Map(x => x.MI).Name("MIViolation_QEL").NameIndex(15);
            Map(x => x.IP).Name("Pressure_QEL").NameIndex(16);
            Map(x => x.PA).Name("PAViolations_QEL").NameIndex(17);
            Map(x => x.FO).Name("Orders_QEL").NameIndex(18);
            Map(x => x.F).Name("Falsification_QEL").NameIndex(19);
            Map(x => x.OV).Name("OtherViolation_QEL").NameIndex(20);
            Map(x => x.OtherViolation_QEL_Specific).Constant(null).NameIndex(21);
            Map(x => x.DOE).Name("EnforcementDate").NameIndex(22);
            Map(x => x.NOV).Name("NOV_QEL").NameIndex(23);
            Map(x => x.CA).Name("ConsentAgree_QEL").NameIndex(24);
            Map(x => x.AO).Name("AdminOrder_QEL").NameIndex(25);
            Map(x => x.CivR).Name("CivilReferral_QEL").NameIndex(26);
            Map(x => x.CrimR).Name("CrimReferral_QEL").NameIndex(27);
            Map(x => x.WSI).Name("ShutIn_QEL").NameIndex(28);
            Map(x => x.PS).Name("PipeSev_QEL").NameIndex(29);
            Map(x => x.OE).Name("OtherEnforcement_QEL").NameIndex(30);
            Map(x => x.OtherEnforcement_QEL_Specific).Constant(null).NameIndex(31);
            Map(x => x.DOC).Name("ComplianceAchieved").NameIndex(32);
        }
    }

    internal sealed class FormInventoryMapping : ClassMap<FormInventory>
    {
        public FormInventoryMapping()
        {
            Map(x => x.PrimaryAgencyCode).Name("PRI_AGENCY_CODE").Constant("UTEQ").NameIndex(0);
            Map(x => x.FedFiscalYr).Name("FedFiscalYr").NameIndex(1);
            Map(x => x.Region).Constant("08").NameIndex(2);
            Map(x => x.State).Constant("Utah").NameIndex(3);
            Map(x => x.Tribe).Constant(null).NameIndex(4);
            Map(x => x.ClassIh).Name("ClassIH").NameIndex(5);
            Map(x => x.ClassIOther).Name("ClassIOther").NameIndex(6);
            Map(x => x.ClassIid).Name("ClassIID").Constant("0").NameIndex(7);
            Map(x => x.ClassIir).Name("ClassIIR").Constant("0").NameIndex(8);
            Map(x => x.ClassIiOther).Name("ClassIIOther").Constant("0").NameIndex(9);
            Map(x => x.ClassIiiWells).Name("ClassIIIWells").NameIndex(10);
            Map(x => x.ClassIiiSites).Name("ClassIIISites").NameIndex(11);
            Map(x => x.ClassIvWells).Name("ClassIVWells").NameIndex(12);
            Map(x => x.ClassVWells).Name("ClassVWells").NameIndex(13);
            Map(x => x.ClassViWells).Name("ClassVIWells").Constant("0").NameIndex(14);

        }
    }
}
