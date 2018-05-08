using System;
using System.Collections.Generic;
using System.Linq;

namespace uic_forms.models
{
    internal class InputMonad
    {
        public InputMonad(string id, QueryParams @params, Func<QueryParams, string> query)
        {
            Id = id;
            Params = @params;
            Query = query;

            if (Id.Contains("{class}"))
            {
                Id = id.Replace("{class}", Params.WellClass.ToString());
            }
        }

        public string Id { get; set; }
        public QueryParams Params { get; set; }
        public Func<QueryParams, string> Query { get; set; }
    }

     internal static class InputMonadGenerator
    {
        internal static void CreateMonadFor(IEnumerable<int> wellClasses, string id, QueryParams options,
                                               Func<QueryParams, string> query, ref List<InputMonad> list)
        {
            if (!wellClasses.Any())
            {
                return;
            }

            var monads = new List<InputMonad>(wellClasses.Count());

            foreach (var wellClass in wellClasses)
            {
                var option = new QueryParams(options)
                {
                    WellClass = wellClass
                };

                monads.Add(new InputMonad(id, option, query));
            }

            list.AddRange(monads);
        }
    }
}
