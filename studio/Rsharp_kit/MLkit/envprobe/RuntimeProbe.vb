Imports System.Runtime
Imports System.Runtime.InteropServices
Imports System.Runtime.Intrinsics.X86
Imports System.Threading
Imports Microsoft.VisualBasic.Math.SIMD

''' <summary>
''' Collect the CLR runtime level information: the garbage collector configuration,
''' the thread pool settings and the .NET runtime switches which may be affects the
''' parallel computing performance.
''' 
''' A server GC disabled runtime or a too small thread pool upper bound is a very 
''' common reason of "the program only uses a part of the cpu cores".
''' </summary>
Public Module EnvProbeRuntime

    Public Const SECTION_NAME As String = "runtime"
    Public Const SECTION_GC As String = "gc"
    Public Const SECTION_THREADPOOL As String = "threadpool"

    Public Const ITEM_FRAMEWORK As String = "Runtime framework"
    Public Const ITEM_CLR As String = "CLR version"
    Public Const ITEM_TIERED As String = "Tiered compilation"
    Public Const ITEM_TIERED_PGO As String = "Tiered PGO"
    Public Const ITEM_QUICK_JIT_LOOPS As String = "Quick JIT for loops"
    Public Const ITEM_INTRINSICS As String = "Hardware intrinsics enabled"
    Public Const ITEM_ENV_PREFIX As String = "Environment: "

    Public Const ITEM_SERVER_GC As String = "Server GC"
    Public Const ITEM_CONCURRENT_GC As String = "Concurrent GC"
    Public Const ITEM_LATENCY As String = "GC latency mode"
    Public Const ITEM_MAX_GEN As String = "Max generation"
    Public Const ITEM_HEAP_SIZE As String = "Managed heap size"
    Public Const ITEM_COMMITTED As String = "Total committed bytes"
    Public Const ITEM_FRAGMENTED As String = "Fragmented bytes"
    Public Const ITEM_GC_AVAILABLE As String = "Memory available for GC"
    Public Const ITEM_MEMORY_LOAD As String = "Physical memory load (GC view)"
    Public Const ITEM_PAUSE_PERCENT As String = "GC pause time (%)"
    Public Const ITEM_COLLECTIONS As String = "GC collections (gen0/gen1/gen2)"
    Public Const ITEM_RETAIN_VM As String = "Retain VM"

    Public Const ITEM_MIN_WORKER As String = "Minimum worker threads"
    Public Const ITEM_MIN_IO As String = "Minimum IO threads"
    Public Const ITEM_MAX_WORKER As String = "Maximum worker threads"
    Public Const ITEM_MAX_IO As String = "Maximum IO threads"
    Public Const ITEM_AVAILABLE_WORKER As String = "Available worker threads"
    Public Const ITEM_PENDING As String = "Pending work items"
    Public Const ITEM_COMPLETED As String = "Completed work items"

    ''' <summary>
    ''' the .NET runtime switches which is related to the computing performance
    ''' </summary>
    Public ReadOnly PerformanceSwitches As String() = {
        "DOTNET_TieredCompilation",
        "DOTNET_TieredPGO",
        "DOTNET_TC_QuickJitForLoops",
        "DOTNET_ReadyToRun",
        "DOTNET_EnableHWIntrinsic",
        "DOTNET_EnableAVX2",
        "DOTNET_gcServer",
        "DOTNET_gcConcurrent",
        "DOTNET_GCHeapHardLimit",
        "DOTNET_GCHeapHardLimitPercent",
        "DOTNET_GCConserveMemory",
        "DOTNET_ThreadPool_ForceMinWorkerThreads",
        "DOTNET_ThreadPool_ForceMaxWorkerThreads",
        "COMPlus_gcServer",
        "COMPlus_TieredCompilation",
        "COMPlus_ThreadPool_ForceMinWorkerThreads"
    }

    ''' <summary>
    ''' run the runtime probe and append the result into the given report object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function probe(report As EnvProbeReport) As EnvProbeSection
        Dim section As EnvProbeSection = report.getSection(SECTION_NAME)

        Call section.add(ITEM_FRAMEWORK, RuntimeInformation.FrameworkDescription)
        Call section.add(ITEM_CLR, System.Environment.Version.ToString)
        Call section.add(ITEM_TIERED, switchText("System.Runtime.TieredCompilation", True))
        Call section.add(ITEM_TIERED_PGO, switchText("System.Runtime.TieredPGO", True))
        Call section.add(ITEM_QUICK_JIT_LOOPS, switchText("System.Runtime.TieredCompilation.QuickJitForLoops", False))
        Call section.add(ITEM_INTRINSICS,
                        If(isSwitchDisabled("System.Runtime.Intrinsics.IsHardwareAccelerationDisabled"), "disabled", "enabled"))

        For Each name As String In PerformanceSwitches
            Dim value As String = System.Environment.GetEnvironmentVariable(name)

            If Not String.IsNullOrWhiteSpace(value) Then
                Call section.add(ITEM_ENV_PREFIX & name, value)
            End If
        Next

        Call probeGC(report)
        Call probeThreadPool(report)

        Return section
    End Function

    Private Function switchValue(key As String, [default] As Boolean) As Boolean
        Dim value As Boolean

        If AppContext.TryGetSwitch(key, value) Then
            Return value
        Else
            Return [default]
        End If
    End Function

    Private Function isSwitchDisabled(key As String) As Boolean
        Return switchValue(key, False)
    End Function

    Private Function switchText(key As String, [default] As Boolean) As String
        Dim value As Boolean

        If AppContext.TryGetSwitch(key, value) Then
            Return $"{If(value, "enabled", "disabled")} (runtimeconfig)"
        Else
            Return $"{If([default], "enabled", "disabled")} (default)"
        End If
    End Function

    Private Sub probeGC(report As EnvProbeReport)
        Dim section As EnvProbeSection = report.getSection(SECTION_GC)
        Dim serverItem As EnvProbeItem = section.add(ITEM_SERVER_GC, If(GCSettings.IsServerGC, "enabled", "disabled"))

        serverItem.raw = GCSettings.IsServerGC

        Dim concurrent As String = If(switchValue("System.GC.Concurrent", True), "enabled", "disabled")

        Call section.add(ITEM_CONCURRENT_GC, concurrent)

        Dim retainVM As String = If(switchValue("System.GC.RetainVM", False), "enabled", "disabled")

        Call section.add(ITEM_RETAIN_VM, retainVM)
        Call section.add(ITEM_LATENCY, GCSettings.LatencyMode.ToString)
        Call section.add(ITEM_MAX_GEN, GC.MaxGeneration)

        Try
            Dim info As GCMemoryInfo = GC.GetGCMemoryInfo

            Dim heapSize As Long = info.HeapSizeBytes

            If heapSize <= 0 Then
                heapSize = GC.GetTotalMemory(forceFullCollection:=False)
            End If

            Call section.add(ITEM_HEAP_SIZE, EnvProbeCommon.formatBytes(heapSize))
            Call section.add(ITEM_COMMITTED, EnvProbeCommon.formatBytes(info.TotalCommittedBytes))
            Call section.add(ITEM_FRAGMENTED, EnvProbeCommon.formatBytes(info.FragmentedBytes))
            Call section.add(ITEM_PAUSE_PERCENT, EnvProbeCommon.formatNumber(info.PauseTimePercentage, 3))

            Dim available As EnvProbeItem =
                section.add(ITEM_GC_AVAILABLE, EnvProbeCommon.formatBytes(info.TotalAvailableMemoryBytes))
            available.raw = info.TotalAvailableMemoryBytes

            If info.MemoryLoadBytes > 0 Then
                Dim memoryLoad As EnvProbeItem =
                    section.add(ITEM_MEMORY_LOAD, EnvProbeCommon.formatBytes(info.MemoryLoadBytes))

                memoryLoad.raw = info.MemoryLoadBytes
            End If
        Catch ex As Exception
            Call section.na(ITEM_HEAP_SIZE, ex.Message)
        End Try

        Call section.add(ITEM_COLLECTIONS,
                         $"gen0={GC.CollectionCount(0)}, gen1={GC.CollectionCount(1)}, gen2={GC.CollectionCount(2)}")
    End Sub

    Private Sub probeThreadPool(report As EnvProbeReport)
        Dim section As EnvProbeSection = report.getSection(SECTION_THREADPOOL)

        Try
            Dim minWorker As Integer
            Dim minIO As Integer
            Dim maxWorker As Integer
            Dim maxIO As Integer

            Call ThreadPool.GetMinThreads(minWorker, minIO)
            Call ThreadPool.GetMaxThreads(maxWorker, maxIO)

            Dim minWorkerItem As EnvProbeItem = section.add(ITEM_MIN_WORKER, minWorker)
            Dim maxWorkerItem As EnvProbeItem = section.add(ITEM_MAX_WORKER, maxWorker)

            Call section.add(ITEM_MIN_IO, minIO)
            Call section.add(ITEM_MAX_IO, maxIO)

            minWorkerItem.raw = minWorker
            maxWorkerItem.raw = maxWorker

            Dim availableWorker As Integer
            Dim availableIO As Integer

            Call ThreadPool.GetAvailableThreads(availableWorker, availableIO)

            Call section.add(ITEM_AVAILABLE_WORKER, availableWorker)
            Call section.add(ITEM_PENDING, ThreadPool.PendingWorkItemCount)
            Call section.add(ITEM_COMPLETED, ThreadPool.CompletedWorkItemCount)
        Catch ex As Exception
            Call section.na(ITEM_MIN_WORKER, ex.Message)
        End Try
    End Sub
End Module

''' <summary>
''' Collect the SIMD capabilities of current processor and the CLR JIT compiler.
''' 
''' The cpu instruction set capability is the basis of the vectorization of the numerical
''' computing; a AVX2/AVX-512 capable server which is running the program in the software
''' fallback mode will has a huge performance gap against the development machine.
''' </summary>
Public Module EnvProbeSIMD

    Public Const SECTION_NAME As String = "simd"

    Public Const ITEM_DESCRIPTION As String = "SIMD capability"
    Public Const ITEM_HW_ACCEL As String = "Hardware accelerated"
    Public Const ITEM_SSE2 As String = "SSE2"
    Public Const ITEM_SSE42 As String = "SSE4.2"
    Public Const ITEM_AVX As String = "AVX"
    Public Const ITEM_AVX2 As String = "AVX2"
    Public Const ITEM_FMA As String = "FMA"
    Public Const ITEM_AVX512F As String = "AVX-512F"
    Public Const ITEM_AVX512BW As String = "AVX-512BW"
    Public Const ITEM_AVX512VNNI As String = "AVX-VNNI"
    Public Const ITEM_ADVSIMD As String = "AdvSIMD/NEON"
    Public Const ITEM_VECTOR_WIDTH As String = "Vector width"
    Public Const ITEM_CPU_FLAGS As String = "CPU flags from /proc/cpuinfo"

    ''' <summary>
    ''' run the simd probe and append the result into the given report object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function probe(report As EnvProbeReport) As EnvProbeSection
        Dim section As EnvProbeSection = report.getSection(SECTION_NAME)
        Dim accelerated As Boolean = SimdCapabilities.IsHardwareAccelerated
        Dim acceleratedItem As EnvProbeItem = section.add(ITEM_HW_ACCEL, If(accelerated, "yes", "no"))

        acceleratedItem.raw = accelerated

        Call section.add(ITEM_DESCRIPTION, SimdCapabilities.Description)
        Call section.add(ITEM_SSE2, SimdCapabilities.IsSse2)
        Call section.add(ITEM_SSE42, SimdCapabilities.IsSse42)
        Call section.add(ITEM_AVX, SimdCapabilities.IsAvx)
        Call section.add(ITEM_AVX2, SimdCapabilities.IsAvx2)
        Call section.add(ITEM_FMA, SimdCapabilities.IsFma)
        Call section.add(ITEM_ADVSIMD, SimdCapabilities.IsAdvSimd)

        Dim avx512 As Boolean = isAvx512Supported()

        Call section.add(ITEM_AVX512F, avx512)
        Call section.add(ITEM_AVX512BW, Avx512BW.IsSupported)
        Call section.add(ITEM_AVX512VNNI, AvxVnni.IsSupported)
        Call section.add(ITEM_VECTOR_WIDTH,
                         $"f64={SimdCapabilities.countDouble}, f32={SimdCapabilities.countFloat}, " &
                         $"i32={SimdCapabilities.countInteger}, i64={SimdCapabilities.countLong}, " &
                         $"i16={SimdCapabilities.countShort}")

        If EnvProbeCommon.isLinux() Then
            Dim flags As String() = linuxCpuFlags()

            If flags.Length > 0 Then
                Call section.add(ITEM_CPU_FLAGS,
                                 String.Join(" ", flags.Where(Function(f) f.StartsWith("avx") OrElse
                                                                     f.StartsWith("sse") OrElse
                                                                     f = "fma" OrElse
                                                                     f = "asimd")))
            End If
        End If

        Return section
    End Function

    Private Function isAvx512Supported() As Boolean
        Try
            Return Avx512F.IsSupported
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function linuxCpuFlags() As String()
        Dim lines As String() = EnvProbeCommon.readProc("/proc/cpuinfo")

        If lines Is Nothing Then
            Return New String() {}
        End If

        Dim flags As String = EnvProbeCommon.procField(lines, "flags")

        If flags Is Nothing Then
            flags = EnvProbeCommon.procField(lines, "Features")
        End If

        Return EnvProbeCommon.tokens(flags)
    End Function
End Module
