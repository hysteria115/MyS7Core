using S7.Net;
using S7.Net.Types;
using MyS7Core.SiemensEthernet;
using MyS7Core.Wrapper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DateTime = System.DateTime;

namespace MyS7Core;

public delegate void PlcDataChangedEventHandler(MonitorAddress monitorAddress, object value);

public delegate void PlcErrorOccurredEventHandler(string message, MonitorRuntime monitorRuntime);

public delegate void PlcMonitorTimeEventHandler(string name, long time);

public class PlcFunction : IDisposable
{
    private static readonly SemaphoreSlim _plcLock = new(1, 1);
    private bool _disposed;
    private bool _isMonitorRunning;
    private IPlc? _plc;


    public PlcFunction(string ipAddress)
    {
        if (!System.Net.IPAddress.TryParse(ipAddress, out _))
            throw new ArgumentException("Invalid IP address format", nameof(ipAddress));
        IPAddress = ipAddress;
    }

    public PlcFunction(string ipAddress, short cpuslot, bool offlineTest)
    {
        if (!System.Net.IPAddress.TryParse(ipAddress, out _))
            throw new ArgumentException("Invalid IP address format", nameof(ipAddress));
        IPAddress = ipAddress;
        CpuSlot = cpuslot;
        Offline = offlineTest;
    }

    // Constructor for offline testing
    internal PlcFunction(string ipAddress, IPlc plc)
    {
        if (!System.Net.IPAddress.TryParse(ipAddress, out _))
            throw new ArgumentException("Invalid IP address format", nameof(ipAddress));
        IPAddress = ipAddress;
        _plc = plc;
    }

    public string IPAddress { get; set; }
    public int ConnectTimeout { get; set; } = ConstClass.DefaultConnectTimeout;
    public int RackAddress { get; set; } = ConstClass.DefaultRackAddress;
    public short RackNumber { get; set; }
    public short CpuSlot { get; set; } = ConstClass.DefaultCpuSlot;
    public int MaxPduItems { get; set; } = ConstClass.DefaultMaxPduItems;
    public int MaxPduBytes { get; set; } = ConstClass.DefaultMaxPduBytes;
    public DataType DataType { get; set; } = DataType.DataBlock;
    public CpuType CpuType { get; set; } = CpuType.S71500;
    public bool Offline { get; set; }
    public bool IsConnected { get; set; }
    public int ThreadCount { get; set; } = ConstClass.DefaultThreadCount;
    public int ThreadSleep { get; set; } = ConstClass.DefaultThreadSleep;
    public int UpdateRate { get; set; }
    public int PlcNetPort { get; set; } = ConstClass.DefaultPlcNetPort;
    public int SubTwo { get; set; } = ConstClass.DefaultSubTwo;
    public int SubThree { get; set; } = ConstClass.DefaultSubThree;
    public int SubFour { get; set; } = ConstClass.DefaultSubFour;
    public int SubFive { get; set; } = ConstClass.DefaultSubFive;
    public int SubSix { get; set; } = ConstClass.DefaultSubSix;
    public int SubSeven { get; set; } = ConstClass.DefaultSubSeven;
    public int SubEight { get; set; } = ConstClass.DefaultSubEight;
    public int Subdivision { get; set; } = ConstClass.DefaultSubdivision;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        lock (_plcLock)
        {
            if (disposing)
            {
                // Release managed resources
                _isMonitorRunning = false;
                _plc?.Close();
            }
            // Release unmanaged resources
            _disposed = true;
        }
    }

    ~PlcFunction()
    {
        Dispose(false);
    }

    public event PlcDataChangedEventHandler PlcDataChangedEvent = delegate { };
    public event PlcErrorOccurredEventHandler PlcErrorOccurredEvent = delegate { };
    public event PlcMonitorTimeEventHandler PlcMonitorTimeEvent = delegate { };

    public void DisMonitor()
    {
        _isMonitorRunning = false;
    }

    /// <summary>
    ///     Connect
    ///     Establish connection with PLC
    /// </summary>
    public void Connect()
    {
        try
        {
            if (Offline)
            {
                IsConnected = true;
            }
            else
            {
                _plc = new PlcWrapper(CpuType, IPAddress, RackNumber, CpuSlot);
                _plc.Open();
                Thread.Sleep(ThreadSleep);
                IsConnected = _plc.IsConnected;
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"Could not connect to PLC：{IPAddress}:RackNumber:{RackNumber},CpuSlot:{CpuSlot},Message:{ex}");
        }
    }

    /// <summary>
    /// Establish an asynchronous connection with the PLC
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        // 主动校验取消令牌
        // Actively verify the cancellation token
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (Offline)
            {
                IsConnected = true;
                return;
            }

            // 每次连接创建新的PlcWrapper，防止旧实例残留
            // Create a new PlcWrapper for each connection to prevent old instances from remaining.
            _plc = new PlcWrapper(CpuType, IPAddress, RackNumber, CpuSlot);
            await _plc.OpenAsync(cancellationToken);
            await Task.Delay(ThreadSleep, cancellationToken);
            IsConnected = _plc.IsConnected;

            if (!IsConnected)
                throw new InvalidOperationException("PLC OpenAsync success but IsConnected is false");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex2)
        {
            IsConnected = false;
            throw new InvalidOperationException($"Could not connect to PLC：{IPAddress}, RackNumber:{RackNumber}, CpuSlot:{CpuSlot}", ex2);
        }
    }

    /// <summary>
    ///     Disconnect
    ///     Disconnect from the PLC
    /// </summary>
    public void Disconnect()
    {
        try
        {
            _isMonitorRunning = false;
            if (_plc != null)
                _plc.Close();
            IsConnected = false;
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"Could not Disconnect to PLC：{IPAddress}:RackNumber:{RackNumber},CpuSlot:{CpuSlot},Message:{ex}");
        }
    }

    /// <summary>
    ///     Monitor
    ///     Loop through the incoming PLC addresses, 
    ///     compare each read value with the previous one, 
    ///     and execute DataChangedEvent when they are different.
    /// </summary>
    /// <param name="maList"></param>
    public async Task MonitorAsync(List<MonitorAddress> maList, CancellationToken cancellationToken = default)
    {
        // 主动校验取消 Active verification canceled
        cancellationToken.ThrowIfCancellationRequested();
        if (ThreadCount <= 0)
        {
            throw new ArgumentException("The PLC threads cannot be 0!");
        }

        if (maList == null || maList.Count == 0)
        {
            throw new ArgumentException("Monitor address list cannot be null or empty.", nameof(maList));
        }

        Dictionary<int, MonitorRuntime> dicMonitorRuntime = new Dictionary<int, MonitorRuntime>();
        for (int i = 0; i < ThreadCount; i++)
        {
            dicMonitorRuntime.Add(i, new MonitorRuntime());
        }

        int cursor = 0;
        foreach (MonitorAddress address in maList)
        {
            dicMonitorRuntime[cursor].AddressList.Add(address);
            dicMonitorRuntime[cursor].UpdateRate = UpdateRate;
            MonitorRuntime monitorRuntime = dicMonitorRuntime[cursor];
            monitorRuntime.Key = monitorRuntime.Key + address.Address + "|";
            cursor = (cursor != ThreadCount - 1) ? cursor + 1 : 0;
            // 监控地址时校验取消
            // Cancel verification when monitoring address
            cancellationToken.ThrowIfCancellationRequested();
        }

        _isMonitorRunning = true;
        // 将取消令牌传入实际的监控执行方法
        // Pass the cancellation token to the actual monitoring execution method
        await RunMonitorTasks(dicMonitorRuntime, cancellationToken);
    }

    /// <summary>
    /// Start monitoring tasks for all MonitorRuntime entries
    /// </summary>
    private async Task RunMonitorTasks(Dictionary<int, MonitorRuntime> dicMonitorRuntime, CancellationToken cancellationToken)
    {
        var monitorTasks = new List<Task>();
        foreach (var kvp in dicMonitorRuntime)
        {
            var runtime = kvp.Value;
            monitorTasks.Add(Task.Run(async () =>
            {
                // 为每个监控任务维护地址值缓存，用于值变化时才触发事件
                // Maintain an address value cache for each monitoring task, so that events are only triggered when the value changes.
                var lastValueCache = new Dictionary<MonitorAddress, object>();
                foreach (var addr in runtime.AddressList)
                {
                    lastValueCache[addr] = null;
                }

                while (!cancellationToken.IsCancellationRequested && _isMonitorRunning && IsConnected)
                {
                    try
                    {
                        foreach (var addr in runtime.AddressList)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var data = await ReadAsync(addr.Address);
                            // 仅当值发生变化时，才触发事件（解决重复信号问题）
                            // The event is triggered only when the value changes (to solve the problem of repeated signals).
                            if (!Equals(data, lastValueCache[addr]))
                            {
                                lastValueCache[addr] = data;
                                PlcDataChangedEvent?.Invoke(addr, data);
                            }
                        }
                        await Task.Delay(runtime.UpdateRate, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        PlcErrorOccurredEvent?.Invoke($"Monitor error: {ex.Message}", runtime);
                        await Task.Delay(ThreadSleep, cancellationToken);
                    }
                }
            }, cancellationToken));
        }
        await Task.WhenAll(monitorTasks);
        // 监控任务终止后，更新状态
        // Update status after monitoring task terminates
        _isMonitorRunning = false;
    }

    /// <summary>
    /// Handle monitoring logic for a single MonitorRuntime
    /// </summary>
    private async Task ProcessMonitorRuntime(MonitorRuntime mr)
    {
        try
        {
            // Create and connect to the PLC
            using var plcInfo = await CreateAndConnectPlcAsync();

            var values = new object[mr.AddressList.Count];
            var sw = new Stopwatch();

            // Loop monitoring until stopped
            while (_isMonitorRunning)
            {
                await MonitorAddresses(mr, plcInfo, values, sw);
            }

            // Disconnect when monitoring ends
            plcInfo.Disconnect();
        }
        catch (Exception ex)
        {
            // Handle exception and trigger error event
            IsConnected = false;
            PlcErrorOccurredEvent?.Invoke(ex.Message, mr);
        }
    }

    /// <summary>
    /// Create and connect to PLC
    /// </summary>
    private async Task<PlcFunction> CreateAndConnectPlcAsync()
    {
        var plcInfo = new PlcFunction(IPAddress, CpuSlot, Offline)
        {
            DataType = DataType,
            RackAddress = RackAddress,
            RackNumber = RackNumber
        };
        await plcInfo.ConnectAsync();
        return plcInfo;
    }

    /// <summary>
    /// Monitor all addresses and record execution time
    /// </summary>
    private async Task MonitorAddresses(MonitorRuntime mr, PlcFunction plcInfo, object[] values, Stopwatch sw)
    {
        sw.Restart();

        // Iterate through all monitored addresses
        for (var i = 0; i < mr.AddressList.Count; i++)
        {
            await HandleAddressRead(mr, plcInfo, values, i);
        }

        sw.Stop();
        // Trigger monitoring time event
        PlcMonitorTimeEvent?.Invoke(Thread.CurrentThread.Name ?? "MonitorTask", sw.ElapsedMilliseconds);
        // Wait before starting the next monitoring cycle
        await Task.Delay(ThreadSleep);
    }

    /// <summary>
    /// Read value from a single address and trigger change/error events
    /// </summary>
    private async Task HandleAddressRead(MonitorRuntime mr, PlcFunction plcInfo, object[] values, int index)
    {
        try
        {
            // Read data from PLC
            var value = await plcInfo.ReadAsync(mr.AddressList[index].Address);
            if (value == null) return;

            // Trigger data changed event if value is different
            if (!Equals(value, values[index]))
            {
                PlcDataChangedEvent?.Invoke(mr.AddressList[index], value);
                values[index] = value;
            }
        }
        catch (Exception ex)
        {
            // Handle exception for single address without affecting others
            Console.WriteLine(ex);
            PlcErrorOccurredEvent?.Invoke(mr.AddressList[index].Address + " Read Exception: " + ex.Message, mr);

            // Delay to avoid retrying too quickly
            await Task.Delay(ThreadSleep);
        }
    }


    /// <summary>
    /// Read the PLC values for address
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<object?> ReadAsync(string address)
    {

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("The address cannot be null or an empty string", nameof(address));

        if (!address.Contains('.'))
            throw new ArgumentException("Invalid address format, must include separator '.'", nameof(address));

        if (_plc == null)
            throw new InvalidOperationException("PLC is not connected. Please connect before reading.");

        try
        {
            var strArray = address.Split('.');
            var readInfo = ParseDbAddress(strArray);

            var strategies = new (Regex regex, Func<Task<object?>> action)[]
        {
            (RegexManager.DbxAddressRegex(), () => _plc.ReadAsync(address)),
            (RegexManager.DbwAddressRegex(), () => _plc.ReadAsync(address)),
            (RegexManager.UdbwAddressRegex(), async () =>
            {
                int start = int.Parse(strArray[1].AsSpan(SubFour), CultureInfo.InvariantCulture);
                var v = await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, start, VarType.Word, 1);
                return (ushort)(v ?? 0);
            }),

             (RegexManager.SitAddressRegex(), async () =>
            {
                var v = await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.Byte, 1);
                return (sbyte)(byte)(v ?? 0);
            }),

              (RegexManager.UsitAddressRegex(), async () =>
            {
                int start = int.Parse(strArray[1].AsSpan(SubFour), CultureInfo.InvariantCulture);
                var v = await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, start, VarType.Byte, 1);
                return (byte)(v ?? 0);
            }),

              (RegexManager.DitAddressRegex(), async () =>
            {
                var v = await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.DInt, 1);
                return (int)(v ?? 0);
            }),

              (RegexManager.UditAddressRegex(), async () =>
            {
                int start = int.Parse(strArray[1].AsSpan(SubFour), CultureInfo.InvariantCulture);
                var v = await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, start, VarType.DWord, 1);
                return (uint)(v ?? 0);
            }),

              (RegexManager.DdwdAddressRegex(), async () =>
            {
                int start = int.Parse(strArray[1].AsSpan(SubFour), CultureInfo.InvariantCulture);
                var v = await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, start, VarType.DWord, 1);
                return FormatHexDWord((uint)(v ?? 0));
            }),

              (RegexManager.DbbAddressRegex(), async () =>
            {
                int len = int.Parse(strArray[SubTwo], CultureInfo.InvariantCulture);
                var v = await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.String, len);
                return v?.ToString()?.TrimEnd('\0');
            }),

              (RegexManager.DbsAddressRegex(), async () =>
            {
                int len = int.Parse(strArray[SubTwo], CultureInfo.InvariantCulture);
                return await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address + SubTwo, VarType.String, len - SubTwo);
            }),

            (RegexManager.DbfAddressRegex(), () => _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.Real, 1)),
            (RegexManager.DdbAddressRegex(), () => _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.LReal, 1)),
            (RegexManager.DbdAddressRegex(), () => _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.DWord, 1)),
            (RegexManager.DwdAddressRegex(), () => _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.Word, 1)),
            (RegexManager.LwdAddressRegex(), async () =>
            {
                var b = await _plc.ReadBytesAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, 8);
                return BitConverter.ToInt64(b, 0);
            }),
            (RegexManager.DbtAddressRegex(), () => _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.Byte, 1)),
            (RegexManager.DtlAddressRegex(), () => _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.DateTimeLong, 1)),
            (RegexManager.DatAddressRegex(), async () =>
            {
                try
                {
                    return await _plc.ReadAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, VarType.DateTime, 1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return Convert.ToDateTime(ConstClass.DefaultDateTime, CultureInfo.InvariantCulture);
                }
            }),
            (RegexManager.LdtAddressRegex(), async () =>
            {
                var bias = ConstClass.Bias;
                var bArrLDT = await _plc.ReadBytesAsync(DataType.DataBlock, readInfo.Block, readInfo.Address, SubEight);
                long Result;
                //Siemens PLC uses big-endian order
                Result = bArrLDT[0];
                Result <<= SubEight;
                Result += bArrLDT[1];
                Result <<= SubEight;
                Result += bArrLDT[SubTwo];
                Result <<= SubEight;
                Result += bArrLDT[SubThree];
                Result <<= SubEight;
                Result += bArrLDT[SubFour];
                Result <<= SubEight;
                Result += bArrLDT[SubFive];
                Result <<= SubEight;
                Result += bArrLDT[SubSix];
                Result <<= SubEight;
                Result += bArrLDT[SubSeven];
                try
                {
                    // Add nanosecond part
                    return new DateTime(Result / Subdivision + bias, DateTimeKind.Utc);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return DateTime.MinValue; // invalid date
                }
            })
        };

            foreach (var (regex, action) in strategies)
            {
                if (regex.IsMatch(address))
                    return await action();
            }
            throw new NotSupportedException("Unrecognisable or unrealised addresses:" + address);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"An exception occurred in the read {address} :", ex);
        }
    }


    /// <summary>
    ///   ReadGroup
    ///   Batch read PLC addresses and return the read set
    /// </summary>
    /// <param name="addressList">List collection of DB addresses</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> ReadGroup(List<string> addressList)
    {
        var result = new ConcurrentDictionary<string, object>();
        var otheraddressList = new List<string>();

        if (addressList == null || addressList.Count == 0 ||_plc == null)
            return new Dictionary<string, object>(result);

        foreach (var item in addressList)
            if (RegexManager.LdtAddressRegex().IsMatch(item)
                || RegexManager.LwdAddressRegex().IsMatch(item)
                || RegexManager.DdwdAddressRegex().IsMatch(item)
                || RegexManager.DatAddressRegex().IsMatch(item))
                result.TryAdd(item, (await ReadAsync(item)));
            else
                otheraddressList.Add(item);

        var groupedAddress = otheraddressList.Select((item, index) => new { item, index })
            .GroupBy(x => x.index / 30)
            .Select(g => g.Select(x => x.item).ToList())
            .ToList();

        await Task.WhenAll(groupedAddress.Select(item => Task.Factory.StartNew(() =>
        {
            var dataItemsRead = item.Select(PlcAddress.GetDataItem).ToList();

            _plc?.ReadMultipleVars(dataItemsRead);
            for (var i = 0; i < item.Count; i++)
            {
                var value = dataItemsRead[i]?.Value;
                if (value != null)
                {
                    result.TryAdd(item[i], value);
                }
            }
        })).ToArray());

        return new Dictionary<string, object>(result);
    }

    /// <summary>
    /// Read entity class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tInfo"></param>
    /// <param name="db">DB block</param>
    /// <param name="startAddr">starting address</param>
    /// <returns></returns>
    public int? ReadClass<T>(T tInfo, int db, int startAddr)
    {
        return _plc?.ReadClass(tInfo, db, startAddr);
    }

    /// <summary>
    /// Reads raw bytes from a PLC data block.
    /// </summary>
    /// <param name="db">PLC data block number</param>
    /// <param name="startAddr">Start byte address in the data block</param>
    /// <param name="count">Number of bytes to read</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<byte[]> ReadBytesAsync(int db, int startAddr , int count, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (_plc == null)
            throw new InvalidOperationException("PLC is not connected.");

        return await _plc.ReadBytesAsync(DataType.DataBlock, db, startAddr, count, cancellationToken);
    }

    /// <summary>
    /// Writes raw bytes to a PLC data block.
    /// </summary>
    /// <param name="db">PLC data block number</param>
    /// <param name="startAddr">Start byte address in the data block</param>
    /// <param name="source">Data to write</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WriteBytesAsync(int db, int startAddr, byte[] source, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (_plc == null)
            throw new InvalidOperationException("PLC is not connected.");

        await _plc.WriteBytesAsync(DataType.DataBlock, db, startAddr, source, cancellationToken);
    }

    /// <summary>
    /// Writes raw bytes to a PLC data block.
    /// </summary>
    /// <param name="db">PLC data block number</param>
    /// <param name="startAddr">Start byte address in the data block</param>
    /// <param name="data">Data to write</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WriteLargeBytesAsync(int db, int startAddr, byte[] data, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (_plc == null)
            throw new InvalidOperationException("PLC is not connected.");

        if (data == null || data.Length == 0)
            throw new ArgumentException("Data to write cannot be null or empty.", nameof(data));

        for (int offset = 0; offset < data.Length; offset += MaxPduBytes)
        {
            int len = Math.Min(MaxPduBytes, data.Length - offset);

            byte[] chunk = new byte[len];
            Array.Copy(data, offset, chunk, 0, len);

            await _plc.WriteBytesAsync(DataType.DataBlock,db,startAddr + offset,chunk, cancellationToken);
        }
    }

    /// <summary>
    /// Write entity class
    /// </summary>
    /// <param name="source">Class T</param>
    /// <param name="db">DB block</param>
    /// <param name="startAddr">starting address</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task WriteClassAsync(object source, int db, int startAddr)
    {
        if (_plc == null)
            throw new InvalidOperationException("PLC is not connected. Please connect before writing.");

        if (source == null)
            throw new ArgumentNullException(nameof(source));

        try
        {
            byte[] array = new byte[(int)Class.GetClassSize(source)];
            Class.ToBytes(source, array);
            await _plc.WriteBytesAsync(DataType.DataBlock, db, startAddr, array);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"write class exceptions: {ex}", nameof(source));
        }
    }

    /// <summary>
    /// Read the input structure
    /// </summary>
    /// <param name="tInfo"></param>
    /// <param name="db">DB block</param>
    /// <param name="startAddr">starting address</param>
    /// <returns></returns>
    public object? ReadStruct(Type tInfo, int db, int startAddr)
    {
        return _plc?.ReadStruct(tInfo, db, startAddr);
    }

    /// <summary>
    /// ReadDictionary
    /// The key of the incoming dictionary represents the attribute, and the value is the DB address. 
    /// The key of the returned dictionary is the attribute, and the value is the value read from the DB address, 
    /// which can be of various types such as String, bool, etc.
    /// </summary>
    /// <param name="keyAddr">Dictionary<string, string></param>
    /// <returns>Dictionary<string, object></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Dictionary<string, object>> ReadDictionary(Dictionary<string, string> keyAddr)
    {
        try
        {
            var addressList = keyAddr.Select(item => item.Value).ToList();
            addressList = addressList.Distinct().ToList();
            var dicValues = await ReadGroup(addressList);
            return keyAddr.ToDictionary(item => item.Key, item => dicValues[item.Value]);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("An exception occurred in the read:", ex);
        }
    }

    public bool WriteGroup(Dictionary<string, object> keyValues)
    {
        if (_plc == null)
            throw new InvalidOperationException("PLC is not connected. Please connect before writing.");

        ArgumentNullException.ThrowIfNull(keyValues);

        foreach (IGrouping<DataType, DtDataItem> group in from n in keyValues
                                                        select DtDataItem.FromAddress(n.Key, n.Value) into n
                                                        group n by n.DataType)
        {
            List<List<DtDataItem>> list = new List<List<DtDataItem>>();
            Dictionary<int, List<DtDataItem>> dictionary = (from n in @group
                                                          group n by n.DB).ToDictionary((IGrouping<int, DtDataItem> n) => n.Key, (IGrouping<int, DtDataItem> j) => j.ToList());
            
            foreach (KeyValuePair<int, List<DtDataItem>> item in dictionary)
            {
                DtDataItem[] array = item.Value.OrderBy((DtDataItem n) => n.StartByteAdr).ToArray();
                List<List<DtDataItem>> list2 = new List<List<DtDataItem>>
                {
                    new List<DtDataItem> { array[0] }
                };
                for (int i = 1; i < array.Length; i++)
                {
                    if (array[i].StartByteAdr - list2[list2.Count - 1][0]?.StartByteAdr < MaxPduBytes - 18)
                    {
                        list2[list2.Count - 1].Add(array[i]);
                        continue;
                    }

                    list2.Add(new List<DtDataItem>());
                    list2[list2.Count - 1].Add(array[i]);
                }

                list.AddRange(list2);
            }

            list.ForEach(delegate (List<DtDataItem> n)
            {
                int dB = n[0].DB;
                int startByteAdr = n[0].StartByteAdr;
                int size = n.Last().Value is byte[] arr ? arr.Length : n.Last().Count;
                int count = n.Last().StartByteAdr + VarTypeToByteLength(n.Last().VarType, size) - startByteAdr;
                byte[] array2 = _plc.ReadBytes(DataType.DataBlock, dB, startByteAdr, count);
                foreach (DtDataItem item2 in n)
                {
                    VarTypeToByteLength(item2.VarType, item2.Count);
                    if (item2.VarType == VarType.Bit)
                    {
                        BitArray bits = new BitArray(new byte[1] { array2[item2.StartByteAdr - startByteAdr] }) { [item2.BitAdr] = Convert.ToBoolean(item2.Value) };
                        array2[item2.StartByteAdr - startByteAdr] = ConvertToByte(bits);
                    }
                    else
                    {
                        byte[] array3 = Serialization.SerializeDataItem(item2);
                        for (int k = 0; k < array3.Length; k++)
                        {
                            array2[item2.StartByteAdr - startByteAdr + k] = array3[k];
                        }
                    }
                }

                _plc.WriteBytesAsync(group.Key, dB, startByteAdr, array2);
            });
        }

        return true;
    }

    /// <summary>
    /// Read entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tInfo">entity</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<T> ReadEntity<T>(T tInfo)
    {
        try
        {
            var addressList = new List<string>();
            var type = typeof(T);
            // Get all properties of the entity
            foreach (var info in type.GetProperties())
            {
                var value = info.GetValue(tInfo)?.ToString();
                if (value != null)
                {
                    addressList.Add(value);
                }
            }
            // Read the PLC values for all addresses
            var dicValues = await ReadGroup(addressList);
            foreach (var info in type.GetProperties())
            {
                var value1 = dicValues[info.GetValue(tInfo)?.ToString() ?? string.Empty];
                info.SetValue(tInfo, value1.ToString());
            }

            return tInfo;
        }
        catch (ArgumentNullException ex)
        {
            throw new ArgumentNullException("An exception occurred in the read:" + ex);
        }
    }

    /// <summary>
    ///  Write PLC address value
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task WriteAsync(string address, object value)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        if (_plc == null)
            throw new InvalidOperationException("PLC is not connected. Please connect before writing.");

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        try
        {
            var strArray = address.Split('.');
            var writeInfo = ParseDbAddress(strArray);

            var strategies = new (Regex regex, Func<Task> action)[]
            {
                (RegexManager.DbxAddressRegex(),async () => await _plc.WriteAsync(address, ConvertValue<bool>(value, nameof(value)))),

                (RegexManager.DbwAddressRegex(), async () =>
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<short>(value, nameof(value)))
                ),

                (RegexManager.UdbwAddressRegex(), async () =>
                        await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<ushort>(value, nameof(value)))
                ),

                (RegexManager.SitAddressRegex(), async () =>{
                    var bytes = BitConverter.GetBytes((short)ConvertValue<sbyte>(value, nameof(value)));
                    await _plc.WriteBytesAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, bytes);
                }),

                (RegexManager.UsitAddressRegex(),async () =>
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<byte>(value, nameof(value)))
                ),

                (RegexManager.DitAddressRegex(),async () =>
                   await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<int>(value, nameof(value)))
                ),

                (RegexManager.UditAddressRegex(), async () =>
                {
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<uint>(value, nameof(value)));
                }),

                (RegexManager.DdwdAddressRegex(), async () =>
                {
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ParseHexDWord(value, nameof(value)));
                }),

                (RegexManager.DbbAddressRegex(),async () =>
                {
                    var str = ConvertValue<string>(value, nameof(value));
                    int maxLen = int.Parse(strArray[SubTwo], CultureInfo.InvariantCulture);
                    if (str.Length > maxLen)
                        str = str[..maxLen];
                    else
                        str = str.PadRight(maxLen, ' ');

                    str = str.Replace('\0', ' ');
                    var bytes = Encoding.ASCII.GetBytes(str);
                    await _plc.WriteBytesAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, bytes);
                }),

                (RegexManager.DbsAddressRegex(), async () =>
                {
                    //var str = ConvertValue<string>(value, nameof(value));
                    //await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address + 1, (byte)str.Length);
                    //await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address + 2, str);

                    var str = ConvertValue<string>(value, nameof(value));
                    int maxLen = str.Length;
                    byte[] bytes = S7String.ToByteArray(str, maxLen);
                    await _plc.WriteBytesAsync(DataType.DataBlock,writeInfo.Block,writeInfo.Address,bytes);
                }),

                (RegexManager.DbfAddressRegex(), async () =>
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address,ConvertValue<float>(value, nameof(value)))
                ),

                (RegexManager.DdbAddressRegex(),async () =>
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<double>(value, nameof(value)))
                ),

                (RegexManager.DbdAddressRegex(), async () =>
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<uint>(value, nameof(value)))
                ),

                (RegexManager.DwdAddressRegex(), async () =>
                    await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<ushort>(value, nameof(value)))
                ),

                (RegexManager.LwdAddressRegex(), async () =>
                {
                    var bValues = BitConverter.GetBytes(ConvertValue<long>(value, nameof(value)));
                    await _plc.WriteBytesAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, bValues);
                }),

                (RegexManager.DbtAddressRegex(),async () =>
                   await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<byte>(value, nameof(value)))
                ),

            (RegexManager.DtlAddressRegex(), async () =>
            {
                var dt = ConvertValue<DateTime>(value, nameof(value));
                var bYear = BitConverter.GetBytes(dt.Year);
                var bArr = new byte[8];
                bArr[0] = bYear[1];
                bArr[1] = bYear[0];
                bArr[2] = (byte)dt.Month;
                bArr[3] = (byte)dt.Day;
                bArr[4] = (byte)(dt.DayOfWeek + 1);
                bArr[5] = (byte)dt.Hour;
                bArr[6] = (byte)dt.Minute;
                bArr[7] = (byte)dt.Second;
                await _plc.WriteBytesAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, bArr);
            }),

            (RegexManager.LdtAddressRegex(), () =>
                throw new NotSupportedException("LDT type not implemented write: " + address)),

            (RegexManager.DatAddressRegex(), async () =>
                await _plc.WriteAsync(DataType.DataBlock, writeInfo.Block, writeInfo.Address, ConvertValue<DateTime>(value, nameof(value))))
            };

            foreach (var (regex, action) in strategies)
            {
                if (regex.IsMatch(address))
                {
                    await action();
                    return;
                }
            }

            throw new ArgumentException("Unrecognisable or unrealised addresses:" + address);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"write {address} exceptions: {ex}", nameof(address));
        }
    }

    /// <summary>
    ///  Parse DB address
    /// </summary>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static (int Block, int Address) ParseDbAddress(string[] parts)
    {
        var letterCount = parts[1].Count(char.IsLetter);
        //Block:PLC DB Blocks , Address：Data Address
        return (
            int.Parse(parts[0].AsSpan(2), CultureInfo.InvariantCulture),
            int.Parse(parts[1].AsSpan(letterCount), CultureInfo.InvariantCulture)
        );
    }

    /// <summary>
    /// Formats a 32-bit DWord as an 8-digit uppercase hexadecimal string (DDWD read).
    /// </summary>
    internal static string FormatHexDWord(uint value) =>
        value.ToString("X8", CultureInfo.InvariantCulture);

    /// <summary>
    /// Parses a hexadecimal DWord value for DDWD addresses (e.g. "12345678", "0xFFFFFFFF").
    /// </summary>
    internal static uint ParseHexDWord(object value, string paramName)
    {
        var s = value switch
        {
            string str => str.Trim(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };

        if (s.Length >= 2 && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            s = s[2..];

        if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new ArgumentException($"Invalid hex DWord value: {value}", paramName);
    }

    /// <summary>
    /// Converts the given object to the specified type <typeparamref name="T"/>.
    /// Throws an <see cref="ArgumentException"/> if conversion fails.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="value">The value to convert. Must not be null.</param>
    /// <param name="paramName">The name of the parameter for exception reporting.</param>
    /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the conversion fails.</exception>
    public static T ConvertValue<T>(object value, string paramName)
    {
        if (!TryConvert(value, out T result))
            throw new ArgumentException($"Invalid {typeof(T).Name} value: {value}", paramName);
        return result;
    }

    /// <summary>
    /// Attempts to convert the given object to the specified type <typeparamref name="T"/>.
    /// Returns a boolean indicating whether the conversion succeeded.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">
    /// When this method returns, contains the converted value if successful; otherwise, the default value of type <typeparamref name="T"/>.
    /// </param>
    /// <returns>True if conversion succeeded; otherwise, false.</returns>
    private static bool TryConvert<T>(object value, out T result)
    {
        try
        {
            // Attempt to change the type of the value to T using Convert.ChangeType
            result = (T)Convert.ChangeType(value, typeof(T));
            return true;
        }
        catch
        {
            // On failure, assign default value and return false
            result = default!;
            return false;
        }
    }

    internal static int VarTypeToByteLength(VarType varType, int varCount)
    {
        switch (varType)
        {
            case VarType.Bit:
                return (varCount + 7) / 8;
            case VarType.Byte:
                if (varCount >= 1)
                {
                    return varCount;
                }

                return 1;
            case VarType.String:
                return varCount;
            case VarType.S7String:
                if (((varCount + 2) & 1) != 1)
                {
                    return varCount + 2;
                }

                return varCount + 3;
            case VarType.S7WString:
                return varCount * 2 + 4;
            case VarType.Word:
            case VarType.Int:
            case VarType.Timer:
            case VarType.Counter:
                return varCount * 2;
            case VarType.DWord:
            case VarType.DInt:
            case VarType.Real:
                return varCount * 4;
            case VarType.LReal:
            case VarType.DateTime:
                return varCount * 8;
            case VarType.DateTimeLong:
                return varCount * 12;
            default:
                return 0;
        }
    }

    private byte ConvertToByte(BitArray bits)
    {
        if (bits.Length > 8)
        {
            throw new ArgumentException("ConvertToByte can only work with a BitArray containing a maximum of 8 values");
        }

        byte b = 0;
        for (byte b2 = 0; b2 < bits.Length; b2++)
        {
            if (bits[b2])
            {
                b |= (byte)(1 << (int)b2);
            }
        }

        return b;
    }
}
