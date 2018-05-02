﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using uic_forms.models;
using uic_forms.services;

namespace uic_forms
{
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
            debug.AlwaysWrite("Reporting from {0} - {1} ({2} days)", options.StartDate.ToShortDateString(),
                              options.EndDate.ToShortDateString(), (options.EndDate - options.StartDate).Days);

            debug.Write("Connecting to UDEQ...");
            using (var sevenFiveTwenty = new Querier(options.StartDate, options.EndDate))
            {
                var formPaths = GetFormLocations(options, "7520-1");
                debug.AlwaysWrite("Loading template for the 7520-1 form...");

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
                                                           AuthActionTypes = new[] {"PT"}
                                                       }, sevenFiveTwenty.GetPermitCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIE_{class}",
                                                       new QueryParams
                                                       {
                                                           AuthTypes = new[] {"IP", "AP", "GP", "OP"},
                                                           AuthActionTypes = new[] {"PM"}
                                                       }, sevenFiveTwenty.GetPermitCount, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIA_{class}A",
                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIA_{class}O",
                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIB_{class}A",
                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);
                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIB_{class}O",
                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);
//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC1_{class}",
//                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);
//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC2_{class}",
//                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);
//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC3_{class}",
//                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);
//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "VIIIC4_{class}",
//                                                       new QueryParams(), sevenFiveTwenty.NoOp, ref formInfo);


                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(x.Params), fields); });

                    // Output the path for manual verification of result
                    debug.AlwaysWrite("Saving 7520-1 form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }

                formPaths = GetFormLocations(options, "7520-2a");
                debug.AlwaysWrite("Loading template for the 7520-2a form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);

                    var formInfo = new List<InputMonad>
                    {
                        new InputMonad("VA_1", new QueryParams(1), sevenFiveTwenty.GetWellViolationCount),
                        new InputMonad("VB1_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"UI"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB2_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"MI", "MO"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB3_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"OM"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB4_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"PA"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB5_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"MR"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB6_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"IP", "FO", "FA", "FI", "FR", "OT"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VIA_1", new QueryParams(1), sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB1_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"NOV"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB2_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CGT"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB3_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"DAO", "FAO"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB4_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CIR"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB5_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CRR"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB6_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"SHT"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB7_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"PSE"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB8_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"INF", "TOA", "OTR"}
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIIA_1", new QueryParams(1), sevenFiveTwenty.GetWellsReturnedToCompliance),
                        new InputMonad("VIIB_1", new QueryParams(1)
                                       {
                                           StartDate = options.StartDate
                                       },
                                       sevenFiveTwenty.GetWellsReturnedToCompliance),
                        new InputMonad("VIII_1", new QueryParams(1), sevenFiveTwenty.GetContaminationViolations)
                    };

                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(x.Params), fields); });

                    debug.AlwaysWrite("Saving 7520-2a form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }

                formPaths = GetFormLocations(options, "7520-2b");
                debug.AlwaysWrite("Loading template for the 7520-2b form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);

                    SetFieldText("VIB7_1", "NA", fields);

                    var formInfo = new List<InputMonad>
                    {
                        new InputMonad("VA_1", new QueryParams(1), sevenFiveTwenty.SncViolations),
                        new InputMonad("VB1_1", new QueryParams(1)
                        {
                            Snc = true
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB2_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"MI", "MO"},
                            Snc = true
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB3_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"IP"},
                            Snc = true
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB4_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"PA"},
                            Snc = true
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB5_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"FO"},
                            Snc = true
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB6_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"FA"},
                            Snc = true
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB7_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"OT"},
                            Snc = true
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VIA_1", new QueryParams(1)
                        {
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB1_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"NOV"},
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB2_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CGT"},
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB3_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"DAO", "FAO"},
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB4_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CIR"},
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB5_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CRR"},
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB6_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"SHT"},
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIB8_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"INF", "TOA", "OTR"},
                            Snc = true
                        }, sevenFiveTwenty.GetWellsWithEnforcements),
                        new InputMonad("VIIA_1", new QueryParams(1)
                        {
                            Snc = true
                        }, sevenFiveTwenty.GetWellsReturnedToCompliance),
                        new InputMonad("VIIB_1", new QueryParams(1)
                                       {
                                           StartDate = options.StartDate,
                                           Snc = true
                                       },
                                       sevenFiveTwenty.GetWellsReturnedToCompliance),
                        new InputMonad("VIII_1", new QueryParams(1)
                        {
                            Snc = true
                        }, sevenFiveTwenty.GetContaminationViolations)
                    };

                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(x.Params), fields); });

                    debug.AlwaysWrite("Saving 7520-2b form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }

                formPaths = GetFormLocations(options, "7520-3");
                debug.AlwaysWrite("Loading template for the 7520-3 form...");

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

                    var formInfo = new List<InputMonad>
                    {
                        new InputMonad("VA_1", new QueryParams(1), sevenFiveTwenty.GetWellsInspected),
                        new InputMonad("VB1_1", new QueryParams(1)
                        {
                            InspectionType = new[] {"MI"}
                        }, sevenFiveTwenty.GetInspections),
                        new InputMonad("VB2_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"EC"}
                        }, sevenFiveTwenty.GetInspections),
                        new InputMonad("VB3_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"CO"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB4_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"WP"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VB5_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"RP"}
                        }, sevenFiveTwenty.GetViolationCount),
                        new InputMonad("VIA_1", new QueryParams(1), sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC1p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"AP"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC1f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"AP"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC2p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"CT"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC2f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"CT"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC3p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"MR"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC3f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"MR"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC4p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"WI", "WA", "AT", "SR", "OL"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIC4f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"WI", "WA", "AT", "SR", "OL"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID1p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"CR"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID1f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"CR"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID2p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"TN"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID2f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"TN"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID3p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"RC"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID3f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"RC"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID4p_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"CB", "OA", "RS", "DC", "OF"},
                            MitResult = new[] {"PS"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VID4f_1", new QueryParams(1)
                        {
                            MitTypes = new[] {"CB", "OA", "RS", "DC", "OF"},
                            MitResult = new[] {"FU", "FP", "FA"}
                        }, sevenFiveTwenty.GetMechIntegrityWells),
                        new InputMonad("VIIA_1", new QueryParams(1), sevenFiveTwenty.GetRemedialWells),
                        new InputMonad("VIIB1_1", new QueryParams(1)
                        {
                            RemedialAction = new[] {"CS"}
                        }, sevenFiveTwenty.GetRemedialWells),
                        new InputMonad("VIIB2_1", new QueryParams(1)
                        {
                            RemedialAction = new[] {"TR", "PR"}
                        }, sevenFiveTwenty.GetRemedialWells),
                        new InputMonad("VIIB3_1", new QueryParams(1)
                        {
                            RemedialAction = new[] {"PA"}
                        }, sevenFiveTwenty.GetRemedialWells),
                        new InputMonad("VIIB4_1", new QueryParams(1)
                        {
                            RemedialAction = new[] {"OT"}
                        }, sevenFiveTwenty.GetRemedialWells)
                    };

                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(x.Params), fields); });

                    debug.AlwaysWrite("Saving 7520-3 form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }

                formPaths = GetFormLocations(options, "7520-4");
                debug.AlwaysWrite("Loading template for the 7520-4 form...");

                using (var file = new FileStream(formPaths.Item1, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;
                    var include = new Collection<ViolationModel>();
                    var formalActions = new[] {"CIR", "CGT", "CRR", "DAO", "FAO", "NOV", "PSE", "TAO", "SHT"};

                    EnableUpdates(document.AcroForm);

                    SetHeader(fields, options);

                    var violations = sevenFiveTwenty.GetViolations();

                    foreach (var violation in violations)
                    {
                        debug.Write("violation {1} date {0}", violation.ViolationDate.ToShortDateString(), violation.EsriId);
                        int days;
                        if (violation.ReturnToComplianceDate.HasValue)
                        {
                            // Yes return to compliance date
                            debug.Write("  ReturnToComplianceDate - Yes");
                            days = (violation.ReturnToComplianceDate.Value - violation.ViolationDate).Days;
                            debug.Write("  return to compliance date of {0}",
                                        violation.ReturnToComplianceDate.Value.ToShortDateString());
                            debug.Write("  {0} days of violation", days);

                            if (days < 180)
                            {
                                debug.Write("  skipping because less than 180");
                                continue;
                            }

                            days = (options.EndDate - violation.ReturnToComplianceDate.Value).Days;
                            debug.Write("  {0} days since violation reporting", days);

                            if (days > 90)
                            {
                                debug.Write("skipping because not within 90 day reporting period");
                                continue;
                            }

                            debug.Write("including");
                            violation.ReturnToCompliance = true;

                            include.Add(violation);
                        }
                        else
                        {
                            debug.Write("  ReturnToComplianceDate - No");
                            // No return to compliance date
                            if (string.IsNullOrEmpty(violation.EnforcementType))
                            {
                                // No enforcement record
                                debug.Write("  Enforcement - No");

                                // No return to compliance date
                                days = (options.EndDate - violation.ViolationDate).Days;
                                debug.Write("  {0} days since violation reporting", days);

                                if (days < 180)
                                {
                                    debug.Write("skipping because not within 180 day reporting period");
                                    continue;
                                }

                                debug.Write("including");

                                include.Add(violation);
                            }
                            else
                            {
                                // Yes enforcement
                                debug.Write("  Enforcement - Yes");
                                debug.Write("  Enforcement type {0}", violation.EnforcementType);

                                if (!formalActions.Contains(violation.EnforcementType))
                                {
                                    debug.Write("  Formal Enforcement Action - No");

                                    days = (options.EndDate - violation.ViolationDate).Days;
                                    debug.Write("  {0} days since violation reporting", days);

                                    if (days < 180)
                                    {
                                        debug.Write("skipping because not within 180 day reporting period");
                                        continue;
                                    }

                                    debug.Write("including");

                                    include.Add(violation);
                                }
                                else
                                {
                                    // Yes formal action type
                                    debug.Write("  Formal Enforcement Action - Yes");
                                    debug.Write("  formal action type {0}", violation.EnforcementType);

                                    if (!violation.EnforcementDate.HasValue)
                                    {
                                        debug.Write("skipping since no enforcement date");
                                        continue;
                                    }

                                    days = (violation.EnforcementDate.Value - violation.ViolationDate).Days;
                                    debug.Write("  {0} days since enforcement", days);

                                    if (days < 180)
                                    {
                                        debug.Write("skipping because not within 180 day period");
                                        continue;
                                    }

                                    days = (violation.EnforcementDate.Value - options.EndDate).Days;
                                    debug.Write("  {0} days since enforcement reporting period", days);

                                    if (days > 90)
                                    {
                                        debug.Write("skipping because not within 90 day reporting period");
                                        continue;
                                    }

                                    debug.Write("including");

                                    violation.Enforcement = true;

                                    include.Add(violation);
                                }
                            }
                        }
                    }

                    // these are not static
                    var checkboxFields = new List<string>
                    {
                        "UI_",
                        "MI_",
                        "IP_",
                        "PA_",
                        "FO_",
                        "F_",
                        "OV_",
                        "NOV_",
                        "CA_",
                        "AO_",
                        "CivR_",
                        "CrimR_",
                        "WSI_",
                        "PS_",
                        "OE_"
                    };
                    // create a new page for every 9 wells?
                    const int pageSize = 9;
                    var row = 1;
                    foreach (var violation in include)
                    {
                        if (row > pageSize)
                        {
                            debug.AlwaysWrite("Too many violations skipping");
                            continue;
                        }

                        var value = sevenFiveTwenty.GetWellSubClass(violation.WellId);
                        var contact = sevenFiveTwenty.GetContactAddress(violation.WellId);

                        SetFieldText("WCT_" + row, value, fields);
//                        SetFieldText("NAO_" + row, contact?.Address(), fields);
                        SetFieldText("NAO_" + row, violation.Id, fields);

                        SetFieldText("WID_" + row, sevenFiveTwenty.GetWellId(violation.WellId), fields);
                        SetFieldText("DOV_" + row, violation.ViolationDate.ToString("MMM dd, yyyy"), fields);
                        if (violation.EnforcementDate.HasValue)
                        {
                            SetFieldText("DOE_" + row, violation.EnforcementDate.Value.ToString("MMM dd, yyyy"),
                                         fields);
                        }

                        if (violation.ReturnToComplianceDate.HasValue)
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

                    debug.AlwaysWrite("Saving 7520-4 form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }
            }

            debug.AlwaysWrite("Reported from {0} - {1} ({2} days)", options.StartDate.ToShortDateString(),
                              options.EndDate.ToShortDateString(), (options.EndDate - options.StartDate).Days);
            debug.AlwaysWrite("Finished: {0}", start.Elapsed);
            Console.ReadKey();
        }

        private static void SetHeader(PdfAcroField.PdfAcroFieldCollection fields, CliOptions options)
        {
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

            ((PdfTextField) fields[field]).Value = new PdfString(value.ToString());
        }

        private static void SetFieldCheck(int row, string field, bool value, PdfAcroField.PdfAcroFieldCollection fields)
        {
            ((PdfCheckBoxField) fields[field + row]).Checked = value;
        }
    }
}
