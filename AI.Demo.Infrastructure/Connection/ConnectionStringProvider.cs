using AI.Demo.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Demo.Infrastructure.Connection;

/// <summary>
/// Connection String Provider.
/// </summary>
public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IOptions<PostgreConfig> _configurationSettings;

    public ConnectionStringProvider(IOptions<PostgreConfig> configurationSettings) 
    {
        _configurationSettings = configurationSettings;
    }

    /// <summary>
    /// Create connection string
    /// </summary>
    /// <returns>Connection string</returns>
    public string GetConnectionString()
    {
        return $"Server={_configurationSettings.Value.Hostname};Port={_configurationSettings.Value.Port};database={_configurationSettings.Value.DatabaseName};user id={_configurationSettings.Value.UserName};password={_configurationSettings.Value.Password}";
    }
}