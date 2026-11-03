Imports System.IO

''' <summary>
''' Collect the CUDA environment information: the driver version, the GPU devices 
''' and the native cuda libraries visibility.
''' 
''' This probe never throws: on a server without any NVIDIA device all of the probe
''' items will be marked as ``n/a`` and the report generation will continue.
''' </summary>
Public Module EnvProbeCUDA

    Public Const SECTION_NAME As String = "cuda"

    Public Const ITEM_AVAILABLE As String = "CUDA available"
    Public Const ITEM_CUDA_PATH As String = "CUDA_PATH"
    Public Const ITEM_CUDA_HOME As String = "CUDA_HOME"
    Public Const ITEM_NVIDIA_SMI As String = "nvidia-smi"
    Public Const ITEM_DRIVER As String = "Driver version"
    Public Const ITEM_CUDA_VERSION As String = "CUDA driver API version"
    Public Const ITEM_DEVICE_COUNT As String = "GPU device count"
    Public Const ITEM_LIBCUDA As String = "Driver library (libcuda/nvcuda)"
    Public Const ITEM_CUDART As String = "CUDA runtime (libcudart)"
    Public Const ITEM_CUBLAS As String = "cuBLAS (libcublas)"
    Public Const ITEM_CUDNN As String = "cuDNN (libcudnn)"

    ''' <summary>
    ''' run the cuda probe and append the result into the given report object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function probe(report As EnvProbeReport) As EnvProbeSection
        Dim section As EnvProbeSection = report.getSection(SECTION_NAME)
        Dim cudaPath As String = System.Environment.GetEnvironmentVariable("CUDA_PATH")
        Dim cudaHome As String = System.Environment.GetEnvironmentVariable("CUDA_HOME")

        If String.IsNullOrWhiteSpace(cudaPath) Then
            Call section.add(ITEM_CUDA_PATH, EnvProbeCommon.NA)
        Else
            Call section.add(ITEM_CUDA_PATH, cudaPath)
        End If

        If String.IsNullOrWhiteSpace(cudaHome) Then
            Call section.add(ITEM_CUDA_HOME, EnvProbeCommon.NA)
        Else
            Call section.add(ITEM_CUDA_HOME, cudaHome)
        End If

        Dim smi As String = EnvProbeCommon.which("nvidia-smi")
        Dim deviceCount As Integer = 0

        If String.IsNullOrWhiteSpace(smi) Then
            Call section.add(ITEM_NVIDIA_SMI, "not found", EnvProbeCommon.WARN)
        Else
            Call section.add(ITEM_NVIDIA_SMI, smi)

            Dim query As String = EnvProbeCommon.runProcess(
                smi,
                "--query-gpu=index,name,memory.total,driver_version --format=csv,noheader",
                timeoutMilliseconds:=10000)

            If String.IsNullOrWhiteSpace(query) Then
                Call section.add(ITEM_DEVICES, EnvProbeCommon.naText("no device was reported by nvidia-smi"),
                                 EnvProbeCommon.WARN)
            Else
                Dim devices As String() = query _
                    .Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries) _
                    .Select(Function(line) line.Trim) _
                    .Where(Function(line) Not String.IsNullOrEmpty(line)) _
                    .ToArray

                deviceCount = devices.Length

                For i As Integer = 0 To devices.Length - 1
                    Dim deviceItem As EnvProbeItem = section.add($"GPU device[{i}]", devices(i))

                    deviceItem.raw = devices(i)
                Next
            End If

            Dim full As String = EnvProbeCommon.runProcess(smi, "", timeoutMilliseconds:=10000)
            Dim driver As String = textAfter(full, "Driver Version:")
            Dim cudaVersion As String = textAfter(full, "CUDA Version:")

            Call section.add(ITEM_DRIVER, EnvProbeCommon.formatText(driver))
            Call section.add(ITEM_CUDA_VERSION, EnvProbeCommon.formatText(cudaVersion))
        End If

        Call section.add(ITEM_DEVICE_COUNT, deviceCount)

        Dim dirs As String() = libraryDirs(cudaPath, cudaHome)
        Dim libcuda As String = findNativeLibrary(dirs, "nvcuda.dll", "libcuda.so.1")
        Dim cudart As String = findNativeLibrary(dirs, "cudart64*.dll", "libcudart.so")
        Dim cublas As String = findNativeLibrary(dirs, "cublas64_*.dll", "libcublas.so")
        Dim cudnn As String = findNativeLibrary(dirs, "cudnn64_*.dll", "libcudnn.so")

        Call addLibrary(section, ITEM_LIBCUDA, libcuda)
        Call addLibrary(section, ITEM_CUDART, cudart)
        Call addLibrary(section, ITEM_CUBLAS, cublas)
        Call addLibrary(section, ITEM_CUDNN, cudnn)

        Dim available As Boolean = deviceCount > 0 AndAlso libcuda IsNot Nothing
        Dim availableItem As EnvProbeItem = section.add(ITEM_AVAILABLE, If(available, "yes", "no"))

        availableItem.raw = available

        Return section
    End Function

    Public Const ITEM_DEVICES As String = "GPU devices"

    Private Sub addLibrary(section As EnvProbeSection, name As String, path As String)
        If path Is Nothing Then
            Call section.add(name, "not found", EnvProbeCommon.WARN)
        Else
            Call section.add(name, path)
        End If
    End Sub

    ''' <summary>
    ''' get the text value which is follows the given marker inside the nvidia-smi output.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="marker"></param>
    ''' <returns></returns>
    Private Function textAfter(text As String, marker As String) As String
        If String.IsNullOrWhiteSpace(text) Then
            Return Nothing
        End If

        Dim i As Integer = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase)

        If i < 0 Then
            Return Nothing
        End If

        Dim rest As String = text.Substring(i + marker.Length).Trim

        ' the value is a single token which is followed by the next column of the 
        ' nvidia-smi table, example as: ``597.06         CUDA Version: 13.2 |``
        Dim values As String() = rest.Split({" "c, vbTab, "|"c, vbCr, vbLf},
                                            StringSplitOptions.RemoveEmptyEntries)

        If values.Length = 0 Then
            Return Nothing
        End If

        Return values(0)
    End Function

    Private Function libraryDirs(cudaPath As String, cudaHome As String) As String()
        Dim dirs As New List(Of String)

        If Not String.IsNullOrWhiteSpace(cudaPath) Then
            Call dirs.Add(Path.Combine(cudaPath, "bin"))
            Call dirs.Add(Path.Combine(cudaPath, "bin", "x64"))
            Call dirs.Add(Path.Combine(cudaPath, "lib64"))
            Call dirs.Add(Path.Combine(cudaPath, "lib", "x64"))
        End If

        If Not String.IsNullOrWhiteSpace(cudaHome) Then
            Call dirs.Add(Path.Combine(cudaHome, "lib64"))
            Call dirs.Add(Path.Combine(cudaHome, "bin"))
        End If

        Dim pathEnv As String = System.Environment.GetEnvironmentVariable("PATH")

        If Not String.IsNullOrWhiteSpace(pathEnv) Then
            For Each part As String In pathEnv.Split(Path.PathSeparator)
                If Not String.IsNullOrWhiteSpace(part) Then
                    Call dirs.Add(part.Trim)
                End If
            Next
        End If

        Dim ldPath As String = System.Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")

        If Not String.IsNullOrWhiteSpace(ldPath) Then
            For Each part As String In ldPath.Split(Path.PathSeparator)
                If Not String.IsNullOrWhiteSpace(part) Then
                    Call dirs.Add(part.Trim)
                End If
            Next
        End If

        Call dirs.AddRange({
            "/usr/local/cuda/lib64",
            "/usr/local/cuda/targets/x86_64-linux/lib",
            "/usr/lib/x86_64-linux-gnu",
            "/usr/lib64",
            "/usr/lib/wsl/lib",
            "C:\Windows\System32"
        })

        Return dirs.Distinct.ToArray
    End Function

    ''' <summary>
    ''' search the native library inside the given directories by the platform specific 
    ''' file name patterns.
    ''' </summary>
    ''' <param name="dirs"></param>
    ''' <param name="windowsPattern"></param>
    ''' <param name="linuxPattern"></param>
    ''' <returns></returns>
    Private Function findNativeLibrary(dirs As String(), windowsPattern As String, linuxPattern As String) As String
        Dim pattern As String = If(EnvProbeCommon.isWindows(), windowsPattern, linuxPattern)

        For Each dir As String In dirs
            Try
                If Not Directory.Exists(dir) Then
                    Continue For
                End If

                Dim files As String() = Directory.GetFiles(dir, pattern)

                If files.Length > 0 Then
                    Array.Sort(files)
                    Return files(files.Length - 1)
                End If
            Catch ex As Exception
                ' access denied or broken symbolic link, just skip it
            End Try
        Next

        Return Nothing
    End Function
End Module
