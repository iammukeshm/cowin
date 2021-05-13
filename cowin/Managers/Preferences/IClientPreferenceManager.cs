using MudBlazor;
using System.Threading.Tasks;

namespace cowin.Managers.Preferences
{
    public interface IClientPreferenceManager
    {
        Task<MudTheme> GetCurrentThemeAsync();

        Task<bool> ToggleDarkModeAsync();
    }
}