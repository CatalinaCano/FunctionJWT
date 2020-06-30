using FxJWT.Class.Serialization;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace FxJWT.Class
{
    class DatabaseConnector
    {

        private const string DatabaseName = "dbalmacenamiento";
        public string DatabaseStringConnection { get; set; }

        public DatabaseConnector()
        {
            try
            {
                DatabaseStringConnection = Settings.GetVariable(DatabaseName);
            }
            catch (NullReferenceException nre)
            {
                throw nre;
            }
        }

        public User GetUser (string username)
        {
            User user = null;
            
            //Crea la conexion
            using (var connection = new SqlConnection(DatabaseStringConnection))
            {
                try
                {
                    //Abre la conexion
                    connection.Open();

                    //Crea la consulta
                    StringBuilder query = new StringBuilder("SELECT * ")
                                                    .Append("FROM dbo.TBL_USERS_API_JWT ")
                                                    .Append("WHERE USERNAME = @username ");

                    //Realiza la consulta en la BD 
                    using (SqlCommand cmd = new SqlCommand(query.ToString(), connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = query.ToString();

                        cmd.Parameters.Add("@username", SqlDbType.VarChar).Value = username;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                user = new User
                                {
                                    IdClient = int.Parse(reader["CLIENT_ID"].ToString()),
                                    MinutesAlive = short.Parse(reader["TOKEN_ALIVE_MINUTES"].ToString()),
                                    Password = reader["PASSWORD"].ToString(),
                                    Username = username
                                };
                                return user;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Captura la excepción
                    throw ex;
                }
                finally
                {
                    //Cierra la conexion
                    connection.Close();
                }
            }

            return user;
        }

    }

}
