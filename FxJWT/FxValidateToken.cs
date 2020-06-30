using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using FxJWT.Class.JWT;
using System;
using System.Text;
using System.Collections.Generic;

namespace FxJWT
{
    public static class FxValidateToken
    {
        [FunctionName("ValidateToken")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validate")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Starting token validation request processing...");

            //Recibe el flujo de entrada
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            //Deserealiza el request y obtiene los parametros de entrada
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string token = data?.token;

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    //Se instancia la clase de servicio JWT
                    TokenService tokenService = new TokenService();

                    //Se valida el token y se obtienen los atributos
                    var claims = tokenService.ValidateToken(token);
                    
                    //Se extraen los atributos del token
                    Dictionary<string, object> tokenClaims = new Dictionary<string, object>();
                    foreach (var claim in claims)
                    {
                        tokenClaims.Add(claim.Type, claim.Value);
                    }

                    return GetResponse(HttpStatusCode.OK, tokenClaims);
                }
                catch (UnauthorizedAccessException uae)
                {
                    log.LogError(uae.ToString());

                    var message = new { message = $"Token is invalid, please try generating another or contact the administrator" };

                    return GetResponse(HttpStatusCode.Unauthorized, message);
                }
                catch (Exception e)
                {
                    log.LogError(e.ToString());

                    var message = new { message = $"Service unavailable, please try again later or contact the administrator" };

                    return GetResponse(HttpStatusCode.InternalServerError, message);
                }
            }
            else
            {
                log.LogError("Unable to get token object");

                var message = new { message = $"Please check the parameter 'token' in the request body" };

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
