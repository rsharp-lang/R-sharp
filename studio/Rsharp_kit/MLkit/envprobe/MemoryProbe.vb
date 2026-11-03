Imports System.Diagnostics
Imports System.Runtime.InteropServices

''' <summary>
''' Collect the physical memory information.
''' 
''' The memory capacity is directly limits the possible degree of parallelism of the 
''' data science program: a server with the plenty cpu cores but the small memory 
''' usually can not uses all of its cpu resources.
''' </summary>
Public Module EnvProbeMemory

    Public Const SECTION_NAME As String = "memory"

    Public Const ITEM_TOTAL As String = "Total physical memory"
    Public Const ITEM_AVAILABLE As String = "Available physical memory"
    Public Const ITEM_USED As String = "Memory in use (%)"
    Public Const ITEM_SWAP_TOTAL As String = "Swap total"
    Public Const ITEM_SWAP_FREE As String = "Swap available"
    Public Const ITEM_PAGE_SIZE As String = "System page size"
    Public Const ITEM_HUGE_PAGE As String = "Huge page size"
    Public Const ITEM_WORKING_SET As String = "Process working set"
    Public Const ITEM_GC_AVAILABLE As String = "Memory available for GC"

    ''' <summary>
    ''' run the memory probe and append the result into the given report object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function probe(report As EnvProbeReport) As EnvProbeSection
        Dim section As EnvProbeSection = report.getSection(SECTION_NAME)
        Dim total As Long = -1
        Dim available As Long = -1

        If EnvProbeCommon.isLinux() Then
            Call probeLinux(section, total, available)
        ElseIf EnvProbeCommon.isWindows() Then
            Call probeWindows(section, total, available)
        End If

        If total <= 0 OrElse available < 0 Then
            ' fallback to the CLR level memory information
            Dim info As GCMemoryInfo = GC.GetGCMemoryInfo

            If total <= 0 Then
                total = info.TotalAvailableMemoryBytes
                Call section.add(ITEM_TOTAL, EnvProbeCommon.formatBytes(total))
            End If

            If available < 0 Then
                available = Math.Max(0, info.TotalAvailableMemoryBytes - info.HeapSizeBytes)
                Call section.add(ITEM_AVAILABLE, EnvProbeCommon.formatBytes(available))
            End If
        End If

        If total > 0 AndAlso available >= 0 Then
            Dim used As Double = 100.0 * (total - available) / total
            Dim usedItem As EnvProbeItem = section.add(ITEM_USED, EnvProbeCommon.formatNumber(used, 1))

            usedItem.raw = used
        End If

        Call section.add(ITEM_PAGE_SIZE, EnvProbeCommon.formatBytes(System.Environment.SystemPageSize))

        Try
            Call section.add(ITEM_WORKING_SET, EnvProbeCommon.formatBytes(Process.GetCurrentProcess.WorkingSet64))
        Catch ex As Exception
            Call section.na(ITEM_WORKING_SET, ex.Message)
        End Try

        Return section
    End Function

    Private Sub probeLinux(section As EnvProbeSection, ByRef total As Long, ByRef available As Long)
        Dim memInfo As String() = EnvProbeCommon.readProc("/proc/meminfo")

        If memInfo Is Nothing Then
            Call section.na(ITEM_TOTAL, "missing /proc/meminfo")
            Call section.na(ITEM_AVAILABLE, "missing /proc/meminfo")

            Return
        End If

        total = EnvProbeCommon.parseSizeToBytes(EnvProbeCommon.procField(memInfo, "MemTotal"))
        available = EnvProbeCommon.parseSizeToBytes(EnvProbeCommon.procField(memInfo, "MemAvailable"))

        If available < 0 Then
            available = EnvProbeCommon.parseSizeToBytes(EnvProbeCommon.procField(memInfo, "MemFree"))
        End If

        Dim swapTotal As Long = EnvProbeCommon.parseSizeToBytes(EnvProbeCommon.procField(memInfo, "SwapTotal"))
        Dim swapFree As Long = EnvProbeCommon.parseSizeToBytes(EnvProbeCommon.procField(memInfo, "SwapFree"))
        Dim hugePageSize As Long = EnvProbeCommon.parseSizeToBytes(EnvProbeCommon.procField(memInfo, "Hugepagesize"))
        Dim hugePages As String = EnvProbeCommon.procField(memInfo, "HugePages_Total")

        TouchLong(section.add(ITEM_TOTAL, EnvProbeCommon.formatBytes(total)), total)

        If available < 0 Then
            Call section.na(ITEM_AVAILABLE, "MemAvailable is not readable")
        Else
            TouchLong(section.add(ITEM_AVAILABLE, EnvProbeCommon.formatBytes(available)), available)
        End If

        If swapTotal <= 0 Then
            Call section.add(ITEM_SWAP_TOTAL, "not configured")
            TouchLong(section.add(ITEM_SWAP_FREE, "-"), 0)
        Else
            TouchLong(section.add(ITEM_SWAP_TOTAL, EnvProbeCommon.formatBytes(swapTotal)), swapTotal)
            TouchLong(section.add(ITEM_SWAP_FREE, EnvProbeCommon.formatBytes(swapFree)), swapFree)
        End If

        If hugePageSize > 0 Then
            Dim hugeItem As EnvProbeItem =
                section.add(ITEM_HUGE_PAGE, EnvProbeCommon.formatBytes(hugePageSize) & $" x {If(hugePages, "0")} pages")

            hugeItem.raw = hugePageSize
        Else
            Call section.na(ITEM_HUGE_PAGE, "not supported")
        End If
    End Sub

    Private Sub probeWindows(section As EnvProbeSection, ByRef total As Long, ByRef available As Long)
        Try
            Dim status As New MEMORYSTATUSEX With {
                .dwLength = CUInt(Marshal.SizeOf(Of MEMORYSTATUSEX)())
            }

            If GlobalMemoryStatusEx(status) Then
                total = CLng(status.ullTotalPhys)
                available = CLng(status.ullAvailPhys)

                TouchLong(section.add(ITEM_TOTAL, EnvProbeCommon.formatBytes(total)), total)
                TouchLong(section.add(ITEM_AVAILABLE, EnvProbeCommon.formatBytes(available)), available)

                Dim pageFile As Long = CLng(status.ullTotalPageFile) - total
                Dim pageFileFree As Long = Math.Max(0, CLng(status.ullAvailPageFile) - available)

                If pageFile > 0 Then
                    TouchLong(section.add(ITEM_SWAP_TOTAL, EnvProbeCommon.formatBytes(pageFile)), pageFile)
                    TouchLong(section.add(ITEM_SWAP_FREE, EnvProbeCommon.formatBytes(pageFileFree)), pageFileFree)
                Else
                    Call section.add(ITEM_SWAP_TOTAL, "not configured")
                    Call section.add(ITEM_SWAP_FREE, "-")
                End If
            Else
                Call section.na(ITEM_TOTAL, "GlobalMemoryStatusEx failed")
                Call section.na(ITEM_AVAILABLE, "GlobalMemoryStatusEx failed")
            End If
        Catch ex As Exception
            Call section.na(ITEM_TOTAL, ex.Message)
            Call section.na(ITEM_AVAILABLE, ex.Message)
        End Try
    End Sub

    Private Sub TouchLong(item As EnvProbeItem, value As Long)
        If item IsNot Nothing Then
            item.raw = value
        End If
    End Sub

    <StructLayout(LayoutKind.Sequential)>
    Private Structure MEMORYSTATUSEX
        Public dwLength As UInteger
        Public dwMemoryLoad As UInteger
        Public ullTotalPhys As ULong
        Public ullAvailPhys As ULong
        Public ullTotalPageFile As ULong
        Public ullAvailPageFile As ULong
        Public ullTotalVirtual As ULong
        Public ullAvailVirtual As ULong
        Public ullAvailExtendedVirtual As ULong
    End Structure

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function GlobalMemoryStatusEx(ByRef lpBuffer As MEMORYSTATUSEX) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function
End Module
