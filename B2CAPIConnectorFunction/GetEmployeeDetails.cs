//===============================================================================
// Microsoft FastTrack for Azure
// Azure AD B2C API Connector Sample
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace B2CAPIConnectorFunction
{
    public static class GetEmployeeDetails
    {
        [FunctionName("GetEmployeeDetails")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            ResponseContent responseContent = new ResponseContent();

            log.LogInformation("GetEmployeeDetails function processed a request.");

            // Validate the request credentials
            if (!Authorize(req, log))
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation("Request body: " + requestBody);

            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
            {
                responseContent.status = "400";
                responseContent.action = "ValidationError";
                responseContent.userMessage = "Invalid request";
            }
            else
            {
                string firstName = data.givenName;
                string lastName = data.surname;

                if (string.IsNullOrEmpty(firstName)
                    || string.IsNullOrEmpty(lastName))
                {
                    responseContent.status = "400";
                    responseContent.action = "ValidationError";
                    responseContent.userMessage = "Invalid request";
                }
                else
                {
                    Employee employee = GetEmployee(firstName, lastName, log);
                    if (employee != null)
                    {
                        responseContent.status = "200";
                        responseContent.jobTitle = employee.JobTitle;
                        responseContent.extension_PublisherID = employee.PublisherID;
                        responseContent.extension_UserRole = employee.UserRole;
                    }
                    else
                    {
                        responseContent.status = "400";
                        responseContent.action = "ValidationError";
                        responseContent.userMessage = "Employee not found";
                    }
                }
            }

            return (ActionResult)new OkObjectResult(responseContent);
        }

        private static Employee GetEmployee(string firstName, string lastName, ILogger log)
        {
            Employee employee = null;
            const string sql = "SELECT e.[emp_id], e.[fname], e.[minit], e.[lname], e.[job_id], e.[job_lvl], e.[pub_id], e.[hire_date], j.[job_desc], p.[pub_name] FROM [dbo].[employee] e inner join [dbo].[jobs] j on j.[job_id] = e.[job_id] inner join [dbo].[publishers] p on p.[pub_id] = e.[pub_id] WHERE e.[fname] = @firstName AND e.[lname] = @lastName";
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlHelper sqlHelper = null;

            parameters.Add(new SqlParameter("@firstName", firstName));
            parameters.Add(new SqlParameter("@lastName", lastName));
            try
            {
                sqlHelper = new SqlHelper();
                SqlDataReader reader = sqlHelper.ExecuteDataReader(sql, CommandType.Text, ref parameters);
                while (reader.Read())
                {
                    employee = new Employee()
                    {
                        EmployeeID = reader["emp_id"].ToString(),
                        FirstName = reader["fname"].ToString(),
                        MiddleInitial = reader["minit"].ToString(),
                        LastName = reader["lname"].ToString(),
                        JobID = Convert.ToInt32(reader["job_id"]),
                        JobTitle = reader["job_desc"].ToString(),
                        JobLevel = Convert.ToInt32(reader["job_lvl"]),
                        PublisherID = reader["pub_id"].ToString(),
                        PublisherName = reader["pub_name"].ToString(),
                        HireDate = Convert.ToDateTime(reader["hire_date"])
                    };
                    Random random = new Random();
                    int roleNumber = random.Next(1, 4);
                    string userRole = string.Empty;
                    switch (roleNumber)
                    {
                        case 1:
                            userRole = "Global Administrator";
                            break;
                        case 2:
                            userRole = "User Administrator";
                            break;
                        case 3:
                            userRole = "Group Owner";
                            break;
                        default:
                            userRole = "User";
                            break;
                    }
                    employee.UserRole = userRole;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Query of employee table failed");
            }
            finally
            {
                if (sqlHelper != null) sqlHelper.Close();
            }

            return employee;
        }

        private static bool Authorize(HttpRequest req, ILogger log)
        {
            // Retrieve credentials from environment variables 
            string userName = Environment.GetEnvironmentVariable("AUTH_USER", EnvironmentVariableTarget.Process);
            string password = Environment.GetEnvironmentVariable("AUTH_PWD", EnvironmentVariableTarget.Process);

            // Verify that an HTTP Authorization header exist
            if (!req.Headers.ContainsKey("Authorization"))
            {
                log.LogWarning("Missing HTTP authorization header.");
                return false;
            }

            // Read the authorization header
            var authorizationHeader = req.Headers["Authorization"].ToString();

            // Ensure the type of the authorization header is 'Basic'
            if (!authorizationHeader.StartsWith("Basic "))
            {
                log.LogWarning("This function requires Basic authentication.");
                return false;
            }

            // Get the request credentials
            string[] credentials = System.Text.UTF8Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Substring(6))).Split(':');

            // Evaluate the credentials and return the result
            return (credentials[0] == userName && credentials[1] == password);
        }
    }
}
