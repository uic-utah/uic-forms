using uic_forms.services;

namespace uic_forms.models
{
    internal class Form4 : Form
    {
        public Form4(Logger logger)
        {
            Logger = logger;
        }

        public string OperatorName { get; set; }
        public string OperatorStreet { get; set; }
        public string OperatorCity { get; set; }
        public string OperatorState { get; set; }
        public string OperatorZIP { get; set; }
        public string WID { get; set; }
        public string DOV { get; set; }
        public string UI { get; set; }
        public string MI { get; set; }
        public string IP { get; set; }
        public string PA { get; set; }
        public string FO { get; set; }
        public string F { get; set; }
        public string OV { get; set; }
        public string OtherViolation_QEL_Specific { get; set; }
        public string DOE { get; set; }
        public string NOV { get; set; }
        public string CA { get; set; }
        public string AO { get; set; }
        public string CivR { get; set; }
        public string CrimR { get; set; }
        public string WSI { get; set; }
        public string PS { get; set; }
        public string OE { get; set; }
        public string OtherEnforcement_QEL_Specific { get; set; }
        public string DOC { get; set; }
    }
}