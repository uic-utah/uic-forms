using System;
using System.Collections.Generic;
using System.Linq;

namespace uic_forms.models
{
    public class QueryParams
    {
        public QueryParams()
        {
            
        }
        public QueryParams(QueryParams queryParams)
        {
            WellClass = queryParams.WellClass;
            StartDate = queryParams.StartDate;
            AuthActionTypes = queryParams.AuthActionTypes;
            AuthTypes = queryParams.AuthTypes;
            ViolationTypes = queryParams.ViolationTypes;
            EnforcementTypes = queryParams.EnforcementTypes;
            InspectionType = queryParams.InspectionType;
            MitTypes = queryParams.MitTypes;
            MitResult = queryParams.MitResult;
            Snc = queryParams.Snc;
            RemedialAction = queryParams.RemedialAction;
            WellType = queryParams.WellType;
            Ident4Ca = queryParams.Ident4Ca;
            CaType = queryParams.CaType;
            HasEnforcement = queryParams.HasEnforcement;
        }
        public QueryParams(int wellClass)
        {
            WellClass = wellClass;
        }

        internal int WellClass { get; set; }
        public DateTime? StartDate { get; set; }
        internal IEnumerable<string> AuthActionTypes { get; set; } = Enumerable.Empty<string>();
        internal IEnumerable<string> AuthTypes { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> ViolationTypes { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> EnforcementTypes { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> InspectionType { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> MitTypes { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> MitResult { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> RemedialAction { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<int> WellType { get; set; } = Enumerable.Empty<int>();
        public bool Snc { get; set; }
        public bool Ident4Ca { get; set; }
        public bool HasEnforcement { get; set; }
        public int CaType { get; set; }
    }
}
