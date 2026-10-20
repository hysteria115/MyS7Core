namespace MyS7Core.SiemensEthernet;

/// <summary>
/// Represents runtime configuration for PLC monitoring operations.
/// Contains address groupings, update rates, and monitoring keys for efficient data collection.
/// </summary>
public class MonitorRuntime
{
    /// <summary>
    /// Gets or sets the grouped list of monitor addresses organized by database number.
    /// This grouping optimizes PLC read operations by batching addresses from the same database.
    /// </summary>
    public List<IGrouping<int, MonitorAddress>> GroupList { get; set; } = new();

    /// <summary>
    /// Gets or sets the complete list of monitor addresses to be tracked.
    /// Contains all addresses that will be monitored during runtime operations.
    /// </summary>
    public List<MonitorAddress> AddressList { get; set; } = new();

    /// <summary>
    /// Gets or sets the update rate in milliseconds for monitoring operations.
    /// Determines how frequently the PLC addresses are polled for changes.
    /// </summary>
    public int UpdateRate { get; set; }

    /// <summary>
    /// Gets or sets the unique key identifier for this monitoring runtime instance.
    /// Used to distinguish between multiple concurrent monitoring operations.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}
