using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
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
                EndDate = new DateTime(2018, 10, 31),
                Quarter = "QSomething",
                StartDate = new DateTime(2017, 10, 1),
                OutputPath = "c:\\temp",
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
                var output = $"{options.OutputPath}\\7520.{DateTime.Now:MM-dd-yyy}.csv";

                var datas = new List<InputMonad>();

                #region 7520-1

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "V",
                                                   new QueryParams
                                                   {
                                                       AuthActionTypes = new[] {"AI", "AM", "AR"}
                                                   }, sevenFiveTwenty.GetPermitCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vian",
                                                   new QueryParams
                                                   {
                                                       AuthTypes = new[] {"IP"},
                                                       AuthActionTypes = new[] {"PI"}
                                                   }, sevenFiveTwenty.GetPermitCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vibn",
                                                   new QueryParams
                                                   {
                                                       AuthTypes = new[] {"AP"},
                                                       AuthActionTypes = new[] {"PI"}
                                                   }, sevenFiveTwenty.GetPermitCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vicn",
                                                   new QueryParams
                                                   {
                                                       AuthTypes = new[] {"AP"},
                                                       AuthActionTypes = new[] {"PI"}
                                                   }, sevenFiveTwenty.GetWellPermitCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid",
                                                   new QueryParams
                                                   {
                                                       AuthTypes = new[]
                                                           {"IP", "AP", "GP", "EP", "OP"},
                                                       AuthActionTypes = new[] {"PD", "MD", "TP"}
                                                   }, sevenFiveTwenty.GetPermitCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vie",
                                                   new QueryParams
                                                   {
                                                       AuthTypes = new[] {"IP", "AP", "GP", "OP"},
                                                       AuthActionTypes = new[] {"PM"}
                                                   }, sevenFiveTwenty.GetPermitCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiiaa",
                                                   new QueryParams
                                                   {
                                                       WellType = new[] {1}
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiiao",
                                                   new QueryParams
                                                   {
                                                       WellType = new[] {2, 3}
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiiba",
                                                   new QueryParams
                                                   {
                                                       WellType = new[] {1},
                                                       Ident4Ca = true
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiibo",
                                                   new QueryParams
                                                   {
                                                       WellType = new[] {2, 3},
                                                       Ident4Ca = true
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiic1",
                                                   new QueryParams
                                                   {
                                                       CaType = 1
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiic2",
                                                   new QueryParams
                                                   {
                                                       CaType = 2
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiic3",
                                                   new QueryParams
                                                   {
                                                       CaType = 3
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viiic4",
                                                   new QueryParams
                                                   {
                                                       CaType = 4
                                                   }, sevenFiveTwenty.GetArtificialPenetrations, datas);

                #endregion

                #region 7520-2a

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Va", new QueryParams(),
                                                   sevenFiveTwenty.GetWellViolationCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb1", new QueryParams
                {
                    ViolationTypes = new[] {"UI"}
                }, sevenFiveTwenty.GetViolationCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb2", new QueryParams
                {
                    ViolationTypes = new[] {"MI", "MO"}
                }, sevenFiveTwenty.GetViolationCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb3", new QueryParams
                {
                    ViolationTypes = new[] {"OM", "IP"}
                }, sevenFiveTwenty.GetViolationCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb4", new QueryParams
                {
                    ViolationTypes = new[] {"PA"}
                }, sevenFiveTwenty.GetViolationCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb5", new QueryParams
                {
                    ViolationTypes = new[] {"MR", "FO", "FA", "FI", "FR"}
                }, sevenFiveTwenty.GetViolationCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb6", new QueryParams
                {
                    ViolationTypes = new[] {"OT"}
                }, sevenFiveTwenty.GetViolationCount, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Via", new QueryParams(),
                                                   sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib1", new QueryParams
                {
                    EnforcementTypes = new[] {"NOV"}
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib2", new QueryParams
                {
                    EnforcementTypes = new[] {"CGT"}
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib3", new QueryParams
                {
                    EnforcementTypes = new[] {"DAO", "FAO"}
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib4", new QueryParams
                {
                    EnforcementTypes = new[] {"CIR"}
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib5", new QueryParams
                {
                    EnforcementTypes = new[] {"CRR"}
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib6", new QueryParams
                {
                    EnforcementTypes = new[] {"SHT"}
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib7", new QueryParams
//                    {
//                        EnforcementTypes = new[] {"PSE"}
//                    }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib8", new QueryParams
                {
                    EnforcementTypes = new[] {"INF", "TOA", "OTR"}
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viia", new QueryParams
                                                   {
                                                       StartDate = options.EndDate - TimeSpan.FromDays(90)
                                                   },
                                                   sevenFiveTwenty.GetWellsReturnedToCompliance, datas);

//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viib", new QueryParams
//                                                       {
//                                                           StartDate = options.StartDate
//                                                       },
//                                                       sevenFiveTwenty.GetWellsReturnedToCompliance, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viii", new QueryParams(),
                                                   sevenFiveTwenty.GetContaminationViolations, datas);

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Ix", new QueryParams(),
                                                   sevenFiveTwenty.CalculatePercentResolved, datas, true);

                #endregion

                #region 7520-2b

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Va_2b", new QueryParams(),
                                                   sevenFiveTwenty.SncViolations, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb1_2b", new QueryParams
                {
                    ViolationTypes = new[] {"UI"},
                    Snc = true
                }, sevenFiveTwenty.GetViolationCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb2_2b", new QueryParams
                {
                    ViolationTypes = new[] {"MI", "MO"},
                    Snc = true
                }, sevenFiveTwenty.GetViolationCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb3_2b", new QueryParams
                {
                    ViolationTypes = new[] {"IP"},
                    Snc = true
                }, sevenFiveTwenty.GetViolationCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb4_2b", new QueryParams
                {
                    ViolationTypes = new[] {"PA"},
                    Snc = true
                }, sevenFiveTwenty.GetViolationCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb5_2b", new QueryParams
                {
                    ViolationTypes = new[] {"FO"},
                    Snc = true
                }, sevenFiveTwenty.GetViolationCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb6_2b", new QueryParams
                {
                    ViolationTypes = new[] {"FA"},
                    Snc = true
                }, sevenFiveTwenty.GetViolationCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb7_2b", new QueryParams
                {
                    ViolationTypes = new[] {"OT", "OM", "MR", "FI", "FR"},
                    Snc = true
                }, sevenFiveTwenty.GetViolationCount, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Via_2b", new QueryParams
                {
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib1_2b", new QueryParams
                {
                    EnforcementTypes = new[] {"NOV"},
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib2_2b", new QueryParams
                {
                    EnforcementTypes = new[] {"CGT"},
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib3_2b", new QueryParams
                {
                    EnforcementTypes = new[] {"DAO", "FAO"},
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib4_2b", new QueryParams
                {
                    EnforcementTypes = new[] {"CIR"},
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib5_2b", new QueryParams
                {
                    EnforcementTypes = new[] {"CRR"},
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib6_2b", new QueryParams
                {
                    EnforcementTypes = new[] {"SHT"},
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vib8_2b", new QueryParams
                {
                    EnforcementTypes = new[] {"INF", "TOA", "OTR"},
                    Snc = true
                }, sevenFiveTwenty.GetWellsWithEnforcements, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viia_2b", new QueryParams
                {
                    Snc = true
                }, sevenFiveTwenty.GetWellsReturnedToCompliance, datas);
//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viib_2b", new QueryParams
//                    {
//                        StartDate = options.StartDate,
//                        Snc = true
//                    }, sevenFiveTwenty.GetWellsReturnedToCompliance, datas);
//                    InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viii_2b", new QueryParams
//                    {
//                        Snc = true
//                    }, sevenFiveTwenty.GetContaminationViolations, datas);

                InputMonadGenerator.CreateMonadFor(new[] {4, 5}, "Ix_2b", new QueryParams
                {
                    HasEnforcement = true
                }, sevenFiveTwenty.GetWellOperatingStatus, datas);
                InputMonadGenerator.CreateMonadFor(new[] {4, 5}, "Ixv_2b", new QueryParams
                {
                    HasEnforcement = false
                }, sevenFiveTwenty.GetWellOperatingStatus, datas);

                #endregion

                #region 7520-3  

                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Va_3b", new QueryParams(),
                                                   sevenFiveTwenty.GetWellsInspected, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb1_3b", new QueryParams
                {
                    InspectionType = new[] {"MI"}
                }, sevenFiveTwenty.GetInspections, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb2_3b", new QueryParams
                {
                    InspectionType = new[] {"EC"}
                }, sevenFiveTwenty.GetInspections, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb3_3b", new QueryParams
                {
                    InspectionType = new[] {"CO"}
                }, sevenFiveTwenty.GetInspections, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb4_3b", new QueryParams
                {
                    InspectionType = new[] {"WP"}
                }, sevenFiveTwenty.GetInspections, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vb5_3b", new QueryParams
                {
                    InspectionType = new[] {"RP", "OT", "NW", "FI"}
                }, sevenFiveTwenty.GetInspections, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Via_3b", new QueryParams(),
                                                   sevenFiveTwenty.GetMechIntegrities, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic1p_3b", new QueryParams
                {
                    MitTypes = new[] {"AP"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic1f_3b", new QueryParams
                {
                    MitTypes = new[] {"AP"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic2p_3b", new QueryParams
                {
                    MitTypes = new[] {"CT"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic2f_3b", new QueryParams
                {
                    MitTypes = new[] {"CT"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic3p_3b", new QueryParams
                {
                    MitTypes = new[] {"MR"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic3f_3b", new QueryParams
                {
                    MitTypes = new[] {"MR"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic4p_3b", new QueryParams
                {
                    MitTypes = new[] {"WI", "WA", "AT", "SR", "OL"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vic4f_3b", new QueryParams
                {
                    MitTypes = new[] {"WI", "WA", "AT", "SR", "OL"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid1p_3b", new QueryParams
                {
                    MitTypes = new[] {"CR"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid1f_3b", new QueryParams
                {
                    MitTypes = new[] {"CR"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid2p_3b", new QueryParams
                {
                    MitTypes = new[] {"TN"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid2f_3b", new QueryParams
                {
                    MitTypes = new[] {"TN"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid3p_3b", new QueryParams
                {
                    MitTypes = new[] {"RC"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid3f_3b", new QueryParams
                {
                    MitTypes = new[] {"RC"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid4p_3b", new QueryParams
                {
                    MitTypes = new[] {"CB", "OA", "RS", "DC", "OF"},
                    MitResult = new[] {"PS"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Vid4f_3b", new QueryParams
                {
                    MitTypes = new[] {"CB", "OA", "RS", "DC", "OF"},
                    MitResult = new[] {"FU", "FP", "FA"}
                }, sevenFiveTwenty.GetMechIntegrityWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viia_3b", new QueryParams(),
                                                   sevenFiveTwenty.GetRemedialWells, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viib1_3b", new QueryParams
                {
                    RemedialAction = new[] {"CS"}
                }, sevenFiveTwenty.GetRemedials, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viib2_3b", new QueryParams
                {
                    RemedialAction = new[] {"TR", "PR"}
                }, sevenFiveTwenty.GetRemedials, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viib3_3b", new QueryParams
                {
                    RemedialAction = new[] {"PA"}
                }, sevenFiveTwenty.GetRemedials, datas);
                InputMonadGenerator.CreateMonadFor(new[] {1, 3, 4, 5}, "Viib4_3b", new QueryParams
                {
                    RemedialAction = new[] {"OT"}
                }, sevenFiveTwenty.GetRemedials, datas);

                #endregion

                datas.ForEach(x => x.Query());

                var class1 =
                    new Forms1Through3(datas.Where(x => x.WellClass == 1).ToList(), Logger)
                    {
                        FyQuarter = options.Quarter,
                        FedFiscalYr = options.EndDate.Year.ToString(),
                        WCT = "1"
                    };

                var class3 =
                    new Forms1Through3(datas.Where(x => x.WellClass == 3).ToList(), Logger)
                    {
                        FyQuarter = options.Quarter,
                        FedFiscalYr = options.EndDate.Year.ToString(),
                        WCT = "3"
                    };

                var class4 =
                    new Forms1Through3(datas.Where(x => x.WellClass == 4).ToList(), Logger)
                    {
                        FyQuarter = options.Quarter,
                        FedFiscalYr = options.EndDate.Year.ToString(),
                        WCT = "4"
                    };

                var class5 =
                    new Forms1Through3(datas.Where(x => x.WellClass == 5).ToList(), Logger)
                    {
                        FyQuarter = options.Quarter,
                        FedFiscalYr = options.EndDate.Year.ToString(),
                        WCT = "5"
                    };

                using (var writer = new StreamWriter(File.Create(output)))
                using (var csv = new CsvWriter(writer))
                {
                    writer.AutoFlush = true;
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.RegisterClassMap<Forms1Through3Mapping>();

                    csv.WriteHeader<Forms1Through3>();
                    csv.NextRecord();

                    csv.WriteRecords(new[] {class1, class3, class4, class5});
                }

                output = $"{options.OutputPath}\\7520-4.{DateTime.Now:MM-dd-yyy}.csv";
                using (var writer = new StreamWriter(File.Create(output)))
                using (var csv = new CsvWriter(writer))
                {
                    writer.AutoFlush = true;
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.RegisterClassMap<Form4Mapping>();

                    csv.WriteHeader<Form4>();
                    csv.NextRecord();

                    #region 7520-4

                    var include = new Collection<QueryModel>();
                    var formalActions = new[] {"CIR", "CGT", "CRR", "DAO", "FAO", "NOV", "PSE", "TAO", "SHT"};

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

                    #endregion

                    foreach (var violation in include)
                    {
                        var form = new Form4(Logger)
                        {
                            FyQuarter = options.Quarter,
                            FedFiscalYr = options.EndDate.Year.ToString()
                        };
                        var checkboxFields = new List<string>
                        {
                            "UI",
                            "MI",
                            "IP",
                            "PA",
                            "FO",
                            "F",
                            "OV"
                        };

                        var value = sevenFiveTwenty.GetWellSubClass(violation.WellId);
                        var contact = sevenFiveTwenty.GetContactAddress(violation.WellId);

                        form["WCT"] = value;
                        form.OperatorName = contact?.ContactName;
                        form.OperatorState = contact?.ContactMailState;
                        form.OperatorStreet = contact?.ContactMailAddress;
                        form.OperatorCity = contact?.ContactMailCity;
                        form.OperatorZIP = contact?.Zip();

                        form["WID"] = sevenFiveTwenty.GetWellId(violation.WellId);
                        form["DOV"] = violation.ViolationDate.ToString("MMM dd, yyyy");
                        if (violation.EnforcementDate.HasValue && violation.EnforcementDate.Value <= options.EndDate)
                        {
                            form["DOE"] = violation.EnforcementDate.Value.ToString("MMM dd, yyyy");
                            checkboxFields.AddRange(new[]
                            {
                                "NOV",
                                "CA",
                                "AO",
                                "CivR",
                                "CrimR",
                                "WSI",
                                "PS",
                                "OE"
                            });
                        }

                        if (violation.ReturnToComplianceDate.HasValue &&
                            violation.ReturnToComplianceDate.Value <= options.EndDate)
                        {
                            form["DOC"] = violation.ReturnToComplianceDate.Value.ToString("MMM dd, yyyy");
                        }

                        var checks = checkboxFields.ToDictionary(key => key, v => false);
                        sevenFiveTwenty.GetViolationCheckmarks(violation.Id, violation.EnforcementType, checks);

                        foreach (var item in checks)
                        {
                            form[item.Key] = item.Value ? "X" : null;
                        }

                        csv.WriteRecords(new[] {form});
                    }
                }
            }

            Logger.AlwaysWrite("Reported from {0} - {1} ({2} days)", options.StartDate.ToShortDateString(),
                               options.EndDate.ToShortDateString(), (options.EndDate - options.StartDate).Days);
            Logger.AlwaysWrite("Finished: {0}", start.Elapsed);
        }
    }
}
