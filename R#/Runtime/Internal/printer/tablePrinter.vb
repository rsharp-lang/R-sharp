Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports stdNum = System.Math

Namespace Runtime.Internal.ConsolePrinter

    Module tablePrinter

        <Extension>
        Public Iterator Function ToContent(table As dataframe, globalEnv As GlobalEnvironment) As IEnumerable(Of ConsoleTableBaseData)
            Dim maxPrint% = globalEnv.options.maxPrint
            Dim nrows As Integer = stdNum.Min(table.nrows, maxPrint)
            Dim rowsNames As String() = {"<mode>"}.JoinIterates(table.getRowNames.Take(nrows)).ToArray
            Dim maxRowNames As Integer = rowsNames.MaxLengthString.Length
            Dim maxColumns As Integer = globalEnv.getMaxColumns
            Dim columns As NamedCollection(Of String)() = table.colnames _
                .Select(Function(colname)
                            Dim type As Type = Nothing
                            Dim arr As String() = printer.getStrings(table(colname), type, globalEnv).Take(nrows).ToArray
                            Dim typeStr As String = $"<{RType.GetRSharpType(type).ToString}>"
                            Dim max As String = {colname, typeStr} _
                                .JoinIterates(arr) _
                                .MaxLengthString

                            arr = arr _
                                .Select(Function(str)
                                            If str Is Nothing Then
                                                str = ""
                                            End If

                                            Return New String(" "c, max.Length - str.Length) & str
                                        End Function) _
                                .ToArray

                            Return New NamedCollection(Of String) With {
                                .name = New String(" "c, max.Length - colname.Length) & colname,
                                .value = {New String(" "c, max.Length - typeStr.Length) & typeStr} _
                                    .JoinIterates(arr) _
                                    .ToArray,
                                .description = max.Length
                            }
                        End Function) _
                .ToArray

            Dim part As New List(Of NamedCollection(Of String))
            Dim size As Integer

            part.Add(New NamedCollection(Of String)("", rowsNames))
            size = maxRowNames

            For Each col In columns
                If size + Integer.Parse(col.description) > maxColumns Then
                    ' create part of table
                    Yield part.PartOfTable(nrows)

                    part.Clear()
                    part.Add(New NamedCollection(Of String)("", rowsNames))
                    size = maxRowNames
                End If

                part.Add(col)
                size += Integer.Parse(col.description)
            Next

            If part.Count > 1 Then
                Yield part.PartOfTable(nrows)
            End If
        End Function

        <Extension>
        Private Function PartOfTable(part As List(Of NamedCollection(Of String)), nrows As Integer) As ConsoleTableBaseData
            Dim data As New ConsoleTableBaseData With {
                .Column = New List(Of Object)(part.Select(Function(v) CObj(v.name))),
                .Rows = New List(Of List(Of Object))
            }
            Dim index As Integer

            For i As Integer = 0 To nrows
                index = i
                data.AppendLine(part.Select(Function(v) CObj(v(index))))
            Next

            Return data
        End Function

        ReadOnly formatParser As Dictionary(Of String, ConsoleTableBuilderFormat) = Enums(Of ConsoleTableBuilderFormat) _
            .ToDictionary(Function(f) f.Description.ToLower,
                          Function(f)
                              Return f
                          End Function)

        <Extension>
        Public Sub PrintTable(table As dataframe, output As RContentOutput, env As GlobalEnvironment)
            Dim maxPrint% = env.options.maxPrint
            Dim reachMax As Boolean = table.nrows >= maxPrint
            Dim delta As Integer = table.nrows - maxPrint
            Dim format As ConsoleTableBuilderFormat = formatParser.TryGetValue(
                index:=Strings.LCase(env.options.getOption("table.format", NameOf(ConsoleTableBuilderFormat.Minimal))),
                [default]:=ConsoleTableBuilderFormat.Minimal
            )

            For Each part As ConsoleTableBaseData In table.ToContent(env)
                Call ConsoleTableBuilder _
                    .From(part) _
                    .WithFormat(format) _
                    .Export _
                    .ToString() _
                    .DoCall(AddressOf output.WriteLine)
            Next

            If reachMax Then
                Call output.WriteLine($" [ reached 'max' / getOption(""max.print"") -- omitted {delta} rows ]")
            End If
        End Sub
    End Module
End Namespace