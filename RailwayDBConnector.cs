using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Mannote
{
    public class RailwayDBConnector
    {
        SqlConnection connect = null;
        string connectionString;

        public RailwayDBConnector()
        {
            connectionString = ConfigurationManager.AppSettings["conStr"];
        }

        public DataTable GetViewTable(string viewTableName)
        {
            try
            {
                OpenConnection(connectionString);
                return GetAllObjectsAsDataTable(viewTableName);
            }
            catch(Exception)
            {
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        void OpenConnection(string connectionString)
        {
            try
            {
                connect = new SqlConnection(connectionString);
                connect.Open();
            }
            catch (SqlException ex)
            {
                // Протоколировать исключение
                Console.WriteLine(ex.Message);
                connect.Close();
            }
        }

        void CloseConnection()
        {
            connect.Close();
        }

        DataTable GetAllObjectsAsDataTable(string viewTableName)
        {
            DataTable loks = new DataTable();
            string sql = "Select * From " + viewTableName;
            using (SqlCommand cmd = new SqlCommand(sql, this.connect))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                loks.Load(dr);
                dr.Close();
            }
            return loks;
        }
    }
}
