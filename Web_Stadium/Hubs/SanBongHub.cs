using Microsoft.AspNetCore.SignalR;

namespace Web_Stadium.Hubs
{
    /// <summary>
    /// SignalR Hub — Real-time cập nhật trạng thái khung giờ
    /// </summary>
    public class SanBongHub : Hub
    {
        // Client gọi để tham gia theo dõi 1 sân cụ thể
        // LƯU Ý: Tên group phải nhất quán với BookingController: "san_{sanBongId}"
        public async Task ThamGiaSan(int sanBongId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"san_{sanBongId}");   // ← lowercase "san_" cho đồng nhất với controller
        }

        // Client rời khỏi nhóm sân
        public async Task RoiSan(int sanBongId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"san_{sanBongId}");
        }
    }
}
