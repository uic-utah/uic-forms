using System;
using System.Collections.Generic;
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

            var form75201 = Path.Combine(options.TemplateLocation, "7520-1.pdf");
            var form75201Output = Path.Combine(options.OutputPath, $"7520-1_{DateTime.Now:MM-dd-yyyy}.pdf");

            var form75202a = Path.Combine(options.TemplateLocation, "7520-2a.pdf");
            var form75202aOutput = Path.Combine(options.OutputPath, $"7520-2a_{DateTime.Now:MM-dd-yyyy}.pdf");

            debug.Write("Connecting to UDEQ...");
            using (var sevenOne = new Querier(options.StartDate, options.EndDate))
            {
                debug.AlwaysWrite("Loading template for the 7520-1 form...");
                using (var file = new FileStream(form75201, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    SetField("DatePrepared", DateTime.Today.ToString("MMM dd, yyyy"), fields);
                    SetField("DateSigned", DateTime.Today.ToString("MMM dd, yyyy"), fields);
                    SetField("ReportingFromDate", options.StartDate.ToString("MMM dd, yyyy"), fields);
                    SetField("ReportingToDate", options.EndDate.ToString("MMM dd, yyyy"), fields);

                    SetField("VIA_1E", "NA", fields);
                    SetField("VIB_1E", "NA", fields);
                    SetField("VIC_1E", "NA", fields);

                    var formInfo = new List<InputMonad>
                    {
                        new InputMonad("V_1", new QueryParams(1)
                        {
                            AuthActionTypes = new[] {"AI", "AM", "AR"}
                        }, sevenOne.GetPermitCount),
                        new InputMonad("VIA_1N", new QueryParams(1)
                        {
                            AuthTypes = new[] {"IP"},
                            AuthActionTypes = new[] {"PI"}
                        }, sevenOne.GetPermitCount),
                        new InputMonad("VIB_1N", new QueryParams(1)
                        {
                            AuthTypes = new[] {"AP"},
                            AuthActionTypes = new[] {"PI"}
                        }, sevenOne.GetPermitCount),
                        new InputMonad("VIC_1N", new QueryParams(1)
                        {
                            AuthTypes = new[] {"AP"},
                            AuthActionTypes = new[] {"PI"}
                        }, sevenOne.GetWellPermitCount),
                        new InputMonad("VID_1", new QueryParams(1)
                        {
                            AuthTypes = new[] {"IP", "AP", "GP", "EP", "OP"},
                            AuthActionTypes = new[] {"PT"}
                        }, sevenOne.GetPermitCount),
                        new InputMonad("VIE_1", new QueryParams(1)
                        {
                            AuthTypes = new[] {"IP", "AP", "GP", "OP"},
                            AuthActionTypes = new[] {"PM"}
                        }, sevenOne.GetPermitCount)
                    };

                    formInfo.ForEach(x => { SetField(x.Id, x.Query(x.Params), fields); });

//                SetField("VIIIA_1A", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);
//
//                SetField("VIIIA_1O", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);
//
//                SetField("VIIIB_1A", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);
//
//                SetField("VIIIB_1O", sevenOne.GetOtherCount(new QueryParams(1)
//                {
//
//                }), fields);


                    //                SetField("V_3", sevenOne.V3(), fields);
                    //                SetField("VIA_3N", sevenOne.VIA3N(), fields);
                    //                SetField("VIA_3E", sevenOne.VIA3E(), fields);
                    //                SetField("VIB_3N", sevenOne.VIB3N(), fields);
                    //                SetField("VIB_3E", sevenOne.VIB3E(), fields);
                    //                SetField("VIC_3N", sevenOne.VIC3N(), fields);
                    //                SetField("VIC_3E", sevenOne.VIC3E(), fields);
                    //                SetField("VID_3", sevenOne.VID3(), fields);
                    //                SetField("VIE_3", sevenOne.VIE3(), fields);
                    //                SetField("VIIIA_3A", sevenOne.VIIIA3A(), fields);
                    //                SetField("VIIIA_3O", sevenOne.VIIIA30(), fields);
                    //                SetField("VIIIB_3A", sevenOne.VIIIB3A(), fields);
                    //                SetField("VIIIB_3O", sevenOne.VIIIB3O(), fields);
                    //                SetField("VIIIC1_3", sevenOne.VIIIC13(), fields);
                    //                SetField("VIIIC2_3", sevenOne.VIIIC23(), fields);
                    //                SetField("VIIIC3_3", sevenOne.VIIIC33(), fields);
                    //                SetField("VIIIC4_3", sevenOne.VIIIC43(), fields);
                    //
                    //                SetField("V_4", sevenOne.V4(), fields);
                    //                SetField("VIA_4N", sevenOne.VIA4N(), fields);
                    //                SetField("VIA_4E", sevenOne.VIA4E(), fields);
                    //                SetField("VIB_4N", sevenOne.VIB4N(), fields);
                    //                SetField("VIB_4E", sevenOne.VIB4E(), fields);
                    //                SetField("VIC_4N", sevenOne.VIC4N(), fields);
                    //                SetField("VIC_4E", sevenOne.VIC4E(), fields);
                    //                SetField("VID_4", sevenOne.VID4(), fields);
                    //                SetField("VIE_4", sevenOne.VIE4(), fields);
                    //                SetField("VIIIA_4A", sevenOne.VIIIA4A(), fields);
                    //                SetField("VIIIA_4O", sevenOne.VIIIA40(), fields);
                    //                SetField("VIIIB_4A", sevenOne.VIIIB4A(), fields);
                    //                SetField("VIIIB_4O", sevenOne.VIIIB4O(), fields);
                    //                SetField("VIIIC1_4", sevenOne.VIIIC14(), fields);
                    //                SetField("VIIIC2_4", sevenOne.VIIIC24(), fields);
                    //                SetField("VIIIC3_4", sevenOne.VIIIC34(), fields);
                    //                SetField("VIIIC4_4", sevenOne.VIIIC44(), fields);
                    //
                    //                SetField("V_5", sevenOne.V5(), fields);
                    //                SetField("VIA_5N", sevenOne.VIA5N(), fields);
                    //                SetField("VIA_5E", sevenOne.VIA5E(), fields);
                    //                SetField("VIB_5N", sevenOne.VIB5N(), fields);
                    //                SetField("VIB_5E", sevenOne.VIB5E(), fields);
                    //                SetField("VIC_5N", sevenOne.VIC5N(), fields);
                    //                SetField("VIC_5E", sevenOne.VIC5E(), fields);
                    //                SetField("VID_5", sevenOne.VID5(), fields);
                    //                SetField("VIE_5", sevenOne.VIE5(), fields);
                    //                SetField("VIIIA_5A", sevenOne.VIIIA5A(), fields);
                    //                SetField("VIIIA_5O", sevenOne.VIIIA50(), fields);
                    //                SetField("VIIIB_5A", sevenOne.VIIIB5A(), fields);
                    //                SetField("VIIIB_5O", sevenOne.VIIIB5O(), fields);
                    //                SetField("VIIIC1_5", sevenOne.VIIIC15(), fields);
                    //                SetField("VIIIC2_5", sevenOne.VIIIC25(), fields);
                    //                SetField("VIIIC3_5", sevenOne.VIIIC35(), fields);
                    //                SetField("VIIIC4_5", sevenOne.VIIIC45(), fields);

                    // Output the path for manual verification of result
                    debug.AlwaysWrite("Saving 7520-1 form to {0}", form75201Output);

                    document.Save(form75201Output);
                }

                using (var file = new FileStream(form75202a, FileMode.Open, FileAccess.Read))
                using (var document = PdfReader.Open(file, PdfDocumentOpenMode.Modify))
                {
                    var fields = document.AcroForm.Fields;

                    EnableUpdates(document.AcroForm);

                    var formInfo = new List<InputMonad>
                    {
                        new InputMonad("VA_1", new QueryParams(1), sevenOne.GetWellViolationCount),
                        new InputMonad("VB1_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"UI"}
                        }, sevenOne.GetViolationCount),
                        new InputMonad("VB2_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"MI", "MO"}
                        }, sevenOne.GetViolationCount),
                        new InputMonad("VB3_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"OM"}
                        }, sevenOne.GetViolationCount),
                        new InputMonad("VB4_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"PA"}
                        }, sevenOne.GetViolationCount),
                        new InputMonad("VB5_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"MR"}
                        }, sevenOne.GetViolationCount),
                        new InputMonad("VB6_1", new QueryParams(1)
                        {
                            ViolationTypes = new[] {"IP", "FO", "FA", "FI", "FR", "OT"}
                        }, sevenOne.GetViolationCount),
                        new InputMonad("VIA_1", new QueryParams(1), sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB1_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"NOV"}
                        }, sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB2_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CGT"}
                        }, sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB3_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"DAO", "FAO"}
                        }, sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB4_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CIR"}
                        }, sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB5_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"CRR"}
                        }, sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB6_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"SHT"}
                        }, sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB7_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"PSE"}
                        }, sevenOne.GetWellsWithEnforcements),
                        new InputMonad("VIB8_1", new QueryParams(1)
                        {
                            EnforcementTypes = new[] {"INF", "TOA", "OTR"}
                        }, sevenOne.GetWellsWithEnforcements),
                    };

                    formInfo.ForEach(x => { SetField(x.Id, x.Query(x.Params), fields); });

                    debug.AlwaysWrite("Saving 7520-2a form to {0}", form75202aOutput);

                    document.Save(form75202aOutput);
                }
            }

            debug.AlwaysWrite("Finished: {0}", start.Elapsed);
            Console.ReadKey();
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

        private static void SetField(string field, object value, PdfAcroField.PdfAcroFieldCollection fields)
        {
            ((PdfTextField) fields[field]).Value = new PdfString(value.ToString());
        }
    }
}
