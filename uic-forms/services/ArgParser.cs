namespace uic_forms.services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using models;
    using Mono.Options;

    internal static class ArgParserService
    {
        internal static CliOptions Parse(string[] args)
        {
            var options = new CliOptions();
            var showHelp = false;

            var p = new OptionSet
            {
                {
                    "s|start=", "REQUIRED. The date you want to start evaluating for the forms. eg: mm/dd/yyyy",
                    v => options.StartDate = DateTime.Parse(v)
                },
                {
                    "e|end=", "Defaults to today. The date you want to start evaluating for the forms. eg: mm/dd/yyyy",
                    v => options.EndDate = DateTime.Parse(v)
                },
                {
                    "t|templates=", "File location of where the pdf templates are. eg: c:\\7520\\pdfs",
                    v => options.TemplateLocation = v
                },
                {
                    "o|output=",
                    "the location to save the output of this tool. eg: c:\\temp. Defaults to current working directory",
                    v => options.OutputPath = v
                },
                {
                    "v|verbose", "increase the debug message verbosity.",
                    v =>
                    {
                        if (v != null)
                        {
                            options.Verbose = true;
                        }
                    }
                },
                {
                    "h|help", "show this message and exit",
                    v => showHelp = v != null
                }
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("uic-forms: ");
                Console.WriteLine(e.Message);
                ShowHelp(p);

                return null;
            }

            if (showHelp)
            {
                ShowHelp(p);

                return null;
            }

            if (options.StartDate == null)
            {
                throw new InvalidOperationException("Missing required option `-s` for the start date.");
            }

            if (string.IsNullOrEmpty(options.TemplateLocation))
            {
                throw new InvalidOperationException("Missing required option -t for the location of the 7520 template pdf's.");
            }

            if (!new DirectoryInfo(options.TemplateLocation).Exists)
            {
                var cwd = Directory.GetCurrentDirectory();
                var location = Path.Combine(cwd, options.TemplateLocation.TrimStart('\\'));

                if (!new DirectoryInfo(location).Exists)
                {
                    throw new InvalidOperationException("The location for the pdf files could not be found.");
                }

                options.TemplateLocation = location;
            }

            // TODO: check for the pdf files

            if (showHelp)
            {
                ShowHelp(p);
            }

            return options;
        }

        private static void ShowHelp(OptionSet p)
        {
            var assembly = Assembly.GetExecutingAssembly();
            {
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

                Console.WriteLine("UIC Forms Tool : {0}", fvi.FileVersion);
            }

            Console.WriteLine("Usage: uicforms [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Options:");

            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
