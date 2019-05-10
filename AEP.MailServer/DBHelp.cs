using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.MailServer
{
    public class DBHelp
    {

        private static string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public DBHelp()
        {
        }


        public static DataTable ExecuteReader(string strSql)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    SqlDataReader dr = command.ExecuteReader();
                    dt.Load(dr);
                    dr.Close();
                    conn.Close();
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string strSql)
        {

            int retcount = -1;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    retcount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            return retcount;

        }
        public static int ExecuteNonQuery(string strSql, SqlParameter[] Parms)
        {
            int retcount = -1;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(strSql, conn))
                {
                    command.Parameters.AddRange(Parms);

                    conn.Open();
                    retcount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            return retcount;

        }

        public static object ExecuteScalar(string strSql)
        {


            object retobject = null;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    retobject = command.ExecuteScalar();
                    conn.Close();
                }
            }
            return retobject;

        }

        public static DataTable GetDataTable(string strSql)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(strSql, conn))
                {
                    conn.Open();
                    SqlDataReader dr = command.ExecuteReader();
                    dt.Load(dr);
                    dr.Close();
                    conn.Close();
                }
            }
            return dt;
        }
        public static DataTable GetDataTable(string strSql, SqlParameter[] Parms)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(strSql, conn))
                {
                    command.Parameters.AddRange(Parms);
                    conn.Open();
                    SqlDataReader dr = command.ExecuteReader();
                    dt.Load(dr);
                    dr.Close();
                    conn.Close();
                }
            }
            return dt;

        }



    }
}
