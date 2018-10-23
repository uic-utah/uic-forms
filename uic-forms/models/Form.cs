using uic_forms.services;

namespace uic_forms.models
{
    internal abstract class Form
    {
        public Logger Logger;

        public object this[string propertyName]
        {
            get
            {
                var property = GetType().GetProperty(propertyName);

                return property?.GetValue(this, null);
            }
            set
            {
                var property = GetType().GetProperty(propertyName);
                if (property != null)
                {
                    property.SetValue(this, value, null);

                    return;
                }

                Logger.AlwaysWrite("field {0} not found", propertyName);
            }
        }

        public string PrimaryAgencyCode { get; set; }
        public string FedFiscalYr { get; set; }
        public string FyQuarter { get; set; }
        public string Region { get; set; }
        public string State { get; set; }
        public string Tribe { get; set; }
        public string WCT { get; set; }
    }
}