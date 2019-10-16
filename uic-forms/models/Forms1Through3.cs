using System.Collections.Generic;
using uic_forms.services;

namespace uic_forms.models
{
    internal class Forms1Through3 : Form
    {
        public string OtherLeakEvalSpec = string.Empty;
        public string OtherFluidTestSpec = string.Empty;
        public string RemedialOtherSpec = string.Empty;

        public Forms1Through3(IReadOnlyList<InputMonad> datas, Logger logger)
        {
            Logger = logger;
            foreach (var inputMonad in datas)
            {
                this[inputMonad.Id] = inputMonad.Result;
            }
        }

        public string V { get; set; }
        public string Vian { get; set; }
        public string Viae { get; set; }
        public string Vibn { get; set; }
        public string Vibe { get; set; }
        public string Vicn { get; set; }
        public string Vice { get; set; }
        public string Vid { get; set; }
        public string Vie { get; set; }
        public string RAIIReview { get; set; }
        public string RAIIDeficient { get; set; }
        public string Viiiaa { get; set; }
        public string Viiiao { get; set; }
        public string Viiiba { get; set; }
        public string Viiibo { get; set; }
        public string Viiic1 { get; set; }
        public string Viiic2 { get; set; }
        public string Viiic3 { get; set; }
        public string Viiic4 { get; set; }
        public string Remark75201 { get; set; }
        public string Va { get; set; }
        public string Vb1 { get; set; }
        public string Vb2 { get; set; }
        public string Vb3 { get; set; }
        public string Vb4 { get; set; }
        public string Vb5 { get; set; }
        public string Vb6 { get; set; }
        public string OtherViolationsSpec { get; set; }
        public string Via { get; set; }
        public string Vib1 { get; set; }
        public string Vib2 { get; set; }
        public string Vib3 { get; set; }
        public string Vib4 { get; set; }
        public string Vib5 { get; set; }
        public string Vib6 { get; set; }
        public string Vib7 { get; set; }
        public string Vib8 { get; set; }
        public string Other_EA_Spec { get; set; }
        public string Viib { get; set; }
        public string Viii { get; set; }
        public string Ix { get; set; }
        public string Remark75202A { get; set; }
        public string Va_2b { get; set; }
        public string Vb1_2b { get; set; }
        public string Vb2_2b { get; set; }
        public string Vb3_2b { get; set; }
        public string Vb4_2b { get; set; }
        public string Vb5_2b { get; set; }
        public string Vb6_2b { get; set; }
        public string Vb7_2b { get; set; }
        public string Other_SNC_Spec { get; set; }
        public string Via_2b { get; set; }
        public string Vib1_2b { get; set; }
        public string Vib2_2b { get; set; }
        public string Vib3_2b { get; set; }
        public string Vib4_2b { get; set; }
        public string Vib5_2b { get; set; }
        public string Vib6_2b { get; set; }
        public string PipeSev_SNC { get; set; }
        public string Vib8_2b { get; set; }
        public string Other_SNC_EA_Spec { get; set; }
        public string Viib_2b { get; set; }
        public string Viii_2b { get; set; }
        public string Ix_2b { get; set; }
        public string Ixv_2b { get; set; }
        public string Va_3b { get; set; }
        public string Vb1_3b { get; set; }
        public string Vb2_3b { get; set; }
        public string Vb3_3b { get; set; }
        public string Vb4_3b { get; set; }
        public string Vb5_3b { get; set; }
        public string Via_3b { get; set; }
        public string RAWellsMIT_Pass { get; set; }
        public string RAWellsMIT_Fail { get; set; }
        public string Vic1p_3b { get; set; }
        public string Vic1f_3b { get; set; }
        public string Vic2p_3b { get; set; }
        public string Vic2f_3b { get; set; }
        public string Vic3p_3b { get; set; }
        public string Vic3f_3b { get; set; }
        public string Vic4p_3b { get; set; }
        public string Vic4f_3b { get; set; }
        public string Vid1p_3b { get; set; }
        public string Vid1f_3b { get; set; }
        public string Vid2p_3b { get; set; }
        public string Vid2f_3b { get; set; }
        public string Vid3p_3b { get; set; }
        public string Vid3f_3b { get; set; }
        public string Vid4p_3b { get; set; }
        public string Vid4f_3b { get; set; }
        public string Viia_3b { get; set; }
        public string Viib1_3b { get; set; }
        public string Viib2_3b { get; set; }
        public string Viib3_3b { get; set; }
        public string Viib4_3b { get; set; }
        public string Remark75203 { get; set; }
    }
}
