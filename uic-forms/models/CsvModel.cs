using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace uic_forms.models
{
    public sealed class Forms1Through3Mapping : ClassMap<Forms1Through3>
    {
        public Forms1Through3Mapping()
        {
            Map(x => x.PRI_AGENCY_CODE).Constant("UTEQ").NameIndex(0);
            Map(x => x.FedFiscalYr).Constant(null).NameIndex(1);
            Map(x => x.FYQuarter).Constant(null).NameIndex(2);
            Map(x => x.Region).Constant(null).NameIndex(3);
            Map(x => x.State).Constant(null).NameIndex(4);
            Map(x => x.Tribe).Constant(null).NameIndex(5);
            Map(x => x.WellClass).NameIndex(6);
            Map(x => x.PermitApps).NameIndex(7);
        }
    }

    public class Forms1Through3 { 
        public string PRI_AGENCY_CODE { get; set; }
        public string FedFiscalYr { get; set; }
        public string FYQuarter { get; set; }
        public string Region { get; set; }
        public string State { get; set; }
        public string Tribe { get; set; }
        public string WellClass { get; set; }
        public string PermitApps { get; set; }
        public string IndPermitNew { get; set; }
        public string IndPermitExist { get; set; }
        public string AreaPermitNew { get; set; }
        public string AreaPermitExist { get; set; }
        public string AreaWellNew { get; set; }
        public string AreaWellExist { get; set; }
        public string PermitDenied { get; set; }
        public string PermitMod { get; set; }
        public string RAIIReview { get; set; }
        public string RAIIDeficient { get; set; }
        public string AORWellAbandon { get; set; }
        public string AORWellOther { get; set; }
        public string AORWellCAAbandon { get; set; }
        public string AORWellCAOther { get; set; }
        public string AORWellCasing { get; set; }
        public string AORWellPlug { get; set; }
        public string AORWellReplug { get; set; }
        public string AORWellOtherCA { get; set; }
        public string Remark75201 { get; set; }
        public string WellsViolation { get; set; }
        public string UAViolations { get; set; }
        public string MIViolations { get; set; }
        public string OMViolations { get; set; }
        public string PAViolations { get; set; }
        public string MRViolations { get; set; }
        public string OtherViolations { get; set; }
        public string OtherViolationsSpec { get; set; }
        public string WellsEA { get; set; }
        public string NOV_EA { get; set; }
        public string ConsentAgree_EA { get; set; }
        public string AdminOrder_EA { get; set; }
        public string CivilReferral_EA { get; set; }
        public string CrimReferral_EA { get; set; }
        public string ShutIn_EA { get; set; }
        public string PipeSev_EA { get; set; }
        public string Other_EA { get; set; }
        public string Other_EA_Spec { get; set; }
        public string WellRTC { get; set; }
        public string Contamination { get; set; }
        public string MITResolved { get; set; }
        public string Remark75202A { get; set; }
        public string WellSNC { get; set; }
        public string UAViolations_SNC { get; set; }
        public string MIViolation_SNC { get; set; }
        public string Pressure_SNC { get; set; }
        public string PAViolations_SNC { get; set; }
        public string Orders_SNC { get; set; }
        public string Falsification_SNC { get; set; }
        public string Other_SNC { get; set; }
        public string Other_SNC_Spec { get; set; }
        public string WellsEA_SNC { get; set; }
        public string NOV_SNC { get; set; }
        public string ConsentAgree_SNC { get; set; }
        public string AdminOrder_SNC { get; set; }
        public string CivilReferral_SNC { get; set; }
        public string CrimReferral_SNC { get; set; }
        public string ShutIn_SNC { get; set; }
        public string PipeSev_SNC { get; set; }
        public string Other_SNC_EA { get; set; }
        public string Other_SNC_EA_Spec { get; set; }
        public string WellRTC_SNC { get; set; }
        public string Contamination_SNC { get; set; }
        public string InvoluntaryClosure { get; set; }
        public string VoluntaryClosure { get; set; }
        public string WellsInspected { get; set; }
        public string MITInspections { get; set; }
        public string ERInspections { get; set; }
        public string Construct_Inspections { get; set; }
        public string PlugInspections { get; set; }
        public string RoutineInspections { get; set; }
        public string WellsMIT { get; set; }
        public string RAWellsMIT_Pass { get; set; }
        public string RAWellsMIT_Fail { get; set; }
        public string APEval_Pass { get; set; }
        public string APEval_Fail { get; set; }
        public string CasingPressure_Pass { get; set; }
        public string CasingPressure_Fail { get; set; }
        public string MonitorEval_Pass { get; set; }
        public string MonitorEval_Fail { get; set; }
        public string OtherLeakEval_Pass { get; set; }
        public string OtherLeakEval_Fail { get; set; }
        public string CementEval_Pass { get; set; }
        public string CementEval_Fail { get; set; }
        public string TempTest_Pass { get; set; }
        public string TempTest_Fail { get; set; }
        public string RadTest_Pass { get; set; }
        public string RadTest_Fail { get; set; }
        public string OtherFluidTest_Pass { get; set; }
        public string OtherFluidTest_Fail { get; set; }
        public string WellsRemedialAction { get; set; }
        public string Remedial_Casing { get; set; }
        public string Remedial_Tubing { get; set; }
        public string Remedial_Plug { get; set; }
        public string Remedial_Other { get; set; }
        public string Remark75203 { get; set; }
    }
}
