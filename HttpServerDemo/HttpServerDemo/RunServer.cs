using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerDemo
{
    internal class RunServer : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                HttpListener listener = null;
                listener = InitialListener();
                listener.Start();
                while (true)
                {
                    Console.WriteLine("waiting...");
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest req = context.Request;
                    HttpListenerResponse resp = context.Response;
                    Console.WriteLine(req.Url.ToString());
                    Console.WriteLine(req.HttpMethod);
                    Console.WriteLine(req.UserHostName);
                    Console.WriteLine(req.UserAgent);
                    Console.WriteLine(req.Url.AbsolutePath);
                    switch (req.HttpMethod)
                    {
                        case "GET":
                            switch (req.Url.AbsolutePath)
                            {
                                case "/GetUser":
                                    GetUserByIdGET(listener, context, req);
                                    break;
                                case "/GetUsers":
                                    GetUsersGET(listener, context);
                                    break;
                                default:
                                    BadRequest(listener, context, "BadRequest");
                                    break;
                            }
                            break;

                        case "POST":
                            if (req.HasEntityBody && req.Url.AbsolutePath == "/CreateUser")
                            {
                                CreateUser(listener, context);
                            }
                            else
                            {
                                if (req.Url.AbsolutePath == "/CreateUser")
                                {
                                    BadRequest(listener, context, "you missing passing Body.");
                                }
                                else
                                {
                                    // request missing body.
                                    BadRequest(listener, context, "BadReques");
                                }
                            }
                            break;

                        case "DELETE":
                            if (req.QueryString.Count == 1 && (req.QueryString.Keys[0].ToString() == "id") && req.Url.AbsolutePath == "/DeleteUser")
                            {
                                //id from query string
                                string id = req.QueryString["id"].ToString();
                                DeleteUser(listener, context, id);
                            }
                            else
                            {
                                if (req.Url.AbsolutePath == "/DeleteUser")
                                {
                                    BadRequest(listener, context, "id is mandatory parameter");
                                }
                                else
                                {
                                    BadRequest(listener, context, "BadRequest");
                                }
                            }
                            break;
                        case "PUT":
                            if (req.HasEntityBody && req.Url.AbsolutePath == "/UpdateUser")
                            {
                                UpdateUser(listener, context);
                            }
                            else
                            {
                                if (req.Url.AbsolutePath == "/UpdateUSer")
                                {
                                    BadRequest(listener, context, "you missing passing Body.");
                                }
                                else
                                {
                                    // request missing body.
                                    BadRequest(listener, context, "BadReques");
                                }
                            }
                            break;
                        default:
                            BadRequest(listener, context, "BadRequest");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static private HttpListener InitialListener()
        {
            HttpListener listener = new HttpListener();
            listener = new HttpListener();

            listener.Prefixes.Add(@"http://localhost:8080/");
            return listener;
        }
        static void GetUsersGET(HttpListener listener, HttpListenerContext context)
        {
            DbConnection db = new DbConnection();
            string dataTableToJson = FromDataTableToString(db.getUsers());
            if (dataTableToJson =="null")
            {
                dataTableToJson = $"no users in DB";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(dataTableToJson);
            }
            else
            {
                context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(dataTableToJson);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            using (Stream stream = context.Response.OutputStream)
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataTableToJson);
                }
            }
        }
        static void GetUserByIdGET(HttpListener listener, HttpListenerContext context, HttpListenerRequest req)
        {

            if (req.QueryString.Count == 1 && (req.QueryString.Keys[0].ToString() == "id") && req.Url.AbsolutePath == "/GetUser")
            {
                //id from query string
                string id = req.QueryString["id"].ToString();
                GetUserById(listener, context, id);
            }
            else
            {
                if (req.Url.AbsolutePath == "/GetUser")
                {
                    BadRequest(listener, context, "id is mandatory parameter");
                }
                else
                {
                    BadRequest(listener, context, "BadRequest");
                }
            }
        }
        static private void GetUserById(HttpListener listener, HttpListenerContext context, string id)
        {
            try
            {
                DbConnection db = new DbConnection();
                string dataTableToJson = DataTableToJSONWithJSONNet(db.GetUserById(id));

                if (dataTableToJson == "null")
                {
                    dataTableToJson = $"no user with id {id}";
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(dataTableToJson);
                }
                else
                {
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(dataTableToJson);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                using (Stream stream = context.Response.OutputStream)
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataTableToJson);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static private void UpdateUser(HttpListener listener, HttpListenerContext context)
        {
            try
            {
                string msg = "";
                Stream body = context.Request.InputStream;
                StreamReader reader = new StreamReader(body, context.Request.ContentEncoding);
                string tempString = reader.ReadToEnd();
                UserDTO userFromBody = JsonConvert.DeserializeObject<UserDTO>(tempString);
                DbConnection db = new DbConnection();
                int result = db.UpdateUser(userFromBody);

                if (result > 0)
                {
                    msg = $"user with id:{userFromBody.Id} update successfuly.\r\nEmail:{userFromBody.Email}\r\nPassword:{userFromBody.Password}";
                    Console.WriteLine(msg);
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (Stream stream = context.Response.OutputStream)
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(msg);
                        }
                    }
                }
                else
                {
                    msg = $"Failure to update user, user with id:{userFromBody.Id} not exist.";
                    Console.WriteLine(msg);
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                    using (Stream stream = context.Response.OutputStream)
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BadRequest(listener, context, ex.Message);
            }
        }
        static private void CreateUser(HttpListener listener, HttpListenerContext context)
        {
            try
            {
                string msg = "";
                Stream body = context.Request.InputStream;
                StreamReader reader = new StreamReader(body, context.Request.ContentEncoding);
                string tempString = reader.ReadToEnd();
                UserDTO userFromBody = JsonConvert.DeserializeObject<UserDTO>(tempString);
                DbConnection db = new DbConnection();
                int result = db.CreateUser(userFromBody);
                if (result > 0)
                {
                    msg = $"user with details:\r\nName:{userFromBody.Name}\r\nEmail:{userFromBody.Email}\r\nPassword:{userFromBody.Password}\r\ncreated successfuly.";
                    Console.WriteLine(msg);
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (Stream stream = context.Response.OutputStream)
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(msg);
                        }
                    }
                }
                else
                {
                    msg = "Failure to create user.";
                    Console.WriteLine(msg);
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                    using (Stream stream = context.Response.OutputStream)
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BadRequest(listener, context, ex.Message);
            }


        }
        static private void DeleteUser(HttpListener listener, HttpListenerContext context, string id)
        {
            try
            {
                string msg = "";
                DbConnection db = new DbConnection();
                int result = db.DeleteUser(id);

                if (result > 0)
                {
                    msg = $"user with id:{id} deleted successfuly.";
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    msg = $"user with id:{id} not exists";
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;

                }
                using (Stream stream = context.Response.OutputStream)
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                BadRequest(listener, context, ex.Message);
            }
        }
        static private void BadRequest(HttpListener listener, HttpListenerContext context, string text)
        {
            try
            {
                bool finished = false;
                while (!finished)
                {
                    string msg = text;
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    using (Stream stream = context.Response.OutputStream)
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(msg);
                        }
                    }
                    Console.WriteLine("Message send!");
                    finished = true;
                }

            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message + " " + ex.Status);
            }

        }
        static private string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }
        static private string FromDataTableToString(DataTable dt)
        {
            if (dt.Columns.Count == 0)
            {
                return null;
            }
            string usersToPrint = "";

            foreach (DataRow row in dt.Rows)
            {
                usersToPrint += $"Id:{row.ItemArray[0].ToString()}, Name:{row.ItemArray[1].ToString()}," +
                    $" Email:{row.ItemArray[2].ToString()}, Password:{row.ItemArray[3].ToString()}\r\n";
            }

            return usersToPrint;
        } 
        
    }
}