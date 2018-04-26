using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using Dapper;
using uic_forms.models;

namespace uic_forms.services
{
    public class Querier : IDisposable
    {
        private readonly SqlConnection _connection;
        private readonly DateTime _endDate;
        private readonly DateTime _startDate;

        public Querier(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate;
            _endDate = endDate;
            _connection = new SqlConnection("Data Source=udeq.agrc.utah.gov\\mspd14;" +
                                            "Initial Catalog=UDEQ;" +
                                            "Persist Security Info=True;" +
                                            $"User ID={ConfigurationManager.AppSettings["username"]};" +
                                            $"Password={ConfigurationManager.AppSettings["password"]}");
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public int GetPermitCount(QueryParams options)
        {
            var actionTypes = options.AuthActionTypes as string[] ?? options.AuthActionTypes.ToArray();
            var types = options.AuthTypes as string[] ?? options.AuthTypes.ToArray();

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;
            vars.end = _endDate;

            var query = "SELECT COUNT(DISTINCT(Permit_view.GUID)) " +
                        "FROM Well_view " +
                        "INNER JOIN Permit_view " +
                        "ON Permit_view.GUID = Well_view.Authorization_FK " +
                        "INNER JOIN Action_view " +
                        "ON Permit_view.GUID = Action_view.Authorization_FK " +
                        "WHERE Well_view.WellClass = @wellClass ";

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

            var variables = (object) vars;

            return _connection.QueryFirstOrDefault<int>(query, variables);
        }

        public int GetWellPermitCount(QueryParams options)
        {
            var actionTypes = options.AuthActionTypes as string[] ?? options.AuthActionTypes.ToArray();
            var types = options.AuthTypes as string[] ?? options.AuthTypes.ToArray();

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;
            vars.end = _endDate;

            var query = "SELECT COUNT(Well_view.WellClass) " +
                        "FROM Permit_view INNER JOIN " +
                        "Well_view ON Permit_view.GUID = Well_view.Authorization_FK INNER JOIN " +
                        "Action_view ON Permit_view.GUID = Action_view.Authorization_FK " +
                        "WHERE Well_view.WellClass = @wellClass ";

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

            var variables = (object) vars;

            return _connection.QueryFirstOrDefault<int>(query, variables);
        }

        public int GetWellViolationCount(QueryParams options)
        {
            const string query = "SELECT count(Well_view.OBJECTID) " +
                                 "FROM Violation_view " +
                                 "INNER JOIN Well_view " +
                                 "ON Violation_view.Well_FK = Well_view.GUID " +
                                 "WHERE Violation_view.ViolationDate >= @start " +
                                 "AND Well_view.WellClass = @wellClass";

            return _connection.QueryFirstOrDefault<int>(query, new
            {
                start = _startDate,
                wellClass = options.WellClass
            });
        }

        public int GetViolationCount(QueryParams options)
        {
            var types = options.ViolationTypes as string[] ?? options.ViolationTypes.ToArray();

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;

            var query = "SELECT COUNT(DISTINCT(Violation_view.GUID)) " +
                        "FROM Violation_view " +
                        "INNER JOIN Well_view " +
                        "ON Violation_view.Well_FK = Well_view.GUID " +
                        "WHERE Violation_view.ViolationDate >= @start " +
                        "AND Well_view.WellClass = @wellClass ";

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

            return _connection.QueryFirstOrDefault<int>(query, (object)vars);
        }

        public int GetWellsWithEnforcements(QueryParams options)
        {
            var types = options.EnforcementTypes as string[] ?? options.EnforcementTypes.ToArray();
            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = "SELECT COUNT(Well_view.OBJECTID) " +
                        "FROM UICViolationToEnforcement_evw " +
                        "INNER JOIN Enforcement_view " +
                        "ON UICViolationToEnforcement_evw.EnforcementGUID = Enforcement_view.GUID " +
                        "INNER JOIN Violation_view " +
                        "ON UICViolationToEnforcement_evw.ViolationGUID = Violation_view.GUID " +
                        "INNER JOIN Well_view " +
                        "ON Violation_view.Well_FK = Well_view.GUID " +
                        "WHERE Enforcement_view.EnforcementDate >= @start " +
                        "AND Well_view.WellClass = @wellClass ";

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

            return _connection.QueryFirstOrDefault<int>(query, (object) vars);
        }

        public int GetWellsReturnedToCompliance(QueryParams options)
        {
            var query = "SELECT COUNT(Well_view.OBJECTID) " +
                        "FROM Violation_view " +
                        "INNER JOIN Well_view " +
                        "ON Violation_view.Well_FK = Well_view.GUID " +
                        "WHERE(Well_view.WellClass = @wellClass) " +
                        "AND (Violation_view.ReturnToComplianceDate BETWEEN @start AND @end) ";

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = options.StartDate ?? _endDate - TimeSpan.FromDays(30);
            vars.end = _endDate;

            if (options.Snc)
            {
                query += "AND Violation_view.SignificantNonCompliance = @snc ";
                vars.snc = "Y";
            }

            return _connection.QueryFirstOrDefault<int>(query, (object) vars);
        }

        public int GetContaminationViolations(QueryParams options)
        {
            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;
            vars.contamination = "Y";

            var query = "SELECT COUNT(Violation_view.OBJECTID) " +
                        "FROM Well_view " +
                        "INNER JOIN Violation_view " +
                        "ON Violation_view.Well_FK = Well_view.GUID " +
                        "WHERE Well_view.WellClass = @wellClass " +
                        "AND Violation_view.USDWContamination = @contamination " +
                        "AND Violation_view.ViolationDate >= @start ";

            if (options.Snc)
            {
                query += "AND Violation_view.SignificantNonCompliance = @snc ";
                vars.snc = "Y";
            }

            return _connection.QueryFirstOrDefault<int>(query, (object) vars);
        }

        public int SncViolations(QueryParams options)
        {
            const string query = "SELECT COUNT(Well_view.OBJECTID) " +
                                 "FROM Well_view " +
                                 "INNER JOIN Violation_view " +
                                 "ON Violation_view.Well_FK = Well_view.GUID " +
                                 "WHERE Well_view.WellClass = @wellClass " +
                                 "AND Violation_view.SignificantNonCompliance = @compliance " +
                                 "AND Violation_view.ViolationDate >= @start";

            return _connection.QueryFirstOrDefault<int>(query, new
            {
                wellClass = options.WellClass,
                start = _startDate,
                compliance = "Y"
            });
        }

        public int GetWellsInspected(QueryParams options)
        {
            const string query = "SELECT COUNT(Well_view.OBJECTID) " +
                                 "FROM Well_view " +
                                 "INNER JOIN Inspection_view " +
                                 "ON Well_view.GUID = Inspection_view.Well_FK " +
                                 "WHERE Well_view.WellClass = @wellClass AND " +
                                 "Inspection_view.InspectionDate > @start AND " +
                                 "Well_view.Facility_FK is NULL";

            return _connection.QueryFirstOrDefault<int>(query, new
            {
                wellClass = options.WellClass,
                start = _startDate
            });
        }

        public int GetInspections(QueryParams options)
        {
            var types = options.InspectionType as string[] ?? options.InspectionType.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = "SELECT COUNT(DISTINCT(Inspection_view.OBJECTID)) " +
                        "FROM Inspection_view " +
                        "INNER JOIN Well_view " +
                        "ON Well_view.GUID = Inspection_view.Well_FK " +
                        "WHERE Well_view.WellClass = @wellClass AND " +
                        "Inspection_view.InspectionDate > @start AND " +
                        "Well_view.Facility_FK is NULL ";

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

            return _connection.QueryFirstOrDefault<int>(query, (object) vars);
        }

        public int GetMechIntegrityWells(QueryParams options)
        {
            var types = options.MitTypes as string[] ?? options.MitTypes.ToArray();
            var results = options.MitResult as string[] ?? options.MitResult.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = "SELECT COUNT(Well_view.OBJECTID) " +
                        "FROM Well_view " +
                        "INNER JOIN Mit_view " +
                        "ON Well_view.GUID = Mit_view.Well_FK " +
                        "WHERE Well_view.WellClass = @wellClass AND " +
                        "Mit_view.MITDate > @start ";

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
                vars.mitResult = types[0];
            }
            else if (results.Length > 1)
            {
                query += "AND Mit_view.MITResult in @mitResult ";
                vars.mitResult = types;
            }

            return _connection.QueryFirstOrDefault<int>(query, (object) vars);
        }

        public int GetRemedialWells(QueryParams options)
        {
            var types = options.RemedialAction as string[] ?? options.RemedialAction.ToArray();

            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = "SELECT COUNT(Well_view.OBJECTID) " +
                        "FROM Well_view " +
                        "INNER JOIN Mit_view " +
                        "ON Well_view.GUID = Mit_view.Well_FK " +
                        "WHERE Well_view.WellClass = @wellClass AND " +
                        "Mit_view.MITRemActDate > @start ";

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

            return _connection.QueryFirstOrDefault<int>(query, (object) vars);
        }

    }
}
