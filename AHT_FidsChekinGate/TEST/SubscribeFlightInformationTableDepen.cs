using DigitalSignageSevice.Hubs;
using DigitalSignageSevice.Models;
using DigitalSignageSevice.Repositories;
using Microsoft.AspNetCore.SignalR;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;

namespace DigitalSignageSevice.SubscribeTableDependencies
{
    public class SubscribeFlightInformationTableDependency
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly FidsRepository _repo;
        private SqlTableDependency<AHT_FlightInformation> _table;

        public SubscribeFlightInformationTableDependency(
            IHubContext<DashboardHub> hubContext,
            FidsRepository repo,
            IConfiguration cfg)
        {
            _hubContext = hubContext;
            _repo = repo;
            _connectionString = cfg.GetConnectionString("DefaultConnection");
        }

        private readonly string _connectionString;

        public void Start()
        {
            // Bảng cần theo dõi
            _table = new SqlTableDependency<AHT_FlightInformation>(_connectionString, tableName: "AHT_FlightInformation");
            _table.OnChanged += OnChanged;
            _table.OnError += (s, e) => Console.WriteLine(e.Error?.Message);
            _table.Start();
        }

        private async void OnChanged(object? sender, RecordChangedEventArgs<AHT_FlightInformation> e)
        {
            var belt = e.Entity?.Carousel;
            if (string.IsNullOrWhiteSpace(belt)) return;

            // Lấy flight đang hiệu lực +-60' và push cho group belt-<belt>
            var flight = _repo.GetCurrentArrivalForBelt(belt, 60);
            await _hubContext.Clients.Group($"belt-{belt}")
                                      .SendAsync("UpdateBeltFlight", flight);
        }

        public void Stop() => _table?.Stop();
    }

    // mapping class cho TableDependency (đủ cột bạn quan tâm)
    public class AHT_FlightInformation
    {
        public string? FlightNumber { get; set; }
        public string? Airline { get; set; }
        public string? Destination { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public DateTime? EstimatedTime { get; set; }
        public DateTime? ActualTime { get; set; }
        public string? Carousel { get; set; }
        public string? Terminal { get; set; }
        public string? Status { get; set; }
        public int IsDeparture { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}