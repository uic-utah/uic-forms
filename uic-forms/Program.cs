using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using uic_forms.models;
using uic_forms.services;

namespace uic_forms
{
    internal class Program
    {
        public static Logger Logger;

        private static void Main()
        {
#if !DEBUG
            var options = ArgParserService.Parse();
#else
            var options = new CliOptions
            {
                EndDate = new DateTime(2018, 3, 31),
                StartDate = new DateTime(2017, 10, 1),
                OutputPath = "c:\\temp",
                TemplateLocation = "C:\\Projects\\GitHub\\uic-7520\\templates",
                Source = "udeq.agrc.utah.gov\\mspd14",
                Verbose = false,
                Password = "stage-pw"
            };
#endif
            
            Console.WriteLine();

            if (options.OutputPath == null || !Directory.Exists(options.OutputPath))
            {
                Console.WriteLine("uic-etl: ");
                Console.WriteLine("output does not exist. exiting.");

                return;
            }

            Logger = new Logger(options.Verbose, options.OutputPath);
            Logger.AlwaysWrite("Version {version}", Assembly.GetExecutingAssembly().GetName().Version);
            var start = Stopwatch.StartNew();

            Logger.AlwaysWrite("Starting: {0}", DateTime.Now.ToString("s"));
            Logger.AlwaysWrite("Reporting from {0} - {1} ({2} days)", options.StartDate.ToShortDateString(),
                                options.EndDate.ToShortDateString(), (options.EndDate - options.StartDate).Days);

            Logger.Write("Connecting to UDEQ...");
            using (var sevenFiveTwenty = new Querier(options))
            {
                var formPaths = GetFormLocations(options, "7520-1");
                Logger.AlwaysWrite("Loading template for the 7520-1 form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);

                    SetFieldText("VIA_1E", "NA", fields);
                    SetFieldText("VIB_1E", "NA", fields);
                    SetFieldText("VIC_1E", "NA", fields);

                    var formInfo = new List<InputMonad>();
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "V_{class}",
                                                       new QueryParams
                                                       {
                                                           AuthActionTypes = new[] {"AI", "AM", "AR"}
                                                       }, sevenFiveTwenty.GetPermitCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIA_{class}N",
                                                       new QueryParams
                                                       {
                                                           AuthTypes = new[] {"IP"},
                                                           AuthActionTypes = new[] {"PI"}
                                                       }, sevenFiveTwenty.GetPermitCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB_{class}N",
                                                       new QueryParams
                                                       {
                                                           AuthTypes = new[] {"AP"},
                                                           AuthActionTypes = new[] {"PI"}
                                                       }, sevenFiveTwenty.GetPermitCount, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC_{class}N",
                                                       new QueryParams
                                                       {
                                                           AuthTypes = new[] {"AP"},
                                                           AuthActionTypes = new[] {"PI"}
                                                       }, sevenFiveTwenty.GetWellPermitCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID_{class}",
                                                       new QueryParams
                                                       {
                                                           AuthTypes = new[]
                                                               {"IP", "AP", "GP", "EP", "OP"},
                                                           AuthActionTypes = new[] {"PD", "MD", "TP"}
                                                       }, sevenFiveTwenty.GetPermitCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIE_{class}",
                                                       new QueryParams
                                                       {
                                                           AuthTypes = new[] {"IP", "AP", "GP", "OP"},
                                                           AuthActionTypes = new[] {"PM"}
                                                       }, sevenFiveTwenty.GetPermitCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIA_{class}A",
                                                       new QueryParams
                                                       {
                                                           WellType = new[] {1}
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIA_{class}O",
                                                       new QueryParams
                                                       {
                                                           WellType = new[] {2, 3}
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIB_{class}A",
                                                       new QueryParams
                                                       {
                                                           WellType = new[] {1},
                                                           Ident4Ca = true
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIB_{class}O",
                                                       new QueryParams
                                                       {
                                                           WellType = new[] {2, 3},
                                                           Ident4Ca = true
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC1_{class}",
                                                       new QueryParams
                                                       {
                                                           CaType = 1
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC2_{class}",
                                                       new QueryParams
                                                       {
                                                           CaType = 2
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC3_{class}",
                                                       new QueryParams
                                                       {
                                                           CaType = 3
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC4_{class}",
                                                       new QueryParams
                                                       {
                                                           CaType = 4
                                                       }, sevenFiveTwenty.GetArtificialPenetrations, ref formInfo);


                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(), fields); });

                    // Output the path for manual verification of result
                    Logger.AlwaysWrite("Saving 7520-1 form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }

                formPaths = GetFormLocations(options, "7520-2a");
                Logger.AlwaysWrite("Loading template for the 7520-2a form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);
//
                    var formInfo = new List<InputMonad>();
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VA_{class}", new QueryParams(),
                                                       sevenFiveTwenty.GetWellViolationCount, ref formInfo);
//
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB1_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"UI"}
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB2_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"MI", "MO"}
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB3_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"OM", "IP"}
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB4_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"PA"}
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB5_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"MR", "FO", "FA", "FI", "FR"}
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB6_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"OT"}
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
//
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIA_{class}", new QueryParams(),
                                                       sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB1_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"NOV"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB2_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"CGT"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB3_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"DAO", "FAO"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB4_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"CIR"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB5_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"CRR"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB6_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"SHT"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB7_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"PSE"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB8_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"INF", "TOA", "OTR"}
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIA_{class}", new QueryParams
                                                       {
                                                           StartDate = options.EndDate - TimeSpan.FromDays(90)
                                                       },
                                                       sevenFiveTwenty.GetWellsReturnedToCompliance, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIB_{class}", new QueryParams
                                                       {
                                                           StartDate = options.StartDate
                                                       },
                                                       sevenFiveTwenty.GetWellsReturnedToCompliance, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIII_{class}", new QueryParams(),
                                                       sevenFiveTwenty.GetContaminationViolations, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "IX_{class}", new QueryParams(),
                                                       sevenFiveTwenty.CalculatePercentResolved, ref formInfo, true);
//
//
                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(), fields); });

                    Logger.AlwaysWrite("Saving 7520-2a form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }
//
                formPaths = GetFormLocations(options, "7520-2b");
                Logger.AlwaysWrite("Loading template for the 7520-2b form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);

                    SetFieldText("VIB7_1", "NA", fields);

                    var formInfo = new List<InputMonad>();
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VA_{class}", new QueryParams(),
                                                       sevenFiveTwenty.SncViolations, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB1_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"UI"},
                        Snc = true
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB2_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"MI", "MO"},
                        Snc = true
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB3_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"IP"},
                        Snc = true
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB4_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"PA"},
                        Snc = true
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB5_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"FO"},
                        Snc = true
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB6_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"FA"},
                        Snc = true
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB7_{class}", new QueryParams
                    {
                        ViolationTypes = new[] {"OT", "OM", "MR", "FI", "FR"},
                        Snc = true
                    }, sevenFiveTwenty.GetViolationCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIA_{class}", new QueryParams
                    {
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB1_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"NOV"},
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB2_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"CGT"},
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB3_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"DAO", "FAO"},
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB4_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"CIR"},
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB5_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"CRR"},
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB6_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"SHT"},
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIB8_{class}", new QueryParams
                    {
                        EnforcementTypes = new[] {"INF", "TOA", "OTR"},
                        Snc = true
                    }, sevenFiveTwenty.GetWellsWithEnforcements, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIA_{class}", new QueryParams
                    {
                        Snc = true
                    }, sevenFiveTwenty.GetWellsReturnedToCompliance, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIB_{class}", new QueryParams
                    {
                        StartDate = options.StartDate,
                        Snc = true
                    }, sevenFiveTwenty.GetWellsReturnedToCompliance, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIII_{class}", new QueryParams
                    {
                        Snc = true
                    }, sevenFiveTwenty.GetContaminationViolations, ref formInfo);

                    InputMonadGenerator.CreateMonadFor(new[] {4, 5}, "IX_{class}", new QueryParams
                    {
                        HasEnforcement = true
                    }, sevenFiveTwenty.GetWellOperatingStatus, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {4, 5}, "IXV_{class}", new QueryParams
                    {
                        HasEnforcement = false
                    }, sevenFiveTwenty.GetWellOperatingStatus, ref formInfo);

                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(), fields); });

                    Logger.AlwaysWrite("Saving 7520-2b form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }

                formPaths = GetFormLocations(options, "7520-3");
                Logger.AlwaysWrite("Loading template for the 7520-3 form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);

                    var na = new List<string>
                    {
                        "VIB_1",
                        "VIBf_1",
                        "VIB_3",
                        "VIBf_3",
                        "VIA_4",
                        "VIB_4",
                        "VIBf_4",
                        "VIC1p_4",
                        "VIC2p_4",
                        "VIC3p_4",
                        "VIC4p_4",
                        "VIC1f_4",
                        "VIC2f_4",
                        "VIC3f_4",
                        "VIC4f_4",
                        "VID1p_4",
                        "VID2p_4",
                        "VID3p_4",
                        "VID4p_4",
                        "VID1f_4",
                        "VID2f_4",
                        "VID3f_4",
                        "VID4f_4",
                        "VIIA_4",
                        "VIIB1_4",
                        "VIIB2_4",
                        "VIIB3_4",
                        "VIIB4_4",
                        "VIB_5",
                        "VIBf_5"
                    };

                    na.ForEach(field => { SetFieldText(field, "NA", fields); });

                    var formInfo = new List<InputMonad>();

                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VA_{class}", new QueryParams(),
                                                       sevenFiveTwenty.GetWellsInspected, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB1_{class}", new QueryParams
                    {
                        InspectionType = new[] {"MI"}
                    }, sevenFiveTwenty.GetInspections, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB2_{class}", new QueryParams
                    {
                        InspectionType = new[] {"EC"}
                    }, sevenFiveTwenty.GetInspections, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB3_{class}", new QueryParams
                    {
                        InspectionType = new[] {"CO"}
                    }, sevenFiveTwenty.GetInspections, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB4_{class}", new QueryParams
                    {
                        InspectionType = new[] {"WP"}
                    }, sevenFiveTwenty.GetInspections, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VB5_{class}", new QueryParams
                    {
                        InspectionType = new[] {"RP", "OT", "NW", "FI"}
                    }, sevenFiveTwenty.GetInspections, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIA_{class}", new QueryParams(),
                                                       sevenFiveTwenty.GetMechIntegrities, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC1p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"AP"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC1f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"AP"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC2p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"CT"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC2f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"CT"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC3p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"MR"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC3f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"MR"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC4p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"WI", "WA", "AT", "SR", "OL"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIC4f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"WI", "WA", "AT", "SR", "OL"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID1p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"CR"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID1f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"CR"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID2p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"TN"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID2f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"TN"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID3p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"RC"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID3f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"RC"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID4p_{class}", new QueryParams
                    {
                        MitTypes = new[] {"CB", "OA", "RS", "DC", "OF"},
                        MitResult = new[] {"PS"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VID4f_{class}", new QueryParams
                    {
                        MitTypes = new[] {"CB", "OA", "RS", "DC", "OF"},
                        MitResult = new[] {"FU", "FP", "FA"}
                    }, sevenFiveTwenty.GetMechIntegrityWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIA_{class}", new QueryParams(),
                                                       sevenFiveTwenty.GetRemedialWells, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIB1_{class}", new QueryParams
                    {
                        RemedialAction = new[] {"CS"}
                    }, sevenFiveTwenty.GetRemedials, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIB2_{class}", new QueryParams
                    {
                        RemedialAction = new[] {"TR", "PR"}
                    }, sevenFiveTwenty.GetRemedials, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIB3_{class}", new QueryParams
                    {
                        RemedialAction = new[] {"PA"}
                    }, sevenFiveTwenty.GetRemedials, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIB4_{class}", new QueryParams
                    {
                        RemedialAction = new[] {"OT"}
                    }, sevenFiveTwenty.GetRemedials, ref formInfo);
//
                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(), fields); });

                    Logger.AlwaysWrite("Saving 7520-3 form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }

                formPaths = GetFormLocations(options, "7520-4");
                Logger.AlwaysWrite("Loading template for the 7520-4 form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;
                    var include = new Collection<QueryModel>();
                    var formalActions = new[] {"CIR", "CGT", "CRR", "DAO", "FAO", "NOV", "PSE", "TAO", "SHT"};

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);

                    var violations = sevenFiveTwenty.GetViolations();

                    foreach (var violation in violations)
                    {
                        Logger.Write("violation {1} date {0}", violation.ViolationDate.ToShortDateString(),
                                      violation.EsriId);
                        int days;
                        if (violation.ReturnToComplianceDate.HasValue &&
                            violation.ReturnToComplianceDate.Value < options.EndDate)
                        {
                            // Yes return to compliance date
                            Logger.Write("  ReturnToComplianceDate - Yes");
                            days = (violation.ReturnToComplianceDate.Value - violation.ViolationDate).Days;
                            Logger.Write("  return to compliance date of {0}",
                                          violation.ReturnToComplianceDate.Value.ToShortDateString());
                            Logger.Write("  {0} days of violation", days);

                            if (days < 180)
                            {
                                Logger.Write("  skipping because less than 180");
                                continue;
                            }

                            days = (options.EndDate - violation.ReturnToComplianceDate.Value).Days;
                            Logger.Write("  {0} days since violation reporting", days);

                            if (days > 90)
                            {
                                Logger.Write("skipping because not within 90 day reporting period");
                                continue;
                            }

                            Logger.Write("including");

                            include.Add(violation);
                        }
                        else
                        {
                            Logger.Write("  ReturnToComplianceDate - No");
                            // No return to compliance date
                            if (string.IsNullOrEmpty(violation.EnforcementType))
                            {
                                // No enforcement record
                                Logger.Write("  Enforcement - No");

                                // No return to compliance date
                                days = (options.EndDate - violation.ViolationDate).Days;
                                Logger.Write("  {0} days since violation reporting", days);

                                if (days < 180)
                                {
                                    Logger.Write("skipping because not within 180 day reporting period");
                                    continue;
                                }

                                Logger.Write("including");

                                include.Add(violation);
                            }
                            else
                            {
                                // Yes enforcement
                                Logger.Write("  Enforcement - Yes");
                                Logger.Write("  Enforcement type {0}", violation.EnforcementType);

                                if (!formalActions.Contains(violation.EnforcementType))
                                {
                                    Logger.Write("  Formal Enforcement Action - No");

                                    days = (options.EndDate - violation.ViolationDate).Days;
                                    Logger.Write("  {0} days since violation reporting", days);

                                    if (days < 180)
                                    {
                                        Logger.Write("skipping because not within 180 day reporting period");
                                        continue;
                                    }

                                    Logger.Write("including");

                                    include.Add(violation);
                                }
                                else
                                {
                                    // Yes formal action type
                                    Logger.Write("  Formal Enforcement Action - Yes");
                                    Logger.Write("  formal action type {0}", violation.EnforcementType);

                                    if (!violation.EnforcementDate.HasValue)
                                    {
                                        Logger.Write("skipping since no enforcement date");
                                        continue;
                                    }

                                    days = (violation.EnforcementDate.Value - violation.ViolationDate).Days;
                                    Logger.Write("  {0} days since enforcement", days);

                                    if (days < 180)
                                    {
                                        Logger.Write("skipping because not within 180 day period");
                                        continue;
                                    }

                                    days = (options.EndDate - violation.EnforcementDate.Value).Days;
                                    Logger.Write("  {0} days since enforcement reporting period", days);

                                    if (days > 90)
                                    {
                                        Logger.Write("skipping because not within 90 day reporting period");
                                        continue;
                                    }

                                    Logger.Write("including");

                                    include.Add(violation);
                                }
                            }
                        }
                    }

                    // create a new page for every 9 wells?
                    const int pageSize = 9;
                    var row = 1;
                    foreach (var violation in include)
                    {
                        var checkboxFields = new List<string>
                        {
                            "UI_",
                            "MI_",
                            "IP_",
                            "PA_",
                            "FO_",
                            "F_",
                            "OV_"
                        };

                        if (row > pageSize)
                        {
                            Logger.AlwaysWrite("Too many violations skipping");
                            continue;
                        }

                        var value = sevenFiveTwenty.GetWellSubClass(violation.WellId);
                        var contact = sevenFiveTwenty.GetContactAddress(violation.WellId);

                        SetFieldText("WCT_" + row, value, fields);
                        SetFieldText("NAO_" + row, contact?.Address(), fields);

                        SetFieldText("WID_" + row, sevenFiveTwenty.GetWellId(violation.WellId), fields);
                        SetFieldText("DOV_" + row, violation.ViolationDate.ToString("MMM dd, yyyy"), fields);
                        if (violation.EnforcementDate.HasValue && violation.EnforcementDate.Value <= options.EndDate)
                        {
                            SetFieldText("DOE_" + row, violation.EnforcementDate.Value.ToString("MMM dd, yyyy"),
                                         fields);
                            checkboxFields.AddRange(new [] {
                                "NOV_",
                                "CA_",
                                "AO_",
                                "CivR_",
                                "CrimR_",
                                "WSI_",
                                "PS_",
                                "OE_"
                            });
                        }

                        if (violation.ReturnToComplianceDate.HasValue &&
                            violation.ReturnToComplianceDate.Value <= options.EndDate)
                        {
                            SetFieldText("DOC_" + row, violation.ReturnToComplianceDate.Value.ToString("MMM dd, yyyy"),
                                         fields);
                        }

                        var checks = checkboxFields.ToDictionary(key => key, v => false);
                        sevenFiveTwenty.GetViolationCheckmarks(violation.Id, violation.EnforcementType, ref checks);

                        foreach (var item in checks)
                        {
                            SetFieldCheck(row, item.Key, item.Value, fields);
                        }

                        row += 1;
                    }

                    Logger.AlwaysWrite("Saving 7520-4 form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }
            }

            Logger.AlwaysWrite("Reported from {0} - {1} ({2} days)", options.StartDate.ToShortDateString(),
                                options.EndDate.ToShortDateString(), (options.EndDate - options.StartDate).Days);
            Logger.AlwaysWrite("Finished: {0}", start.Elapsed);
            Console.ReadLine();
        }

        private static void SetHeader(PdfAcroField.PdfAcroFieldCollection fields, CliOptions options)
        {
            if (fields.Names.Contains("Preparer"))
            {
                SetFieldText("Preparer", "Candace C. Cady\r\nEnvironmental Scientist", fields);
            }
            if (fields.Names.Contains("Telephone"))
            {
                SetFieldText("Telephone", "801.536.4352", fields);
            }
            if (fields.Names.Contains("DatePrepared"))
            {
                SetFieldText("DatePrepared", DateTime.Today.ToString("MMM dd, yyyy"), fields);
            }
            if (fields.Names.Contains("Date Signed"))
            {
                SetFieldText("Date Signed", DateTime.Today.ToString("MMM dd, yyyy"), fields);
            }
            if (fields.Names.Contains("DateSigned"))
            {
                SetFieldText("DateSigned", DateTime.Today.ToString("MMM dd, yyyy"), fields);
            }
            SetFieldText("ReportingFromDate", options.StartDate.ToString("MMM dd, yyyy"), fields);
            SetFieldText("ReportingToDate", options.EndDate.ToString("MMM dd, yyyy"), fields);
        }

        private static Tuple<string, string> GetFormLocations(CliOptions options, string formNumber)
        {
            var template = Path.Combine(options.TemplateLocation, $"{formNumber}.pdf");
            var outputLocation = Path.Combine(options.OutputPath, $"{formNumber}_{DateTime.Now:MM-dd-yyyy}.pdf");

            return new Tuple<string, string>(template, outputLocation);
        }

        private static void EnableUpdates(PdfDictionary form)
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

        private static void SetFieldText(string field, object value, PdfAcroField.PdfAcroFieldCollection fields)
        {
            if (value == null)
            {
                return;
            }

            try
            {
                ((PdfTextField) fields[field]).Value = new PdfString(value.ToString());
            }
            catch (Exception)
            {
                Logger.AlwaysWrite("Cound not find field {0}. Skipping", field);
            }
        }

        private static void SetFieldCheck(int row, string field, bool value, PdfAcroField.PdfAcroFieldCollection fields)
        {
            ((PdfCheckBoxField) fields[field + row]).Checked = value;
        }
    }
}
