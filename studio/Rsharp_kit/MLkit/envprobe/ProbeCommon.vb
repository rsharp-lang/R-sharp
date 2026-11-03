Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices

''' <summary>
''' A single normalized probe result entry.
''' 
''' A probe item always carries a human readable <see cref="value"/> for the report and
''' the original typed <see cref="raw"/> value so that the diagnosis rules can reuse the
''' number without parsing the formatted text back again.
''' </summary>
Public Class EnvProbeItem

    ''' <summary>
    ''' the display name of this probe item
    ''' </summary>
    ''' <returns></returns>
    Public Property name As String
    ''' <summary>
    ''' the formatted value which is rendering on the report document
    ''' </summary>
    ''' <returns></returns>
    Public Property value As String
    ''' <summary>
    ''' the original typed value, used by the diagnosis rules
    ''' </summary>
    ''' <returns></returns>
    Public Property raw As Object
    ''' <summary>
    ''' one of the values: ``ok`` / ``warn`` / ``n/a``
    ''' </summary>
    ''' <returns></returns>
    Public Property status As String

    Public Overrides Function ToString() As String
        Return $"{name}: {value}"
    End Function
End Class

''' <summary>
''' A named group of probe items, for example the ``cpu`` section or the ``memory`` section.
''' </summary>
Public Class EnvProbeSection

    Public Property section As String
    Public Property items As New List(Of EnvProbeItem)

    Default Public ReadOnly Property Item(name As String) As EnvProbeItem
        Get
            Return items _
                .FirstOrDefault(Function(i) String.Equals(i.name, name, StringComparison.OrdinalIgnoreCase))
        End Get
    End Property

    ''' <summary>
    ''' append a new probe item into current section
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="value"></param>
    ''' <param name="status"></param>
    ''' <returns></returns>
    Public Function add(name As String, value As Object,
                        Optional status As String = EnvProbeCommon.OK) As EnvProbeItem

        Dim item As New EnvProbeItem With {
            .name = name,
            .value = If(value Is Nothing, EnvProbeCommon.NA, CStr(value)),
            .raw = value,
            .status = status
        }

        Call items.Add(item)

        Return item
    End Function

    ''' <summary>
    ''' append a ``n/a`` item with the given reason
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="reason"></param>
    ''' <returns></returns>
    Public Function na(name As String, reason As String) As EnvProbeItem
        Return add(name, EnvProbeCommon.na(reason), EnvProbeCommon.NA)
    End Function
End Class

''' <summary>
''' The whole environment probe result: all sections plus the diagnosis output.
''' </summary>
Public Class EnvProbeReport

    Public Property host As String
    Public Property timestamp As String
    Public Property sections As New List(Of EnvProbeSection)
    ''' <summary>
    ''' the collected problems which may limits the CPU utilization
    ''' </summary>
    ''' <returns></returns>
    Public Property warnings As New List(Of String)
    ''' <summary>
    ''' the optimization suggestions which is generated from the <see cref="warnings"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property suggestions As New List(Of String)
    ''' <summary>
    ''' the overall conclusion paragraph
    ''' </summary>
    ''' <returns></returns>
    Public Property conclusion As String
    ''' <summary>
    ''' the time cost of the benchmark profiling
    ''' </summary>
    ''' <returns></returns>
    Public Property benchmark_time As Double

    ''' <summary>
    ''' get the target section by its name, a new empty section will be created 
    ''' when the target section is not found.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function getSection(name As String) As EnvProbeSection
        Dim section = sections.FirstOrDefault(
            Function(s) String.Equals(s.section, name, StringComparison.OrdinalIgnoreCase))

        If section Is Nothing Then
            section = New EnvProbeSection With {.section = name}
            Call sections.Add(section)
        End If

        Return section
    End Function

    ''' <summary>
    ''' get the formatted value of a specific probe item
    ''' </summary>
    ''' <param name="section"></param>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function getValue(section As String, name As String) As String
        Dim s = sections.FirstOrDefault(
            Function(x) String.Equals(x.section, section, StringComparison.OrdinalIgnoreCase))

        If s Is Nothing Then
            Return Nothing
        End If

        Return s(name)?.value
    End Function

    ''' <summary>
    ''' get the raw value of a specific probe item
    ''' </summary>
    ''' <param name="section"></param>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function getRaw(section As String, name As String) As Object
        Dim s = sections.FirstOrDefault(
            Function(x) String.Equals(x.section, section, StringComparison.OrdinalIgnoreCase))

        If s Is Nothing Then
            Return Nothing
        End If

        Return s(name)?.raw
    End Function

    ''' <summary>
    ''' get the raw value as integer, returns <paramref name="fallback"/> when 
    ''' the target item is not found or the value is not a valid number.
    ''' </summary>
    ''' <param name="section"></param>
    ''' <param name="name"></param>
    ''' <param name="fallback"></param>
    ''' <returns></returns>
    Public Function getInteger(section As String, name As String,
                               Optional fallback As Integer = -1) As Integer

        Dim raw = getRaw(section, name)

        If raw Is Nothing Then
            Return fallback
        End If

        Try
            Return CInt(raw)
        Catch ex As Exception
            Dim parsed As Integer

            If Integer.TryParse(CStr(raw), parsed) Then
                Return parsed
            Else
                Return fallback
            End If
        End Try
    End Function

    ''' <summary>
    ''' get the raw value as double, returns <paramref name="fallback"/> when 
    ''' the target item is not found or the value is not a valid number.
    ''' </summary>
    ''' <param name="section"></param>
    ''' <param name="name"></param>
    ''' <param name="fallback"></param>
    ''' <returns></returns>
    Public Function getDouble(section As String, name As String,
                              Optional fallback As Double = -1) As Double

        Dim raw = getRaw(section, name)

        If raw Is Nothing Then
            Return fallback
        End If

        Try
            Return CDbl(raw)
        Catch ex As Exception
            Dim parsed As Double

            If Double.TryParse(CStr(raw), NumberStyles.Float, CultureInfo.InvariantCulture, parsed) Then
                Return parsed
            Else
                Return fallback
            End If
        End Try
    End Function

    Public Function getLong(section As String, name As String,
                            Optional fallback As Long = -1) As Long

        Dim raw = getRaw(section, name)

        If raw Is Nothing Then
            Return fallback
        End If

        Try
            Return CLng(raw)
        Catch ex As Exception
            Dim parsed As Long

            If Long.TryParse(CStr(raw), parsed) Then
                Return parsed
            Else
                Return fallback
            End If
        End Try
    End Function

    ''' <summary>
    ''' add a warning message and its optimization suggestion at once
    ''' </summary>
    ''' <param name="warning"></param>
    ''' <param name="suggestion"></param>
    Public Sub warn(warning As String, Optional suggestion As String = Nothing)
        If Not String.IsNullOrWhiteSpace(warning) Then
            Call warnings.Add(warning)
        End If
        If Not String.IsNullOrWhiteSpace(suggestion) Then
            Call suggestions.Add(suggestion)
        End If
    End Sub
End Class

''' <summary>
''' Shared helpers for reading the operation system files, running the external 
''' command line tools and formatting the probe values.
''' </summary>
Public Module EnvProbeCommon

    Public Const OK As String = "ok"
    Public Const WARN As String = "warn"
    Public Const NA As String = "n/a"

    ''' <summary>
    ''' the marker text for the missing value.
    ''' </summary>
    ''' <param name="reason"></param>
    ''' <returns></returns>
    Public Function na(reason As String) As String
        If String.IsNullOrWhiteSpace(reason) Then
            Return NA
        Else
            Return $"{NA} ({reason})"
        End If
    End Function

    Public Function isWindows() As Boolean
        Return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    End Function

    Public Function isLinux() As Boolean
        Return RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
    End Function

    Public Function isMacOS() As Boolean
        Return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
    End Function

    ''' <summary>
    ''' run an external command and capture its full standard output text.
    ''' 
    ''' (returns an empty string when the command is not found, or it was 
    ''' time out, or any exception is thrown. this helper never throws.)
    ''' </summary>
    ''' <param name="command"></param>
    ''' <param name="arguments"></param>
    ''' <param name="timeoutMilliseconds"></param>
    ''' <returns></returns>
    Public Function runProcess(command As String, arguments As String,
                               Optional timeoutMilliseconds As Integer = 5000) As String

        If String.IsNullOrWhiteSpace(command) Then
            Return ""
        End If

        Try
            Dim info As New ProcessStartInfo With {
                .FileName = command,
                .Arguments = arguments,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True
            }

            Using proc As Process = Process.Start(info)
                If proc Is Nothing Then
                    Return ""
                End If

                Dim stdout = proc.StandardOutput.ReadToEndAsync
                Dim stderr = proc.StandardError.ReadToEndAsync

                If Not proc.WaitForExit(timeoutMilliseconds) Then
                    Try
                        Call proc.Kill(entireProcessTree:=True)
                    Catch ex As Exception
                        ' ignore the kill error
                    End Try

                    Return ""
                End If

                Dim outText As String = ""

                Try
                    outText = stdout.Result
                Catch ex As Exception
                    outText = ""
                End Try

                If String.IsNullOrWhiteSpace(outText) Then
                    Try
                        outText = stderr.Result
                    Catch ex As Exception
                        outText = ""
                    End Try
                End If

                Return If(outText, "")
            End Using
        Catch ex As Exception
            ' the target command may not exists on current environment
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' resolve the executable file path from the ``PATH`` environment variable
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function which(name As String) As String
        Try
            Return ProgramPathSearchTool.Which(name)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' read all lines of a procfs/sysfs file, returns ``Nothing`` when the target 
    ''' file is not readable on current environment.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function readProc(path As String) As String()
        Try
            If Not File.Exists(path) Then
                Return Nothing
            End If

            Return File.ReadAllLines(path)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' read the whole text of a procfs/sysfs file
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function readProcText(path As String) As String
        Try
            If Not File.Exists(path) Then
                Return Nothing
            End If

            Return File.ReadAllText(path).Trim
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' get the ``Key: value`` field from the given text lines, the key matching is 
    ''' case insensitive. returns ``Nothing`` when the field is not found.
    ''' </summary>
    ''' <param name="lines"></param>
    ''' <param name="key"></param>
    ''' <returns></returns>
    Public Function procField(lines As String(), key As String) As String
        If lines Is Nothing Then
            Return Nothing
        End If

        For Each line As String In lines
            Dim i As Integer = line.IndexOf(":"c)

            If i < 0 Then
                Continue For
            End If

            If String.Equals(line.Substring(0, i).Trim, key, StringComparison.OrdinalIgnoreCase) Then
                Return line.Substring(i + 1).Trim
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' parse a size text like ``16384000 kB`` into the byte count.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function parseSizeToBytes(text As String) As Long
        If String.IsNullOrWhiteSpace(text) Then
            Return -1
        End If

        Dim tokens As String() = text.Trim.Split({" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)
        Dim digits As New StringBuilder

        For Each ch As Char In tokens(0)
            If Char.IsDigit(ch) OrElse ch = "."c Then
                Call digits.Append(ch)
            Else
                Exit For
            End If
        Next

        Dim num As Double

        If Not Double.TryParse(digits.ToString, NumberStyles.Float, CultureInfo.InvariantCulture, num) Then
            Return -1
        End If

        If tokens.Length > 1 Then
            Select Case tokens(1).ToLower
                Case "kb", "kib" : num *= 1024
                Case "mb", "mib" : num *= 1024 * 1024
                Case "gb", "gib" : num *= 1024& * 1024 * 1024
            End Select
        End If

        Return CLng(num)
    End Function

    ''' <summary>
    ''' format the byte count into a human readable text, example as ``31.9 GB``
    ''' </summary>
    ''' <param name="bytes"></param>
    ''' <returns></returns>
    Public Function formatBytes(bytes As Long) As String
        If bytes < 0 Then
            Return NA
        End If

        Const kb As Double = 1024
        Const mb As Double = kb * 1024
        Const gb As Double = mb * 1024
        Const tb As Double = gb * 1024

        Select Case bytes
            Case Is >= CLng(tb) : Return formatNumber(bytes / tb) & " TB"
            Case Is >= CLng(gb) : Return formatNumber(bytes / gb) & " GB"
            Case Is >= CLng(mb) : Return formatNumber(bytes / mb) & " MB"
            Case Is >= CLng(kb) : Return formatNumber(bytes / kb) & " KB"
            Case Else : Return bytes & " B"
        End Select
    End Function

    ''' <summary>
    ''' format the number value in invariant culture
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="digits"></param>
    ''' <returns></returns>
    Public Function formatNumber(value As Double, Optional digits As Integer = 2) As String
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            Return NA
        End If

        Return value.ToString("F" & digits, CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' format a text value, an ``n/a`` marker will be returned for the empty text
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function formatText(text As String) As String
        If String.IsNullOrWhiteSpace(text) Then
            Return NA
        Else
            Return text.Trim
        End If
    End Function

    ''' <summary>
    ''' get the first non-empty line of the given text
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function firstLine(text As String) As String
        If String.IsNullOrWhiteSpace(text) Then
            Return Nothing
        End If

        Return text _
            .Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries) _
            .Select(Function(line) line.Trim) _
            .FirstOrDefault(Function(line) Not String.IsNullOrEmpty(line))
    End Function

    ''' <summary>
    ''' split a text value like ``sse2 sse3 avx avx2`` or ``sse2,sse3`` into 
    ''' the token array with a lower case and trimmed style.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function tokens(text As String) As String()
        If String.IsNullOrWhiteSpace(text) Then
            Return New String() {}
        End If

        Return text _
            .Split({" "c, ","c, vbTab, ";"c}, StringSplitOptions.RemoveEmptyEntries) _
            .Select(Function(s) s.Trim.ToLower) _
            .Where(Function(s) Not String.IsNullOrEmpty(s)) _
            .ToArray
    End Function

    ''' <summary>
    ''' check that the target flag is exists in the given flags token array
    ''' </summary>
    ''' <param name="flags"></param>
    ''' <param name="flag"></param>
    ''' <returns></returns>
    Public Function hasFlag(flags As String(), flag As String) As Boolean
        If flags Is Nothing OrElse flags.Length = 0 Then
            Return False
        End If

        Return flags.Any(Function(f) String.Equals(f, flag, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>
    ''' write the text content to a file with the UTF8 encoding without BOM
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function saveText(path As String, text As String) As String
        Dim file As String = IO.Path.GetFullPath(path)
        Dim dir As String = IO.Path.GetDirectoryName(file)

        If Not String.IsNullOrWhiteSpace(dir) AndAlso Not IO.Directory.Exists(dir) Then
            Call IO.Directory.CreateDirectory(dir)
        End If

        Call File.WriteAllText(file, text, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

        Return file
    End Function
End Module
