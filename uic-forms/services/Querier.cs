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

            _connection = new SqlConnection(config.GetConnectionString(options.Source));

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

            var query = @"SELECT DISTINCT(UICAuthorizationAction_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                        FROM UICAuthorizationAction_evw 
                        INNER JOIN UICAuthorization_evw 
                            ON UICAuthorizationAction_evw.Authorization_FK = UICAuthorization_evw.GUID 
                        LEFT OUTER JOIN UICWell_evw 
                            ON UICAuthorization_evw.GUID = UICWell_evw.Authorization_FK 
                        WHERE UICAuthorizationAction_evw.AuthorizationActionDate BETWEEN @start AND @end ";

            if (actionTypes.Length == 1)
            {
                query += "AND UICAuthorizationAction_evw.AuthorizationActionType = @actionCodes ";
                vars.actionCodes = actionTypes[0];
            }
            else if (actionTypes.Length > 1)
            {
                query += "AND UICAuthorizationAction_evw.AuthorizationActionType in @actionCodes ";
                vars.actionCodes = actionTypes;
            }

            if (types.Length == 1)
            {
                query += "AND UICAuthorization_evw.AuthorizationType = @authTypes ";
                vars.authTypes = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICAuthorization_evw.AuthorizationType in @authTypes ";
                vars.authTypes = types;
            }

            query += "AND UICWell_evw.WellClass = @wellClass";

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

            var query = @"SELECT DISTINCT(UICWell_evw.GUID) as ItemId, UICWell_evw.Facility_FK as FacilityId
                        FROM UICAuthorization_evw 
                        INNER JOIN UICWell_evw 
                            ON UICAuthorization_evw.GUID = UICWell_evw.Authorization_FK 
                        INNER JOIN UICAuthorizationAction_evw 
                            ON UICAuthorization_evw.GUID = UICAuthorizationAction_evw.Authorization_FK 
                        WHERE UICWell_evw.WellClass = @wellClass ";

            if (actionTypes.Length == 1)
            {
                query += "AND UICAuthorizationAction_evw.AuthorizationActionType = @actionCodes ";
                vars.actionCodes = actionTypes[0];
            }
            else if (actionTypes.Length > 1)
            {
                query += "AND UICAuthorizationAction_evw.AuthorizationActionType in @actionCodes ";
                vars.actionCodes = actionTypes;
            }

            if (types.Length == 1)
            {
                query += "AND UICAuthorization_evw.AuthorizationType = @authTypes ";
                vars.authTypes = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICAuthorization_evw.AuthorizationType in @authTypes ";
                vars.authTypes = types;
            }

            query += "AND UICAuthorizationAction_evw.AuthorizationActionDate BETWEEN @start AND @end";

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
            const string query = @"SELECT DISTINCT(UICWell_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                                 FROM UICViolation_evw
                                 INNER JOIN UICWell_evw
                                     ON UICViolation_evw.Well_FK = UICWell_evw.GUID
                                 WHERE UICViolation_evw.ViolationDate >= @start
                                     AND UICWell_evw.WellClass = @wellClass";

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

            var query = @"SELECT DISTINCT UICArtPen_evw.Guid as ItemId, UICWell_evw.Facility_FK as FacilityId
                FROM 
                  UICArtPen_evw
                JOIN AreaOfReviewToArtPen_evw
                  ON AreaOfReviewToArtPen_evw.ArtPen_FK = UICArtPen_evw.GUID
                JOIN UICWell_evw
                  ON UICWell_evw.AOR_FK = AreaOfReviewToArtPen_evw.AOR_FK
                WHERE
                  UICWell_evw.wellClass = @wellClass ";

            if (options.CaType > 0)
            {
                query += "AND UICArtPen_evw.ArtPen_CAType = @catype ";
                query += "AND UICArtPen_evw.ArtPen_CADate BETWEEN @start AND @end ";
                vars.catype = options.CaType;
            }
            else
            {
                query += "AND UICArtPen_evw.ArtPen_ReviewDate BETWEEN @start AND @end ";
            }

            if (options.Ident4Ca)
            {
                query += "AND UICArtPen_evw.Ident4CA = @yes ";
                vars.yes = 1;
            }

            if (options.WellType.Count() == 1)
            {
                query += "AND UICArtPen_evw.ArtPen_WellType = @wellType ";
                vars.wellType = types[0];
            }
            else if (options.WellType.Count() > 1)
            {
                query += "AND UICArtPen_evw.ArtPen_WellType in @wellType ";
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

            var query = @"SELECT DISTINCT(UICViolation_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                        FROM UICViolation_evw 
                        INNER JOIN UICWell_evw 
                            ON UICViolation_evw.Well_FK = UICWell_evw.GUID 
                        WHERE UICViolation_evw.ViolationDate >= @start 
                            AND UICWell_evw.WellClass = @wellClass ";

            if (types.Length == 1)
            {
                query += "AND UICViolation_evw.ViolationType = @violationTypes ";
                vars.violationTypes = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICViolation_evw.ViolationType in @violationTypes ";
                vars.violationTypes = types;
            }

            if (options.Snc)
            {
                query += "AND UICViolation_evw.SignificantNonCompliance = @snc ";
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

            var query = @"SELECT DISTINCT(UICWell_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId 
                        FROM UICViolationToEnforcement_evw 
                        INNER JOIN UICEnforcement_evw 
                            ON UICViolationToEnforcement_evw.EnforcementGUID = UICEnforcement_evw.GUID 
                        INNER JOIN UICViolation_evw
                            ON UICViolationToEnforcement_evw.ViolationGUID = UICViolation_evw.GUID
                        INNER JOIN UICWell_evw
                            ON UICViolation_evw.Well_FK = UICWell_evw.GUID
                        WHERE UICEnforcement_evw.EnforcementDate >= @start
                            AND UICWell_evw.WellClass = @wellClass ";

            if (types.Length == 1)
            {
                query += "AND UICEnforcement_evw.EnforcementType = @enforcementType ";
                vars.enforcementType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICEnforcement_evw.EnforcementType in @enforcementType ";
                vars.enforcementType = types;
            }

            if (options.Snc)
            {
                query += "AND UICViolation_evw.SignificantNonCompliance = @snc ";
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
            var query = @"SELECT DISTINCT(UICWell_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                        FROM UICViolation_evw
                        INNER JOIN UICWell_evw
                            ON UICViolation_evw.Well_FK = UICWell_evw.GUID
                        WHERE(UICWell_evw.WellClass = @wellClass)
                            AND (UICViolation_evw.ReturnToComplianceDate BETWEEN @start AND @end) ";

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = options.StartDate ?? _endDate - TimeSpan.FromDays(90);
            vars.end = _endDate;

            if (options.Snc)
            {
                query += "AND UICViolation_evw.SignificantNonCompliance = @snc ";
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

            var query = @"SELECT DISTINCT(UICViolation_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                        FROM UICWell_evw
                        INNER JOIN UICViolation_evw
                            ON UICViolation_evw.Well_FK = UICWell_evw.GUID
                        WHERE UICWell_evw.WellClass = @wellClass
                            AND UICViolation_evw.USDWContamination = @contamination
                            AND UICViolation_evw.ViolationDate >= @start ";

            if (options.Snc)
            {
                query += "AND UICViolation_evw.SignificantNonCompliance = @snc ";
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
            const string query = @"SELECT UICViolation_evw.OBJECTID as esriid,
                            UICViolation_evw.ViolationDate,
                            UICViolation_evw.ReturnToComplianceDate
                        FROM UICViolation_evw 
                        INNER JOIN UICWell_evw 
                            ON UICViolation_evw.Well_FK = UICWell_evw.GUID 
                        WHERE UICWell_evw.WellClass = @wellClass
                            AND UICViolation_evw.ViolationType = 'MI'";

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
            const string query = @"SELECT DISTINCT(UICWell_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                                 FROM UICWell_evw 
                                 INNER JOIN UICViolation_evw 
                                    ON UICViolation_evw.Well_FK = UICWell_evw.GUID 
                                 WHERE UICWell_evw.WellClass = @wellClass 
                                    AND UICViolation_evw.SignificantNonCompliance = @compliance 
                                    AND UICViolation_evw.ViolationDate >= @start";

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
            const string query = @"SELECT UICEnforcement_evw.EnforcementDate,
                UICViolation_evw.objectid as esriid, UICWell_evw.facility_FK as FacilityId, UICWellOperatingStatus_evw.guid as ItemId
            FROM 
                UICViolation_evw
            INNER JOIN UICWell_evw 
                ON UICWell_evw.GUID = UICViolation_evw.Well_FK
            INNER JOIN UICWellOperatingStatus_evw 
                ON UICWellOperatingStatus_evw.Well_FK = UICWell_evw.GUID
            LEFT OUTER JOIN UICViolationToEnforcement_evw 
                ON UICViolationToEnforcement_evw.ViolationGUID = UICViolation_evw.GUID
            LEFT OUTER JOIN UICEnforcement_evw 
                ON UICViolationToEnforcement_evw.EnforcementGUID = UICEnforcement_evw.GUID
            WHERE UICWell_evw.WellClass = @wellClass
                AND UICViolation_evw.SignificantNonCompliance = @yes
                AND UICViolation_evw.Endanger = @yes
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
            const string query = @"SELECT DISTINCT(UICWell_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                                    FROM UICWell_evw 
                                 INNER JOIN UICInspection_evw 
                                    ON UICWell_evw.GUID = UICInspection_evw.Well_FK 
                                 WHERE UICWell_evw.WellClass = @wellClass 
                                    AND UICInspection_evw.InspectionDate >= @start
                                    AND UICInspection_evw.Facility_FK is NULL";

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

            var query = @"SELECT DISTINCT(UICInspection_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                            FROM UICInspection_evw 
                        INNER JOIN UICWell_evw 
                            ON UICWell_evw.GUID = UICInspection_evw.Well_FK 
                        WHERE UICWell_evw.WellClass = @wellClass 
                            AND UICInspection_evw.InspectionDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND UICInspection_evw.InspectionType = @inspectionType ";
                vars.inspectionType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICInspection_evw.InspectionType in @inspectionType ";
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

            var query = @"SELECT DISTINCT(UICWell_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                            FROM UICWell_evw
                        INNER JOIN UICMIT_evw
                            ON UICWell_evw.GUID = UICMIT_evw.Well_FK 
                        WHERE UICWell_evw.WellClass = @wellClass 
                            AND UICMIT_evw.MITDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND UICMIT_evw.MITType = @mitType ";
                vars.mitType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICMIT_evw.mitType in @mitType ";
                vars.mitType = types;
            }

            if (results.Length == 1)
            {
                query += "AND UICMIT_evw.MITResult = @mitResult ";
                vars.mitResult = results[0];
            }
            else if (results.Length > 1)
            {
                query += "AND UICMIT_evw.MITResult in @mitResult ";
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

            var query = @"SELECT DISTINCT(UICMIT_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                            FROM UICWell_evw
                        INNER JOIN UICMIT_evw
                            ON UICWell_evw.GUID = UICMIT_evw.Well_FK 
                        WHERE UICWell_evw.WellClass = @wellClass 
                            AND UICMIT_evw.MITDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND UICMIT_evw.MITType = @mitType ";
                vars.mitType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICMIT_evw.mitType in @mitType ";
                vars.mitType = types;
            }

            if (results.Length == 1)
            {
                query += "AND UICMIT_evw.MITResult = @mitResult ";
                vars.mitResult = results[0];
            }
            else if (results.Length > 1)
            {
                query += "AND UICMIT_evw.MITResult in @mitResult ";
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
            const string query = @"SELECT DISTINCT(UICWell_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                            FROM UICWell_evw 
                        INNER JOIN UICMIT_evw
                            ON UICWell_evw.GUID = UICMIT_evw.Well_FK 
                        WHERE UICWell_evw.WellClass = @wellClass 
                            AND UICMIT_evw.MITRemActDate >= @start ";

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

            var query = @"SELECT DISTINCT(UICMIT_evw.Guid) as ItemId, UICWell_evw.Facility_FK as FacilityId
                            FROM UICWell_evw 
                        INNER JOIN UICMIT_evw
                            ON UICWell_evw.GUID = UICMIT_evw.Well_FK 
                        WHERE UICWell_evw.WellClass = @wellClass 
                            AND UICMIT_evw.MITRemActDate >= @start ";

            if (types.Length == 1)
            {
                query += "AND UICMIT_evw.MITRemediationAction = @action ";
                vars.action = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICMIT_evw.MITRemediationAction in @action ";
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
            UICViolation_evw.GUID as Id,
            UICViolation_evw.OBJECTID as esriid,
            UICViolation_evw.Well_FK as wellid,
            UICViolation_evw.ViolationDate, 
            UICViolation_evw.SignificantNonCompliance, 
            UICViolation_evw.ReturnToComplianceDate, 
            UICEnforcement_evw.Guid as EnforcementId,
            UICEnforcement_evw.EnforcementDate, 
            UICEnforcement_evw.EnforcementType
                FROM
            UICEnforcement_evw
                INNER JOIN UICViolationToEnforcement_evw ON UICEnforcement_evw.GUID = UICViolationToEnforcement_evw.EnforcementGUID
                FULL JOIN UICViolation_evw ON UICViolationToEnforcement_evw.ViolationGUID = UICViolation_evw.GUID
            WHERE
            UICViolation_evw.SignificantNonCompliance = @yes
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
                                 "FROM UICWell_evw " +
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
    Facility_FK = @facilityId";

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
                                                         "FROM UICWell_evw " +
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
            UICWell_evw 
                FULL JOIN UICAuthorization_evw ON UICAuthorization_evw.GUID = UICWell_evw.Authorization_FK
            WHERE
                UICWell_evw.GUID = @wellId", new
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
                                                               "FROM UICViolation_evw " +
                                                               "WHERE GUID = @violationId", new
            {
                violationId
            });

            static void SetValueIfExists(string field, bool value, Dictionary<string, bool> dict)
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
        DISTINCT(UICWell_evw.GUID)
    ) 
FROM 
    UICWell_evw 
    INNER JOIN UICWellOperatingStatus_evw ON UICWell_evw.GUID = UICWellOperatingStatus_evw.Well_FK 
WHERE 
    UICWell_evw.WellSubClass = 1001 
    AND UICWELLOperatingStatus_evw.OperatingStatusType in ('PW', 'UC', 'AC', 'TA') 
    AND UICWell_evw.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    )"),
                ClassIOther = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(UICWell_evw.GUID)
    ) 
FROM 
    UICWell_evw 
    INNER JOIN UICWellOperatingStatus_evw ON UICWell_evw.GUID = UICWellOperatingStatus_evw.Well_FK 
WHERE 
    UICWell_evw.WellSubClass in (1000, 1002, 1003, 1999) 
    AND UICWELLOperatingStatus_evw.OperatingStatusType in ('PW', 'UC', 'AC', 'TA') 
    AND UICWell_evw.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    )"),
                ClassIiiWells = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(UICWell_evw.GUID)
    ) 
FROM 
    UICWell_evw 
    INNER JOIN UICWellOperatingStatus_evw ON UICWell_evw.GUID = UICWellOperatingStatus_evw.Well_FK 
WHERE 
    UICWell_evw.WellClass = 3 
    AND UICWELLOperatingStatus_evw.OperatingStatusType in ('PW', 'UC', 'AC', 'TA') 
    AND UICWell_evw.GUID NOT IN (
        SELECT 
            UICWellOperatingStatus_evw.Well_FK 
        FROM 
            UICWellOperatingStatus_evw 
        WHERE 
            UICWellOperatingStatus_evw.OperatingStatusType in ('PA', 'AN')
    )"),
                ClassIiiSites = _connection.QueryFirstOrDefault<string>(@"SELECT COUNT(DISTINCT(UICWell_evw.facility_FK))
	FROM UICWell_evw
WHERE
	UICWell_evw.WellClass = 3"),
                ClassIvWells = _connection.QueryFirstOrDefault<string>(@"SELECT 
    COUNT(
        DISTINCT(UICWell_evw.GUID)
    ) 
FROM 
    UICWell_evw 
    INNER JOIN UICWellOperatingStatus_evw ON UICWellOperatingStatus_evw.Well_FK = UICWell_evw.Guid 
WHERE 
    WellClass = 4 
    AND UICWell_evw.GUID NOT IN (
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
        DISTINCT(UICWell_evw.GUID)
    ) 
FROM 
    UICWell_evw 
    INNER JOIN UICWellOperatingStatus_evw ON UICWellOperatingStatus_evw.Well_FK = UICWell_evw.Guid 
WHERE 
    WellClass = 5 
    AND UICWellOperatingStatus_evw.OperatingStatusType in ('PW', 'PR', 'UC', 'AC', 'TA') 
    AND UICWell_evw.GUID NOT IN (
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
