using System.Linq;

namespace cowin.Settings
{
    public record ClientPreference : IPreference
    {
        public bool IsDarkMode { get; set; }
    }
    public interface IPreference
    {
    }
}