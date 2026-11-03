Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' Render the probe result into the different report formats:
''' 
''' + the <see cref="Block"/> array, which can be rendered into md/html/docx/pdf/rtf/xps
'''   documents via the ``report::render`` api in R#;
''' + the markdown text report;
''' + the JSON text content, which is designed for the batch collection from the 
'''   multiple production servers;
''' + the R# ``list`` object for the script level analyzing.
''' </summary>
Public Module EnvProbeReportBuilder

    Private ReadOnly Title As String = "Production Server Environment Diagnostics"

    ''' <summary>
    ''' build the markdown content blocks of the given probe report.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function buildBlocks(report As EnvProbeReport) As Block()
        Dim blocks As New List(Of Block)

        Call blocks.Add(New Block With {
            .type = "heading",
            .level = 1,
            .content = Title
        })
        Call blocks.Add(New Block With {
            .type = "paragraph",
            .content = $"Host: {report.host} | Generated at: {report.timestamp} | " &
                       $"Runtime: {EnvProbeCommon.formatText(report.getValue(EnvProbeRuntime.SECTION_NAME, EnvProbeRuntime.ITEM_FRAMEWORK))}"
        })

        Call blocks.Add(heading("Summary", 2))
        Call blocks.Add(summaryTable(report))

        For Each section As EnvProbeSection In report.sections
            Call blocks.Add(heading(sectionTitle(section.section), 2))
            Call blocks.Add(tableBlock(
                New String() {"Item", "Value", "Status"},
                section.items.Select(Function(item) New String() {item.name, item.value, item.status}).ToArray))
        Next

        Call blocks.Add(heading("Diagnosis", 2))
        Call blocks.Add(New Block With {
            .type = "paragraph",
            .content = If(report.conclusion, EnvProbeCommon.NA)
        })

        If report.warnings.Count > 0 Then
            Call blocks.Add(heading("Warnings", 3))
            Call blocks.Add(New Block With {
                .type = "list",
                .ordered = True,
                .items = report.warnings.ToArray
            })
        End If

        If report.suggestions.Count > 0 Then
            Call blocks.Add(heading("Suggestions", 3))
            Call blocks.Add(New Block With {
                .type = "list",
                .ordered = True,
                .items = report.suggestions.ToArray
            })
        End If

        Return blocks.ToArray
    End Function

    ''' <summary>
    ''' render the given probe report as the markdown text content.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function buildMarkdown(report As EnvProbeReport) As String
        Return buildBlocks(report).ToMarkdown
    End Function

    Private Function summaryTable(report As EnvProbeReport) As Block
        Dim rows As New List(Of String())

        Call addSummaryRow(rows, "Visible / machine processors",
                           $"{EnvProbeCommon.formatText(report.getValue(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_VISIBLE))} / " &
                           EnvProbeCommon.formatText(report.getValue(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_MACHINE_LOGICAL)))
        Call addSummaryRow(rows, "CPU model", report.getValue(EnvProbeCPU.SECTION_NAME, EnvProbeCPU.ITEM_MODEL))
        Call addSummaryRow(rows, "SIMD", report.getValue(EnvProbeSIMD.SECTION_NAME, EnvProbeSIMD.ITEM_DESCRIPTION))
        Call addSummaryRow(rows, "Server GC", report.getValue(EnvProbeRuntime.SECTION_GC, EnvProbeRuntime.ITEM_SERVER_GC))
        Call addSummaryRow(rows, "Memory",
                           $"{EnvProbeCommon.formatText(report.getValue(EnvProbeMemory.SECTION_NAME, EnvProbeMemory.ITEM_AVAILABLE))} available / " &
                           EnvProbeCommon.formatText(report.getValue(EnvProbeMemory.SECTION_NAME, EnvProbeMemory.ITEM_TOTAL)))
        Call addSummaryRow(rows, "Peak throughput (GFLOPS)", report.getValue(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_PEAK))
        Call addSummaryRow(rows, "Thread scaling", report.getValue(EnvProbeBenchmark.SECTION_NAME, EnvProbeBenchmark.ITEM_SCALING))
        Call addSummaryRow(rows, "CUDA", report.getValue(EnvProbeCUDA.SECTION_NAME, EnvProbeCUDA.ITEM_AVAILABLE))

        Return tableBlock(New String() {"Item", "Value"}, rows.ToArray)
    End Function

    Private Sub addSummaryRow(rows As List(Of String()), name As String, value As String)
        Call rows.Add(New String() {name, EnvProbeCommon.formatText(value)})
    End Sub

    Private Function heading(text As String, level As Integer) As Block
        Return New Block With {
            .type = "heading",
            .level = level,
            .content = text
        }
    End Function

    ''' <summary>
    ''' create a markdown table block. the header cell count and the row cell count
    ''' must be matched, otherwise the document render will be misaligned.
    ''' </summary>
    ''' <param name="headers"></param>
    ''' <param name="rows"></param>
    ''' <returns></returns>
    Private Function tableBlock(headers As String(), rows As String()()) As Block
        Dim normalized As New List(Of String())

        For Each row As String() In rows
            Call normalized.Add(normalizeRow(row, headers.Length))
        Next

        Return New Block With {
            .type = "table",
            .headers = headers,
            .alignments = headers.Select(Function(h) "left").ToArray,
            .rows = normalized.ToArray
        }
    End Function

    ''' <summary>
    ''' keep the row cell count matched with the table header cell count: the document
    ''' renderer will be misaligned when the cell count is not matched.
    ''' </summary>
    ''' <param name="row"></param>
    ''' <param name="size"></param>
    ''' <returns></returns>
    Private Function normalizeRow(row As String(), size As Integer) As String()
        Dim cells As String() = New String(size - 1) {}

        For i As Integer = 0 To size - 1
            If row IsNot Nothing AndAlso i < row.Length Then
                cells(i) = If(row(i), "")
            Else
                cells(i) = ""
            End If
        Next

        Return cells
    End Function

    ''' <summary>
    ''' the human readable title of the given report section
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function sectionTitle(name As String) As String
        Select Case LCase(name)
            Case "os" : Return "Operating system"
            Case "cpu" : Return "Processor"
            Case "simd" : Return "SIMD instruction set"
            Case "memory" : Return "Memory"
            Case "runtime" : Return "CLR runtime"
            Case "gc" : Return "Garbage collector"
            Case "threadpool" : Return "Thread pool"
            Case "cuda" : Return "CUDA / GPU"
            Case "benchmark" : Return "Measured performance"
            Case Else : Return name
        End Select
    End Function

    ''' <summary>
    ''' build the JSON text content of the given probe report.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function buildJson(report As EnvProbeReport) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine("{")
        Call sb.AppendLine($"  ""host"": {quote(report.host)},")
        Call sb.AppendLine($"  ""timestamp"": {quote(report.timestamp)},")
        Call sb.AppendLine($"  ""runtime"": {quote(report.getValue(EnvProbeRuntime.SECTION_NAME, EnvProbeRuntime.ITEM_FRAMEWORK))},")
        Call sb.AppendLine("  ""sections"": {")

        For i As Integer = 0 To report.sections.Count - 1
            Dim section As EnvProbeSection = report.sections(i)

            Call sb.AppendLine($"    {quote(section.section)}: {{")

            For j As Integer = 0 To section.items.Count - 1
                Dim item As EnvProbeItem = section.items(j)
                Dim tail As String = If(j = section.items.Count - 1, "", ",")
                Dim data As String = jsonData(item)

                Call sb.AppendLine($"      {quote(item.name)}: {{""value"": {quote(item.value)}, ""status"": {quote(item.status)}{data}}}{tail}")
            Next

            Call sb.AppendLine($"    }}{If(i = report.sections.Count - 1, "", ",")}")
        Next

        Call sb.AppendLine("  },")
        Call sb.AppendLine("  ""diagnosis"": {")
        Call sb.AppendLine($"    ""warnings"": {jsonArray(report.warnings)},")
        Call sb.AppendLine($"    ""suggestions"": {jsonArray(report.suggestions)},")
        Call sb.AppendLine($"    ""conclusion"": {quote(report.conclusion)}")
        Call sb.AppendLine("  }")
        Call sb.Append("}")

        Return sb.ToString
    End Function

    Private Function jsonData(item As EnvProbeItem) As String
        Dim numbers As Double() = TryCast(item.raw, Double())

        If numbers IsNot Nothing Then
            Dim text As String = String.Join(",", numbers.Select(Function(d) EnvProbeCommon.formatNumber(d, 6)))
            Return $", ""data"": [{text}]"
        End If

        Dim integers As Integer() = TryCast(item.raw, Integer())

        If integers IsNot Nothing Then
            Return $", ""data"": [{String.Join(",", integers)}]"
        End If

        If TypeOf item.raw Is Long AndAlso CLng(item.raw) > 0 Then
            Return $", ""bytes"": {CLng(item.raw)}"
        End If

        Return ""
    End Function

    Private Function jsonArray(values As List(Of String)) As String
        If values Is Nothing OrElse values.Count = 0 Then
            Return "[]"
        End If

        Dim parts As String() = values.Select(Function(v) quote(v)).ToArray

        Return "[" & String.Join(", ", parts) & "]"
    End Function

    Private Function quote(text As String) As String
        If text Is Nothing Then
            Return "null"
        End If

        Dim sb As New StringBuilder(text.Length + 2)

        Call sb.Append(""""c)

        For Each ch As Char In text
            Select Case ch
                Case """"c : Call sb.Append("\""")
                Case "\"c : Call sb.Append("\\")
                Case vbCr : Call sb.Append("\r")
                Case vbLf : Call sb.Append("\n")
                Case vbTab : Call sb.Append("\t")
                Case Else
                    If AscW(ch) < 32 Then
                        Call sb.Append("\u" & AscW(ch).ToString("x4"))
                    Else
                        Call sb.Append(ch)
                    End If
            End Select
        Next

        Call sb.Append(""""c)

        Return sb.ToString
    End Function

    ''' <summary>
    ''' convert the probe report into the R# list object.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function buildList(report As EnvProbeReport) As Object
        Dim slots As New Dictionary(Of String, Object)

        slots("host") = report.host
        slots("timestamp") = report.timestamp

        For Each section As EnvProbeSection In report.sections
            Dim items As New Dictionary(Of String, Object)

            For Each item As EnvProbeItem In section.items
                items(item.name) = If(item.raw Is Nothing, item.value, item.raw)
            Next

            slots(section.section) = New list With {.slots = items}
        Next

        slots("diagnosis") = New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"warnings", report.warnings.ToArray},
                {"suggestions", report.suggestions.ToArray},
                {"conclusion", report.conclusion},
                {"benchmark_time", report.benchmark_time}
            }
        }

        Return New list With {.slots = slots}
    End Function

    ''' <summary>
    ''' save the markdown report text into the given file path.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function saveMarkdown(report As EnvProbeReport, path As String) As String
        Return EnvProbeCommon.saveText(path, buildMarkdown(report))
    End Function

    ''' <summary>
    ''' save the JSON report text into the given file path.
    ''' </summary>
    ''' <param name="report"></param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function saveJson(report As EnvProbeReport, path As String) As String
        Return EnvProbeCommon.saveText(path, buildJson(report))
    End Function
End Module
