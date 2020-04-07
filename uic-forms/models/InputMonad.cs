using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace uic_forms.models
{
    internal class InputMonad
    {
        private readonly QueryParams _params;
        private readonly Func<QueryParams, IReadOnlyCollection<string>> _query;
        private readonly bool _returnFirstValue;

        internal InputMonad(string id, QueryParams @params, Func<QueryParams, IReadOnlyCollection<string>> query,
                            bool returnFirstValue)
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

        public IReadOnlyCollection<Guid> ItemIds { get; private set; } = new List<Guid>();

        public string Query()
        {
            var result = _query(_params);
            var count = result.Count;

            if (count > 0)
            {
                Program.Logger.Write("Narrative for {0}", GetId(Id));
            }
            else
            {
                Result = "0";

                return Result;
            }

            var itemIds = new List<Guid>();
            foreach (var item in result)
            {
                Program.Logger.Write(item);
                var itemId = GetItemId(item);
                if (itemId == Guid.Empty)
                {
                    continue;
                }

                itemIds.Add(itemId);
            }

            ItemIds = itemIds.AsReadOnly();

            if (_returnFirstValue)
            {
                Result = result.FirstOrDefault();

                return Result;
            }

            Result = count.ToString();

            return Result;
        }

        private Guid GetItemId(string metadata) 
        {
            var match = Regex.Match(metadata, "uicwell: guid='(.+?)'", RegexOptions.IgnoreCase);
            if (match.Groups.Count < 2) 
            {
                return Guid.Empty;
            }

            return Guid.Parse(match.Groups[1].Value);
        }

        private string GetId(string id)
        {
            services.FieldMapper.Lookup.TryGetValue(id, out var value);

            return value ?? id;
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
