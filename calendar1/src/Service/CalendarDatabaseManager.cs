using System;
using System.Data.SQLite;
using System.Collections.Generic;

namespace BitCalendarService
{
    class CalendarDatabaseManager
    {
        private SQLiteConnection _connection;

        public void PreAction()
        {
            _connection = new SQLiteConnection("Data Source=BitCalendar.db;Version=3;New=True;Compress=True;");

            try
            {
                _connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var command = _connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS CalendarEvents(" +
                                  "UniqueID     INTEGER     PRIMARY KEY     AUTOINCREMENT, " +
                                  "CreatorID    TEXT," +
                                  "StartDate    TEXT," +
                                  "EndDate      TEXT," +
                                  "Header       TEXT," +
                                  "Description  TEXT)";
            command.ExecuteNonQuery();
          
            command = _connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS CalendarUsers " +
                                  "(IPAddress    TEXT   PRIMARY KEY     NOT NULL);";
            command.ExecuteNonQuery();
        }
        public void PostAction()
        {
            _connection.Close();
        }

        public bool CreateCalendarEvent(CalendarEvent calendarEvent)
        {
            Console.WriteLine("Creating Calendar Event: " + calendarEvent.Header);

            var result = false;

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO CalendarEvents" +
                                      "(CreatorID,StartDate,EndDate,Header,Description) " +
                                      "VALUES (";
                if (calendarEvent.CreatorID != null)
                    command.CommandText += "\"" + calendarEvent.CreatorID + "\"";
                else
                    command.CommandText += "\"\"";

                if (calendarEvent.StartDate != null)
                    command.CommandText += ",\"" + calendarEvent.StartDate + "\"" ;
                else
                    command.CommandText += ",\"\"";

                if (calendarEvent.EndDate != null)
                    command.CommandText += ",\"" + calendarEvent.EndDate + "\"";
                else
                    command.CommandText += ",\"\"";

                if (calendarEvent.Header != null)
                    command.CommandText += ",\"" + calendarEvent.Header + "\"";
                else
                    command.CommandText += ",\"\"";

                if (calendarEvent.Description != null)
                    command.CommandText += ",\"" + calendarEvent.Description + "\"";
                else
                    command.CommandText += ",\"\"";

                command.CommandText += ");";

                command.ExecuteNonQuery();
                result = true;
                Console.WriteLine("Created Calendar Event: " + calendarEvent.Header);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Create Calendar Event): " + e.Message);
            }

            PostAction();

            return result;
        }
        public bool EditCalendarEvent(CalendarEvent calendarEvent)
        {
            Console.WriteLine("Editing Calendar Event: " + calendarEvent.Header);

            var result = false;

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "UPDATE CalendarEvents ";

                if (calendarEvent.CreatorID != null)
                    command.CommandText += "SET CreatorID = \"" + calendarEvent.CreatorID + "\" , ";
                else
                    command.CommandText += "SET CreatorID = \"\" , ";

                if (calendarEvent.StartDate != null)
                    command.CommandText += " StartDate = \"" + calendarEvent.StartDate + "\" , ";
                else
                    command.CommandText += " StartDate = \"\" , ";

                if (calendarEvent.EndDate != null)
                    command.CommandText += " EndDate = \"" + calendarEvent.EndDate + "\" , ";
                else
                    command.CommandText += " EndDate = \"\" , ";

                if (calendarEvent.Header != null)
                    command.CommandText += " Header = \"" + calendarEvent.Header + "\" , ";
                else
                    command.CommandText += " Header = \"\" , ";

                if (calendarEvent.Description != null)
                    command.CommandText += " Description = \"" + calendarEvent.Description + "\" ";
                else
                    command.CommandText += " Description = \"\" ";

                command.CommandText += " WHERE UniqueID=" + calendarEvent.UniqueID + ";";

                command.ExecuteNonQuery();
                result = true;
                Console.WriteLine("Edited Calendar Event: " + calendarEvent.Header);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Edit Calendar Event): " + e.Message);
            }

            PostAction();

            return result;
        }
        public bool DeleteCalendarEvent(CalendarEvent calendarEvent)
        {
            Console.WriteLine("Deleting Calendar Event with ID: " + calendarEvent.UniqueID);
            var result = false;

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "DELETE from CalendarEvents where UniqueID="
                                    + calendarEvent.UniqueID + ";";
                result = command.ExecuteNonQuery() >= 0;
                Console.WriteLine("Deleted Calendar Event with ID: " + calendarEvent.UniqueID);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Delete Calendar Event): " + e.Message);
            }

            PostAction();

            return result;
        }
        public CalendarEvent[] GetCalendarEvents()
        {
            Console.WriteLine("Getting Calendar Events... ");

            var calendarEventsList = new List<CalendarEvent>();
                
            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM CalendarEvents;";

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var uniqueID    = reader.GetInt32(0);
                    var creatorID   = (string)reader["CreatorID"];
                    var startDate   = (string)reader["StartDate"];
                    var endDate     = (string)reader["EndDate"];
                    var header      = (string)reader["Header"];
                    var description = (string)reader["Description"];

                    calendarEventsList.Add(new CalendarEvent() {
                            UniqueID    = uniqueID,
                            CreatorID   = creatorID,
                            StartDate   = startDate,
                            EndDate     = endDate,
                            Header      = header,
                            Description = description });
                }
                Console.WriteLine("Got Calendar Events. ");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Get Calendar Events): " + e.Message);
            }
            
            PostAction();

            return calendarEventsList.ToArray();
        }

        public bool CreateUser(CalendarUser calendarUser)
        {
            Console.WriteLine("Creating Calendar User: " + calendarUser.IPAddress);

            var result = false;

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO CalendarUsers" +
                    "(IPAddress) VALUES (\"" + calendarUser.IPAddress + "\");";
                command.ExecuteNonQuery();
                result = true;
                Console.WriteLine("Created Calendar User: " + calendarUser.IPAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Create User): " + e.Message);
            }

            PostAction();

            return result;
        }
        public bool DeleteUser(CalendarUser calendarUser)
        {
            Console.WriteLine("Deleting Calendar User: " + calendarUser.IPAddress);

            var result = false;

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "DELETE from CalendarUsers where IPAddress=\""
                                + calendarUser.IPAddress + "\";";
                command.ExecuteNonQuery();
                result = true;
                Console.WriteLine("Deleted Calendar User: " + calendarUser.IPAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Delete User): " + e.Message);
            }

            PostAction();

            return result;
        }
        public CalendarUser[] GetUsers()
        {
            Console.WriteLine("Getting users...");

            var calendarUsersList = new List<CalendarUser>();

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM CalendarUsers;";

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var ipAddress = (string)reader["IPAddress"];

                    calendarUsersList.Add(new CalendarUser() { IPAddress = ipAddress });
                }
                Console.WriteLine("Got users.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Get Users): " + e.Message);
            }

            PostAction();

            return calendarUsersList.ToArray();
        }

        public void ClearCalendarEvents()
        {
            Console.WriteLine("Clearing calendar events table.");

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "DROP TABLE CalendarEvents";
                command.ExecuteNonQuery();
                Console.WriteLine("Cleared calendar events table.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Clear Calendar Events): " + e.Message);
            }

            PostAction();
        }
        public void ClearUsers()
        {
            Console.WriteLine("Clearing calendar users table.");

            PreAction();

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "DROP TABLE CalendarUsers";
                command.ExecuteNonQuery();
                Console.WriteLine("Cleared calendar users table.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception (Clear Calendar Users): " + e.Message);
            }

            PostAction();
        }
        public void CreateUsers(CalendarUser[] users)
        {
            foreach (var user in users)
                CreateUser(user);
        }
        public void CreateCalendarEvents(CalendarEvent[] calendarEvents)
        {
            foreach (var calendarEvent in calendarEvents)
                CreateCalendarEvent(calendarEvent);
        }
    }
}
