using System.Threading.Tasks;
using LoginResult = NexaCRM.UI.Models.LoginResult;

namespace NexaCRM.UI.Services.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResult> SignInAsync(string username, string password);

    Task LogoutAsync();
}
