namespace uic_forms
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using models;
    using PdfSharp.Pdf;
    using PdfSharp.Pdf.AcroForms;
    using PdfSharp.Pdf.IO;
    using services;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new CliOptions();
            try

            {
                options = ArgParserService.Parse(args);
                if (options == null)
                {
                    return;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.Write("uic-forms: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("press any key to continue");
                Console.ReadKey();
            }

            if (!Directory.Exists(options?.OutputPath))
            {
                Console.Write("uic-forms: ");
                Console.WriteLine("{0} does not exists. Do you want to create it now? (Y/n)", options.OutputPath);
                var key = Console.ReadKey();

                DirectoryInfo output = null;
                if (new[] {ConsoleKey.Enter, ConsoleKey.Y}.Contains(key.Key))
                {
                    output = Directory.CreateDirectory(options.OutputPath);
                }

                if (output == null || !output.Exists)
                {
                    Console.Write("uic-etl: ");
                    Console.WriteLine("output does not exist. exiting.");
                    Console.ReadKey();

                    return;
                }
            }

            var debug = new Logger(options.Verbose);
            var start = Stopwatch.StartNew();

            debug.AlwaysWrite("Starting: {0}", DateTime.Now.ToString("s"));

            debug.AlwaysWrite("Loading template for the 7520-1 form...");
            var form75201 = Path.Combine(options.TemplateLocation, "7520-1.pdf");
            var form75201Output = Path.Combine(options.OutputPath, $"7520-1-{DateTime.Today:M-dd-yyyy}.pdf");

            using (var file = new FileStream(form75201, FileMode.Open, FileAccess.Read))
            using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
            {
                var fields = document.AcroForm.Fields;

                EnableUpdates(document.AcroForm); 

                Set("DatePrepared", DateTime.Today.ToString("MMM dd, yyyy"), fields);
                Set("ReportingFromDate", options.StartDate.ToString("MMM dd, yyyy"), fields);
                Set("ReportingToDate", options.EndDate.ToString("MMM dd, yyyy"), fields);

                // Output the path for manual verification of result
                debug.AlwaysWrite("Saving 7520-1 form to {0}", form75201Output);

                document.Save(form75201Output);
            }

            debug.AlwaysWrite("Finished: {0}", start.Elapsed);
            Console.ReadKey();
        }

        private static void EnableUpdates(PdfAcroForm form)
        {
            if (form.Elements.ContainsKey("/NeedAppearances"))
            {
                form.Elements["/NeedAppearances"] = new PdfBoolean(true);
            }
            else
            {
                form.Elements.Add("/NeedAppearances", new PdfBoolean(true));
            }
        }

        static void Set(string field, string value, PdfAcroField.PdfAcroFieldCollection fields)
        {
            ((PdfTextField)fields[field]).Value = new PdfString(value);
        }
    }
}
