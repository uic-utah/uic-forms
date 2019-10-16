using InquirerCS;

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

      Inquirer.Prompt(Question.List("Which database would you like to report from",
          new[] { "Production", "Staging" })).Then(configuration =>
       {
        switch (configuration)
        {
          case "Production":
            {
              options.Source = "udeq.agrc.utah.gov";
              options.Password = "prod-pw";
              break;
            }
          case "Staging":
            {
              options.Source = "udeq.agrc.utah.gov\\mspd14";
              options.Password = "stage-pw";
              break;
            }
          default:
            throw new Exception("how did I get here?");
        }
      });

      Inquirer.Prompt(Question.Input<int>("Reporting start year")
                              .WithDefaultValue(DateTime.Now.Year))
              .Then(year => options.StartDate = new DateTime(year, 10, 1));

      Inquirer.Prompt(Question.List("Which federal fiscal year quarter would you like to report",
                                    new[] { "1Q", "2Q", "3Q", "4Q" })).Then(quarter =>
      {
        switch (quarter)
        {
          case "1Q":
            {
              options.EndDate = new DateTime(options.StartDate.Year, 12, 31);
              break;
            }
          case "2Q":
            {
              options.EndDate = new DateTime(options.StartDate.Year + 1, 3, 31);
              break;
            }
          case "3Q":
            {
              options.EndDate = new DateTime(options.StartDate.Year + 1, 6, 30);
              break;
            }
          case "4Q":
            {
              options.EndDate = new DateTime(options.StartDate.Year + 1, 9, 30);
              break;
            }
          default:
            throw new Exception("how did I get here?");
        }

        options.Quarter = quarter;
      });

      Inquirer.Prompt(Question.Input("Where would you like the csv saved")
                              .WithDefaultValue(Path.GetTempPath()))
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

      Inquirer.Prompt(Question.Confirm("Would you like to see the debug output")
                              .WithDefaultValue(true))
              .Bind(() => options.Verbose);

      Inquirer.Go();

      return options;
    }
  }
}
