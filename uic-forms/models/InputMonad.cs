using System;

namespace uic_forms.models
{
    internal class InputMonad
    {
        public InputMonad(string id, QueryParams @params, Func<QueryParams, int> query)
        {
            Id = id;
            Params = @params;
            Query = query;
        }

        public string Id { get; set; }
        public QueryParams Params { get; set; }
        public Func<QueryParams, int> Query { get; set; }
    }
}
