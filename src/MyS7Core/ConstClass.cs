using S7.Net;

namespace MyS7Core;

/// <summary>
/// Global constant definitions for PLC communication.
/// This class contains default configuration values used by S7.Net
/// to communicate with Siemens PLCs (e.g. S7-1500 series).
/// </summary>
public static class ConstClass
{
    /// <summary>
    /// Default timeout for PLC connection and communication operations (in milliseconds).
    /// Value is taken from <see cref="Plc.DefaultTimeout"/> (default: 10000 ms).
    /// </summary>
    public const int DefaultConnectTimeout = Plc.DefaultTimeout;

    /// <summary>
    /// Default PLC rack number.
    /// For S7-1500, this is typically 0 or 2 depending on the hardware configuration.
    /// </summary>
    public const int DefaultRackAddress = 2;

    /// <summary>
    /// Default PLC CPU slot number.
    /// For S7-1500, the CPU is usually located in slot 1.
    /// </summary>
    public const short DefaultCpuSlot = 1;

    /// <summary>
    /// Maximum number of items allowed in a single PDU (Protocol Data Unit).
    /// This affects how many read/write variables can be transferred in one request.
    /// </summary>
    public const int DefaultMaxPduItems = 20;

    /// <summary>
    /// Maximum number of bytes allowed in a single PDU (Protocol Data Unit).
    /// Exceeding this value may cause communication failures.
    /// </summary>
    public const int DefaultMaxPduBytes = 240;

    /// <summary>
    /// Default number of worker threads used for PLC communication.
    /// A value of 1 ensures serialized access and avoids concurrency issues.
    /// </summary>
    public const int DefaultThreadCount = 1;

    /// <summary>
    /// Sleep time (in milliseconds) between PLC communication cycles
    /// when using background or polling threads.
    /// </summary>
    public const int DefaultThreadSleep = 1000;

    /// <summary>
    /// Default TCP port used for Siemens S7 PLC communication.
    /// Standard S7 communication port is 102.
    /// </summary>
    public const int DefaultPlcNetPort = Plc.DefaultPort;

    /// <summary>
    /// Constant value representing subdivision factor 2.
    /// Typically used for scaling or addressing calculations.
    /// </summary>
    public const int DefaultSubTwo = 2;

    /// <summary>
    /// Constant value representing subdivision factor 3.
    /// Typically used for scaling or addressing calculations.
    /// </summary>
    public const int DefaultSubThree = 3;

    /// <summary>
    /// Constant value representing subdivision factor 4.
    /// Typically used for scaling or addressing calculations.
    /// </summary>
    public const int DefaultSubFour = 4;

    /// <summary>
    /// Constant value representing subdivision factor 5.
    /// Typically used for scaling or addressing calculations.
    /// </summary>
    public const int DefaultSubFive = 5;

    /// <summary>
    /// Constant value representing subdivision factor 6.
    /// Typically used for scaling or addressing calculations.
    /// </summary>
    public const int DefaultSubSix = 6;

    /// <summary>
    /// Constant value representing subdivision factor 7.
    /// Typically used for scaling or addressing calculations.
    /// </summary>
    public const int DefaultSubSeven = 7;

    /// <summary>
    /// Constant value representing subdivision factor 8.
    /// Typically used for scaling or addressing calculations.
    /// </summary>
    public const int DefaultSubEight = 8;

    /// <summary>
    /// Default subdivision base value.
    /// Commonly used for percentage, scaling, or resolution calculations.
    /// </summary>
    public const int DefaultSubdivision = 100;

    /// <summary>
    /// DateTime tick bias used to convert between .NET DateTime ticks
    /// and PLC or Unix-based time representations.
    /// </summary>
    public const long Bias = 621355968000000000;

    /// <summary>
    /// Default DateTime string used as an initial or fallback value.
    /// Format: yyyy/M/d H:mm:ss.
    /// </summary>
    public const string DefaultDateTime = "1990/1/1 0:00:00";
}
