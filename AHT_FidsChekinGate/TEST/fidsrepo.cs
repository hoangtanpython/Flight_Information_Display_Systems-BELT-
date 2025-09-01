using Dapper;
using DigitalSignageSevice.Hubs;
using DigitalSignageSevice.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DigitalSignageSevice.Repositories
{
    public class FidsRepository
    {
        private readonly string connectionString;
        private readonly IHubContext<DashboardHub> _hubContext;
        public FidsRepository(IConfiguration configuration, IHubContext<DashboardHub> hubContext)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _hubContext = hubContext;
        }

        public bool UpdateOnConnectionId(string ip, string connect)
        {
            string query = "UPDATE [FidsDemo].[dbo].[AHT_CountersInformation] SET ConnectionId =@ConnectionId WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ConnectionId", connect);
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }

        public bool GetFlight(string ip)
        {
            var data = getConnectionIdClinets(ip);
            Console.WriteLine(data.Rows.Count);
            if (data.Rows.Count > 0) 
            {
                DataRow row = data.Rows[0];
                string query = "UPDATE [FidsDemo].[dbo].[AHT_CountersInformation] SET " +
                               "Time = @Schedule, " +
                               "OpenTime = @CounterStart,  " +
                               "CloseTime = @CounterEnd, " +
                               "Flight = @Flight, " +
                               "AutoImg = @LineCode, "+
                               "SetImg = @City, " +
                               "Status = @Status," +
                               "TimeMcdt = @Mcdt, " +
                               "Counters = @CheckInCounter, " +
                               "LastUpdated = GETDATE() " +
                               "WHERE Ip = @Ip";
                DataTable dataTable = new DataTable();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Schedule", row["Schedule"] != DBNull.Value ? Convert.ToDateTime(row["Schedule"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@CounterStart", row["CounterStart"] != DBNull.Value ? Convert.ToDateTime(row["CounterStart"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@CounterEnd", row["CounterEnd"] != DBNull.Value ? Convert.ToDateTime(row["CounterEnd"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@Flight", row["LineCode"].ToString() + row["Number"].ToString());
                            command.Parameters.AddWithValue("@LineCode", row["LineCode"].ToString());
                            command.Parameters.AddWithValue("@City", row["City"].ToString());
                            command.Parameters.AddWithValue("@Status", row["Status"].ToString());
                            command.Parameters.AddWithValue("@Mcdt", row["Mcdt"] != DBNull.Value ? Convert.ToDateTime(row["Mcdt"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@CheckInCounter", row["CheckInCounters"].ToString());
                            command.Parameters.AddWithValue("@Ip", ip);
                            command.ExecuteNonQuery();
                        }
                        Console.WriteLine(query);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return false;
                    }
                    finally { connection.Close(); }
                }
            }
            return false;
        }

        public bool GetFlightByName(string name)
        {
            var data = getFlightClinets(name);
            Console.WriteLine(data.Rows.Count);
            if (data.Rows.Count > 0)
            {
                DataRow row = data.Rows[0];
                string query = "UPDATE [FidsDemo].[dbo].[AHT_CountersInformation] SET " +
                               "Time = @Schedule, " +
                               "OpenTime = @CounterStart,  " +
                               "CloseTime = @CounterEnd, " +
                               "Flight = @Flight, " +
                               "AutoImg = @LineCode, " +
                               "SetImg = @City, " +
                               "Status = @Status," +
                               "TimeMcdt = @Mcdt, " +
                               "Counters = @CheckInCounter, " +
                               "LastUpdated = GETDATE() " +
                               "WHERE Name = @name";
                DataTable dataTable = new DataTable();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Schedule", row["Schedule"] != DBNull.Value ? Convert.ToDateTime(row["Schedule"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@CounterStart", row["CounterStart"] != DBNull.Value ? Convert.ToDateTime(row["CounterStart"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@CounterEnd", row["CounterEnd"] != DBNull.Value ? Convert.ToDateTime(row["CounterEnd"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@Flight", row["LineCode"].ToString() + row["Number"].ToString());
                            command.Parameters.AddWithValue("@LineCode", row["LineCode"].ToString());
                            command.Parameters.AddWithValue("@City", row["City"].ToString());
                            command.Parameters.AddWithValue("@Status", row["Status"].ToString());
                            command.Parameters.AddWithValue("@Mcdt", row["Mcdt"] != DBNull.Value ? Convert.ToDateTime(row["Mcdt"]) : DateTime.MinValue);
                            command.Parameters.AddWithValue("@CheckInCounter", row["CheckInCounters"].ToString());
                            command.Parameters.AddWithValue("@name", name);
                            command.ExecuteNonQuery();
                        }
                        Console.WriteLine(query);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return false;
                    }
                    finally { connection.Close(); }
                }
            }
            return false;
        }

        public DataTable getFlightClinets(string name)
        {
            string query = "SELECT TOP 1 fi.* FROM [FidsDemo].[dbo].[AHT_FlightInformation] fi JOIN [FidsDemo].[dbo].[AHT_CountersInformation] c " +
                           "ON ',' + fi.CheckInCounters + ',' LIKE '%,' + REPLACE(c.Location, 'Counter_', '') + ',%' " +
                           "WHERE fi.CounterEnd > GETDATE() AND C.Name = '" + name + "' order by fi.CounterEnd ASC";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { connection.Close(); }
            }
        }

        public DataTable getConnectionIdClinets(string ip)
        {
            string query = "SELECT TOP 1 fi.* FROM [FidsDemo].[dbo].[AHT_FlightInformation] fi JOIN [FidsDemo].[dbo].[AHT_CountersInformation] c "+
                           "ON ',' + fi.CheckInCounters + ',' LIKE '%,' + REPLACE(c.Location, 'Counter_', '') + ',%' "+
                           "WHERE fi.CounterEnd > GETDATE() AND C.Ip = '"+ip+"' order by fi.CounterEnd ASC";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { connection.Close(); }
            }
        }

        public bool UpdateDisConnectionId(string ip)
        {
            string query = "UPDATE [FidsDemo].[dbo].[AHT_CountersInformation] SET ConnectionId =@ConnectionId WHERE Ip = @Ip";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ConnectionId", "");
                        command.Parameters.AddWithValue("@Ip", ip);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }

        public bool deleteFlight(string name)
        {
            string query = "UPDATE [FidsDemo].[dbo].[AHT_CountersInformation] SET " +
                "Time = '', OpenTime ='', Flight ='', AutoImg ='AHT', SetImg ='', " +
                "TimeMcdt ='', Status ='', Counters ='' " +
                "WHERE Name = @Name";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { connection.Close(); }
            }
        }


        #region Get data By Gate Name
        public List<FidsLocation> GetConnectionId(string gate)
        {
            List<FidsLocation> fidsLocations = new List<FidsLocation>();
            FidsLocation fidsLocation;
            var data = getConnectionId(gate);
            foreach (DataRow row in data.Rows)
            {
                fidsLocation = new FidsLocation
                {
                    ConnectionId = row["ConnectionId"].ToString(),
                };
                fidsLocations.Add(fidsLocation);
            }
            return fidsLocations;
        }
        public DataTable getConnectionId(string local)
        {
            string query = "SELECT ConnectionId FROM [FidsDemo].[dbo].[AHT_FidsLocation] where Location = '" + local + "' ";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { connection.Close(); }
            }
        }
        #endregion 

        #region Get ConnectionId of Clients
        public List<FidsLocation> GetConnectionIdClients(string checkins, string gate)
        {
            List<FidsLocation> fidsLocations = new List<FidsLocation>();
            FidsLocation fidsLocation;
            var data = getConnectionIdClinets(checkins, gate);
            foreach (DataRow row in data.Rows)
            {
                fidsLocation = new FidsLocation
                {
                    ConnectionId = row["ConnectionId"].ToString(),
                };
                fidsLocations.Add(fidsLocation);
            }
            return fidsLocations;
        }
        public DataTable getConnectionIdClinets(string checkins, string gate)
        {
            string query = "SELECT ConnectionId FROM [FidsDemo].[dbo].[AHT_FidsLocation] "+
                           "WHERE(REPLACE(Name, 'C', '') IN(SELECT value FROM STRING_SPLIT('"+checkins+"', ',')) "+
                           "AND Description = 'Checkin') "+
                           "OR(Description = 'Gate' AND Name = '"+gate+"') "+
                           "OR Description = 'Dep' OR Description = 'Arr'";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { connection.Close(); }
            }
        }
        #endregion 


        #region Get ConnectionId of Clients
        public List<AHT_CountersInformation> GetAllCheckin(string checkins)
        {
            List<AHT_CountersInformation> aHT_CountersInformations = new List<AHT_CountersInformation>();
            AHT_CountersInformation aHT_CountersInformation;
            var data = getAllCheckin(checkins);
            foreach (DataRow row in data.Rows)
            {
                aHT_CountersInformation = new AHT_CountersInformation
                {
                    Id = Convert.ToInt32(row["ID"]),
                    Name = row["Name"]?.ToString(),
                    Location = row["Location"]?.ToString(),
                    Ip = row["Ip"]?.ToString(),
                    Time = row["Time"] != DBNull.Value ? Convert.ToDateTime(row["Time"]) : (DateTime?)null,
                    OpenTime = row["OpenTime"] != DBNull.Value ? Convert.ToDateTime(row["OpenTime"]) : (DateTime?)null,
                    CloseTime = row["CloseTime"] != DBNull.Value ? Convert.ToDateTime(row["CloseTime"]) : (DateTime?)null,
                    Flight = row["Flight"]?.ToString(),
                    AutoImg = row["AutoImg"]?.ToString(),
                    SetImg = row["SetImg"]?.ToString(),
                    Status = row["Status"]?.ToString(),
                    Counters = row["Counters"]?.ToString(),
                    Mode = row["Mode"]?.ToString(),
                    Auto = row["Auto"]?.ToString(),
                    ConnectionId = row["ConnectionId"]?.ToString(),
                    ImageName = row["ImageName"]?.ToString(),
                    Nomal = row["Nomal"]?.ToString(),
                    Eco = row["Eco"]?.ToString(),
                    Bus = row["Bus"]?.ToString(),
                    Manual = row["Manual"]?.ToString(),
                };
                aHT_CountersInformations.Add(aHT_CountersInformation);
            }
            return aHT_CountersInformations;
        }
        public DataTable getAllCheckin(string checkins)
        {
            //string query = "SELECT * FROM[FidsDemo].[dbo].[AHT_CountersInformation] WHERE Location = 'Server' or Location LIKE '%"+checkins+"%'";
            string query = "SELECT ACI.*, AIF.Name AS ImageName, AIF.Nomal, AIF.Eco, AIF.Bus, AIF.Manual "+
                           "FROM [FidsDemo].[dbo].[AHT_CountersInformation] AS ACI "+
                           "LEFT JOIN [FidsDemo].[dbo].[AHT_ImageForFlight] AS AIF " +
                           "ON LEFT(ACI.Flight, 2) = AIF.Name  WHERE Location = 'Server' or Location LIKE '%" + checkins+"%'";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { connection.Close(); }
            }
        }
        #endregion

        #region getEntity
        public AHT_CountersInformation? GetEntity(string checkins)
        {
            return GetSingleCheckinByLocation(checkins);
        }

        public AHT_CountersInformation? GetSingleCheckinByLocation(string locationToSearch)
        {
            AHT_CountersInformation? aHT_CountersInformation = null;
            var data = getCounterByLocal(locationToSearch);
            foreach (DataRow row in data.Rows)
            {
                // Kiểm tra nếu Location trong dòng hiện tại khớp với locationToSearch
                if (row["Location"]?.ToString() == locationToSearch)
                {
                    // Khởi tạo đối tượng AHT_CountersInformation từ dữ liệu dòng
                    aHT_CountersInformation = new AHT_CountersInformation
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Name = row["Name"]?.ToString(),
                        Ip = row["Ip"]?.ToString(),
                        Location = row["Location"]?.ToString(),
                        Time = row["Time"] != DBNull.Value ? Convert.ToDateTime(row["Time"]) : (DateTime?)null,
                        OpenTime = row["OpenTime"] != DBNull.Value ? Convert.ToDateTime(row["OpenTime"]) : (DateTime?)null,
                        CloseTime = row["CloseTime"] != DBNull.Value ? Convert.ToDateTime(row["CloseTime"]) : (DateTime?)null,
                        Flight = row["Flight"]?.ToString(),
                        AutoImg = row["AutoImg"]?.ToString(),
                        SetImg = row["SetImg"]?.ToString(),
                        TimeMcdt = row["TimeMcdt"] != DBNull.Value ? Convert.ToDateTime(row["TimeMcdt"]) : (DateTime?)null,
                        Status = row["Status"]?.ToString(),
                        Counters = row["Counters"]?.ToString(),
                        Mode = row["Mode"]?.ToString(),
                        Auto = row["Auto"]?.ToString(),
                        ConnectionId = row["ConnectionId"]?.ToString(),
                        LastUpdated = row["LastUpdated"] != DBNull.Value ? Convert.ToDateTime(row["LastUpdated"]) : (DateTime?)null,
                        ImageName = row["ImageName"]?.ToString(),
                        Nomal = row["Nomal"]?.ToString(),
                        Eco = row["Eco"]?.ToString(),
                        Bus = row["Bus"]?.ToString(),
                        Manual = row["Manual"]?.ToString(),
                    };

                    // Thoát khỏi vòng lặp sau khi tìm thấy bản ghi đầu tiên
                    break;
                }
            }

            // Trả về bản ghi hoặc null nếu không tìm thấy
            return aHT_CountersInformation;

        }


        public DataTable getCounterByLocal(string checkins)
        {
            //string query = "SELECT * FROM[FidsDemo].[dbo].[AHT_CountersInformation] WHERE Location = 'Server' or Location LIKE '%"+checkins+"%'";
            string query = "SELECT ACI.*, AIF.Name AS ImageName, AIF.Nomal, AIF.Eco, AIF.Bus, AIF.Manual " +
                           "FROM [FidsDemo].[dbo].[AHT_CountersInformation] AS ACI " +
                           "LEFT JOIN [FidsDemo].[dbo].[AHT_ImageForFlight] AS AIF " +
                           "ON LEFT(ACI.Flight, 2) = AIF.Name  WHERE Location = '" + checkins + "'";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { connection.Close(); }
            }
        }
        #endregion

        public string GetNameByIp(string ip)
        {
            string query = "SELECT Location FROM [FidsDemo].[dbo].[AHT_CountersInformation] WHERE Ip = @Ip";
            string flightName = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Ip", ip);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read()) 
                            {
                                flightName = reader["Location"]?.ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            return flightName; 
        }
        public List<dynamic> GetArrivalFlights()
        {
            string query = @"
        SELECT 
            FlightNumber, Airline, Destination, ScheduledTime, EstimatedTime, ActualTime,
            Carousel, Terminal, Status
        FROM AHT_FlightInformation
        WHERE IsDeparture = 0
        ORDER BY ScheduledTime ASC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var result = connection.Query(query);
                return result.ToList();
            }
        }

        public bool UpdateBeltAndStatus(string flightNumber, string carousel, string status)
        {
            string query = @"UPDATE [FidsDemo].[dbo].[AHT_FlightInformation] 
                         SET Carousel = @Carousel, Status = @Status, UpdatedAt = GETDATE()
                         WHERE FlightNumber = @FlightNumber";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Carousel", carousel);
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@FlightNumber", flightNumber);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            var flight = GetArrivalFlightByFlightNumber(flightNumber);
                            _hubContext.Clients.Group($"belt-{carousel}").SendAsync("UpdateBeltFlight", flight).Wait(); // Push dữ liệu
                        }

                        return rowsAffected > 0;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public Flight? GetArrivalFlightByFlightNumber(string flightNumber)
        {
            string query = "SELECT FlightNumber, Airline, Destination, ScheduledTime, Carousel, Status FROM [FidsDemo].[dbo].[AHT_FlightInformation] WHERE FlightNumber = @FlightNumber";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FlightNumber", flightNumber);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Flight
                            {
                                FlightNumber = reader["FlightNumber"].ToString(),
                                Airline = reader["Airline"].ToString(),
                                Destination = reader["Destination"].ToString(),
                                ScheduledTime = reader["ScheduledTime"].ToString(),
                                Carousel = reader["Carousel"].ToString(),
                                Status = reader["Status"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }
        public Flight? GetCurrentArrivalForBelt(string belt, int windowMinutes = 60)
        {
            const string sql = @"
            SELECT TOP 1
                FlightNumber, Airline, Destination, Status,
                CONVERT(varchar(19), ScheduledTime, 120)  AS ScheduledTime,
                CONVERT(varchar(19), EstimatedTime, 120)  AS EstimatedTime,
                CONVERT(varchar(19), ActualTime, 120)     AS ActualTime,
                Carousel, Terminal
            FROM [FidsDemo].[dbo].[AHT_FlightInformation]
            WHERE IsDeparture = 0
              AND Carousel = @Belt
              AND ActualTime IS NOT NULL
              AND ABS(DATEDIFF(MINUTE, ActualTime, GETDATE())) <= @Window
            ORDER BY ABS(DATEDIFF(MINUTE, ActualTime, GETDATE())) ASC, UpdatedAt DESC;";

            using var conn = new SqlConnection(connectionString);
            return conn.QueryFirstOrDefault<Flight>(sql, new { Belt = belt, Window = windowMinutes });
        }


    }
}