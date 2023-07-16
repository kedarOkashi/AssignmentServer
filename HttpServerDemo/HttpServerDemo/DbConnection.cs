using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerDemo
{
    class DbConnection
    {
        public MySqlConnection conn = new MySqlConnection();

        MySqlDataAdapter adapter = new MySqlDataAdapter();
        DataSet ds;
        DataTable dt;

        static string server = "localhost;";
        static string database = "mydatabase;";
        static string Uid = "ServerApp;";
        static string password = "a123456!;";


        private MySqlConnection dataSource()
        {
            conn = new MySqlConnection($"server={server} database={database} Uid={Uid} password={password}");
            return conn;
        }

        public DataTable getUsers()
        {
            try
            {
                conn = dataSource();
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("GetUsers", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                //Dataset + DataTable
                dt = new DataTable();
                ds = new DataSet();

                adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(ds, "users");
                dt = ds.Tables["users"];

                if (dt.Rows.Count == 0)
                {
                    return null;
                }
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetUserById(string id)
        {
            try
            {
                conn = dataSource();
                string cmdText = $"SELECT * FROM mydatabase.users where ID={id} ;";

                //Dataset + DataTable
                dt = new DataTable();
                ds = new DataSet();

                conn.Open();
                MySqlCommand cmd = new MySqlCommand(cmdText, conn);

                //adapter
                adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(ds, "users");
                dt = ds.Tables["users"];

                if (dt.Rows.Count > 0)
                {
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                // close connection.
                conn.Close();
            }
        }

        public int CreateUser(UserDTO userToAdd)
        {
            try
            {
                int result;
                conn = dataSource();
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("InsertUser", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", userToAdd.Name);
                cmd.Parameters.AddWithValue("@Email", userToAdd.Email);
                cmd.Parameters.AddWithValue("@Password", userToAdd.Password);
                return result = cmd.ExecuteNonQuery();
                //result explain the count of rows effected.
            }
            catch (Exception ex)
            {
                //write log.
                return -1;
            }
            finally
            {
                //finally close connection.
                conn.Close();
            }
        }

        public int DeleteUser(string id)
        {
            try
            {
                int result;
                conn = dataSource();
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("DeleteUserById", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", id);
                return result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return -1;
            }
            finally
            {
                //finally close connection.
                conn.Close();
            }
        }
        public int UpdateUser(UserDTO userToUpdate)
        {
            try
            {
                int result;
                conn = dataSource();
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UpdateUserInformation", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", userToUpdate.Id);
                cmd.Parameters.AddWithValue("@userName", userToUpdate.Name);
                cmd.Parameters.AddWithValue("@userEmail", userToUpdate.Email);
                cmd.Parameters.AddWithValue("@userPassword", userToUpdate.Password);
                return result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return -1;
            }
            finally
            {
                conn.Close();
            }
        }
  
    }
}
