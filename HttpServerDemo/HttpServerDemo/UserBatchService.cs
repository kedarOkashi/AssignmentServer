using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerDemo
{
    class UserBatchService : BackgroundService
    {
       protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Schedule schedule = GenerateSchedule();

                    // Wait for the next interval
                    if (schedule.Day == DayOfWeek.Sunday && schedule.Hour == "12" && schedule.Minute == "12")
                    {
                      
                        // Run your batch service logic here
                        Console.WriteLine($"Batch service executed at: {DateTime.Now}");

                        //Send Emails
                        Console.WriteLine("Starting to send emails.");

                        // db object
                        DbConnection db = new DbConnection();

                        //create list of UserDTO
                        List<UserDTO> users = GetUsers(db.getUsers());

                        foreach (UserDTO user in users)
                        {
                            EmailSender.SendEmail(user.Email);
                        }

                        // Check if the scheduled end time has passed
                        if (DateTime.Now >= schedule.EndTime)
                        {
                            Console.WriteLine("Batch service has completed its scheduled run.");
                        }
                        await Task.Delay(60000, stoppingToken);
                    }
                    Console.WriteLine("this is no time to send emails!");
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(1000, stoppingToken);
                }
 
            }
        }

        private Schedule GenerateSchedule()
        {
            var schedule = new Schedule
            {
                // Set the interval at which the batch service should run
                Interval = TimeSpan.FromMinutes(0.2),

                Day = DateTime.Now.DayOfWeek,

                Hour = DateTime.Now.Hour.ToString(),

                Minute = DateTime.Now.Minute.ToString()
            };

            return schedule;
        }

        private List<UserDTO> GetUsers(DataTable dt)
        {
            //Create list of UsersDTO
            List<UserDTO> users = new List<UserDTO>();

            foreach (DataRow row in dt.Rows)
            {
                UserDTO user = new UserDTO();
                user.Email = row.ItemArray[2].ToString();
                users.Add(user);
            }
            return users;
        }
    }
}
