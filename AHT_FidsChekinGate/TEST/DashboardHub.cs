using DigitalSignageSevice.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace DigitalSignageSevice.Hubs
{
    public class DashboardHub : Hub
    {
        private readonly FidsRepository _fidsRepository;

        public DashboardHub(FidsRepository fidsRepository)
        {
            _fidsRepository = fidsRepository ?? throw new ArgumentNullException(nameof(fidsRepository));
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            string belt = httpContext?.Request.Query["belt"]; // belt=B1, B2...
                                                              // Cho phép client join vào group riêng từng belt nếu truyền lên
            if (!string.IsNullOrEmpty(belt))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"belt-{belt}");
            }
            await base.OnConnectedAsync();

            var connectionId = Context.ConnectionId;
            var clientIp = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()?.Trim();

            // ✅ Sửa lỗi IP "::1" (IPv6 loopback)
            if (clientIp == "::1") clientIp = "127.0.0.1";

            if (string.IsNullOrWhiteSpace(clientIp))
            {
                Console.WriteLine("❌ Không xác định được địa chỉ IP client.");
                await base.OnConnectedAsync();
                return;
            }

            Console.WriteLine($"✅ Kết nối mới từ IP: {clientIp} - ConnectionId: {connectionId}");

            var location = _fidsRepository.GetNameByIp(clientIp);

            if (string.IsNullOrWhiteSpace(location))
            {
                Console.WriteLine($"⚠️ Không tìm thấy Location tương ứng với IP: {clientIp}");
                await base.OnConnectedAsync();
                return;
            }

            bool updated = _fidsRepository.UpdateOnConnectionId(clientIp, connectionId);

            if (!updated)
            {
                Console.WriteLine($"⚠️ Không thể cập nhật ConnectionId cho IP: {clientIp}");
                await base.OnConnectedAsync();
                return;
            }

            if (location.ToLower().Contains("counter"))
            {
                if (_fidsRepository.GetFlight(clientIp))
                {
                    await SendToClient(location);
                    Console.WriteLine("✅ Cập nhật ConnectionId + Flight thành công.");
                }
                else
                {
                    Console.WriteLine("✅ Chỉ cập nhật ConnectionId (chưa có Flight).");
                }
            }
            else if (location.ToLower().Contains("server"))
            {
                await SendToServer("Counter");
                Console.WriteLine("🔁 Gửi lại thông tin Counter về Server.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var clientIp = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()?.Trim();

            if (clientIp == "::1") clientIp = "127.0.0.1"; // ✅ xử lý ở cả OnDisconnectedAsync

            if (!string.IsNullOrWhiteSpace(clientIp))
            {
                Console.WriteLine($"🔌 Ngắt kết nối từ IP: {clientIp} - ConnectionId: {connectionId}");
                _fidsRepository.UpdateDisConnectionId(clientIp);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task ReloadServerDashboard()
        {
            var data = _fidsRepository.GetConnectionId("Admin");
            if (data == null || !data.Any())
            {
                Console.WriteLine("⚠ Không tìm thấy Server Admin để reload.");
                return;
            }

            foreach (var d in data.Where(x => !string.IsNullOrWhiteSpace(x.ConnectionId)))
            {
                await Clients.Client(d.ConnectionId).SendAsync("ReceivedServerReload", d.ConnectionId, "Reload");
            }
        }

        public async Task ReloadClients(string checkin, string gate)
        {
            var data = _fidsRepository.GetConnectionIdClients(checkin, gate);
            if (data == null || !data.Any()) return;

            foreach (var d in data.Where(x => !string.IsNullOrWhiteSpace(x.ConnectionId)))
            {
                await Clients.Client(d.ConnectionId).SendAsync("ReceivedServerReloadToClient", d.ConnectionId, "ReloadClient");
            }
        }

        public async Task SendToServer(string local)
        {
            var data = _fidsRepository.GetAllCheckin(local);
            if (data == null || !data.Any()) return;

            foreach (var d in data.Where(x =>
                !string.IsNullOrWhiteSpace(x.ConnectionId) &&
                !string.IsNullOrWhiteSpace(x.Location) &&
                x.Location.ToLower().Contains("server")))
            {
                await Clients.Client(d.ConnectionId).SendAsync("SendToServer", data);
            }
        }

        public async Task SendToClient(string location)
        {
            Console.WriteLine($"📤 Gửi thông tin tới client tại location: {location}");
            var entity = _fidsRepository.GetEntity(location);

            if (entity == null || string.IsNullOrWhiteSpace(entity.ConnectionId))
            {
                Console.WriteLine("⚠ Không tìm thấy ConnectionId hợp lệ cho client.");
                return;
            }

            await Clients.Client(entity.ConnectionId).SendAsync("SendToClient", entity);
        }

        public async Task DeleteFlightFromClient1(string flightName)
        {
            if (string.IsNullOrWhiteSpace(flightName))
            {
                Console.WriteLine("❌ Tên chuyến bay trống.");
                return;
            }

            flightName = flightName.Trim();

            if (_fidsRepository.GetFlightByName(flightName))
            {
                await SendToServer("Counter");
                Console.WriteLine("🗑 Đã xóa và cập nhật chuyến bay.");
            }
            else
            {
                Console.WriteLine("🗑 Đã xóa chuyến bay nhưng không có cập nhật.");
            }
        }

        public async Task<bool> DeleteFlightFromClient(string flightName)
        {
            if (string.IsNullOrWhiteSpace(flightName)) return false;

            flightName = flightName.Trim();

            if (!_fidsRepository.GetFlightByName(flightName))
            {
                Console.WriteLine("⚠ Chuyến bay không tồn tại trong hệ thống.");
                return false;
            }

            try
            {
                await SendToServer("Counter");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi gửi thông tin xóa chuyến bay: {ex.Message}");
                return false;
            }
        }

        public async Task PushBeltUpdate(string belt)
        {
            var flight = _fidsRepository.GetCurrentArrivalForBelt(belt, 60);
            await Clients.Group($"belt-{belt}").SendAsync("UpdateBeltFlight", flight);
        }

    }
}