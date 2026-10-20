using System.Diagnostics.CodeAnalysis;
using S7.Net;
using S7.Net.Protocol;

namespace MyS7Core.Wrapper;

[ExcludeFromCodeCoverage]
public class PlcWrapper : Plc, IPlc
{
    public PlcWrapper(CpuType cpu, string ip, short rack, short slot) : base(cpu, ip, rack, slot) { }

    public PlcWrapper(CpuType cpu, string ip, int port, short rack, short slot) : base(cpu, ip, port, rack, slot) { }

    public PlcWrapper(string ip, TsapPair tsapPair) : base(ip, tsapPair) { }

    public PlcWrapper(string ip, int port, TsapPair tsapPair) : base(ip, port, tsapPair) { }
}
