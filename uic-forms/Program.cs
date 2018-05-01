using System;
using System.CodeDom;
using System.Collections;
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

                    var formInfo = new List<InputMonad>
                    {
                        new InputMonad("V_1", new QueryParams(1)
                        {
                            AuthActionTypes = new[] {"AI", "AM", "AR"}
                        }, sevenFiveTwenty.GetPermitCount),
                        new InputMonad("VIA_1N", new QueryParams(1)
                        {
                            AuthTypes = new[] {"IP"},
                            AuthActionTypes = new[] {"PI"}
                        }, sevenFiveTwenty.GetPermitCount),
                        new InputMonad("VIB_1N", new QueryParams(1)
                        {
                            AuthTypes = new[] {"AP"},
                            AuthActionTypes = new[] {"PI"}
                        }, sevenFiveTwenty.GetPermitCount),
                        new InputMonad("VIC_1N", new QueryParams(1)
                        {
                            AuthTypes = new[] {"AP"},
                            AuthActionTypes = new[] {"PI"}
                        }, sevenFiveTwenty.GetWellPermitCount),
                        new InputMonad("VID_1", new QueryParams(1)
                        {
                            AuthTypes = new[] {"IP", "AP", "GP", "EP", "OP"},
                            AuthActionTypes = new[] {"PT"}
                        }, sevenFiveTwenty.GetPermitCount),
                        new InputMonad("VIE_1", new QueryParams(1)
                        {
                            AuthTypes = new[] {"IP", "AP", "GP", "OP"},
                            AuthActionTypes = new[] {"PM"}
                        }, sevenFiveTwenty.GetPermitCount)
                    };

                    formInfo.ForEach(x => { SetFieldText(x.Id, x.Query(x.Params), fields); });

//                SetFieldText("VIIIA_1A", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);
//
//                SetFieldText("VIIIA_1O", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);
//
//                SetFieldText("VIIIB_1A", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);
//
//                SetFieldText("VIIIB_1O", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);


                    //                SetFieldText("V_3", sevenOne.V3(), fields);
                    //                SetFieldText("VIA_3N", sevenOne.VIA3N(), fields);
                    //                SetFieldText("VIA_3E", sevenOne.VIA3E(), fields);
                    //                SetFieldText("VIB_3N", sevenOne.VIB3N(), fields);
                    //                SetFieldText("VIB_3E", sevenOne.VIB3E(), fields);
                    //                SetFieldText("VIC_3N", sevenOne.VIC3N(), fields);
                    //                SetFieldText("VIC_3E", sevenOne.VIC3E(), fields);
                    //                SetFieldText("VID_3", sevenOne.VID3(), fields);
                    //                SetFieldText("VIE_3", sevenOne.VIE3(), fields);
                    //                SetFieldText("VIIIA_3A", sevenOne.VIIIA3A(), fields);
                    //                SetFieldText("VIIIA_3O", sevenOne.VIIIA30(), fields);
                    //                SetFieldText("VIIIB_3A", sevenOne.VIIIB3A(), fields);
                    //                SetFieldText("VIIIB_3O", sevenOne.VIIIB3O(), fields);
                    //                SetFieldText("VIIIC1_3", sevenOne.VIIIC13(), fields);
                    //                SetFieldText("VIIIC2_3", sevenOne.VIIIC23(), fields);
                    //                SetFieldText("VIIIC3_3", sevenOne.VIIIC33(), fields);
                    //                SetFieldText("VIIIC4_3", sevenOne.VIIIC43(), fields);
                    //
                    //                SetFieldText("V_4", sevenOne.V4(), fields);
                    //                SetFieldText("VIA_4N", sevenOne.VIA4N(), fields);
                    //                SetFieldText("VIA_4E", sevenOne.VIA4E(), fields);
                    //                SetFieldText("VIB_4N", sevenOne.VIB4N(), fields);
                    //                SetFieldText("VIB_4E", sevenOne.VIB4E(), fields);
                    //                SetFieldText("VIC_4N", sevenOne.VIC4N(), fields);
                    //                SetFieldText("VIC_4E", sevenOne.VIC4E(), fields);
                    //                SetFieldText("VID_4", sevenOne.VID4(), fields);
                    //                SetFieldText("VIE_4", sevenOne.VIE4(), fields);
                    //                SetFieldText("VIIIA_4A", sevenOne.VIIIA4A(), fields);
                    //                SetFieldText("VIIIA_4O", sevenOne.VIIIA40(), fields);
                    //                SetFieldText("VIIIB_4A", sevenOne.VIIIB4A(), fields);
                    //                SetFieldText("VIIIB_4O", sevenOne.VIIIB4O(), fields);
                    //                SetFieldText("VIIIC1_4", sevenOne.VIIIC14(), fields);
                    //                SetFieldText("VIIIC2_4", sevenOne.VIIIC24(), fields);
                    //                SetFieldText("VIIIC3_4", sevenOne.VIIIC34(), fields);
                    //                SetFieldText("VIIIC4_4", sevenOne.VIIIC44(), fields);
                    //
                    //                SetFieldText("V_5", sevenOne.V5(), fields);
                    //                SetFieldText("VIA_5N", sevenOne.VIA5N(), fields);
                    //                SetFieldText("VIA_5E", sevenOne.VIA5E(), fields);
                    //                SetFieldText("VIB_5N", sevenOne.VIB5N(), fields);
                    //                SetFieldText("VIB_5E", sevenOne.VIB5E(), fields);
                    //                SetFieldText("VIC_5N", sevenOne.VIC5N(), fields);
                    //                SetFieldText("VIC_5E", sevenOne.VIC5E(), fields);
                    //                SetFieldText("VID_5", sevenOne.VID5(), fields);
                    //                SetFieldText("VIE_5", sevenOne.VIE5(), fields);
                    //                SetFieldText("VIIIA_5A", sevenOne.VIIIA5A(), fields);
                    //                SetFieldText("VIIIA_5O", sevenOne.VIIIA50(), fields);
                    //                SetFieldText("VIIIB_5A", sevenOne.VIIIB5A(), fields);
                    //                SetFieldText("VIIIB_5O", sevenOne.VIIIB5O(), fields);
                    //                SetFieldText("VIIIC1_5", sevenOne.VIIIC15(), fields);
                    //                SetFieldText("VIIIC2_5", sevenOne.VIIIC25(), fields);
                    //                SetFieldText("VIIIC3_5", sevenOne.VIIIC35(), fields);
                    //                SetFieldText("VIIIC4_5", sevenOne.VIIIC45(), fields);

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
                        new InputMonad("VIII_1", new QueryParams(1), sevenFiveTwenty.GetContaminationViolations),
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
                        }, sevenFiveTwenty.GetContaminationViolations),
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
                        "VIB_1", "VIBf_1", "VIB_3", "VIBf_3", "VIA_4", "VIB_4", "VIBf_4", "VIC1p_4", "VIC2p_4",
                        "VIC3p_4", "VIC4p_4", "VIC1f_4", "VIC2f_4", "VIC3f_4", "VIC4f_4", "VID1p_4", "VID2p_4",
                        "VID3p_4", "VID4p_4", "VID1f_4", "VID2f_4", "VID3f_4", "VID4f_4", "VIIA_4", "VIIB1_4",
                        "VIIB2_4", "VIIB3_4", "VIIB4_4", "VIB_5", "VIBf_5"
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
                            RemedialAction = new [] {"CS"}
                        }, sevenFiveTwenty.GetRemedialWells),
                        new InputMonad("VIIB2_1", new QueryParams(1)
                        {
                            RemedialAction = new [] {"TR", "PR"}
                        }, sevenFiveTwenty.GetRemedialWells),
                        new InputMonad("VIIB3_1", new QueryParams(1)
                        {
                            RemedialAction = new [] {"PA"}
                        }, sevenFiveTwenty.GetRemedialWells),
                        new InputMonad("VIIB4_1", new QueryParams(1)
                        {
                            RemedialAction = new [] {"OT"}
                        }, sevenFiveTwenty.GetRemedialWells),
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

                    EnableUpdates(document.AcroForm);

//                    SetHeader(fields, options);

                    var violations = sevenFiveTwenty.GetViolations();

                    var include = new Collection<ViolationModel>();

                    // this might be wrong
                    foreach (var violation in violations)
                    {
                        // is there an enforcement?
                        if (string.IsNullOrEmpty(violation.EnforcementType))
                        {
                            // no enforcement record
                            if (violation.ReturnToComplianceDate.HasValue)
                            {
                                if ((violation.ReturnToComplianceDate.Value - violation.ViolationDate).Days < 180)
                                {
                                    continue;
                                }

                                violation.ReturnToCompliance = true;

                                include.Add(violation);
                            }
                            else
                            {
                                // No return to compliance date
                                if ((options.EndDate - violation.ViolationDate).Days < 180)
                                {
                                    continue;
                                }

                                include.Add(violation);
                            }
                        }
                        else
                        {
                            // SNC has enforcement
                            if (!violation.EnforcementDate.HasValue || new[] {"INF", "OTR"}.Contains(violation.EnforcementType))
                            {
                                continue;
                            }

                            if ((violation.EnforcementDate.Value - violation.ViolationDate).Days < 180)
                            {
                                continue;
                            }

                            violation.Enforcement = true;

                            include.Add(violation);
                        }
                    }

                    // these are not static
                    var statics = new List<string>
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
                        var value = sevenFiveTwenty.GetWellSubClass(violation.WellId);
                        var contact = sevenFiveTwenty.GetContactAddress(violation.WellId);

                        SetFieldText("WCT_" + row, value, fields);
                        SetFieldText("NAO_" + row, contact?.Address(), fields);
                        SetFieldText("WID_" + row, sevenFiveTwenty.GetWellId(violation.WellId), fields);
                        SetFieldText("DOV_" + row, violation.ViolationDate.ToString("MMM dd, yyyy"), fields);
                        if (violation.Enforcement)
                        {
                            SetFieldText("DOE_" + row, violation.EnforcementDate.Value.ToString("MMM dd, yyyy"), fields);
                        }

                        if(violation.ReturnToCompliance)
                        {
                            SetFieldText("DOC_" + row, violation.ReturnToComplianceDate.Value.ToString("MMM dd, yyyy"), fields);
                        }

                        statics.ForEach(field => { SetFieldCheck(row, field, true, fields); });
                        row += 1;
                    }

                    debug.AlwaysWrite("Saving 7520-4 form to {0}", formPaths.Item2);

                    document.Save(formPaths.Item2);
                }
            }

            debug.AlwaysWrite("Finished: {0}", start.Elapsed);
            Console.ReadKey();
        }

        private static void SetHeader(PdfAcroField.PdfAcroFieldCollection fields, CliOptions options)
        {
            SetFieldText("DatePrepared", DateTime.Today.ToString("MMM dd, yyyy"), fields);
            SetFieldText("DateSigned", DateTime.Today.ToString("MMM dd, yyyy"), fields);
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
