using S7.Net;
using S7.Net.Types;
using MyS7Core.SiemensEthernet;
using System;

namespace MyS7Core
{
    /// <summary>
    /// Extends the S7.Net DataItem class with additional properties for Siemens S7 PLC address metadata
    /// and simplified address parsing/initialization functionality.
    /// </summary>
    /// <remarks>
    /// This class encapsulates all necessary information for reading/writing values to a specific
    /// Siemens S7 PLC address, including data type metadata, address components, and the value to transfer.
    /// Inherits from S7.Net's base DataItem to maintain compatibility with S7.Net library operations.
    /// </remarks>
    public class DtDataItem : DataItem
    {
        /// <summary>
        /// Gets or sets the PLC data type category (e.g., DataBlock, Input, Output, Memory, Timer, Counter)
        /// </summary>
        /// <remarks>
        /// Corresponds to the S7.Net DataType enumeration, identifying the memory area of the PLC address
        /// </remarks>
        public new DataType DataType { get; set; }
        /// <summary>
        /// Gets or sets the specific variable type (e.g., Byte, Word, DInt, Real, String) for the PLC address
        /// </summary>
        /// <remarks>
        /// Maps to S7.Net VarType enumeration, defining the data format for value conversion/transmission
        /// </remarks>
        public new VarType VarType { get; set; }
        /// <summary>
        /// Gets or sets the Data Block number (DB number) for DB-type addresses (0 for non-DB addresses like Input/Output/Memory)
        /// </summary>
        /// <example>
        /// For address "DB10.DBX5.2", DB = 10; for "IW20", DB = 0
        /// </example>
        public new int DB { get; set; }
        /// <summary>
        /// Gets or sets the starting byte address within the specified memory area/DB
        /// </summary>
        /// <example>
        /// For address "DB10.DBX5.2", StartByteAdr = 5; for "MB15", StartByteAdr = 15
        /// </example>
        public new int StartByteAdr { get; set; }
        /// <summary>
        /// Gets or sets the bit address (0-7) for bit-level PLC addresses (e.g., DBX, I, Q, M bits)
        /// </summary>
        /// <remarks>
        /// Set to 0 for non-bit address types (e.g., DBW, DBB, MW) since they operate on full bytes/words
        /// </remarks>
        public new byte BitAdr { get; set; }
        /// <summary>
        /// Gets or sets the number of elements to read/write (length for strings, count for arrays, 1 for scalar values)
        /// </summary>
        /// <remarks>
        /// Defaults to 1 for single values; used for string length specification (e.g., DBB10.20 = 20 characters)
        /// </remarks>
        public new int Count { get; set; }
        /// <summary>
        /// Gets or sets the value to read from/write to the PLC address (supports all PLC-compatible types: bool, byte, int, float, string, etc.)
        /// </summary>
        /// <remarks>
        /// Value type must match the VarType property (e.g., float for VarType.Real, string for VarType.String)
        /// </remarks>
        public new object? Value { get; set; }
        /// <summary>
        /// Gets the original PLC address string (e.g., "DB1.DBX10.0", "IW20", "MB5") used to create this DtDataItem
        /// </summary>
        public string Address { get; internal set; }
        /// <summary>
        /// Initializes a new instance of the DtDataItem class with default values
        /// </summary>
        /// <remarks>
        /// Default configuration:
        /// - VarType = Byte (8-bit unsigned)
        /// - Count = 1 (single scalar value)
        /// All other properties initialized to their type defaults (0 for numeric, null for Value)
        /// </remarks>
        public DtDataItem()
        {
            VarType = VarType.Byte;
            Count = 1;
        }
        /// <summary>
        /// Creates a DtDataItem instance by parsing a Siemens S7 PLC address string and associating it with a value
        /// </summary>
        /// <param name="address">Siemens S7 PLC address string (e.g., "DB1.DBX10.0", "IW20", "MB5")</param>
        /// <param name="value">Value to associate with the address (will be type-converted to match address type)</param>
        /// <returns>Configured DtDataItem with parsed address metadata and type-converted value</returns>
        /// <exception cref="InvalidAddressException">Thrown if the address string is invalid or unrecognized</exception>
        /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to the address's required type</exception>

        public static DtDataItem FromAddress(string address, object value)
        {
            PlcAddress.Parse(address, out var dataType, out var dbNumber, out var varType, out var address2, out var bitNumber, ref value);
            return new DtDataItem
            {
                Address = address,
                DataType = dataType,
                DB = dbNumber,
                VarType = varType,
                StartByteAdr = address2,
                BitAdr = (byte)((bitNumber != -1) ? ((uint)bitNumber) : 0u),
                Value = value
            };
        }
    }
}
