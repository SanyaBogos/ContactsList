using System.Threading.Tasks;

namespace ContactsList.Server.Services.Abstract
{
    public interface ISmsSender
    {
        Task<bool> SendSmsTwillioAsync(string number, string message);
        Task<bool> SendSmsFastSmsAsync(string number, string message);
    }
}
