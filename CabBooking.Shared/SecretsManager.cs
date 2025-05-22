using Microsoft.Extensions.Configuration;

namespace CabBooking.Shared
{
    public static class SecretsManager
    {
        private static IConfigurationRoot? _config;

        public static void Load()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
                .Build();
        }

        public static string? Get(string section, string key)
        {
            return _config?[section + ":" + key];
        }
    }
}
