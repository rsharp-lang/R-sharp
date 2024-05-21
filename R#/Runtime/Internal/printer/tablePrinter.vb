#Region "Microsoft.VisualBasic::9638718e47c07486b717098d3c01a125, R#\Runtime\Internal\printer\tablePrinter.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 158
    '    Code Lines: 133 (84.18%)
    ' Comment Lines: 1 (0.63%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 24 (15.19%)
    '     File Size: 6.73 KB


    '     Module tablePrinter
    ' 
    '         Function: getColumnPrintVector, PartOfTable, PrintTable, ToContent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports stdNum = System.Math

Namespace Runtime.Internal.ConsolePrinter

    Public Module tablePrinter

        <Extension>
        Private Function getColumnPrintVector(table As dataframe,
                                              colname As String,
                                              nrows As Integer,
                                              maxWidth%,
                                              globalEnv As GlobalEnvironment) As NamedCollection(Of String)
            Dim type As Type = Nothing
            Dim arr As String() = printer.getStrings(table(colname), type, globalEnv) _
                .Take(nrows) _
                .Select(Function(si)
                            If si IsNot Nothing AndAlso maxWidth > 0 Then
                                If si.Length > maxWidth Then
                                    Dim truncated As Integer = si.Length - maxWidth

                                    si = si.Substring(0, maxWidth - 3) & "..."
                                    si = si & $"|{truncated} chars truncated"
                                End If
                            End If

                            Return si
                        End Function) _
                .ToArray
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
        End Function

        <Extension>
        Public Iterator Function ToContent(table As dataframe, maxPrint%, maxWidth%, globalEnv As GlobalEnvironment) As IEnumerable(Of ConsoleTableBaseData)
            Dim nrows As Integer = stdNum.Min(table.nrows, maxPrint)
            Dim rowsNames As String() = {"<mode>"}.JoinIterates(table.getRowNames.Take(nrows)).ToArray
            Dim maxRowNames As Integer = rowsNames.MaxLengthString.Length
            Dim maxColumns As Integer = globalEnv.getMaxColumns
            Dim columns As NamedCollection(Of String)() = table.colnames _
                .Select(Function(colname)
                            Return table.getColumnPrintVector(colname, nrows, maxWidth, globalEnv)
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
                .Rows = New List(Of Object())
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
        Public Function PrintTable(table As dataframe, maxPrint%, maxWidth%, output As TextWriter, env As GlobalEnvironment) As Message
            Dim reachMax As Boolean = table.nrows >= maxPrint
            Dim delta As Integer = table.nrows - maxPrint
            Dim format As ConsoleTableBuilderFormat = formatParser.TryGetValue(
                index:=Strings.LCase(env.options.getOption("table.format", NameOf(ConsoleTableBuilderFormat.Minimal))),
                [default]:=ConsoleTableBuilderFormat.Minimal
            )
            Dim check = table.CheckRowDimension(env)

            If Program.isException(check) Then
                Return check
            Else
                check = table.CheckDimension(env)
            End If
            If Program.isException(check) Then
                Return check
            End If

            For Each part As ConsoleTableBaseData In table.ToContent(maxPrint%, maxWidth%, env)
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

            Return Nothing
        End Function
    End Module
End Namespace
