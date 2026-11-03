Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' Production server environment diagnostics for the R# data science program.
''' 
''' The ``envprobe`` package is designed for answering one question: why the data science
''' program can use all of the computing resources of the development machine but only a 
''' part of the cpu resources of the production server.
''' 
''' It collects all of the environment factors which may be affects the cpu utilization
''' rate:
''' 
''' + the operation system, kernel and the native software library versions;
''' + the cpu model, the physical/visible core count, the process affinity and the 
'''   container (cgroup) resource quotas;
''' + the SIMD instruction set capability and the measured SIMD/thread scaling 
'''   performance;
''' + the physical memory capacity and the measured memory bandwidth;
''' + the .NET CLR garbage collector and the thread pool configuration;
''' + the CUDA driver and the GPU devices.
''' 
''' ```r
''' imports "envprobe" from "MLkit";
''' 
''' let info = envprobe.report();
''' print(info$diagnosis$warnings);
''' ```
''' </summary>
<Package("envprobe", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gcmodeller.org")>
Public Module envprobe

    ''' <summary>
    ''' Collect all of the environment diagnosis information of current server.
    ''' 
    ''' The result is a nested list object which contains the ``os`` / ``cpu`` / ``simd`` /
    ''' ``memory`` / ``runtime`` / ``gc`` / ``threadpool`` / ``cuda`` / ``benchmark`` 
    ''' sections and the ``diagnosis`` section.
    ''' </summary>
    ''' <param name="file">
    ''' optional, the file path of the markdown text report. the file extension should be 
    ''' ``.md`` / ``.markdown`` / ``.txt``.
    ''' </param>
    ''' <param name="json">
    ''' optional, the file path of the JSON report, which is designed for the batch 
    ''' collection from the multiple production servers.
    ''' </param>
    ''' <param name="benchmark">
    ''' run the cpu/memory micro benchmark or not? default is ``TRUE``, this benchmark 
    ''' will takes about 1-5 seconds.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' this function returns a list object with the slots: 
    ''' ``host``, ``timestamp``, ``os``, ``cpu``, ``simd``, ``memory``, ``runtime``, 
    ''' ``gc``, ``threadpool``, ``cuda``, ``benchmark`` and ``diagnosis``.
    ''' </returns>
    <ExportAPI("report")>
    <RApiReturn("host", "timestamp", "os", "cpu", "simd", "memory", "runtime",
                "gc", "threadpool", "cuda", "benchmark", "diagnosis")>
    Public Function report(Optional file As String = Nothing,
                           Optional json As String = Nothing,
                           Optional benchmark As Boolean = True,
                           Optional env As Environment = Nothing) As Object

        Try
            Dim probe As EnvProbeReport = collect(benchmark)
            Dim result As list = DirectCast(EnvProbeReportBuilder.buildList(probe), list)

            If Not String.IsNullOrWhiteSpace(file) Then
                result.slots("report_file") = EnvProbeReportBuilder.saveMarkdown(probe, file)
            End If

            If Not String.IsNullOrWhiteSpace(json) Then
                result.slots("json_file") = EnvProbeReportBuilder.saveJson(probe, json)
            End If

            Return result
        Catch ex As Exception
            Return RInternal.debug.stop(ex, env)
        End Try
    End Function

    ''' <summary>
    ''' Collect the environment diagnosis information and returns the markdown content 
    ''' blocks, which can be rendered into the md/html/docx/pdf/rtf/xps document via 
    ''' the ``report::render`` api:
    ''' 
    ''' ```r
    ''' imports "envprobe" from "MLkit";
    ''' imports "report" from "base";
    ''' 
    ''' report::render(envprobe.blocks(), "server_diagnostics.docx");
    ''' ```
    ''' </summary>
    ''' <param name="benchmark"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("blocks")>
    Public Function blocks(Optional benchmark As Boolean = True,
                           Optional env As Environment = Nothing) As Object

        Try
            Return EnvProbeReportBuilder.buildBlocks(collect(benchmark))
        Catch ex As Exception
            Return RInternal.debug.stop(ex, env)
        End Try
    End Function

    ''' <summary>
    ''' Run the cpu / memory micro benchmark only.
    ''' 
    ''' ```r
    ''' let result = envprobe.benchmark();
    ''' print(result$peak_gflops);
    ''' ```
    ''' </summary>
    ''' <param name="threads">
    ''' the maximum thread count of the thread scaling test, the default value ``0`` 
    ''' means use all of the visible logical processors.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("benchmark")>
    <RApiReturn("scalar_gflops", "simd_gflops", "simd_speedup", "threads",
                "thread_gflops", "efficiency", "peak_gflops", "copy_gbps",
                "triad_gbps", "elapsed_ms")>
    Public Function benchmark(Optional threads As Integer = 0,
                              Optional env As Environment = Nothing) As Object

        Try
            Dim result As EnvProbeBenchmark.EnvProbeBenchResult = EnvProbeBenchmark.run(threads)

            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"scalar_gflops", result.scalarGflops},
                    {"simd_gflops", result.simdGflops},
                    {"simd_speedup", result.simdSpeedup},
                    {"threads", result.threadCounts},
                    {"thread_gflops", result.threadGflops},
                    {"efficiency", result.efficiency},
                    {"peak_gflops", result.peakGflops},
                    {"copy_gbps", result.copyBandwidth},
                    {"triad_gbps", result.triadBandwidth},
                    {"elapsed_ms", result.elapsedMs}
                }
            }
        Catch ex As Exception
            Return RInternal.debug.stop(ex, env)
        End Try
    End Function

    ''' <summary>
    ''' run all of the probes and the diagnosis rules.
    ''' </summary>
    ''' <param name="benchmark"></param>
    ''' <returns></returns>
    Private Function collect(benchmark As Boolean) As EnvProbeReport
        Dim report As New EnvProbeReport With {
            .host = System.Environment.MachineName,
            .timestamp = Now.ToString("yyyy-MM-dd HH:mm:ss")
        }

        Call runSafe(report, EnvProbeOS.SECTION_NAME, Sub() EnvProbeOS.probe(report))
        Call runSafe(report, EnvProbeCPU.SECTION_NAME, Sub() EnvProbeCPU.probe(report))
        Call runSafe(report, EnvProbeSIMD.SECTION_NAME, Sub() EnvProbeSIMD.probe(report))
        Call runSafe(report, EnvProbeMemory.SECTION_NAME, Sub() EnvProbeMemory.probe(report))
        Call runSafe(report, EnvProbeRuntime.SECTION_NAME, Sub() EnvProbeRuntime.probe(report))
        Call runSafe(report, EnvProbeCUDA.SECTION_NAME, Sub() EnvProbeCUDA.probe(report))

        If benchmark Then
            Call runSafe(report, EnvProbeBenchmark.SECTION_NAME, Sub() EnvProbeBenchmark.probe(report))
        End If

        Call runSafe(report, "diagnosis", Sub() EnvProbeDiagnosis.diagnose(report))

        Return report
    End Function

    ''' <summary>
    ''' run a probe job, a failed probe will never breaks the whole report generation.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <param name="section"></param>
    ''' <param name="job"></param>
    Private Sub runSafe(report As EnvProbeReport, section As String, job As Action)
        Try
            Call job()
        Catch ex As Exception
            Call report.getSection(section).na("probe failed", ex.Message)
            Call report.warnings.Add($"The '{section}' probe failed: {ex.Message}")
        End Try
    End Sub
End Module
