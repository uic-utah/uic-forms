using System;
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

            var query = "SELECT COUNT(UICAuthorization_evw.OBJECTID) " +
                        "FROM UICWell_evw INNER JOIN " +
                        "UICAuthorization_evw ON UICAuthorization_evw.GUID = UICWell_evw.Authorization_FK INNER JOIN " +
                        "UICAuthorizationAction_evw ON UICAuthorization_evw.GUID = UICAuthorizationAction_evw.Authorization_FK " +
                        "WHERE UICWell_evw.WellClass = @wellClass ";

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

            var query = "SELECT COUNT(UICWell_evw.WellClass) " +
                        "FROM UICAuthorization_evw INNER JOIN " +
                        "UICWell_evw ON UICAuthorization_evw.GUID = UICWell_evw.Authorization_FK INNER JOIN " +
                        "UICAuthorizationAction_evw ON UICAuthorization_evw.GUID = UICAuthorizationAction_evw.Authorization_FK " +
                        "WHERE UICWell_evw.WellClass = @wellClass ";

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

            var variables = (object) vars;

            return _connection.QueryFirstOrDefault<int>(query, variables);
        }

        public int GetWellViolationCount(QueryParams options)
        {
            const string query = "SELECT count(UICWell_evw.OBJECTID) " +
                                 "FROM UICViolation_evw " +
                                 "INNER JOIN UICWell_evw " +
                                 "ON UICViolation_evw.Well_FK = UICWell_evw.GUID " +
                                 "WHERE UICViolation_evw.ViolationDate >= @start " +
                                 "AND UICWell_evw.WellClass = @wellClass";

            return _connection.QueryFirstOrDefault<int>(query, new
            {
                start = _startDate,
                wellClass = options.WellClass
            });
        }

        public int GetViolationCount(QueryParams options)
        {
            if (!options.ViolationTypes.Any())
            {
                throw new ArgumentException("options.ViolationTypes cannot be epty");
            }
            var types = options.ViolationTypes as string[] ?? options.ViolationTypes.ToArray();

            dynamic vars = new ExpandoObject();
            vars.wellClass = options.WellClass;
            vars.start = _startDate;

            var query = "SELECT count(UICViolation_evw.ViolationType) " +
                        "FROM UICViolation_evw " +
                        "INNER JOIN UICWell_evw " +
                        "ON UICViolation_evw.Well_FK = UICWell_evw.GUID " +
                        "WHERE UICViolation_evw.ViolationDate >= @start " +
                        "AND UICWell_evw.WellClass = @wellClass ";

            if (types.Length == 1)
            {
                query += "AND UICViolation_evw.ViolationType = @violationTypes";
                vars.violationTypes = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICViolation_evw.ViolationType in @violationTypes";
                vars.violationTypes = types;
            }

            return _connection.QueryFirstOrDefault<int>(query, (object)vars);
        }

        public int GetWellsWithEnforcements(QueryParams options)
        {
            var types = options.EnforcementTypes as string[] ?? options.EnforcementTypes.ToArray();
            dynamic vars = new ExpandoObject();
            vars.start = _startDate;
            vars.wellClass = options.WellClass;

            var query = "SELECT COUNT(UICWell_evw.OBJECTID) " +
                        "FROM UICViolationToEnforcement_evw " +
                        "INNER JOIN UICEnforcement_evw " +
                        "ON UICViolationToEnforcement_evw.EnforcementGUID = UICEnforcement_evw.GUID " +
                        "INNER JOIN UICViolation_evw " +
                        "ON UICViolationToEnforcement_evw.ViolationGUID = UICViolation_evw.GUID " +
                        "INNER JOIN UICWell_evw " +
                        "ON UICViolation_evw.Well_FK = UICWell_evw.GUID " +
                        "WHERE UICEnforcement_evw.EnforcementDate >= @start " +
                        "AND UICWell_evw.WellClass = @wellClass ";

            if (types.Length == 1)
            {
                query += "AND UICEnforcement_evw.EnforcementType = @enforcementType";
                vars.enforcementType = types[0];
            }
            else if (types.Length > 1)
            {
                query += "AND UICEnforcement_evw.EnforcementType in @enforcementType";
                vars.enforcementType = types;
            }

            return _connection.QueryFirstOrDefault<int>(query, (object)vars);
        }

        }
    }
}
