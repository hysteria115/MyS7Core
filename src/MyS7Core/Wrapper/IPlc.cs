using S7.Net;
using S7.Net.Types;
using System.Collections;

namespace MyS7Core.Wrapper;

public interface IPlc
{
    void Close();
    bool IsConnected { get; }
    void Open();
    Task OpenAsync(CancellationToken cancellationToken);
    object? Read(string address);
    object? Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0);
    Task<object?> ReadAsync(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0, CancellationToken cancellationToken = default(CancellationToken));
    Task<object?> ReadAsync(string variable, CancellationToken cancellationToken = default(CancellationToken));
    byte[] ReadBytes(DataType dataType, int db, int startByteAdr, int count);
    Task<byte[]> ReadBytesAsync(DataType dataType, int db, int startByteAdr, int count, CancellationToken cancellationToken = default(CancellationToken));
    int ReadClass(object sourceClass, int db, int startByteAdr = 0);
    void WriteClass(object sourceClass, int db, int startByteAdr = 0);
    void ReadMultipleVars(List<DataItem> dataItems);
    object? ReadStruct(Type structType, int db, int startByteAdr = 0);
    void Write(string address, object value);
    void Write(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1);
    void WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value);
    Task WriteAsync(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1, CancellationToken cancellationToken = default(CancellationToken));
    Task WriteAsync(string variable, object value, CancellationToken cancellationToken = default(CancellationToken));
    Task WriteBytesAsync(DataType dataType, int db, int startByteAdr, byte[] value, CancellationToken cancellationToken = default(CancellationToken));
}
