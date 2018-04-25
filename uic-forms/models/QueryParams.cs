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
            AuthActionTypes = Enumerable.Empty<string>();
            AuthTypes = Enumerable.Empty<string>();
            ViolationTypes = Enumerable.Empty<string>();
            EnforcementTypes = Enumerable.Empty<string>();
        }

        internal int WellClass { get; set; }
        public DateTime? StartDate { get; set; }
        internal IEnumerable<string> AuthActionTypes { get; set; }
        internal IEnumerable<string> AuthTypes { get; set; }
        public IEnumerable<string> ViolationTypes { get; set; }
        public IEnumerable<string> EnforcementTypes { get; set; }
        public bool Snc { get; set; }
    }
}
