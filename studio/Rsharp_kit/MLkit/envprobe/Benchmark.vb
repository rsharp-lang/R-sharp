Imports System.Diagnostics
Imports System.Linq
Imports System.Numerics
Imports System.Threading.Tasks

''' <summary>
''' Run a small set of micro benchmarks which is used for measuring the real usable 
''' computing power of current server.
''' 
''' The benchmark is designed for answering the question of "why the program can use 
''' all of the cpu resources on the development machine but only a part of the cpu 
''' resources on the production server":
''' 
''' + the ``scalar`` vs ``SIMD`` throughput ratio tells that the CLR JIT compiler is 
'''   really emitting the vector instructions or not;
''' + the thread scaling curve tells that the parallel computing is limited by the 
'''   memory bandwidth, the hyper-threading or the cpu frequency scaling;
''' + the memory bandwidth tells the theoretical upper bound of the data parallelism.
''' </summary>
Public Module EnvProbeBenchmark

    Public Const SECTION_NAME As String = "benchmark"

    Public Const ITEM_SCALAR As String = "Scalar throughput (GFLOPS)"
    Public Const ITEM_SIMD As String = "SIMD throughput (GFLOPS)"
    Public Const ITEM_SPEEDUP As String = "SIMD speed-up"
    Public Const ITEM_SCALING As String = "Thread scaling (GFLOPS)"
    Public Const ITEM_THREADS As String = "Thread counts"
    Public Const ITEM_EFFICIENCY As String = "Parallel efficiency"
    Public Const ITEM_PEAK As String = "Peak throughput (GFLOPS)"
    Public Const ITEM_COPY As String = "Memory copy bandwidth (GB/s)"
    Public Const ITEM_TRIAD As String = "Memory triad bandwidth (GB/s)"
    Public Const ITEM_ELAPSED As String = "Benchmark elapsed (ms)"

    ''' <summary>
    ''' the benchmark vector buffer size: 65536 * 8 bytes = 512KB, which should be 
    ''' fit inside the L2 cache of the modern server processor
    ''' </summary>
    Private Const VECTOR_SIZE As Integer = 1 << 16
    Private Const MEMORY_SIZE As Integer = 2_000_000
    Private Const SIMD_ITERATIONS As Integer = 1200
    Private Const SCALAR_ITERATIONS As Integer = 300
    Private Const MEMORY_PASSES As Integer = 8

    ''' <summary>
    ''' the benchmark result sink, this field is used for preventing the JIT compiler
    ''' from removing the benchmark kernel
    ''' </summary>
    Private sink As Double

    ''' <summary>
    ''' the benchmark measurement result.
    ''' </summary>
    Public Class EnvProbeBenchResult
        Public Property scalarGflops As Double
        Public Property simdGflops As Double
        Public Property simdSpeedup As Double
        Public Property threadCounts As Integer()
        Public Property threadGflops As Double()
        Public Property efficiency As Double()
        Public Property peakGflops As Double
        Public Property copyBandwidth As Double
        Public Property triadBandwidth As Double
        Public Property elapsedMs As Double
    End Class

    ''' <summary>
    ''' run the benchmark and append the result into the given report object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <param name="threads">the max thread count for the thread scaling test, 
    ''' 0 means use all of the visible logical processors.</param>
    ''' <returns></returns>
    Public Function probe(report As EnvProbeReport, Optional threads As Integer = 0) As EnvProbeSection
        Dim section As EnvProbeSection = report.getSection(SECTION_NAME)

        Try
            Dim result As EnvProbeBenchResult = run(threads)
            Dim scalarItem As EnvProbeItem =
                section.add(ITEM_SCALAR, EnvProbeCommon.formatNumber(result.scalarGflops, 3))
            Dim simdItem As EnvProbeItem =
                section.add(ITEM_SIMD, EnvProbeCommon.formatNumber(result.simdGflops, 3))
            Dim speedupItem As EnvProbeItem =
                section.add(ITEM_SPEEDUP, EnvProbeCommon.formatNumber(result.simdSpeedup, 2) & "x")
            Dim scalingItem As EnvProbeItem = section.add(ITEM_SCALING, scalingText(result))
            Dim threadsItem As EnvProbeItem =
                section.add(ITEM_THREADS, String.Join(",", result.threadCounts))
            Dim efficiencyItem As EnvProbeItem =
                section.add(ITEM_EFFICIENCY, efficiencyText(result))
            Dim peakItem As EnvProbeItem =
                section.add(ITEM_PEAK, EnvProbeCommon.formatNumber(result.peakGflops, 3))
            Dim copyItem As EnvProbeItem =
                section.add(ITEM_COPY, EnvProbeCommon.formatNumber(result.copyBandwidth, 2))
            Dim triadItem As EnvProbeItem =
                section.add(ITEM_TRIAD, EnvProbeCommon.formatNumber(result.triadBandwidth, 2))
            Dim elapsedItem As EnvProbeItem =
                section.add(ITEM_ELAPSED, EnvProbeCommon.formatNumber(result.elapsedMs, 0))

            scalarItem.raw = result.scalarGflops
            simdItem.raw = result.simdGflops
            speedupItem.raw = result.simdSpeedup
            scalingItem.raw = result.threadGflops
            threadsItem.raw = result.threadCounts
            efficiencyItem.raw = result.efficiency
            peakItem.raw = result.peakGflops
            copyItem.raw = result.copyBandwidth
            triadItem.raw = result.triadBandwidth
            elapsedItem.raw = result.elapsedMs

            report.benchmark_time = result.elapsedMs
        Catch ex As Exception
            Call section.na(ITEM_SIMD, ex.Message)
            Call section.na(ITEM_COPY, ex.Message)
        End Try

        Return section
    End Function

    Private Function scalingText(result As EnvProbeBenchResult) As String
        If result.threadCounts Is Nothing OrElse result.threadCounts.Length = 0 Then
            Return EnvProbeCommon.NA
        End If

        Dim parts As New List(Of String)

        For i As Integer = 0 To result.threadCounts.Length - 1
            Call parts.Add($"{result.threadCounts(i)}T={EnvProbeCommon.formatNumber(result.threadGflops(i), 2)}")
        Next

        Return String.Join(", ", parts)
    End Function

    Private Function efficiencyText(result As EnvProbeBenchResult) As String
        If result.threadCounts Is Nothing OrElse result.threadCounts.Length = 0 Then
            Return EnvProbeCommon.NA
        End If

        Dim parts As New List(Of String)

        For i As Integer = 0 To result.threadCounts.Length - 1
            Call parts.Add($"{result.threadCounts(i)}T={EnvProbeCommon.formatNumber(result.efficiency(i) * 100, 1)}%")
        Next

        Return String.Join(", ", parts)
    End Function

    ''' <summary>
    ''' run all of the benchmark kernels.
    ''' </summary>
    ''' <param name="threads"></param>
    ''' <returns></returns>
    Public Function run(Optional threads As Integer = 0) As EnvProbeBenchResult
        Dim timer As Stopwatch = Stopwatch.StartNew
        Dim cores As Integer = System.Environment.ProcessorCount

        If threads <= 0 OrElse threads > cores Then
            threads = cores
        End If

        Dim data As Double() = initVector(VECTOR_SIZE)

        ' warm up the JIT compiler
        sink = simdKernel(data, 8)
        sink = scalarKernel(data, 4)
        GC.KeepAlive(data)

        Dim scalarMs As Double = timeKernel(Function() scalarKernel(data, SCALAR_ITERATIONS))
        Dim simdMs As Double = timeKernel(Function() simdKernel(data, SIMD_ITERATIONS))
        Dim flops As Double = VECTOR_SIZE * 2.0

        Dim scalarGflops As Double = gflops(flops * SCALAR_ITERATIONS, scalarMs)
        Dim simdGflops As Double = gflops(flops * SIMD_ITERATIONS, simdMs)
        Dim speedup As Double = If(scalarGflops > 0, simdGflops / scalarGflops, 0)

        Dim counts As Integer() = threadGradient(threads)
        Dim throughput As New List(Of Double)
        Dim efficiency As New List(Of Double)

        For Each n As Integer In counts
            Dim ms As Double = timeParallel(data, n, SIMD_ITERATIONS)
            Dim value As Double = gflops(flops * SIMD_ITERATIONS * n, ms)

            Call throughput.Add(value)

            If throughput.Count = 1 OrElse throughput(0) <= 0 Then
                Call efficiency.Add(1.0)
            Else
                Call efficiency.Add(value / throughput(0) / n)
            End If
        Next

        Dim copy As Double = memoryCopyBandwidth()
        Dim triad As Double = memoryTriadBandwidth()

        timer.Stop()

        Return New EnvProbeBenchResult With {
            .scalarGflops = scalarGflops,
            .simdGflops = simdGflops,
            .simdSpeedup = speedup,
            .threadCounts = counts,
            .threadGflops = throughput.ToArray,
            .efficiency = efficiency.ToArray,
            .peakGflops = throughput.DefaultIfEmpty(0).Max,
            .copyBandwidth = copy,
            .triadBandwidth = triad,
            .elapsedMs = timer.ElapsedMilliseconds
        }
    End Function

    Private Function threadGradient(maxThreads As Integer) As Integer()
        Dim counts As New List(Of Integer)
        Dim n As Integer = 1

        While n < maxThreads
            Call counts.Add(n)
            n *= 2
        End While

        If counts.Count = 0 OrElse counts(counts.Count - 1) <> maxThreads Then
            Call counts.Add(maxThreads)
        End If

        Return counts.ToArray
    End Function

    Private Function initVector(size As Integer) As Double()
        Dim data As Double() = New Double(size - 1) {}
        Dim rand As New Random(seed:=1234567)

        For i As Integer = 0 To data.Length - 1
            data(i) = rand.NextDouble
        Next

        Return data
    End Function

    Private Function simdKernel(data As Double(), iterations As Integer) As Double
        Dim width As Integer = Vector(Of Double).Count
        Dim acc As Vector(Of Double) = Vector(Of Double).Zero
        Dim a As New Vector(Of Double)(1.000001)
        Dim b As New Vector(Of Double)(0.0000001)
        Dim limit As Integer = data.Length - width

        For it As Integer = 1 To iterations
            Dim i As Integer = 0

            While i <= limit
                Dim v As New Vector(Of Double)(data, i)

                acc = acc + v * a + b
                i += width
            End While
        Next

        Return Vector.Dot(acc, acc)
    End Function

    Private Function scalarKernel(data As Double(), iterations As Integer) As Double
        Dim acc As Double = 0
        Dim limit As Integer = data.Length - 1

        For it As Integer = 1 To iterations
            For i As Integer = 0 To limit
                acc += data(i) * 1.000001 + 0.0000001
            Next
        Next

        Return acc
    End Function

    Private Function timeKernel(kernel As Func(Of Double)) As Double
        Dim timer As Stopwatch = Stopwatch.StartNew

        sink = kernel()

        timer.Stop()

        Return Math.Max(timer.Elapsed.TotalMilliseconds, 0.001)
    End Function

    Private Function timeParallel(data As Double(), threads As Integer, iterations As Integer) As Double
        Dim results As Double() = New Double(threads - 1) {}
        Dim timer As Stopwatch = Stopwatch.StartNew

        If threads <= 1 Then
            results(0) = simdKernel(data, iterations)
        Else
            Dim tasks As Task() = New Task(threads - 1) {}

            For i As Integer = 0 To threads - 1
                Dim index As Integer = i

                tasks(i) = Task.Factory.StartNew(
                    Sub()
                        results(index) = simdKernel(data, iterations)
                    End Sub,
                    TaskCreationOptions.LongRunning)
            Next

            Call Task.WaitAll(tasks)
        End If

        timer.Stop()

        Dim total As Double = 0

        For Each value As Double In results
            total += value
        Next

        sink = total

        Return Math.Max(timer.Elapsed.TotalMilliseconds, 0.001)
    End Function

    Private Function memoryCopyBandwidth() As Double
        Dim source As Double() = New Double(MEMORY_SIZE - 1) {}
        Dim target As Double() = New Double(MEMORY_SIZE - 1) {}

        For i As Integer = 0 To 1023
            source(i) = i
        Next

        ' warm up
        Call Array.Copy(source, target, MEMORY_SIZE)

        Dim timer As Stopwatch = Stopwatch.StartNew

        For pass As Integer = 1 To MEMORY_PASSES
            Call Array.Copy(source, target, MEMORY_SIZE)
        Next

        timer.Stop()

        sink = target(MEMORY_SIZE - 1)
        GC.KeepAlive(source)
        GC.KeepAlive(target)

        ' 2 * n * 8 bytes: one read stream and one write stream
        Return bytesPerSecondGigabytes(2.0 * MEMORY_SIZE * 8 * MEMORY_PASSES, timer.Elapsed.TotalSeconds)
    End Function

    Private Function memoryTriadBandwidth() As Double
        Dim a As Double() = New Double(MEMORY_SIZE - 1) {}
        Dim b As Double() = New Double(MEMORY_SIZE - 1) {}
        Dim c As Double() = New Double(MEMORY_SIZE - 1) {}

        For i As Integer = 0 To 1023
            a(i) = i
            b(i) = i * 0.5
        Next

        ' warm up
        Call triad(a, b, c)

        Dim timer As Stopwatch = Stopwatch.StartNew

        For pass As Integer = 1 To MEMORY_PASSES
            Call triad(a, b, c)
        Next

        timer.Stop()

        sink = c(MEMORY_SIZE - 1)
        GC.KeepAlive(a)
        GC.KeepAlive(b)
        GC.KeepAlive(c)

        ' 3 read streams + 1 write stream
        Return bytesPerSecondGigabytes(4.0 * MEMORY_SIZE * 8 * MEMORY_PASSES, timer.Elapsed.TotalSeconds)
    End Function

    Private Sub triad(a As Double(), b As Double(), c As Double())
        Dim width As Integer = Vector(Of Double).Count
        Dim scalar As New Vector(Of Double)(1.0000001)
        Dim limit As Integer = a.Length - width
        Dim i As Integer = 0

        While i <= limit
            Dim va As New Vector(Of Double)(a, i)
            Dim vb As New Vector(Of Double)(b, i)

            Dim vc As Vector(Of Double) = va + vb * scalar

            vc.CopyTo(c, i)
            i += width
        End While

        While i < a.Length
            c(i) = a(i) + b(i) * 1.0000001
            i += 1
        End While
    End Sub

    Private Function gflops(flops As Double, milliseconds As Double) As Double
        If milliseconds <= 0 Then
            Return 0
        End If

        Return flops / (milliseconds / 1000.0) / 1000000000.0
    End Function

    Private Function bytesPerSecondGigabytes(bytes As Double, seconds As Double) As Double
        If seconds <= 0 Then
            Return 0
        End If

        Return bytes / seconds / 1000000000.0
    End Function
End Module
