using System.Collections.Concurrent;
using S7.Net;
using S7.Net.Types;
using MyS7Core.SiemensEthernet;

namespace MyS7Core.Wrapper;

/// <summary>In-memory PLC transport. Memory is shared by IP and retained until the process exits.</summary>
internal sealed class OfflinePlc : IPlc
{
    private sealed class Memory
    {
        internal readonly object Sync = new();
        internal readonly Dictionary<(DataType Area, int Db, int Address), byte> Bytes = new();
    }

    private static readonly ConcurrentDictionary<string, Memory> Memories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Memory _memory;
    public bool IsConnected { get; private set; }

    internal OfflinePlc(string ipAddress) => _memory = Memories.GetOrAdd(ipAddress, _ => new Memory());
    public void Open() => IsConnected = true;
    public void Close() => IsConnected = false;
    public Task OpenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Open();
        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (!IsConnected) throw new InvalidOperationException("Offline PLC is not connected.");
    }

    public byte[] ReadBytes(DataType dataType, int db, int startByteAdr, int count)
    {
        EnsureConnected();
        if (startByteAdr < 0 || count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        var bytes = new byte[count];
        lock (_memory.Sync)
            for (int i = 0; i < count; i++)
                _memory.Bytes.TryGetValue((dataType, db, checked(startByteAdr + i)), out bytes[i]);
        return bytes;
    }

    public void WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
    {
        EnsureConnected();
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (startByteAdr < 0) throw new ArgumentOutOfRangeException(nameof(startByteAdr));
        lock (_memory.Sync)
            for (int i = 0; i < value.Length; i++)
                _memory.Bytes[(dataType, db, checked(startByteAdr + i))] = value[i];
    }

    public object? Read(string address)
    {
        var item = PlcAddress.GetDataItem(address) ?? throw new ArgumentException("Invalid address", nameof(address));
        return Read(item.DataType, item.DB, item.StartByteAdr, item.VarType, item.Count, item.BitAdr);
    }

    public object? Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
    {
        if (varCount < 1) throw new ArgumentOutOfRangeException(nameof(varCount));
        if (varType == VarType.Bit)
        {
            if (bitAdr > 7) throw new ArgumentOutOfRangeException(nameof(bitAdr));
            return (ReadBytes(dataType, db, startByteAdr, 1)[0] & (1 << bitAdr)) != 0;
        }
        var bytes = ReadBytes(dataType, db, startByteAdr, PlcFunction.VarTypeToByteLength(varType, varCount));
        if (varType == VarType.String) return System.Text.Encoding.ASCII.GetString(bytes);
        if (varType == VarType.S7String) return S7String.FromByteArray(bytes);
        if (varType == VarType.S7WString) return S7WString.FromByteArray(bytes);
        object Decode(byte[] b) => varType switch
        {
            VarType.Byte => b[0],
            VarType.Int => Int.FromByteArray(b),
            VarType.Word => Word.FromByteArray(b),
            VarType.DInt => DInt.FromByteArray(b),
            VarType.DWord => DWord.FromByteArray(b),
            VarType.Real => Real.FromByteArray(b),
            VarType.LReal => LReal.FromByteArray(b),
            VarType.DateTime => b.All(x => x == 0) ? System.DateTime.MinValue : S7.Net.Types.DateTime.FromByteArray(b),
            VarType.DateTimeLong => b.All(x => x == 0) ? System.DateTime.MinValue : DateTimeLong.FromByteArray(b),
            VarType.Counter => Counter.FromByteArray(b),
            VarType.Timer => S7.Net.Types.Timer.FromByteArray(b),
            _ => throw new NotSupportedException($"Unsupported offline type: {varType}")
        };
        if (varCount == 1) return Decode(bytes);
        int size = bytes.Length / varCount;
        var result = Array.CreateInstance(Decode(bytes.Take(size).ToArray()).GetType(), varCount);
        for (int i = 0; i < varCount; i++) result.SetValue(Decode(bytes.Skip(i * size).Take(size).ToArray()), i);
        return result;
    }

    public void Write(string address, object value)
    {
        var item = PlcAddress.GetDataItem(address) ?? throw new ArgumentException("Invalid address", nameof(address));
        Write(item.DataType, item.DB, item.StartByteAdr, value, item.VarType == VarType.Bit ? item.BitAdr : -1);
    }

    public void Write(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1)
    {
        if (bitAdr == -1)
        {
            WriteBytes(dataType, db, startByteAdr, Serialization.SerializeValue(value));
            return;
        }
        if (bitAdr < 0 || bitAdr > 7) throw new ArgumentOutOfRangeException(nameof(bitAdr));
        lock (_memory.Sync)
        {
            var bytes = ReadBytes(dataType, db, startByteAdr, 1);
            bytes[0] = Convert.ToBoolean(value) ? (byte)(bytes[0] | (1 << bitAdr)) : (byte)(bytes[0] & ~(1 << bitAdr));
            WriteBytes(dataType, db, startByteAdr, bytes);
        }
    }

    public void ReadMultipleVars(List<DataItem> dataItems)
    {
        foreach (var item in dataItems)
            item.Value = Read(item.DataType, item.DB, item.StartByteAdr, item.VarType, item.Count, item.BitAdr);
    }
    public int ReadClass(object sourceClass, int db, int startByteAdr = 0)
    {
        int size = (int)Class.GetClassSize(sourceClass);
        Class.FromBytes(sourceClass, ReadBytes(DataType.DataBlock, db, startByteAdr, size));
        return size;
    }
    public void WriteClass(object sourceClass, int db, int startByteAdr = 0)
    {
        var bytes = new byte[(int)Class.GetClassSize(sourceClass)];
        Class.ToBytes(sourceClass, bytes);
        WriteBytes(DataType.DataBlock, db, startByteAdr, bytes);
    }
    public object? ReadStruct(Type structType, int db, int startByteAdr = 0) =>
        Struct.FromBytes(structType, ReadBytes(DataType.DataBlock, db, startByteAdr, (int)Struct.GetStructSize(structType)));

    public Task<object?> ReadAsync(string variable, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Read(variable));
    }
    public Task<object?> ReadAsync(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Read(dataType, db, startByteAdr, varType, varCount, bitAdr));
    }
    public Task<byte[]> ReadBytesAsync(DataType dataType, int db, int startByteAdr, int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ReadBytes(dataType, db, startByteAdr, count));
    }
    public Task WriteAsync(string variable, object value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Write(variable, value);
        return Task.CompletedTask;
    }
    public Task WriteAsync(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Write(dataType, db, startByteAdr, value, bitAdr);
        return Task.CompletedTask;
    }
    public Task WriteBytesAsync(DataType dataType, int db, int startByteAdr, byte[] value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        WriteBytes(dataType, db, startByteAdr, value);
        return Task.CompletedTask;
    }
}

