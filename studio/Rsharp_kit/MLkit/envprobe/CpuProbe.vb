Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

''' <summary>
''' Collect the processor level information: the cpu model, the physical/visible core 
''' count, the process cpu affinity and the container resource quotas.
''' 
''' These values is very important for explains why the scientific computing program 
''' can not uses all of the cpu resources as it running on the development machine.
''' </summary>
Public Module EnvProbeCPU

    Public Const SECTION As String = "cpu"

    Public Const ITEM_VISIBLE As String = "Visible logical processors"
    Public Const ITEM_MACHINE_LOGICAL As String = "Machine logical processors"
    Public Const ITEM_PHYSICAL As String = "Physical cores"
    Public Const ITEM_MODEL As String = "CPU model"
    Public Const ITEM_VENDOR As String = "CPU vendor"
    Public Const ITEM_CLOCK As String = "Nominal clock (MHz)"
    Public Const ITEM_CACHE As String = "Cache"
    Public Const ITEM_NUMA As String = "NUMA nodes"
    Public Const ITEM_HYPERTHREAD As String = "Hyper-threading"
    Public Const ITEM_AFFINITY As String = "Process CPU affinity"
    Public Const ITEM_CGROUP_QUOTA As String = "cgroup CPU quota"
    Public Const ITEM_CGROUP_CPUSET As String = "cgroup cpuset"
    Public Const ITEM_CGROUP_MEMORY As String = "cgroup memory limit"
    Public Const ITEM_EFFECTIVE As String = "Effective CPU budget"

    ''' <summary>
    ''' run the cpu probe and append the result into the given report object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function probe(report As EnvProbeReport) As EnvProbeSection
        Dim section As EnvProbeSection = report.getSection(SECTION)
        Dim visible As Integer = System.Environment.ProcessorCount

        Call section.add(ITEM_VISIBLE, visible)

        If EnvProbeCommon.isLinux() Then
            Call probeLinux(section)
        ElseIf EnvProbeCommon.isWindows() Then
            Call probeWindows(section)
        Else
            Call section.na(ITEM_MODEL, "unsupported platform")
        End If

        Call probeAffinity(section)
        Call probeCgroup(section)

        Return section
    End Function

    Private Sub probeLinux(section As EnvProbeSection)
        Dim cpus As List(Of Dictionary(Of String, String)) = parseCpuInfo()

        If cpus.Count = 0 Then
            Call section.na(ITEM_MODEL, "missing /proc/cpuinfo")
            Call section.na(ITEM_PHYSICAL, "missing /proc/cpuinfo")

            Return
        End If

        Dim first As Dictionary(Of String, String) = cpus(0)

        Call section.add(ITEM_MODEL, EnvProbeCommon.formatText(field(first, "model name")))
        Call section.add(ITEM_VENDOR, EnvProbeCommon.formatText(field(first, "vendor_id")))
        Call section.add(ITEM_MACHINE_LOGICAL, cpus.Count)

        Dim physicalKeys As String() = cpus _
            .Select(Function(cpu)
                        Dim socket As String = field(cpu, "physical id")
                        Dim core As String = field(cpu, "core id")

                        If socket Is Nothing OrElse core Is Nothing Then
                            Return Nothing
                        Else
                            Return $"{socket}/{core}"
                        End If
                    End Function) _
            .Where(Function(key) key IsNot Nothing) _
            .Distinct() _
            .ToArray

        Dim physical As Integer = If(physicalKeys.Length > 0, physicalKeys.Length, cpus.Count)

        Call section.add(ITEM_PHYSICAL, physical)

        Dim mhz As Double = cpus _
            .Select(Function(cpu) parseDouble(field(cpu, "cpu MHz"))) _
            .Where(Function(value) value > 0) _
            .DefaultIfEmpty(-1) _
            .Max

        If mhz > 0 Then
            Dim clockItem As EnvProbeItem = section.add(ITEM_CLOCK, EnvProbeCommon.formatNumber(mhz, 0))
            clockItem.raw = mhz
        Else
            Call section.na(ITEM_CLOCK, "not exposed by /proc/cpuinfo")
        End If

        Call section.add(ITEM_CACHE, linuxCacheDescription())
        Call section.add(ITEM_NUMA, countNumaNodesLinux())
        Call section.add(ITEM_HYPERTHREAD, hyperThreadingText(cpus.Count, physical))
    End Sub

    Private Sub probeWindows(section As EnvProbeSection)
        Dim sysInfo As New SYSTEM_INFO
        Dim machineLogical As Integer = -1

        Try
            Call GetSystemInfo(sysInfo)
            machineLogical = CInt(sysInfo.dwNumberOfProcessors)
            Call section.add(ITEM_MACHINE_LOGICAL, machineLogical)
        Catch ex As Exception
            Call section.na(ITEM_MACHINE_LOGICAL, ex.Message)
        End Try

        Try
            Using key As RegistryKey = Registry.LocalMachine.OpenSubKey("HARDWARE\DESCRIPTION\System\CentralProcessor\0")
                If key IsNot Nothing Then
                    Dim model As Object = key.GetValue("ProcessorNameString")
                    Dim mhz As Object = key.GetValue("~MHz")

                    Call section.add(ITEM_MODEL, EnvProbeCommon.formatText(CStr(model)))
                    Call section.add(ITEM_VENDOR, EnvProbeCommon.formatText(CStr(key.GetValue("VendorIdentifier"))))

                    If mhz IsNot Nothing Then
                        Dim clockItem As EnvProbeItem = section.add(ITEM_CLOCK, CStr(mhz))
                        clockItem.raw = CDbl(CLng(mhz))
                    Else
                        Call section.na(ITEM_CLOCK, "not exposed by the registry")
                    End If
                Else
                    Call section.na(ITEM_MODEL, "registry key is not readable")
                End If
            End Using
        Catch ex As Exception
            Call section.na(ITEM_MODEL, ex.Message)
        End Try

        Dim topology As CpuTopology = readWindowsTopology()

        If topology.cores > 0 Then
            Call section.add(ITEM_PHYSICAL, topology.cores)
        Else
            Call section.na(ITEM_PHYSICAL, "GetLogicalProcessorInformation is not available")
        End If

        Call section.add(ITEM_CACHE, topology.cache)
        Call section.add(ITEM_NUMA, topology.numaNodes)

        Dim logical As Integer = If(machineLogical > 0, machineLogical, System.Environment.ProcessorCount)

        Call section.add(ITEM_HYPERTHREAD, hyperThreadingText(logical, topology.cores))
    End Sub

    Private Function hyperThreadingText(logical As Integer, physical As Integer) As String
        If physical <= 0 OrElse logical <= 0 Then
            Return EnvProbeCommon.NA
        End If

        Dim ratio As Double = logical / physical

        If ratio >= 1.5 Then
            Return $"enabled ({logical} logical / {physical} physical)"
        Else
            Return $"disabled or not detected ({logical} logical / {physical} physical)"
        End If
    End Function

    Private Function parseCpuInfo() As List(Of Dictionary(Of String, String))
        Dim cpus As New List(Of Dictionary(Of String, String))
        Dim lines As String() = EnvProbeCommon.readProc("/proc/cpuinfo")

        If lines Is Nothing Then
            Return cpus
        End If

        Dim current As Dictionary(Of String, String) = Nothing

        For Each line As String In lines
            If String.IsNullOrWhiteSpace(line) Then
                If current IsNot Nothing AndAlso current.Count > 0 Then
                    Call cpus.Add(current)
                End If

                current = Nothing
                Continue For
            End If

            Dim i As Integer = line.IndexOf(":"c)

            If i < 0 Then
                Continue For
            End If

            If current Is Nothing Then
                current = New Dictionary(Of String, String)
            End If

            Dim key As String = line.Substring(0, i).Trim

            If Not current.ContainsKey(key) Then
                current(key) = line.Substring(i + 1).Trim
            End If
        Next

        If current IsNot Nothing AndAlso current.Count > 0 Then
            Call cpus.Add(current)
        End If

        Return cpus
    End Function

    Private Function field(data As Dictionary(Of String, String), key As String) As String
        Dim value As String = Nothing

        If data IsNot Nothing AndAlso data.TryGetValue(key, value) Then
            Return value
        End If

        Return Nothing
    End Function

    Private Function parseDouble(text As String) As Double
        Dim value As Double

        If Double.TryParse(text, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, value) Then
            Return value
        Else
            Return -1
        End If
    End Function

    ''' <summary>
    ''' read the cache topology from sysfs, example as ``L1d=48K, L1i=32K, L2=1.25M/core, L3=36M``
    ''' </summary>
    ''' <returns></returns>
    Private Function linuxCacheDescription() As String
        Const cacheRoot As String = "/sys/devices/system/cpu/cpu0/cache"

        Try
            If Not Directory.Exists(cacheRoot) Then
                Return EnvProbeCommon.na("missing sysfs cache directory")
            End If

            Dim parts As New List(Of String)

            For Each index As String In Directory.GetDirectories(cacheRoot, "index*").OrderBy(Function(d) d)
                Dim level As String = EnvProbeCommon.readProcText(index & "/level")
                Dim type As String = EnvProbeCommon.readProcText(index & "/type")
                Dim size As String = EnvProbeCommon.readProcText(index & "/size")

                If String.IsNullOrWhiteSpace(size) Then
                    Continue For
                End If

                Dim suffix As String = ""

                If Not String.IsNullOrWhiteSpace(type) Then
                    suffix = type.Substring(0, 1).ToLower
                End If

                Call parts.Add($"L{level}{suffix}={size}")
            Next

            If parts.Count = 0 Then
                Return EnvProbeCommon.na("empty sysfs cache directory")
            Else
                Return String.Join(", ", parts)
            End If
        Catch ex As Exception
            Return EnvProbeCommon.na(ex.Message)
        End Try
    End Function

    Private Function countNumaNodesLinux() As Object
        Const nodeRoot As String = "/sys/devices/system/node"

        Try
            If Not Directory.Exists(nodeRoot) Then
                Return EnvProbeCommon.na("missing numa sysfs directory")
            End If

            Return Directory.GetDirectories(nodeRoot, "node*").Length
        Catch ex As Exception
            Return EnvProbeCommon.na(ex.Message)
        End Try
    End Function

    Private Sub probeAffinity(section As EnvProbeSection)
        If EnvProbeCommon.isLinux() Then
            Dim status As String() = EnvProbeCommon.readProc("/proc/self/status")
            Dim allowed As String = EnvProbeCommon.procField(status, "Cpus_allowed_list")

            If String.IsNullOrWhiteSpace(allowed) Then
                Call section.na(ITEM_AFFINITY, "Cpus_allowed_list is not readable")
            Else
                Dim affinityItem As EnvProbeItem =
                    section.add(ITEM_AFFINITY, $"{allowed} ({countCpuList(allowed)} processors)")

                affinityItem.raw = countCpuList(allowed)
            End If
        ElseIf EnvProbeCommon.isWindows() Then
            Try
                Dim processMask As UIntPtr
                Dim systemMask As UIntPtr

                Using proc As Process = Process.GetCurrentProcess
                    If GetProcessAffinityMask(proc.Handle, processMask, systemMask) Then
                        Dim count As Integer = countBits(processMask)
                        Dim affinityItem As EnvProbeItem =
                            section.add(ITEM_AFFINITY, $"0x{processMask.ToUInt64:X} ({count} processors)")

                        affinityItem.raw = count
                    Else
                        Call section.na(ITEM_AFFINITY, "GetProcessAffinityMask failed")
                    End If
                End Using
            Catch ex As Exception
                Call section.na(ITEM_AFFINITY, ex.Message)
            End Try
        Else
            Call section.na(ITEM_AFFINITY, "unsupported platform")
        End If
    End Sub

    Private Sub probeCgroup(section As EnvProbeSection)
        If Not EnvProbeCommon.isLinux() Then
            Return
        End If

        Dim visible As Integer = System.Environment.ProcessorCount
        Dim budget As Integer = visible

        ' --- cpu quota ---
        Dim quotaText As String = EnvProbeCommon.readProcText("/sys/fs/cgroup/cpu.max")
        Dim quota As Double = -1

        If quotaText IsNot Nothing Then
            Dim tokens As String() = quotaText.Split({" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)

            If tokens.Length >= 2 AndAlso Not String.Equals(tokens(0), "max", StringComparison.OrdinalIgnoreCase) Then
                Dim quotaValue As Long
                Dim period As Long

                If Long.TryParse(tokens(0), quotaValue) AndAlso Long.TryParse(tokens(1), period) AndAlso period > 0 Then
                    quota = quotaValue / period
                End If
            End If

            Call section.add(ITEM_CGROUP_QUOTA, quotaText)
        Else
            Dim cfsQuota As String = EnvProbeCommon.readProcText("/sys/fs/cgroup/cpu/cpu.cfs_quota_us")
            Dim cfsPeriod As String = EnvProbeCommon.readProcText("/sys/fs/cgroup/cpu/cpu.cfs_period_us")

            If cfsQuota Is Nothing Then
                Call section.na(ITEM_CGROUP_QUOTA, "cgroup cpu controller is not mounted")
            Else
                Dim quotaValue As Long
                Dim period As Long = 100000

                Long.TryParse(cfsPeriod, period)

                If Long.TryParse(cfsQuota, quotaValue) AndAlso quotaValue > 0 AndAlso period > 0 Then
                    quota = quotaValue / period
                End If

                Call section.add(ITEM_CGROUP_QUOTA, $"quota={cfsQuota} period={cfsPeriod}")
            End If
        End If

        If quota > 0 Then
            budget = Math.Min(budget, CInt(Math.Max(1, Math.Floor(quota))))
        End If

        section(ITEM_CGROUP_QUOTA).raw = If(quota > 0, quota, -1.0)

        ' --- cpuset ---
        Dim cpuset As String = EnvProbeCommon.readProcText("/sys/fs/cgroup/cpuset.cpus.effective")

        If cpuset Is Nothing Then
            cpuset = EnvProbeCommon.readProcText("/sys/fs/cgroup/cpuset/cpuset.cpus")
        End If

        If cpuset Is Nothing Then
            Call section.na(ITEM_CGROUP_CPUSET, "cgroup cpuset controller is not mounted")
        Else
            Dim count As Integer = countCpuList(cpuset)

            Call section.add(ITEM_CGROUP_CPUSET, $"{cpuset} ({count} processors)")
            section(ITEM_CGROUP_CPUSET).raw = count

            If count > 0 Then
                budget = Math.Min(budget, count)
            End If
        End If

        ' --- memory ---
        Dim memoryMax As String = EnvProbeCommon.readProcText("/sys/fs/cgroup/memory.max")

        If memoryMax Is Nothing Then
            memoryMax = EnvProbeCommon.readProcText("/sys/fs/cgroup/memory/memory.limit_in_bytes")
        End If

        If memoryMax Is Nothing Then
            Call section.na(ITEM_CGROUP_MEMORY, "cgroup memory controller is not mounted")
        ElseIf String.Equals(memoryMax, "max", StringComparison.OrdinalIgnoreCase) Then
            Call section.add(ITEM_CGROUP_MEMORY, "unlimited")
            section(ITEM_CGROUP_MEMORY).raw = -1L
        Else
            Dim bytes As Long

            If Long.TryParse(memoryMax, bytes) Then
                Call section.add(ITEM_CGROUP_MEMORY, EnvProbeCommon.formatBytes(bytes))
                section(ITEM_CGROUP_MEMORY).raw = bytes
            Else
                Call section.add(ITEM_CGROUP_MEMORY, memoryMax)
            End If
        End If

        Call section.add(ITEM_EFFECTIVE, budget)
    End Sub

    ''' <summary>
    ''' count the processors described by a linux cpu list text, example as ``0-7,16-23``
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function countCpuList(text As String) As Integer
        If String.IsNullOrWhiteSpace(text) Then
            Return -1
        End If

        Dim count As Integer = 0

        For Each part As String In text.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
            Dim range As String() = part.Trim.Split("-"c)

            If range.Length = 2 Then
                Dim min As Integer
                Dim max As Integer

                If Integer.TryParse(range(0), min) AndAlso Integer.TryParse(range(1), max) AndAlso max >= min Then
                    count += max - min + 1
                End If
            Else
                Dim single As Integer

                If Integer.TryParse(range(0), single) Then
                    count += 1
                End If
            End If
        Next

        Return count
    End Function

    Private Function countBits(mask As UIntPtr) As Integer
        Dim value As ULong = mask.ToUInt64
        Dim count As Integer = 0

        While value <> 0
            count += CInt(value And 1UL)
            value >>= 1
        End While

        Return count
    End Function

    Private Function readWindowsTopology() As CpuTopology
        Dim topology As New CpuTopology With {
            .cores = -1,
            .numaNodes = EnvProbeCommon.NA,
            .cache = EnvProbeCommon.NA
        }

        Dim buffer As IntPtr = IntPtr.Zero

        Try
            Dim length As UInteger

            Call GetLogicalProcessorInformation(IntPtr.Zero, length)

            If length = 0 Then
                Return topology
            End If

            buffer = Marshal.AllocHGlobal(CInt(length))

            If Not GetLogicalProcessorInformation(buffer, length) Then
                Return topology
            End If

            Dim itemSize As Integer = Marshal.SizeOf(Of SYSTEM_LOGICAL_PROCESSOR_INFORMATION)()
            Dim ptr As IntPtr = buffer
            Dim [end] As Long = buffer.ToInt64 + CLng(length)
            Dim cores As Integer = 0
            Dim numa As Integer = 0
            Dim l2 As Long = 0
            Dim l3 As Long = 0

            While ptr.ToInt64 + itemSize <= [end]
                Dim info As SYSTEM_LOGICAL_PROCESSOR_INFORMATION =
                    Marshal.PtrToStructure(Of SYSTEM_LOGICAL_PROCESSOR_INFORMATION)(ptr)

                Select Case info.Relationship
                    Case 0 ' RelationProcessorCore
                        cores += 1
                    Case 1 ' RelationNumaNode
                        numa += 1
                    Case 2 ' RelationCache
                        Select Case info.Union.Cache.Level
                            Case 2 : l2 = Math.Max(l2, info.Union.Cache.CacheSize)
                            Case 3 : l3 += info.Union.Cache.CacheSize
                        End Select
                End Select

                ptr = New IntPtr(ptr.ToInt64 + itemSize)
            End While

            topology.cores = cores
            topology.numaNodes = numa

            Dim parts As New List(Of String)

            If l2 > 0 Then
                Call parts.Add("L2=" & EnvProbeCommon.formatBytes(l2) & "/core")
            End If
            If l3 > 0 Then
                Call parts.Add("L3=" & EnvProbeCommon.formatBytes(l3))
            End If

            If parts.Count > 0 Then
                topology.cache = String.Join(", ", parts)
            End If

            Return topology
        Catch ex As Exception
            topology.cache = EnvProbeCommon.na(ex.Message)
            Return topology
        Finally
            If buffer <> IntPtr.Zero Then
                Call Marshal.FreeHGlobal(buffer)
            End If
        End Try
    End Function

    Private Class CpuTopology
        Public Property cores As Integer
        Public Property numaNodes As Object
        Public Property cache As String
    End Class

#Region "Windows Native API"

    <StructLayout(LayoutKind.Sequential)>
    Private Structure SYSTEM_INFO
        Public wProcessorArchitecture As UShort
        Public wReserved As UShort
        Public dwPageSize As UInteger
        Public lpMinimumApplicationAddress As IntPtr
        Public lpMaximumApplicationAddress As IntPtr
        Public dwActiveProcessorMask As UIntPtr
        Public dwNumberOfProcessors As UInteger
        Public dwProcessorType As UInteger
        Public dwAllocationGranularity As UInteger
        Public wProcessorLevel As UShort
        Public wProcessorRevision As UShort
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure CACHE_DESCRIPTOR
        Public Level As Byte
        Public Associativity As Byte
        Public LineSize As UShort
        Public CacheSize As UInteger
        Public Type As Integer
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Private Structure PROCESSOR_UNION
        <FieldOffset(0)>
        Public ProcessorCoreFlags As Byte
        <FieldOffset(0)>
        Public NumaNodeNumber As UInteger
        <FieldOffset(0)>
        Public Cache As CACHE_DESCRIPTOR
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure SYSTEM_LOGICAL_PROCESSOR_INFORMATION
        Public ProcessorMask As UIntPtr
        Public Relationship As Integer
        Public Union As PROCESSOR_UNION
    End Structure

    <DllImport("kernel32.dll")>
    Private Sub GetSystemInfo(ByRef lpSystemInfo As SYSTEM_INFO)
    End Sub

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function GetLogicalProcessorInformation(lpBuffer As IntPtr, ByRef ReturnLength As UInteger) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function GetProcessAffinityMask(hProcess As IntPtr,
                                            ByRef lpProcessAffinityMask As UIntPtr,
                                            ByRef lpSystemAffinityMask As UIntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

#End Region
End Module
