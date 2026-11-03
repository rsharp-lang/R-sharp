Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

''' <summary>
''' Collect the operation system level information: the distribution, the kernel version
''' and the native software libraries which may affects the performance of the scientific
''' computing runtime (OpenMP/BLAS/MKL and the C runtime library).
''' </summary>
Public Module EnvProbeOS

    Public Const SECTION_NAME As String = "os"

    Public Const ITEM_PLATFORM As String = "Platform"
    Public Const ITEM_OS As String = "Operating system"
    Public Const ITEM_DISTRIBUTION As String = "Distribution"
    Public Const ITEM_DISTRIBUTION_ID As String = "Distribution id"
    Public Const ITEM_VERSION_ID As String = "Distribution version"
    Public Const ITEM_KERNEL As String = "Kernel"
    Public Const ITEM_GLIBC As String = "C runtime (glibc)"
    Public Const ITEM_OS_ARCH As String = "OS architecture"
    Public Const ITEM_PROCESS_ARCH As String = "Process architecture"
    Public Const ITEM_PROCESS_64BIT As String = "64-bit process"
    Public Const ITEM_HOST As String = "Host name"
    Public Const ITEM_CLR As String = "CLR version"
    Public Const ITEM_FRAMEWORK As String = "Runtime framework"
    Public Const ITEM_LD_LIBRARY As String = "LD_LIBRARY_PATH"

    ''' <summary>
    ''' the native libraries which may provides the parallel computing acceleration
    ''' </summary>
    Public ReadOnly NativeLibraries As String() = {
        "libgomp",
        "libiomp5",
        "libomp",
        "libmkl_rt",
        "libopenblas",
        "libblas",
        "liblapack",
        "libtbb"
    }

    ''' <summary>
    ''' run the operation system probe and append the result into the given report object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function probe(report As EnvProbeReport) As EnvProbeSection
        Dim section As EnvProbeSection = report.getSection(SECTION_NAME)

        Call section.add(ITEM_HOST, System.Environment.MachineName)
        Call section.add(ITEM_OS, RuntimeInformation.OSDescription)
        Call section.add(ITEM_OS_ARCH, RuntimeInformation.OSArchitecture.ToString)
        Call section.add(ITEM_PROCESS_ARCH, RuntimeInformation.ProcessArchitecture.ToString)
        Call section.add(ITEM_PROCESS_64BIT, System.Environment.Is64BitProcess)
        Call section.add(ITEM_CLR, System.Environment.Version.ToString)
        Call section.add(ITEM_FRAMEWORK, RuntimeInformation.FrameworkDescription)

        If EnvProbeCommon.isLinux() Then
            Call section.add(ITEM_PLATFORM, "linux")
            Call probeLinux(section, report)
        ElseIf EnvProbeCommon.isWindows() Then
            Call section.add(ITEM_PLATFORM, "windows")
            Call probeWindows(section, report)
        Else
            Call section.add(ITEM_PLATFORM, RuntimeInformation.OSDescription)
            Call section.na(ITEM_DISTRIBUTION, "unsupported platform")
        End If

        Return section
    End Function

    Private Sub probeLinux(section As EnvProbeSection, report As EnvProbeReport)
        Dim release As String() = EnvProbeCommon.readProc("/etc/os-release")

        If release Is Nothing Then
            Call section.na(ITEM_DISTRIBUTION, "missing /etc/os-release")
        Else
            Call section.add(ITEM_DISTRIBUTION, EnvProbeCommon.formatText(releaseValue(release, "PRETTY_NAME")))
            Call section.add(ITEM_DISTRIBUTION_ID, EnvProbeCommon.formatText(releaseValue(release, "ID")))
            Call section.add(ITEM_VERSION_ID, EnvProbeCommon.formatText(releaseValue(release, "VERSION_ID")))
        End If

        Dim kernel As String = EnvProbeCommon.readProcText("/proc/version")

        If kernel Is Nothing Then
            kernel = EnvProbeCommon.runProcess("uname", "-a")
        End If

        Call section.add(ITEM_KERNEL, EnvProbeCommon.formatText(EnvProbeCommon.firstLine(kernel)))

        Dim glibc As String = EnvProbeCommon.firstLine(EnvProbeCommon.runProcess("ldd", "--version"))

        If String.IsNullOrWhiteSpace(glibc) Then
            glibc = EnvProbeCommon.firstLine(EnvProbeCommon.runProcess("libc.so.6", ""))
        End If

        If String.IsNullOrWhiteSpace(glibc) Then
            Call section.na(ITEM_GLIBC, "ldd not available")
        Else
            Call section.add(ITEM_GLIBC, glibc)
        End If

        Dim ldPath As String = System.Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")

        If String.IsNullOrWhiteSpace(ldPath) Then
            Call section.add(ITEM_LD_LIBRARY, EnvProbeCommon.naText("not defined"))
        Else
            Call section.add(ITEM_LD_LIBRARY, ldPath)
        End If

        Dim dirs As String() = librarySearchDirs(ldPath)
        Dim missing As New List(Of String)

        For Each name As String In NativeLibraries
            Dim found As String = findSharedLibrary(name, dirs)

            If found Is Nothing Then
                Call section.add(name, "not found", EnvProbeCommon.WARN)
                Call missing.Add(name)
            Else
                Call section.add(name, found)
            End If
        Next
    End Sub

    Private Sub probeWindows(section As EnvProbeSection, report As EnvProbeReport)
        Dim osInfo As OperatingSystem = System.Environment.OSVersion

        Call section.add(ITEM_DISTRIBUTION, osInfo.VersionString)
        Call section.add(ITEM_VERSION_ID, osInfo.Version.ToString)
        Call section.add(ITEM_KERNEL, EnvProbeCommon.formatText($"{osInfo.VersionString} {osInfo.ServicePack}"))

        Dim is64BitOS As String = If(System.Environment.Is64BitOperatingSystem, "yes", "no")
        Call section.add("64-bit operating system", is64BitOS)

        Call section.add(ITEM_LD_LIBRARY, EnvProbeCommon.naText("LD_LIBRARY_PATH is a linux only setting"))
    End Sub

    Private Function releaseValue(lines As String(), key As String) As String
        For Each line As String In lines
            Dim i As Integer = line.IndexOf("="c)

            If i < 0 Then
                Continue For
            End If

            If String.Equals(line.Substring(0, i).Trim, key, StringComparison.OrdinalIgnoreCase) Then
                Return line.Substring(i + 1).Trim.Trim(""""c)
            End If
        Next

        Return Nothing
    End Function

    Private Function librarySearchDirs(ldPath As String) As String()
        Dim dirs As New List(Of String)

        If Not String.IsNullOrWhiteSpace(ldPath) Then
            For Each part As String In ldPath.Split(IO.Path.PathSeparator)
                If Not String.IsNullOrWhiteSpace(part) Then
                    Call dirs.Add(part.Trim)
                End If
            Next
        End If

        Call dirs.AddRange({
            "/usr/lib/x86_64-linux-gnu",
            "/usr/lib/aarch64-linux-gnu",
            "/lib/x86_64-linux-gnu",
            "/lib64",
            "/usr/lib64",
            "/usr/lib",
            "/usr/local/lib",
            "/usr/local/lib64",
            "/opt/intel/oneapi/mkl/latest/lib/intel64",
            "/opt/intel/mkl/lib/intel64"
        })

        Return dirs.Distinct.ToArray
    End Function

    ''' <summary>
    ''' search the shared library file (``libname.so`` or ``libname.so.x.y``) inside the 
    ''' given directories. returns ``Nothing`` when the target library is not found.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="dirs"></param>
    ''' <returns></returns>
    Public Function findSharedLibrary(name As String, dirs As String()) As String
        For Each dir As String In dirs
            Try
                If Not Directory.Exists(dir) Then
                    Continue For
                End If

                Dim files As String() = Directory.GetFiles(dir, name & ".so*")

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
