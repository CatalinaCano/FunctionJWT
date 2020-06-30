using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using FxJWT.Class.Serialization;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System;
using FxJWT.Class;
using FxJWT.Class.JWT;

namespace FxJWT
{
    public static class FxGenerateToken
    {
        [FunctionName("GenerateToken")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "authenticate")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Starting authentication request processing...");

            //Recibe el flujo de entrada
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            //Deserealiza el request y obtiene los parametros de entrada
            Credentials credentials = JsonConvert.DeserializeObject<Credentials>(requestBody);

            if (credentials != null)
            {
                //Validacion del username
                if (string.IsNullOrWhiteSpace(credentials.Username))
                {
                    log.LogError("Username cannot be null or empty");

                    var message = new { message = $"Username is required, please check the parameter 'user' in the request body" };

                    return GetResponse(HttpStatusCode.BadRequest, message);
                }

                //Validacion del password
                if (string.IsNullOrWhiteSpace(credentials.Password))
                {
                    log.LogError("Password cannot be null or empty");

                    var message = new { message = $"Password is required, please check the parameter 'password' in the request body" };

                    return GetResponse(HttpStatusCode.BadRequest, message);
                } else
                {
                    try
                    {
                        //Se instancia clase para conexion y consulta en la BD
                        DatabaseConnector databaseConnector = new DatabaseConnector();

                        //Se consulta el usuario
                        User user = databaseConnector.GetUser(credentials.Username);

                        if (user != null)
                        {
                            //Se obtiene el SHA-256 del password
                            string encryptedPassword = Utilities.GetSha256Hash(credentials.Password);

                            if (user.Password.ToUpper().Equals(encryptedPassword.ToUpper()))
                            {
                                TokenService tokenService = new TokenService();
                                var token = new { token = tokenService.GenerateToken(user) };

                                return GetResponse(HttpStatusCode.OK, token);
                            } else
                            {
                                log.LogError("Wrong password");

                                var message = new { message = $"Username or password is invalid, please check the parameters in the request body or contact the administrator" };

                                return GetResponse(HttpStatusCode.Unauthorized, message);
                            }
                        } else
                        {
                            log.LogError("User not exist");

                            var message = new { message = $"Username or password is invalid, please check the parameters in the request body or contact the administrator" };

                            return GetResponse(HttpStatusCode.Unauthorized, message);
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError(e.ToString());

                        var message = new { message = $"Service unavailable, please try again later or contact the administrator" };

                        return GetResponse(HttpStatusCode.InternalServerError, message);
                    }
                }
            } else
            {
                log.LogError("Unable to get credentials object");

                var message = new { message = $"Please check the parameters in the body request" };

                return GetResponse(HttpStatusCode.BadRequest, message);
            }
        }

        private static HttpResponseMessage GetResponse(HttpStatusCode httpStatusCode, object response)
        {
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
            };
        }
    }
}
