using System;
using System.Collections.Generic;
using System.Linq;

namespace uic_forms.models
{
    public class QueryParams
    {
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
        public bool Snc { get; set; }
        public IEnumerable<string> RemedialAction { get; set; } = Enumerable.Empty<string>();
    }
}
