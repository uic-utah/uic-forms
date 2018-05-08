﻿using InquirerCS;

namespace uic_forms.services
{
    using System;
    using System.IO;
    using models;

    internal static class ArgParserService
    {
        internal static CliOptions Parse()
        {
            var options = new CliOptions();

            Inquirer.Prompt(Question.Input<DateTime>("Reporting Start Date")
                                    .WithDefaultValue(new DateTime(DateTime.Now.Year, 10, 1)))
                    .Bind(() => options.StartDate);

            Inquirer.Prompt(Question.List("Which quarter would you like to report",
                                          new[] { "1st", "2nd", "3rd", "4th" })).Then(quarter =>
            {
                switch (quarter)
                {
                    case "1st":
                    {
                        options.EndDate = new DateTime(options.StartDate.Year, 12, 31);
                        break;
                    }
                    case "2nd":
                    {
                        options.EndDate = new DateTime(options.StartDate.Year + 1, 3, 31);
                        break;
                    }
                    case "3rd":
                    {
                        options.EndDate = new DateTime(options.StartDate.Year + 1, 6, 30);
                        break;
                    }
                    case "4th":
                    {
                        options.EndDate = new DateTime(options.StartDate.Year + 1, 8, 30);
                        break;
                    }
                    default: 
                        throw new Exception("how did I get here?");
                }
            });

            Inquirer.Prompt(Question.Input("What is the file path to the 7520 PDF forms?")
                                    .WithDefaultValue(@"C:\Projects\GitHub\uic-7520\templates")
                                    .WithValidation(path =>
                                    {
                                        if (new DirectoryInfo(path).Exists)
                                        {
                                            return true;
                                        }

                                        var cwd = Directory.GetCurrentDirectory();
                                        var location = Path.Combine(cwd, path.TrimStart('\\'));

                                        return new DirectoryInfo(location).Exists;
                                    }, "The location for the pdf files could not be found. Try again."))
                    .Bind(() => options.TemplateLocation);

            Inquirer.Prompt(Question.Input("Where would you like the forms saved?")
                                    .WithDefaultValue(@"C:\temp"))
                    .Then(path =>
                    {
                        if (new DirectoryInfo(path).Exists)
                        {
                            options.OutputPath = path;
                            return;
                        }

                        var cwd = Directory.GetCurrentDirectory();
                        var location = Path.Combine(cwd, path.TrimStart('\\'));

                        if (new DirectoryInfo(location).Exists)
                        {
                            options.OutputPath = path;
                            return;
                        }

                        Inquirer.Prompt(Question.Confirm("The location does not exist. Create it now?")
                                                .WithDefaultValue(true))
                                .Then(yes =>
                                {
                                    if (!yes)
                                    {
                                        return;
                                    }

                                    Directory.CreateDirectory(path);
                                    options.OutputPath = path;
                                });
                    });

            Inquirer.Prompt(Question.Confirm("Would you like to see the debug output?")
                                    .WithDefaultValue(false))
                    .Bind(() => options.Verbose);

            Inquirer.Go();

            return options;
        }
    }
}
