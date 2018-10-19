using System;
using System.Collections.Generic;
using System.Linq;

namespace uic_forms.models
{
    internal class InputMonad
    {
        private readonly QueryParams _params;
        private readonly Func<QueryParams, IReadOnlyCollection<string>> _query;
        private readonly bool _returnFirstValue;

        internal InputMonad(string id, QueryParams @params, Func<QueryParams, IReadOnlyCollection<string>> query, bool returnFirstValue)
        {
            Id = id;
            _params = @params;
            _query = query;
            _returnFirstValue = returnFirstValue;
            WellClass = _params.WellClass;
        }

        public int WellClass { get; set; }

        public string Id { get; }

        public string Result { get; private set; }

        public string Query()
        {
            var result = _query(_params);
            var count = result.Count;

            if (count > 0)
            {
                Program.Logger.Write("Narrative for {0}", Id);
            }
            else
            {
                Result = "0";
               
                return Result;
            }

            foreach (var item in result)
            {
                Program.Logger.Write(item);
            }

            if (_returnFirstValue)
            {
                Result = result.FirstOrDefault();

                return Result;
            }

            Result = count.ToString();

            return Result;
        }
    }

    internal static class InputMonadGenerator
    {
        internal static void CreateMonadFor(IList<int> wellClasses, string id, QueryParams options,
                                            Func<QueryParams, IReadOnlyCollection<string>> query,
                                            List<InputMonad> list, bool returnFirstValue = false)
        {
            if (!wellClasses.Any())
            {
                return;
            }

            var monads = new List<InputMonad>(wellClasses.Count);

            foreach (var wellClass in wellClasses)
            {
                var option = new QueryParams(options)
                {
                    WellClass = wellClass
                };

                monads.Add(new InputMonad(id, option, query, returnFirstValue));
            }

            list.AddRange(monads);
        }
    }
}
