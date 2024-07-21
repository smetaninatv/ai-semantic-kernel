namespace AI.Demo.Infrastructure.Configuration;

/// <summary>
/// PostgreSQL configuration
/// </summary>
public class PostgreConfig
{  
    /// <summary>
    /// Hostname
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    /// Port
    /// </summary>
    public string Port { get; set; }

    /// <summary>
    /// UserName
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// DatabaseName
    /// </summary>
    public string DatabaseName { get; set; }
  
}
