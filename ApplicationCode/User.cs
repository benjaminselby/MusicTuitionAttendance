using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;

namespace ApplicationCode
{
    public class User
    {
        public string id;
        public string networkLogin;
        public string fullName;
        public string emailAddress;

        public User(string networkLogin)
        {
            using (SqlConnection synergyOneConnection = new SqlConnection())
            {
                try
                {
                    synergyOneConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["SynergyOneConnectionString"].ConnectionString;

                    string userInfoQuery = ConfigurationManager.AppSettings["getUserInfoFromNetworkLoginSql"];

                    using (SqlCommand userInfoCommand = new SqlCommand(userInfoQuery, synergyOneConnection))
                    {
                        userInfoCommand.Parameters.AddWithValue("networkLogin", networkLogin);

                        synergyOneConnection.Open();

                        SqlDataReader userInfoReader = userInfoCommand.ExecuteReader();
                        if (userInfoReader.HasRows)
                        {
                            userInfoReader.Read();

                            this.id = userInfoReader["ID"].ToString();
                            this.networkLogin = networkLogin;
                            this.fullName = userInfoReader["FullName"].ToString();
                            this.emailAddress = userInfoReader["OccupEmail"].ToString();
                        }
                        else
                        {
                            throw new Exception(String.Format(
                                "Could not retrieve user information from Synergy for user with network login {0}.",
                                networkLogin.ToUpper()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);                    
                    throw (ex);
                }
                finally
                {
                    synergyOneConnection.Close();
                }
            }
        }
    }
}