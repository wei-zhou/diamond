namespace Home.Services.SsisService
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using Home.Services.SecurityService;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.Management.IntegrationServices;

    [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
    [ServiceHandleError(ServiceName = ServiceHandleErrorHelper.SsisServiceName)]
    public class SsisController : BaseODataController
    {
        // GET odata/GetStatus(name='name')
        [HttpGet]
        [ODataRoute("GetStatus(name={name})")]
        public IHttpActionResult GetStatus([FromODataUri] string name)
        {
            var descriptor = SsisServiceHelper.SsisJobDescriptors[name];
            var application = new Application();
            var runningPackages = application.GetRunningPackages(null);
            if (runningPackages != null && runningPackages.Count > 0)
            {
                foreach (var packageName in descriptor.PackageNames)
                {
                    if (runningPackages.Cast<RunningPackage>().Any(p => StringComparer.OrdinalIgnoreCase.Compare(p.PackageName, packageName) == 0))
                    {
                        return Ok(new JobStatusResult() { Status = JobStatus.Running });
                    }
                }
            }

            return Ok(new JobStatusResult() { Status = JobStatus.Completed });
        }

        // POST odata/Run()
        [HttpPost]
        [ODataRoute("Run()")]
        public async Task<IHttpActionResult> RunAsync(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var name = (string)parameters["name"];
            var variables = (IEnumerable<JobParameter>)parameters["parameters"];
            var descriptor = SsisServiceHelper.SsisJobDescriptors[name];

            {
                var connection = default(SqlConnection);
                try
                {
                    connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["MasterConnection"].ConnectionString);
                    var services = new IntegrationServices(connection);
                    var environment = services.Catalogs["SSISDB"].Folders["Diamond"].Environments[descriptor.EnvironmentName];
                    foreach (var variable in variables)
                    {
                        environment.Variables[variable.Name].Value = SsisServiceHelper.Convert(variable.Value, variable.Type);
                    }

                    environment.Alter();
                }
                finally
                {
                    if (connection != null) connection.Dispose();
                }
            }

            {
                var connection = default(SqlConnection);
                var command = default(SqlCommand);
                try
                {
                    connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["MSDBConnection"].ConnectionString);
                    command = connection.CreateCommand();
                    command.CommandText = "sp_start_job";
                    command.CommandType = CommandType.StoredProcedure;

                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@return_value";
                        parameter.SqlDbType = SqlDbType.Int;
                        parameter.Direction = ParameterDirection.ReturnValue;
                        command.Parameters.Add(parameter);
                    }

                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@job_name";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.SqlDbType = SqlDbType.VarChar;
                        parameter.Value = descriptor.JobName;
                        command.Parameters.Add(parameter);
                    }

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    var result = (int)command.Parameters["@return_value"].Value;
                    if (result != 0)
                    {
                        throw new InvalidOperationException(string.Format("SQL Server Agent job, {0}, failed to start.", descriptor.JobName));
                    }

                    return Ok();
                }
                finally
                {
                    if (command != null) command.Dispose();
                    if (connection != null) connection.Dispose();
                }
            }
        }

        // POST odata/Stop()
        [HttpPost]
        [ODataRoute("Stop()")]
        public IHttpActionResult Stop(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var name = (string)parameters["name"];
            var descriptor = SsisServiceHelper.SsisJobDescriptors[name];
            var application = new Application();
            var runningPackages = application.GetRunningPackages(null);
            if (runningPackages != null)
            {
                foreach (var packageName in descriptor.PackageNames)
                {
                    var packages = runningPackages.Cast<RunningPackage>().Where(p => StringComparer.OrdinalIgnoreCase.Compare(p.PackageName, packageName) == 0);
                    foreach (var package in packages)
                    {
                        package.Stop();
                    }
                }
            }

            return Ok();
        }
    }
}