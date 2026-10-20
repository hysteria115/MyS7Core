# MyS7Core

同一份源代码同时生成 .NET 8 (`net8.0`) 和 .NET Framework 4.7.2 (`net472`) 的库。

## 构建与使用

在仓库目录运行（使用 `global.json` 指定的 .NET 8 SDK）：

```powershell
dotnet build MyS7Core.sln -c Release
```

产物：

- `src/MyS7Core/bin/Release/net8.0/MyS7Core.dll`
- `src/MyS7Core/bin/Release/net472/MyS7Core.dll`
- `src/MyS7Core/bin/Release/MyS7Core.1.0.0.nupkg`（同时包含两个目标）

优先通过项目引用或 NuGet 包引用，让调用方自动选择对应目标，并还原 `S7netplus 0.20.0` 等依赖。不要只复制 `MyS7Core.dll` 而遗漏依赖。

库的构建使用 C# 12；这不会要求调用方运行 .NET 8。`net472` 版本仍在 .NET Framework 上运行。异步方法及事件名称保持不变，例如 `ConnectAsync`、`ReadAsync`、`WriteAsync`、`MonitorAsync`；`ReadGroup` 仍返回 `Task<Dictionary<string, object>>`，调用时需要 `await`。

## 框架兼容实现

- .NET 8 使用原有 `GeneratedRegex`，.NET Framework 使用缓存的 `RegexOptions.Compiled` 实例；地址规则及方法签名一致。
- 替换 Framework 不支持的字符串范围、Span 数字解析、字符查找重载和 `ArgumentNullException.ThrowIfNull`。
- 为 Framework 添加内部 `IsExternalInit` 编译器标记，保留 `MonitorAddress` 的 record API。
- 删除未使用的 JavaScript 导入和 `System.Drawing.Common` 包依赖。
- `Microsoft.NETFramework.ReferenceAssemblies` 仅用于构建，不作为消费者依赖发布。
- 仓库根目录如存在 `License.txt`，打包时自动包含；当前仓库未提供该文件，不生成或声明未经确认的许可证。对外发布前应补齐正确的许可证信息。

## 无硬件回归测试

```powershell
dotnet run --project tests/MyS7Core.CompatibilityTests/MyS7Core.CompatibilityTests.csproj -c Release -f net8.0
dotnet run --project tests/MyS7Core.CompatibilityTests/MyS7Core.CompatibilityTests.csproj -c Release -f net472
```

`net472` 测试需在安装了兼容 .NET Framework 运行时的 Windows 上执行。

同一组测试在两个目标下运行，覆盖 19 类地址正则、record 元数据、地址/十六进制转换、异步连接与取消、模拟 PLC 异步读写、字符串截断与补齐、原始字节、监控事件与停止。测试不连接真实 PLC。

实际硬件连接及协议行为仍需联调。
## OffLine 离线模拟

`Offline` 与 `OffLine` 是同一个配置，默认 `false`。也可以使用已有的三参数构造函数 `new PlcFunction(ip, 1, true)`。

```csharp
using MyS7Core;

using var plc = new PlcFunction("192.168.0.10") { OffLine = true };
await plc.ConnectAsync(); // 也支持 plc.Connect()
await plc.WriteAsync("DB1.DBW0", (short)123);
var value = await plc.ReadAsync("DB1.DBW0");

plc.WriteGroup(new Dictionary<string, object>
{
    ["DB1.DBW2"] = (short)456,
    ["DB1.DBX10.0"] = true
});
var values = await plc.ReadGroup(new List<string> { "DB1.DBW2", "DB1.DBX10.0" });
plc.Disconnect();
```

离线连接不会访问真实 PLC。模拟器在传输层接入，现有异步读写、原始字节、批量操作、实体操作及 `MonitorAsync` 使用同一套内存。相同 IP 的实例共享内存，不同 IP 隔离；不同 DB 隔离，同一 DB 的重叠地址共享字节。未写入的字节为零，未初始化的日期返回 `DateTime.MinValue`。断开再连接保留数据，进程退出后清空，不做磁盘持久化。修改离线开关或 IP 后需要重新连接。

离线模式沿用现有地址规则、类型转换及限制（例如 LDT 写入仍未实现），不模拟网络故障、PLC 程序执行或真实扫描周期。
