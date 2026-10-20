namespace MyS7Core.SiemensEthernet;

/// <summary>
/// Represents a monitored PLC address with optional associated parameters for Siemens S7 protocol communication
/// </summary>
/// <param name="Address">Siemens S7 PLC address string (e.g., DB1.DBX10.0, DB2.DBW20, DB3.DBF30)</param>
/// <param name="Parameters">Optional read-only dictionary of custom parameters associated with the address; 
/// Can store metadata, conversion rules, or address-specific configurations (null if no parameters needed)</param>
public record MonitorAddress(string Address, IReadOnlyDictionary<string, object>? Parameters = null)
{
    /// <summary>
    /// Creates a new MonitorAddress instance with the specified PLC address and no additional parameters
    /// </summary>
    /// <param name="address">Siemens S7 PLC address string to monitor</param>
    /// <returns>A new MonitorAddress instance with empty parameters</returns>
    public static MonitorAddress Create(string address) => new(address);
}
