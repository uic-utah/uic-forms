﻿using System;

namespace uic_forms.models
{
    public class ViolationModel
    {
        public Guid WellId { get; set; }
        public DateTime ViolationDate { get; set; }
        public char SignificantNonCompliance { get; set; }
        public DateTime? ReturnToComplianceDate { get; set; }
        public DateTime? EnforcementDate { get; set; }
        public string EnforcementType { get; set; }
        public bool ReturnToCompliance { get; set; }
        public bool Enforcement { get; set; }
    }
}