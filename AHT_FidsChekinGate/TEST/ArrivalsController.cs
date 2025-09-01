using Microsoft.AspNetCore.Mvc;
using DigitalSignageSevice.Repositories;
using DigitalSignageSevice.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DigitalSignageSevice.Controllers
{
    [Route("api/arrivals")]
    [ApiController]
    public class ArrivalsController : ControllerBase
    {
        private readonly FidsRepository _fidsRepository;
        private readonly IHubContext<DashboardHub> _hubContext;

        public ArrivalsController(FidsRepository fidsRepository, IHubContext<DashboardHub> hubContext)
        {
            _fidsRepository = fidsRepository;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetArrivals([FromQuery] string? belt)
        {
            try
            {
                var flights = _fidsRepository.GetArrivalFlights();
                if (!string.IsNullOrEmpty(belt))
                {
                    // Lọc theo belt (B1, B2, ...)
                    flights = flights.Where(x => (x.Carousel ?? "") == belt).ToList();
                }
                return Ok(flights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy dữ liệu chuyến bay đến: " + ex.Message);
            }
        }

        [HttpGet("belt-auto")]
        public IActionResult GetArrivalForBeltAuto([FromQuery] string belt)
        {
            if (string.IsNullOrWhiteSpace(belt))
                return BadRequest("belt is required.");

            try
            {
                var flight = _fidsRepository.GetCurrentArrivalForBelt(belt, 60);
                return Ok(flight); // có thể null -> frontend sẽ hiện ảnh nền
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpPost("update-belt-realtime")]
        public async Task<IActionResult> UpdateBeltRealtime([FromBody] BeltUpdateRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.FlightNumber) || string.IsNullOrEmpty(req.Carousel) || string.IsNullOrEmpty(req.Status))
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ");
            }

            // Cập nhật dữ liệu trong DB
            var success = _fidsRepository.UpdateBeltAndStatus(req.FlightNumber, req.Carousel, req.Status);
            if (!success)
            {
                return BadRequest("Không thể cập nhật dữ liệu chuyến bay");
            }

            // Lấy thông tin chuyến bay đã cập nhật
            var flight = _fidsRepository.GetArrivalFlightByFlightNumber(req.FlightNumber);
            if (flight == null)
            {
                return NotFound("Không tìm thấy chuyến bay với số hiệu này sau khi cập nhật");
            }

            // Gửi sự kiện SignalR với dữ liệu chuyến bay
            await _hubContext.Clients.Group($"belt-{req.Carousel}")
                .SendAsync("UpdateBeltFlight", flight);

            return Ok(flight);
        }

        public class BeltUpdateRequest
        {
            public string FlightNumber { get; set; }
            public string Carousel { get; set; } // Belt (B1, B2, ...)
            public string Status { get; set; }
        }
    }
}