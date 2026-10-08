using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    public interface IDialogService
    {
        Task ShowMessageAsync(string title, string message);
        Task<bool> ConfirmAsync(string title, string message);
    }
}
