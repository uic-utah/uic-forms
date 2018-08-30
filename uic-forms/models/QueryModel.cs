using System;

namespace uic_forms.models
{
    public struct QueryModel
    {
        public Guid ItemId { get; set; }
        public Guid FacilityId { get; set; }
        public Guid Id { get; set; }
        public int EsriId { get; set; }
        public Guid WellId { get; set; }
        public DateTime ViolationDate { get; set; }
        public DateTime? ReturnToComplianceDate { get; set; }
        public DateTime? EnforcementDate { get; set; }
        public string EnforcementType { get; set; }
    }
}
