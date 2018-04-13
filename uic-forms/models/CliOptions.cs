namespace uic_forms.models
{
    using System;

    internal class CliOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } = DateTime.Today;
        public bool Verbose { get; set; }
        public string TemplateLocation { get; set; }
        public string OutputPath { get; set; } = "c:\\temp";
    }
}
