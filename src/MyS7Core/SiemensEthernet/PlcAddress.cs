using S7.Net;
using S7.Net.Types;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DateTime = System.DateTime;

namespace MyS7Core.SiemensEthernet;

/// <summary>
/// Represents a PLC address configuration with parsing capabilities for Siemens S7 address formats.
/// Provides utilities for converting address strings into typed DataItem objects for PLC communication.
/// </summary>
public class PlcAddress
{
    /// <summary>
    /// Gets or sets the PLC address string in Siemens S7 format.
    /// </summary>
    /// <example>Examples: "DB1.DBX10.0", "DB100.DBW254", "DB5.DBF20"</example>
    public string AddressStr { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default value to use when the PLC address cannot be read or is uninitialized.
    /// The type should match the expected data type of the address.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the default rack address offset used to extract numeric address values from PLC address strings.
    /// </summary>
    public static int RackAddress { get; set; } = ConstClass.DefaultRackAddress;

    /// <summary>
    /// Parse a PLC address string into a <see cref="DataItem"/> object.
    /// Uses regex-based strategies to detect address type and map it to the correct VarType.
    /// </summary>
    /// <param name="address">PLC address string (e.g., "DB1.DBX10.0").</param>
    /// <returns>A <see cref="DataItem"/> describing the parsed PLC memory location.</returns>
    /// <exception cref="ArgumentNullException">Thrown if address is null/empty or not recognized.</exception>
    public static DataItem? GetDataItem(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return null;
        }

        var strArray = address.Split('.');
        var db = 0;
        const int startIndexForDB = 2;
        const int startIndexForStartByteAdr3 = 3;
        const int startIndexForStartByteAdr4 = 4;

        if (strArray.Length > 0)
        {
            db = int.Parse(strArray[0].Substring(startIndexForDB));
        }

        // Define strategy table:
        // Each strategy is a (Regex, Factory Function) pair
        var strategies = new (Regex regex, Func<DataItem> factory)[]
        {
        // DBX: Bit inside a byte
        (RegexManager.DbxAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, byte.Parse(strArray[2]), false)),

        // DBW: 16-bit signed integer
        (RegexManager.DbwAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.Int, 0)),

        // UDBW: 16-bit unsigned integer
        (RegexManager.UdbwAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr4)), 1, VarType.Word, 0)),

        // SIT: Signed byte
        (RegexManager.SitAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.Byte, 0)),

        // USIT: Unsigned byte
        (RegexManager.UsitAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr4)), 1, VarType.Byte, 0)),

        // DIT: 32-bit signed integer
        (RegexManager.DitAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.DInt, 0)),

        // UDIT: 32-bit unsigned integer
        (RegexManager.UditAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr4)), 1, VarType.DWord, 0)),

        // DDWD: 32-bit DWord with hexadecimal value representation
        (RegexManager.DdwdAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr4)), 1, VarType.DWord, 0)),

        // DBB: String with specified length
        (RegexManager.DbbAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), int.Parse(strArray[2]), VarType.String, "")),

        // DBS: Substring (string with offset of +2)
        (RegexManager.DbsAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)) + 2, int.Parse(strArray[2]), VarType.String, "")),

        // DBF: 32-bit floating point (REAL)
        (RegexManager.DbfAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.Real, 0.0)),

        // DDB: 64-bit floating point (LREAL)
        (RegexManager.DdbAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.LReal, 0.00)),

        // DWD: 16-bit unsigned word
        (RegexManager.DwdAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.Word, 0)),

        // DBD: 32-bit unsigned double word
        (RegexManager.DbdAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.DWord, 0)),

        // DBT: Single byte
        (RegexManager.DbtAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.Byte, 0)),

        // DTL: DateTime (long format)
        (RegexManager.DtlAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.DateTimeLong, null)),

        // LDT: Large DateTime (treated as DateTime)
        (RegexManager.LdtAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.DateTime, null)),

        // DAT: Standard DateTime
        (RegexManager.DatAddressRegex(), () =>
            ParamsToDataItem(db, int.Parse(strArray[1].Substring(startIndexForStartByteAdr3)), 1, VarType.DateTime, null))
        };

        // Iterate strategies and return the first matching DataItem
        foreach (var (regex, factory) in strategies)
        {
            if (regex.IsMatch(address))
                return factory();
        }

        throw new ArgumentNullException("Unrecognised address: " + address);
    }

    /// <summary>
    /// Creates a DataItem for bit-specific addresses (DBX) with bit address information.
    /// </summary>
    /// <param name="db">Data block number (e.g., 1 for DB1)</param>
    /// <param name="startByteAdr">Starting byte address within the DB</param>
    /// <param name="count">Number of elements to read/write (typically 1 for bits)</param>
    /// <param name="bitAdr">Bit position (0-7) within the byte</param>
    /// <param name="value">Default value for the DataItem</param>
    /// <returns>Configured DataItem for bit-level PLC operations</returns>
    private static DataItem ParamsToDataItem(int db, int startByteAdr, int count, byte bitAdr, object value)
    {
        return new()
        {
            DataType = DataType.DataBlock,
            DB = db,
            StartByteAdr = startByteAdr,
            BitAdr = bitAdr,
            Count = count,
            VarType = VarType.Bit,
            Value = value
        };
    }

    /// <summary>
    /// Creates a DataItem for byte/word/double-word based addresses (non-bit types).
    /// </summary>
    /// <param name="db">Data block number (e.g., 1 for DB1)</param>
    /// <param name="startByteAdr">Starting byte address within the DB</param>
    /// <param name="count">Number of elements to read/write (1 for scalar values, n for arrays)</param>
    /// <param name="varType">PLC data type (VarType) for the address</param>
    /// <param name="value">Default value for the DataItem</param>
    /// <returns>Configured DataItem for byte/word-level PLC operations</returns>
    private static DataItem ParamsToDataItem(int db, int startByteAdr, int count, VarType varType, object value)
    {
        return new()
        {
            DataType = DataType.DataBlock,
            DB = db,
            StartByteAdr = startByteAdr,
            Count = count,
            VarType = varType,
            Value = value
        };
    }

    /// <summary>
    /// Parses a Siemens S7 address string into its component parts and converts the input value to the correct type.
    /// Supports DB, Input (I/E), Output (Q/A/O), Memory (M), Timer (T), and Counter (C/Z) address types.
    /// </summary>
    /// <param name="input">Siemens S7 address string (e.g., "DB1.DBX10.0", "IW20", "MB5")</param>
    /// <param name="dataType">Output: PLC data type category (DataBlock, Input, Output, Memory, Timer, Counter)</param>
    /// <param name="dbNumber">Output: Data block number (0 for non-DB addresses)</param>
    /// <param name="varType">Output: Specific variable type (Byte, Word, DInt, Real, etc.)</param>
    /// <param name="address">Output: Starting byte address (or timer/counter number for T/C addresses)</param>
    /// <param name="bitNumber">Output: Bit position (0-7) for bit-level addresses (-1 for non-bit addresses)</param>
    /// <param name="value">Reference: Input value to convert; updated with type-converted value matching the address type</param>
    /// <exception cref="InvalidAddressException">Thrown for invalid address format, missing separators, or out-of-range bit numbers</exception>
    public static void Parse(string input, out DataType dataType, out int dbNumber, out VarType varType, out int address, out int bitNumber, ref object value)
    {
        // Initialize output parameters with default values
        bitNumber = -1;
        dbNumber = 0;

        // Extract first two characters to identify address category
        switch (input.Substring(0, 2))
        {
            case "DB": // Data Block addresses (most complex - multiple sub-types)
                {
                    var strArray = input.Split('.');
                    if (strArray.Length < 2)
                    {
                        throw new InvalidAddressException("To few periods for DB address");
                    }
                    var readInfo = PlcFunction.ParseDbAddress(strArray);
                    dataType = DataType.DataBlock;
                    dbNumber = readInfo.Block;
                    address = readInfo.Address;

                    var context = new DtDataItem
                    {
                        Count  = dbNumber,
                        VarType = VarType.Byte,
                        Value = value
                    };

                    // Strategy pattern for DB address type-specific processing
                    var strategies = new (Regex regex, Action action)[]
                    {
                        (RegexManager.DbbAddressRegex(), () => {
                            if (strArray.Length == 3)
                            {
                                context.VarType = VarType.Byte;
                                int num2 = int.Parse(strArray[2]);
                                object obj2 = context.Value;
                                if (obj2 != null && obj2.ToString().Length > num2)
                                {
                                    context.Value = context.Value?.ToString().Substring(0, num2);
                                }
                                else
                                {
                                    context.Value = context.Value?.ToString().PadRight(num2, ' ');
                                }

                                string valueStr = context.Value?.ToString() ?? string.Empty;
                                context.Value = Encoding.ASCII.GetBytes(valueStr);
                            }
                            else
                            {
                                context.VarType = VarType.Byte;
                                context.Value = ConvertValue<byte>(context.Value);
                            }
                        }),
                        (RegexManager.DbwAddressRegex(), () => {
                            context.VarType = VarType.Word;
                            context.Value = ConvertValue<short>(context.Value);
                        }),
                        (RegexManager.UdbwAddressRegex(), () => {
                            context.VarType = VarType.Word;
                            context.Value = ConvertValue<ushort>(context.Value);
                        }),
                        (RegexManager.DbxAddressRegex(), () => {
                            context.Count = int.Parse(strArray[2]);
                            context.VarType = VarType.Bit;
                            context.Value = ConvertValue<bool>(context.Value);
                        }),
                        (RegexManager.SitAddressRegex(), () => {
                            var bytes = BitConverter.GetBytes((short)ConvertValue<sbyte>(context.Value));
                            context.VarType = VarType.Byte;
                            context.Value = bytes;
                        }),
                        (RegexManager.UsitAddressRegex(), () => {
                            context.VarType = VarType.Byte;
                            context.Value = ConvertValue<byte>(context.Value);
                        }),
                        (RegexManager.DitAddressRegex(), () => {
                            context.VarType = VarType.DInt;
                            context.Value = ConvertValue<int>(context.Value);
                        }),
                        (RegexManager.UditAddressRegex(), () => {
                            context.VarType = VarType.DWord;
                            context.Value = ConvertValue<uint>(context.Value);
                        }),
                        (RegexManager.DdwdAddressRegex(), () => {
                            context.VarType = VarType.DWord;
                            context.Value = PlcFunction.ParseHexDWord(context.Value, nameof(value));
                        }),
                        (RegexManager.DbbAddressRegex(), () => {
                            byte[] bytes = Encoding.ASCII.GetBytes((string)ConvertValue<string>(context.Value));
                            context.VarType = VarType.String;
                            context.Value = bytes;
                        }),
                        (RegexManager.DbfAddressRegex(), () => {
                            context.VarType = VarType.Real;
                            context.Value = ConvertValue<float>(context.Value);
                        }),
                        (RegexManager.DdbAddressRegex(), () => {
                            context.VarType = VarType.LReal;
                            context.Value = ConvertValue<double>(context.Value);
                        }),
                       (RegexManager.DbdAddressRegex(), () => {
                            context.VarType = VarType.DWord;
                            context.Value = ConvertValue<uint>(context.Value);
                        }),
                       (RegexManager.DwdAddressRegex(), () => {
                            context.VarType = VarType.Word;
                            context.Value = ConvertValue<ushort>(context.Value);
                        }),
                       (RegexManager.LwdAddressRegex(), () => {
                            var bValues = BitConverter.GetBytes(PlcFunction.ConvertValue<long>(context.Value, nameof(value)));
                            context.VarType = VarType.Byte;
                            context.Value =bValues;
                        }),
                       (RegexManager.DbtAddressRegex(), () => {
                            context.VarType = VarType.Byte;
                            context.Value = ConvertValue<byte>(context.Value);
                        }),
                        (RegexManager.DatAddressRegex(), () => {
                            context.VarType = VarType.DateTime;
                            context.Value =ConvertValue<DateTime>(context.Value);
                        })
                    };
                    //Execute the action after matching the traversed strategies
                    foreach (var strategy in strategies)
                    {
                        if (strategy.regex.IsMatch(input))
                        {
                            strategy.action();
                            break;
                        }
                    }

                    //Assign the value of the wrapper type back to the out/ref parameter.
                    bitNumber = context.Count;
                    varType = context.VarType;
                    value = context.Value;
                    return;
                }
        }

        // Handle text prefix addresses (bit-level: I, Q, M, T, C, Z)
        string text = input.Substring(0, 1);
        if (text != null)
        {
            int length = text.Length;
            if (length == 1)
            {
                string text2;
                switch (text[0])
                {
                    // Input Bit (I/E) - Boolean
                    case 'E':
                    case 'I':
                        dataType = DataType.Input;
                        varType = VarType.Bit;
                        value = ConvertValue<bool>(value);
                        goto IL_07c3;
                    // Output Bit (Q/A/O) - Boolean
                    case 'A':
                    case 'O':
                    case 'Q':
                        dataType = DataType.Output;
                        varType = VarType.Bit;
                        value = ConvertValue<bool>(value);
                        goto IL_07c3;
                    // Memory Bit (M) - Boolean
                    case 'M':
                        dataType = DataType.Memory;
                        varType = VarType.Bit;
                        value = ConvertValue<bool>(value);
                        goto IL_07c3;
                    // Timer (T) - Double precision value (time in seconds)
                    case 'T':
                        dataType = DataType.Timer;
                        dbNumber = 0;
                        address = int.Parse(input.Substring(1));
                        varType = VarType.Timer;
                        value = ConvertValue<double>(value);
                        return;
                    // Counter (C/Z) - 16-bit unsigned integer
                    case 'C':
                    case 'Z':
                        {
                            dataType = DataType.Counter;
                            dbNumber = 0;
                            address = int.Parse(input.Substring(1));
                            varType = VarType.Counter;
                            value = ConvertValue<ushort>(value);
                            return;
                        }

                    IL_07c3:
                        text2 = input.Substring(1);
                        if (text2.IndexOf('.') < 0)
                            throw new InvalidAddressException("To few periods for DB address");

                        address = int.Parse(text2.Substring(0, text2.IndexOf('.')));
                        bitNumber = int.Parse(text2.Substring(text2.IndexOf('.') + 1));
                        if (bitNumber > 7)
                        {
                            throw new InvalidAddressException("Bit can only be 0-7");
                        }

                        return;
                }
            }
        }

        throw new InvalidAddressException($"{input.Substring(0, 1)} is not a valid address");
    }

    /// <summary>
    /// Converts an input object to the specified target type T, supporting single values, arrays, and lists.
    /// Handles type conversion for PLC data types (Byte, Int16, Int32, Float, etc.).
    /// </summary>
    /// <typeparam name="T">Target type to convert to (e.g., byte, short, int, float)</typeparam>
    /// <param name="inputArray">Input value/collection to convert (single value, array, or IList)</param>
    /// <returns>Converted value of type T (or array of T for collections)</returns>
    /// <exception cref="InvalidCastException">Thrown if conversion to target type T is not possible</exception>
    private static   object ConvertValue<T>(object inputArray)
    {
        // Return immediately if already correct type (single value or array)
        if (inputArray is T || inputArray is T[])
        {
            return inputArray;
        }
        // Convert System.Array to strongly-typed array of T
        if (inputArray is Array array)
        {
            T[] array2 = new T[array.Length];
            int num = 0;
            {
                foreach (object item in array)
                {
                    array2[num] = (T)Convert.ChangeType(item, typeof(T));
                    num++;
                }

                return array2;
            }
        }
        // Convert IList to strongly-typed array of T
        if (inputArray is IList list)
        {
            T[] array3 = new T[list.Count];
            int num2 = 0;
            {
                foreach (object item2 in list)
                {
                    array3[num2] = (T)Convert.ChangeType(item2, typeof(T));
                    num2++;
                }

                return array3;
            }
        }
        // Convert single value to type T
        return (T)Convert.ChangeType(inputArray, typeof(T));
    }

}
