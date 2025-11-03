using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace GymInfrastructure.Hubs
{
    public class DrawingHub : Hub
    {
        public async Task SendDrawing(float x, float y)
        {
            await Clients.Others.SendAsync("ReceiveDrawing", x, y);
        }
    }
}
