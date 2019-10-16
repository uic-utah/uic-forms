using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using uic_forms.models;

namespace uic_forms.services
{
    internal class Querier : IDisposable
    {
        private readonly SqlConnection _connection;
        private readonly DateTime _endDate;
        private readonly DateTime _startDate;
        private readonly Dictionary<dynamic, dynamic> _facilityLookup;

        internal Querier(CliOptions options)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("App.json", false, true)
                .Build();

            _startDate = options.StartDate;
            _endDate = options.EndDate;

            _connection = new SqlConnection(config.GetConnectionString("dev"));

            try
            {
                _connection.Open();
            }
            catch (Exception)
            {
                throw new Exception("Could not connect to database");
            }

            _facilityLookup = _connection.Query("SELECT Guid, FacilityName from UICFacility_evw")
                                         .ToDictionary(x => x.Guid, y => y.FacilityName);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public IReadOnlyCollection<string> GetPermitCount(QueryParams options)
        {
            var actionTypes = options.AuthActionTypes as string[] ?? options.AuthActionTypes.ToArray();
            var types = options.AuthTypes as string[] ?? options.AuthTypes.ToArray();

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;
            vars.end = _endDate;

            var query = @"SELECT DISTINCT(Action_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                        FROM Action_view 
                        INNER JOIN Permit_view 
                            ON Action_view.Authorization_FK = Permit_view.GUID 
                        LEFT OUTER JOIN Well_view 
                            ON Permit_view.GUID = Well_view.Authorization_FK 
                        WHERE Action_view.AuthorizationActionDate BETWEEN @start AND @end ";

            if (actionTypes.Length == 1)
            {
                query += "AND Action_view.AuthorizationActionType = @actionCodes ";
                vars.actionCodes = actionTypes[0];
            }
            else if (actionTypes.Length > 1)
            {
                query += "AND Action_view.AuthorizationActionType in @actionCodes ";
                vars.actionCodes = actionTypes;
            }

            if (types.Length == 1)
            {
                query += "AND Permit_view.AuthorizationType = @authTypes ";
                vars.authTypes = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Permit_view.AuthorizationType in @authTypes ";
                vars.authTypes = types;
            }

            query += "AND Well_view.WellClass = @wellClass";

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICAUTHORIZATIONACTION: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetWellPermitCount(QueryParams options)
        {
            var actionTypes = options.AuthActionTypes as string[] ?? options.AuthActionTypes.ToArray();
            var types = options.AuthTypes as string[] ?? options.AuthTypes.ToArray();

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;
            vars.end = _endDate;

            var query = @"SELECT DISTINCT(Well_view.GUID) as ItemId, Well_view.Facility_FK as FacilityId
                        FROM Permit_view 
                        INNER JOIN Well_view 
                            ON Permit_view.GUID = Well_view.Authorization_FK 
                        INNER JOIN Action_view 
                            ON Permit_view.GUID = Action_view.Authorization_FK 
                        WHERE Well_view.WellClass = @wellClass ";

            if (actionTypes.Length == 1)
            {
                query += "AND Action_view.AuthorizationActionType = @actionCodes ";
                vars.actionCodes = actionTypes[0];
            }
            else if (actionTypes.Length > 1)
            {
                query += "AND Action_view.AuthorizationActionType in @actionCodes ";
                vars.actionCodes = actionTypes;
            }

            if (types.Length == 1)
            {
                query += "AND Permit_view.AuthorizationType = @authTypes ";
                vars.authTypes = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Permit_view.AuthorizationType in @authTypes ";
                vars.authTypes = types;
            }

            query += "AND Action_view.AuthorizationActionDate BETWEEN @start AND @end";

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWell: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetWellViolationCount(QueryParams options)
        {
            const string query = @"SELECT DISTINCT(Well_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                                 FROM Violation_view
                                 INNER JOIN Well_view
                                     ON Violation_view.Well_FK = Well_view.GUID
                                 WHERE Violation_view.ViolationDate >= @start
                                     AND Well_view.WellClass = @wellClass";

            var result = _connection.Query<NarrativeMetadata>(query, new
            {
                start = _startDate,
                wellClass = options.WellClass
            });

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWell: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetArtificialPenetrations(QueryParams options)
        {
            var types = options.WellType as int[] ?? options.WellType.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;
            vars.end = _endDate;

            var query = @"SELECT DISTINCT(Artificial_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
               FROM 
                   Artificial_view 
               LEFT OUTER JOIN Well_view 
                   ON Artificial_view.GUID = Well_view.AOR_FK 
               WHERE 
                  Well_view.wellClass = @wellClass ";

            if (options.CaType > 0)
            {
                query += "AND Artificial_view.ArtPen_CAType = @catype ";
                query += "AND Artificial_view.ArtPen_CADate BETWEEN @start AND @end ";
                vars.catype = options.CaType;
            }
            else
            {
                query += "AND Artificial_view.ArtPen_ReviewDate BETWEEN @start AND @end ";
            }

            if (options.Ident4Ca)
            {
                query += "AND Artificial_view.Ident4CA = @yes ";
                vars.yes = 1;
            }

            if (options.WellType.Count() == 1)
            {
                query += "AND Artificial_view.ArtPen_WellType = @wellType ";
                vars.wellType = types[0];
            }
            else if (options.WellType.Count() > 1)
            {
                query += "AND Artificial_view.ArtPen_WellType in @wellType ";
                vars.wellType = types;
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICARTPEN: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetViolationCount(QueryParams options)
        {
            var types = options.ViolationTypes as string[] ?? options.ViolationTypes.ToArray();

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;

            var query = @"SELECT DISTINCT(Violation_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                        FROM Violation_view 
                        INNER JOIN Well_view 
                            ON Violation_view.Well_FK = Well_view.GUID 
                        WHERE Violation_view.ViolationDate >= @start 
                            AND Well_view.WellClass = @wellClass ";

            if (types.Length == 1)
            {
                query += "AND Violation_view.ViolationType = @violationTypes ";
                vars.violationTypes = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Violation_view.ViolationType in @violationTypes ";
                vars.violationTypes = types;
            }

            if (options.Snc)
            {
                query += "AND Violation_view.SignificantNonCompliance = @snc ";
                vars.snc = "Y";
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICVIOLATION: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetWellsWithEnforcements(QueryParams options)
        {
            var types = options.EnforcementTypes as string[] ?? options.EnforcementTypes.ToArray();
            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = @"SELECT DISTINCT(Well_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId 
                        FROM UICViolationToEnforcement_evw 
                        INNER JOIN Enforcement_view 
                            ON UICViolationToEnforcement_evw.EnforcementGUID = Enforcement_view.GUID 
                        INNER JOIN Violation_view
                            ON UICViolationToEnforcement_evw.ViolationGUID = Violation_view.GUID
                        INNER JOIN Well_view
                            ON Violation_view.Well_FK = Well_view.GUID
                        WHERE Enforcement_view.EnforcementDate >= @start
                            AND Well_view.WellClass = @wellClass ";

            if (types.Length == 1)
            {
                query += "AND Enforcement_view.EnforcementType = @enforcementType ";
                vars.enforcementType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Enforcement_view.EnforcementType in @enforcementType ";
                vars.enforcementType = types;
            }

            if (options.Snc)
            {
                query += "AND Violation_view.SignificantNonCompliance = @snc ";
                vars.snc = "Y";
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWELL: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetWellsReturnedToCompliance(QueryParams options)
        {
            var query = @"SELECT DISTINCT(Well_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                        FROM Violation_view
                        INNER JOIN Well_view
                            ON Violation_view.Well_FK = Well_view.GUID
                        WHERE(Well_view.WellClass = @wellClass)
                            AND (Violation_view.ReturnToComplianceDate BETWEEN @start AND @end) ";

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = options.StartDate ?? _endDate - TimeSpan.FromDays(90);
            vars.end = _endDate;

            if (options.Snc)
            {
                query += "AND Violation_view.SignificantNonCompliance = @snc ";
                vars.snc = "Y";
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWELL: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetContaminationViolations(QueryParams options)
        {
            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;
            vars.contamination = "Y";

            var query = @"SELECT DISTINCT(Violation_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                        FROM Well_view
                        INNER JOIN Violation_view
                            ON Violation_view.Well_FK = Well_view.GUID
                        WHERE Well_view.WellClass = @wellClass
                            AND Violation_view.USDWContamination = @contamination
                            AND Violation_view.ViolationDate >= @start ";

            if (options.Snc)
            {
                query += "AND Violation_view.SignificantNonCompliance = @snc ";
                vars.snc = "Y";
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICVIOLATION: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }
        
        public IReadOnlyCollection<string> CalculatePercentResolved(QueryParams options)
        {
            const string query = @"SELECT Violation_view.OBJECTID as esriid,
                            Violation_view.ViolationDate,
                            Violation_view.ReturnToComplianceDate
                        FROM Violation_view 
                        INNER JOIN Well_view 
                            ON Violation_view.Well_FK = Well_view.GUID 
                        WHERE Well_view.WellClass = @wellClass
                            AND Violation_view.ViolationType = 'MI'";

            var violations = _connection.Query<QueryModel>(query, new
            {
                wellClass = options.WellClass
            });

            var a = 0M;
            var b = 0M;

            // WellClass code = '1'.   
            // A = Number of Violations where in the UICViolation tbl, 
            // ViolationType code = 'MI' AND 
            // ViolationDate is between (ReportingFromDate - 90 days) and ReportingToDate  

            // B = Number of Violations where in the UICViolation tbl, 
            // ViolationType code = 'MI' 
            // AND ViolationDate is between (ReportingFromDate - 90 days) and ReportingToDate 
            // AND ReturnToComplianceDate is between (inclusive) ReportingFromDate and ReportingToDate 
            // AND ReturnToComplianceDate - ViolationDate for ViolationType code = 'MI' is less than 90 days.   

            // Percentage = 100 x (B/A)

            foreach (var violation in violations)
            {
                var violationDate = violation.ViolationDate;


                if (violationDate < _endDate &&
                    violationDate >= _startDate - TimeSpan.FromDays(90))
                {
                    a += 1;
                }

                if (!violation.ReturnToComplianceDate.HasValue)
                {
                    continue;
                }

                var returnToComplianceDate = violation.ReturnToComplianceDate.Value;

                if (violationDate < _endDate &&
                    violationDate >= _startDate - TimeSpan.FromDays(90) &&
                    returnToComplianceDate >= _startDate &&
                    returnToComplianceDate <= _endDate &&
                    (returnToComplianceDate - violationDate).Days < TimeSpan.FromDays(90).Days)
                {
                    b += 1;
                }
            }

            if (a == 0 || b == 0)
            {
                return new [] {0.ToString()};
            }

            return new []{ $"{ Math.Round(b / a * 100, 2) }%"};
        }

        public IReadOnlyCollection<string> SncViolations(QueryParams options)
        {
            const string query = @"SELECT DISTINCT(Well_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                                 FROM Well_view 
                                 INNER JOIN Violation_view 
                                    ON Violation_view.Well_FK = Well_view.GUID 
                                 WHERE Well_view.WellClass = @wellClass 
                                    AND Violation_view.SignificantNonCompliance = @compliance 
                                    AND Violation_view.ViolationDate >= @start";

            var result = _connection.Query<NarrativeMetadata>(query, new
            {
                wellClass = options.WellClass,
                start = _startDate,
                compliance = "Y"
            });

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWELL: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetWellOperatingStatus(QueryParams options)
        {
            const string query = @"SELECT Enforcement_view.EnforcementDate,
                Violation_view.objectid as esriid, Well_view.facility_FK as FacilityId, UICWellOperatingStatus_evw.guid as ItemId
            FROM 
                Violation_view
            INNER JOIN Well_view 
                ON Well_view.GUID = Violation_view.Well_FK
            INNER JOIN UICWellOperatingStatus_evw 
                ON UICWellOperatingStatus_evw.Well_FK = Well_view.GUID
            LEFT OUTER JOIN UICViolationToEnforcement_evw 
                ON UICViolationToEnforcement_evw.ViolationGUID = Violation_view.GUID
            LEFT OUTER JOIN Enforcement_view 
                ON UICViolationToEnforcement_evw.EnforcementGUID = Enforcement_view.GUID
            WHERE Well_view.WellClass = @wellClass
                AND Violation_view.SignificantNonCompliance = @yes
                AND Violation_view.Endanger = @yes
                AND UICWellOperatingStatus_evw.OperatingStatusType = @operatingType
                AND UICWellOperatingStatus_evw.OperatingStatusDate >= @start";

            var result = _connection.Query<QueryModel>(query, new
            {
                yes = 'Y',
                operatingType = "PA",
                start = _startDate,
                wellClass = options.WellClass
            });

            var response = new List<string>();
            foreach (var metadata in result.Where(x => x.EnforcementDate.HasValue == options.HasEnforcement))
            {
                response.Add($"UICWELLOPERATINGSTATUS: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetWellsInspected(QueryParams options)
        {
            const string query = @"SELECT DISTINCT(Well_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                                    FROM Well_view 
                                 INNER JOIN Inspection_view 
                                    ON Well_view.GUID = Inspection_view.Well_FK 
                                 WHERE Well_view.WellClass = @wellClass 
                                    AND Inspection_view.InspectionDate >= @start
                                    AND Inspection_view.Facility_FK is NULL";

            var result = _connection.Query<NarrativeMetadata>(query, new
            {
                wellClass = options.WellClass,
                start = _startDate
            });

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWELL: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetInspections(QueryParams options)
        {
            var types = options.InspectionType as string[] ?? options.InspectionType.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = @"SELECT DISTINCT(Inspection_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                            FROM Inspection_view 
                        INNER JOIN Well_view 
                            ON Well_view.GUID = Inspection_view.Well_FK 
                        WHERE Well_view.WellClass = @wellClass 
                            AND Inspection_view.InspectionDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND Inspection_view.InspectionType = @inspectionType ";
                vars.inspectionType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Inspection_view.InspectionType in @inspectionType ";
                vars.inspectionType = types;
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object) vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICINSPECTION: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetMechIntegrities(QueryParams options)
        {
            var types = options.MitTypes as string[] ?? options.MitTypes.ToArray();
            var results = options.MitResult as string[] ?? options.MitResult.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = @"SELECT DISTINCT(Well_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                            FROM Well_view
                        INNER JOIN Mit_view
                            ON Well_view.GUID = Mit_view.Well_FK 
                        WHERE Well_view.WellClass = @wellClass 
                            AND Mit_view.MITDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND Mit_view.MITType = @mitType ";
                vars.mitType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Mit_view.mitType in @mitType ";
                vars.mitType = types;
            }

            if (results.Length == 1)
            {
                query += "AND Mit_view.MITResult = @mitResult ";
                vars.mitResult = results[0];
            }
            else if (results.Length > 1)
            {
                query += "AND Mit_view.MITResult in @mitResult ";
                vars.mitResult = results;
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object) vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWell: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetMechIntegrityWells(QueryParams options)
        {
            var types = options.MitTypes as string[] ?? options.MitTypes.ToArray();
            var results = options.MitResult as string[] ?? options.MitResult.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = @"SELECT DISTINCT(Mit_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                            FROM Well_view
                        INNER JOIN Mit_view
                            ON Well_view.GUID = Mit_view.Well_FK 
                        WHERE Well_view.WellClass = @wellClass 
                            AND Mit_view.MITDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND Mit_view.MITType = @mitType ";
                vars.mitType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Mit_view.mitType in @mitType ";
                vars.mitType = types;
            }

            if (results.Length == 1)
            {
                query += "AND Mit_view.MITResult = @mitResult ";
                vars.mitResult = results[0];
            }
            else if (results.Length > 1)
            {
                query += "AND Mit_view.MITResult in @mitResult ";
                vars.mitResult = results;
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICMIT: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetRemedialWells(QueryParams options)
        {
            const string query = @"SELECT DISTINCT(Well_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                            FROM Well_view 
                        INNER JOIN Mit_view
                            ON Well_view.GUID = Mit_view.Well_FK 
                        WHERE Well_view.WellClass = @wellClass 
                            AND Mit_view.MITRemActDate >= @start ";

            var result = _connection.Query<NarrativeMetadata>(query, new
            {
                start = _startDate,
                wellClass = options.WellClass
            });

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICWELL: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetRemedials(QueryParams options)
        {
            var types = options.RemedialAction as string[] ?? options.RemedialAction.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = @"SELECT DISTINCT(Mit_view.Guid) as ItemId, Well_view.Facility_FK as FacilityId
                            FROM Well_view 
                        INNER JOIN Mit_view
                            ON Well_view.GUID = Mit_view.Well_FK 
                        WHERE Well_view.WellClass = @wellClass 
                            AND Mit_view.MITRemActDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND Mit_view.MITRemediationAction = @action ";
                vars.action = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND Mit_view.MITRemediationAction in @action ";
                vars.action = types;
            }

            var result = _connection.Query<NarrativeMetadata>(query, (object)vars);

            var response = new List<string>();
            foreach (var metadata in result)
            {
                response.Add($"UICMIT: GUID='{metadata.ItemId:B}' UICFACILITY: FacilityName='{_facilityLookup[metadata.FacilityId]}'");
            }

            return response.AsReadOnly();
        }

        public IEnumerable<QueryModel> GetViolations()
        {
            const string query = @"SELECT 
            Violation_view.GUID as Id,
            Violation_view.OBJECTID as esriid,
            Violation_view.Well_FK as wellid,
            Violation_view.ViolationDate, 
            Violation_view.SignificantNonCompliance, 
            Violation_view.ReturnToComplianceDate, 
            Enforcement_view.Guid as EnforcementId,
            Enforcement_view.EnforcementDate, 
            Enforcement_view.EnforcementType
                FROM
            Enforcement_view
                INNER JOIN UICViolationToEnforcement_evw ON Enforcement_view.GUID = UICViolationToEnforcement_evw.EnforcementGUID
                FULL JOIN Violation_view ON UICViolationToEnforcement_evw.ViolationGUID = Violation_view.GUID
            WHERE
            Violation_view.SignificantNonCompliance = @yes
            ORDER BY esriid";

            return _connection.Query<QueryModel>(query, new
            {
                yes = 'Y'
            });
        }

        public string GetWellSubClass(Guid wellid)
        {
//           Class 1 wells only:
//              WellSubClass coded values of '1001' '1003' should be reported as '1H';
//              '1000' as '1I';
//              '1002' as '1M'
//              Coded value of '1999' is a catch all code
//
//           Class 3, 4, and 5 wells should be reported only with their WellClass
            const string query = "SELECT WellClass, WellSubClass " +
                                 "FROM Well_view " +
                                 "WHERE GUID = @wellId";

            var codes = _connection.QueryFirstOrDefault(query, new
            {
                wellid
            });

            var wellClass = codes.WellClass;
            var subClass = codes.WellSubClass;

            if (wellClass != 1)
            {
                return wellClass.ToString();
            }

            switch (subClass)
            {
                case 1000:
                {
                    return "1I";
                }
                case 1002:
                {
                    return "1M";
                }
                case 1001:
                case 1003:
                {
                    return "1H";
                }
                default:
                {
                    return "";
                }
            }
        }

        public Contact GetContactAddress(Guid wellId)
        {
            const string query = @"SELECT 
    ContactName, 
    ContactMailAddress, 
    ContactMailCity, 
    ContactMailState, 
    ZipCode5, 
    ZipCode4, 
    ContactType 
FROM 
    UICContact_evw 
WHERE 
    ContactType in (1, 3, 2) 
AND
    ContactType.Facility_FK = @facilityId";

            var facilityId = GetFacilityFromWell(wellId);

            var contacts = _connection.Query<Contact>(query, new
            {
                facilityId = facilityId.ToString().ToUpperInvariant()
            }).ToList();

            return contacts.FirstOrDefault(x => x.ContactType == 1) ??
                   contacts.FirstOrDefault(x => x.ContactType == 3) ??
                   contacts.FirstOrDefault(x => x.ContactType == 2);
        }

        public Guid GetFacilityFromWell(Guid wellId)
        {
            return _connection.QueryFirstOrDefault<Guid>("SELECT Facility_FK " +
                                                         "FROM Well_view " +
                                                         "WHERE GUID = @wellId", new
            {
                wellId
            });
        }

        public IEnumerable<DomainModel> GetDomainValuesFor(string domainName)
        {
            const string query = @"SELECT
	cv_domain.value('DomainName[1]', 'nvarchar(50)') AS 'DomainName',
	coded_value.value('Code[1]','nvarchar(50)') AS 'Code',
	coded_value.value('Name[1]','nvarchar(50)') AS 'Value'
FROM
	sde.GDB_ITEMS AS items INNER JOIN sde.GDB_ITEMTYPES AS itemtypes ON
		items.Type = itemtypes.UUID
CROSS APPLY	
	items.Definition.nodes('/GPCodedValueDomain2/CodedValues/CodedValue') AS CodedValues(coded_value)
CROSS APPLY	
	items.Definition.nodes('/GPCodedValueDomain2') AS CVDomains(cv_domain)
WHERE 
	itemtypes.Name = 'Coded Value Domain'";

            var domains = _connection.Query<DomainModel>(query);

            return domains.Where(x => x.DomainName.Equals(domainName.ToUpperInvariant(),
                                                          StringComparison.InvariantCultureIgnoreCase));
        }

        public string GetWellId(Guid wellId)
        {
            var well = _connection.QueryFirstOrDefault<WellId>(@"SELECT 
            WellId as Id,
            AuthorizationID,
            AuthorizationType as code
                FROM
            Well_view 
                FULL JOIN Permit_view ON Permit_view.GUID = Well_view.Authorization_FK
            WHERE
                Well_view.GUID = @wellId", new
            {
                wellId
            });

            if (new[] {"IP", "AP", "GP", "EP", "OP"}.Contains(well?.Code))
            {
                return well?.AuthorizationId;
            }

            return well?.Id;
        }

        public void GetViolationCheckmarks(Guid violationId, string enforcementType,
                                           Dictionary<string, bool> fields)
        {
            var type = _connection.QueryFirstOrDefault<string>("SELECT ViolationType " +
                                                               "FROM Violation_view " +
                                                               "WHERE GUID = @violationId", new
            {
                violationId
            });

            void SetValueIfExists(string field, bool value, Dictionary<string, bool> dict)
            {
                if (dict.ContainsKey(field))
                {
                    dict[field] = value;
                }
            }

            switch (type)
            {
                case "UI":
                    SetValueIfExists("UI", true, fields);
                    break;
                case "MI":
                case "MO":
                    SetValueIfExists("MI", true, fields);
                    break;
                case "IP":
                    SetValueIfExists("IP", true, fields);
                    break;
                case "PA":
                    SetValueIfExists("PA", true, fields);
                    break;
                case "FO":
                    SetValueIfExists("FO", true, fields);
                    break;
                case "FA":
                    SetValueIfExists("F", true, fields);
                    break;
                case "OT":
                    SetValueIfExists("OV", true, fields);
                    break;
            }

            switch (enforcementType)
            {
                case "NOV":
                    SetValueIfExists("NOV", true, fields);
                    break;
                case "CGT":
                    SetValueIfExists("CA", true, fields);
                    break;
                case "DAO":
                case "FAO":
                    SetValueIfExists("AO", true, fields);
                    break;
                case "CIR":
                    SetValueIfExists("CivR", true, fields);
                    break;
                case "CRR":
                    SetValueIfExists("CrimR", true, fields);
                    break;
                case "SHT":
                    SetValueIfExists("WSI", true, fields);
                    break;
                default:
                    SetValueIfExists("OE", true, fields);
                    break;
            }
        }

        public FormInventory GetInventory(string fiscalYear)
        {
            var inventory = new FormInventory
            {
                FedFiscalYr = fiscalYear,
                ClassIh = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(Well_view.GUID)
    ) 
FROM 
    Well_view 
    INNER JOIN UICWellOperatingStatus_evw ON Well_view.GUID = UICWellOperatingStatus_evw.Well_FK 
WHERE 
    Well_view.WellSubClass = 1001 
    AND UICWELLOperatingStatus_evw.OperatingStatusType in ('PW', 'UC', 'AC', 'TA') 
    AND Well_view.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    )"),
                ClassIOther = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(Well_view.GUID)
    ) 
FROM 
    Well_view 
    INNER JOIN UICWellOperatingStatus_evw ON Well_view.GUID = UICWellOperatingStatus_evw.Well_FK 
WHERE 
    Well_view.WellSubClass in (1000, 1002, 1003, 1999) 
    AND UICWELLOperatingStatus_evw.OperatingStatusType in ('PW', 'UC', 'AC', 'TA') 
    AND Well_view.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    )"),
                ClassIiiWells = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(Well_view.GUID)
    ) 
FROM 
    Well_view 
    INNER JOIN UICWellOperatingStatus_evw ON Well_view.GUID = UICWellOperatingStatus_evw.Well_FK 
WHERE 
    Well_view.WellClass = 3 
    AND UICWELLOperatingStatus_evw.OperatingStatusType in ('PW', 'UC', 'AC', 'TA') 
    AND Well_view.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    )"),
                ClassIiiSites = _connection.QueryFirstOrDefault<string>(@"SELECT COUNT(DISTINCT(Well_view.facility_FK))
	FROM Well_view
WHERE
	Well_view.WellClass = 3"),
                ClassIvWells = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(Well_view.GUID)
    ) 
FROM 
    Well_view 
    INNER JOIN UICWellOperatingStatus_evw ON UICWellOperatingStatus_evw.Well_FK = Well_view.Guid 
WHERE 
    WellClass = 4 
    AND Well_view.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    ) 
    AND UICWellOperatingStatus_evw.OperatingStatusType in ('PW', 'PR', 'UC', 'AC', 'TA')"),
                ClassVWells = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(Well_view.GUID)
    ) 
FROM 
    Well_view 
    INNER JOIN UICWellOperatingStatus_evw ON UICWellOperatingStatus_evw.Well_FK = Well_view.Guid 
WHERE 
    WellClass = 5 
    AND UICWellOperatingStatus_evw.OperatingStatusType in ('PW', 'PR', 'UC', 'AC', 'TA') 
    AND Well_view.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    )")
            };

            return inventory;
        }

        private struct NarrativeMetadata
        {
            public Guid ItemId { get; set; }
            public Guid FacilityId { get; set; }
        }
    }
}
