Imports System.Linq
Imports System.Text

''' <summary>
''' The diagnosis rule engine.
''' 
''' It scans the collected probe items and produces the ``warnings`` (which is the 
''' detected potential bottleneck) and the ``suggestions`` (which is the optimization 
''' action for fixing the bottleneck) in English.
''' </summary>
Public Module EnvProbeDiagnosis

    ''' <summary>
    ''' run all of the diagnosis rules against the given report.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function diagnose(report As EnvProbeReport) As EnvProbeReport
        Call checkCpuBudget(report)
        Call checkGC(report)
        Call checkSIMD(report)
        Call checkThreadPool(report)
        Call checkMemory(report)
        Call checkScaling(report)
        Call checkNuma(report)
        Call checkNativeLibraries(report)
        Call checkCuda(report)
        Call summarize(report)

        Return report
    End Function

    Private Sub checkCpuBudget(report As EnvProbeReport)
        Dim visible As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_VISIBLE)
        Dim machine As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_MACHINE_LOGICAL)
        Dim quota As Double = report.getDouble(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_CGROUP_QUOTA)
        Dim cpuset As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_CGROUP_CPUSET)
        Dim affinity As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_AFFINITY)

        If machine <= 0 OrElse visible <= 0 Then
            Return
        End If

        If visible >= machine Then
            Return
        End If

        Dim detail As New StringBuilder

        If quota > 0 Then
            Call detail.Append($"cgroup cpu.max = {EnvProbeCommon.formatNumber(quota, 2)} cpus; ")
        End If
        If cpuset > 0 Then
            Call detail.Append($"cgroup cpuset = {cpuset} cpus; ")
        End If
        If affinity > 0 Then
            Call detail.Append($"process affinity = {affinity} cpus; ")
        End If

        report.warn(
            $"Only {visible} of {machine} logical processors are visible to the CLR runtime.",
            "Check the container cpu limit (docker run --cpus / kubernetes resources.limits.cpu), " &
            "the cgroup cpu.max and cpuset.cpus.effective files, and the process affinity mask " &
            $"(taskset / numactl / Windows affinity). Detected: {If(detail.Length > 0, detail.ToString, "no cgroup/affinity limit was found")}" &
            "the parallelism of the R# program can not exceed this visible processor count.")
    End Sub

    Private Sub checkGC(report As EnvProbeReport)
        Dim serverGC As Object = report.getRaw(EnvProbeRuntime.SECTION_GC, EnvProbeRuntime.ITEM_SERVER_GC)
        Dim latency As String = report.getValue(EnvProbeRuntime.SECTION_GC, EnvProbeRuntime.ITEM_LATENCY)

        If TypeOf serverGC Is Boolean AndAlso Not CBool(serverGC) Then
            report.warn(
                "Server GC is disabled: the garbage collector will run on a single thread and can not scale with the cpu cores.",
                "Enable the server GC in the runtimeconfig.json file: <ServerGarbageCollection>true</ServerGarbageCollection>, " &
                "or set the DOTNET_gcServer=1 environment variable before starting the server process.")
        End If

        If String.Equals(latency, "SustainedLowLatency", StringComparison.OrdinalIgnoreCase) Then
            report.warn(
                "The GC latency mode is SustainedLowLatency: the gen2 collections are suppressed and the memory footprint will keep growing.",
                "Use GCSettings.LatencyMode = GCLatencyMode.Batch for the batch data science jobs, the SustainedLowLatency mode is designed for the desktop user interface.")
        End If
    End Sub

    Private Sub checkSIMD(report As EnvProbeReport)
        Dim accelerated As Object = report.getRaw(EnvProbeSIMD.SECTION_NAME, EnvProbeSIMD.ITEM_HW_ACCEL)
        Dim avx2 As Object = report.getRaw(EnvProbeSIMD.SECTION_NAME, EnvProbeSIMD.ITEM_AVX2)
        Dim flags As String = report.getValue(EnvProbeSIMD.SECTION_NAME, EnvProbeSIMD.ITEM_CPU_FLAGS)

        If TypeOf accelerated Is Boolean AndAlso Not CBool(accelerated) Then
            report.warn(
                "The CLR reports that the hardware SIMD acceleration is not available: System.Numerics.Vector(Of T) will fallback to the software implementation which is usually slower than the plain scalar loop.",
                "Make sure the process is running as a 64-bit process on a x64/arm64 host, and check that the DOTNET_EnableHWIntrinsic switch is not set to 0.")
        End If

        If Not String.IsNullOrWhiteSpace(flags) Then
            Dim cpuTokens As String() = EnvProbeCommon.tokens(flags)

            If EnvProbeCommon.hasFlag(cpuTokens, "avx2") AndAlso TypeOf avx2 Is Boolean AndAlso Not CBool(avx2) Then
                report.warn(
                    "The /proc/cpuinfo reports the AVX2 capability but the CLR JIT reports AVX2 as unsupported.",
                    "This usually means the JIT hardware intrinsic was disabled by the environment variable, or the operating system is not saving the AVX2 register state.")
            End If

            If EnvProbeCommon.hasFlag(cpuTokens, "avx512f") Then
                Dim avx512 As Object = report.getRaw(EnvProbeSIMD.SECTION_NAME, EnvProbeSIMD.ITEM_AVX512F)

                If TypeOf avx512 Is Boolean AndAlso Not CBool(avx512) Then
                    report.warn(
                        "The /proc/cpuinfo reports the AVX-512 capability but the CLR JIT reports AVX-512 as unsupported.",
                        "Check the OS support of the AVX-512 register state (XSAVE) and the hypervisor cpu model settings.")
                End If
            End If
        End If

        Dim speedup As Double = report.getDouble(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_SPEEDUP)

        If speedup >= 0 AndAlso speedup < 1.5 Then
            report.warn(
                $"The measured SIMD speed-up is only {EnvProbeCommon.formatNumber(speedup, 2)}x comparing with the scalar loop; the JIT compiler may not be vectorizing the hot loop.",
                "Check that the application was built with the optimization enabled, and avoid the branch/boxing/complex delegate call in the numeric hot loop.")
        End If
    End Sub

    Private Sub checkThreadPool(report As EnvProbeReport)
        Dim visible As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_VISIBLE)
        Dim minWorker As Integer = report.getInteger(EnvProbeRuntime.SECTION_THREADPOOL, EnvProbeRuntime.ITEM_MIN_WORKER)
        Dim maxWorker As Integer = report.getInteger(EnvProbeRuntime.SECTION_THREADPOOL, EnvProbeRuntime.ITEM_MAX_WORKER)

        If visible > 0 AndAlso minWorker > 0 AndAlso minWorker < visible Then
            report.warn(
                $"The thread pool minimum worker thread count ({minWorker}) is lower than the visible processor count ({visible}): the thread pool will inject the new threads slowly.",
                "Call ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount) at the application startup, or set the DOTNET_ThreadPool_ForceMinWorkerThreads environment variable.")
        End If

        If visible > 0 AndAlso maxWorker > 0 AndAlso maxWorker < visible Then
            report.warn(
                $"The thread pool maximum worker thread count ({maxWorker}) is lower than the visible processor count ({visible}).",
                "Increase the thread pool maximum worker thread count, otherwise the Parallel/PLINQ workload can not use all of the cpu cores.")
        End If
    End Sub

    Private Sub checkMemory(report As EnvProbeReport)
        Dim used As Double = report.getDouble(EnvProbeMemory.SECTION_NAME, EnvProbeMemory.ITEM_USED)
        Dim total As Long = report.getLong(EnvProbeMemory.SECTION_NAME, EnvProbeMemory.ITEM_TOTAL)

        If used < 0 Then
            Return
        End If

        If used >= 85 Then
            report.warn(
                $"The physical memory usage is {EnvProbeCommon.formatNumber(used, 1)}%: the available memory is not enough for a larger parallel workload.",
                "Reduce the batch size of the parallel job, or increase the machine memory. A server which is (nearly) out of memory usually can not use all of its cpu cores because of the swap/numa remote access and the GC pressure.")
        End If

        Dim cgroupMemory As Long = report.getLong(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_CGROUP_MEMORY)

        If cgroupMemory > 0 AndAlso total > 0 AndAlso cgroupMemory < total / 2 Then
            report.warn(
                $"The container memory limit ({EnvProbeCommon.formatBytes(cgroupMemory)}) is much smaller than the machine memory ({EnvProbeCommon.formatBytes(total)}).",
                "The workload parallelism should be planned according to the container memory limit rather than the machine memory size.")
        End If
    End Sub

    Private Sub checkScaling(report As EnvProbeReport)
        Dim counts As Integer() = TryCast(
            report.getRaw(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_THREADS), Integer())
        Dim efficiency As Double() = TryCast(
            report.getRaw(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_EFFICIENCY), Double())
        Dim throughput As Double() = TryCast(
            report.getRaw(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_SCALING), Double())

        If counts Is Nothing OrElse efficiency Is Nothing Then
            Return
        End If

        If counts.Length <> efficiency.Length OrElse counts.Length = 0 Then
            Return
        End If

        Dim first As Integer = -1

        For i As Integer = 0 To counts.Length - 1
            If counts(i) >= 4 AndAlso efficiency(i) >= 0 AndAlso efficiency(i) < 0.7 Then
                first = i
                Exit For
            End If
        Next

        Dim copy As Double = report.getDouble(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_COPY)
        Dim bandwidth As String = If(copy > 0, $"{EnvProbeCommon.formatNumber(copy, 2)} GB/s", "n/a")

        If first >= 0 Then
            report.warn(
                $"The parallel efficiency drops to {EnvProbeCommon.formatNumber(efficiency(first) * 100, 1)}% at {counts(first)} threads" &
                If(throughput IsNot Nothing AndAlso throughput.Length = counts.Length,
                   $" (peak {EnvProbeCommon.formatNumber(throughput.DefaultIfEmpty(0).Max, 2)} GFLOPS)",
                   "") & ".",
                "The poor scaling is usually caused by: (1) the memory bandwidth saturation " &
                $"(measured copy bandwidth is {bandwidth}); (2) the SMT/hyper-threading sharing of the FPU; " &
                "(3) the turbo frequency drop when all cores are busy; (4) the NUMA remote memory access. " &
                "Try to limit the worker count to the physical core count and keep the data local to the NUMA node.")
        ElseIf counts.Length > 0 Then
            Dim last As Integer = counts.Length - 1

            If efficiency(last) >= 0.7 Then
                report.suggestions.Add(
                    $"The parallel scaling is good: {EnvProbeCommon.formatNumber(efficiency(last) * 100, 1)}% efficiency at {counts(last)} threads " &
                    $"with a memory copy bandwidth of {bandwidth}.")
            End If
        End If
    End Sub

    Private Sub checkNuma(report As EnvProbeReport)
        Dim nodes As Object = report.getRaw(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_NUMA)

        If Not (TypeOf nodes Is Integer) Then
            Return
        End If

        If CInt(nodes) > 1 Then
            report.suggestions.Add(
                $"The system has {CInt(nodes)} NUMA nodes: the memory allocation of a worker thread should stay on the local node. " &
                "Use numactl --localalloc / --interleave, or set the DOTNET_GCgen0size and the thread affinity to keep the data locality.")
        End If
    End Sub

    Private Sub checkNativeLibraries(report As EnvProbeReport)
        If Not EnvProbeCommon.isLinux() Then
            Return
        End If

        Dim openmp As String() = {"libgomp", "libiomp5", "libomp"}
        Dim found As Boolean = False

        For Each name As String In openmp
            Dim status As String = report.getStatus(EnvProbeOS.SECTION_NAME, name)

            If Not String.Equals(status, EnvProbeCommon.NA, StringComparison.OrdinalIgnoreCase) Then
                found = True
            End If
        Next

        If Not found Then
            report.suggestions.Add(
                "No OpenMP runtime (libgomp / libiomp5 / libomp) was found in LD_LIBRARY_PATH: " &
                "the native numeric libraries which depends on OpenMP will fallback to the single thread mode. " &
                "Install libgomp1 (apt) or add the OpenMP runtime directory into LD_LIBRARY_PATH.")
        End If
    End Sub

    Private Sub checkCuda(report As EnvProbeReport)
        Dim cudaPath As String = report.getValue(EnvProbeCUDA.SECTION_NAME, EnvProbeCUDA.ITEM_CUDA_PATH)
        Dim libcuda As String = report.getValue(EnvProbeCUDA.SECTION_NAME, EnvProbeCUDA.ITEM_LIBCUDA)
        Dim devices As Integer = report.getInteger(EnvProbeCUDA.SECTION_NAME, EnvProbeCUDA.ITEM_DEVICE_COUNT)

        If String.IsNullOrWhiteSpace(cudaPath) OrElse String.Equals(cudaPath, EnvProbeCommon.NA, StringComparison.OrdinalIgnoreCase) Then
            If devices <= 0 Then
                report.suggestions.Add(
                    "No CUDA environment was detected: the GPU acceleration is not available on this server, all of the computing will be processed by the CPU.")
            End If

            Return
        End If

        If libcuda Is Nothing OrElse String.Equals(libcuda, "not found", StringComparison.OrdinalIgnoreCase) Then
            report.warn(
                $"The CUDA_PATH environment variable is set to '{cudaPath}' but the CUDA driver library is not found.",
                "Install the NVIDIA driver and verify that libcuda.so.1 / nvcuda.dll is visible to the process. " &
                "An incomplete CUDA installation makes the GPU backend fallback to the CPU silently.")
        End If
    End Sub

    Private Sub summarize(report As EnvProbeReport)
        Dim text As New StringBuilder
        Dim visible As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_VISIBLE)
        Dim machine As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_MACHINE_LOGICAL)
        Dim physical As Integer = report.getInteger(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_PHYSICAL)
        Dim serverGC As Object = report.getRaw(EnvProbeRuntime.SECTION_GC, EnvProbeRuntime.ITEM_SERVER_GC)
        Dim peak As Double = report.getDouble(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_PEAK)
        Dim unavailable As Integer = countUnavailable(report)

        Call text.Append($"Host '{report.host}' exposes {visible} visible logical processors")

        If machine > 0 AndAlso machine <> visible Then
            Call text.Append($" out of {machine} machine logical processors")
        End If
        If physical > 0 Then
            Call text.Append($" ({physical} physical cores)")
        End If

        Call text.Append(". ")
        Call text.Append($"Server GC is {(If(TypeOf serverGC Is Boolean AndAlso CBool(serverGC), "enabled", "disabled"))}. ")

        If peak > 0 Then
            Call text.Append($"The measured peak throughput is {EnvProbeCommon.formatNumber(peak, 2)} GFLOPS. ")
        End If

        If report.warnings.Count = 0 Then
            Call text.Append("No obvious environment bottleneck was detected.")
        Else
            Call text.Append($"{report.warnings.Count} potential bottleneck(s) were detected, see the warnings and suggestions below.")
        End If

        If unavailable > 0 Then
            Call text.Append($" {unavailable} probe item(s) are not available on this platform and were marked as '{EnvProbeCommon.NA}'.")
        End If

        report.conclusion = text.ToString
    End Sub

    Private Function countUnavailable(report As EnvProbeReport) As Integer
        Dim count As Integer = 0

        For Each section As EnvProbeSection In report.sections
            For Each item As EnvProbeItem In section.items
                If String.Equals(item.status, EnvProbeCommon.NA, StringComparison.OrdinalIgnoreCase) Then
                    count += 1
                End If
            Next
        Next

        Return count
    End Function
End Module
