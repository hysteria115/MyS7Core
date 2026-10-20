using S7.Net;
using S7.Net.Types;
using System;

namespace MyS7Core
{
    /// <summary>
    /// Provides serialization methods for S7 PLC data items and values
    /// Converts .NET types to byte arrays compatible with S7.Net library
    /// </summary>
    internal static class Serialization
    {
        /// <summary>
        /// Serializes DtDataItem to byte array for PLC communication
        /// </summary>
        /// <param name="dataItem">PLC data item with value and type info</param>
        /// <returns>Byte array in S7 PLC protocol format</returns>
        /// <exception cref="Exception">Thrown when dataItem.Value is null</exception>
        public static byte[] SerializeDataItem(DtDataItem dataItem)
        {
            // Check for null value (cannot serialize null)
            if (dataItem.Value == null)
            {
                throw new Exception($"DataItem.Value is null, cannot serialize. StartAddr={dataItem.StartByteAdr} VarType={dataItem.VarType}");
            }

            // Handle string values with specific S7 string type serialization
            if (dataItem.Value is string value)
            {
                return dataItem.VarType switch
                {
                    VarType.S7String => S7String.ToByteArray(value, dataItem.Count),
                    VarType.S7WString => S7WString.ToByteArray(value, dataItem.Count),
                    _ => global::S7.Net.Types.String.ToByteArray(value, dataItem.Count),
                };
            }
            // Serialize non-string values
            return SerializeValue(dataItem.Value);
        }

        /// <summary>
        /// Serializes single value/array to S7 PLC byte array
        /// </summary>
        /// <param name="value">Value to serialize (basic type or array)</param>
        /// <returns>Byte array in S7 format</returns>
        /// <exception cref="InvalidVariableTypeException">Thrown for unsupported types</exception>
        public static byte[] SerializeValue(object value)
        {
            // Serialize based on type name (supports basic types and arrays)
            switch (value.GetType().Name)
            {
                case "Boolean":
                    return new byte[1] { (byte)(((bool)value) ? 1u : 0u) };
                case "Byte":
                    return global::S7.Net.Types.Byte.ToByteArray((byte)value);
                case "Int16":
                    return Int.ToByteArray((short)value);
                case "UInt16":
                    return Word.ToByteArray((ushort)value);
                case "Int32":
                    return DInt.ToByteArray((int)value);
                case "UInt32":
                    return DWord.ToByteArray((uint)value);
                case "Single":
                    return Real.ToByteArray((float)value);
                case "Double":
                    return LReal.ToByteArray((double)value);
                case "DateTime":
                    return global::S7.Net.Types.DateTime.ToByteArray((System.DateTime)value);
                case "Byte[]":
                    return (byte[])value;
                case "Int16[]":
                    return Int.ToByteArray((short[])value);
                case "UInt16[]":
                    return Word.ToByteArray((ushort[])value);
                case "Int32[]":
                    return DInt.ToByteArray((int[])value);
                case "UInt32[]":
                    return DWord.ToByteArray((uint[])value);
                case "Single[]":
                    return Real.ToByteArray((float[])value);
                case "Double[]":
                    return LReal.ToByteArray((double[])value);
                case "String":
                    {
                        string obj = (string)value;
                        return global::S7.Net.Types.String.ToByteArray(obj, obj.Length);
                    }
                case "DateTime[]":
                    return global::S7.Net.Types.DateTime.ToByteArray((System.DateTime[])value);
                case "DateTimeLong[]":
                    return DateTimeLong.ToByteArray((System.DateTime[])value);
                default:
                    throw new InvalidVariableTypeException();
            }
        }
    }
}
