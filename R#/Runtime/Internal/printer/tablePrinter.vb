﻿#Region "Microsoft.VisualBasic::d234231c080d5c9f9ffb71a747e544a7, R#\Runtime\Internal\printer\tablePrinter.vb"

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

    '   Total Lines: 216
    '    Code Lines: 162 (75.00%)
    ' Comment Lines: 22 (10.19%)
    '    - Xml Docs: 40.91%
    ' 
    '   Blank Lines: 32 (14.81%)
    '     File Size: 9.41 KB


    '     Module tablePrinter
    ' 
    '         Function: getColumnPrintVector, PartOfTable, PrintTable, simpleTruncatedString, ToContent
    '                   truncateDisplayString
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
Imports consoleDevice = Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.ConsoleTableBuilderExtensions
Imports std = System.Math

Namespace Runtime.Internal.ConsolePrinter

    ''' <summary>
    ''' R# runtime dataframe object printer
    ''' </summary>
    Public Module tablePrinter

        Private Function simpleTruncatedString(si As String, maxWidth As Integer) As String
            ' try to make string truncated for deal with the long string
            If si IsNot Nothing AndAlso maxWidth > 0 Then
                Dim strlen As Integer = consoleDevice.RealLength(si, withUtf8Characters:=True)

                If strlen > maxWidth Then
                    Dim truncated As Integer = strlen - maxWidth
                    ' 20250226
                    ' Index and length must refer to a location within the string. (Parameter 'length')
                    Dim len As Integer = maxWidth - 3

                    If len < si.Length Then
                        si = si.Substring(0, len) & "..."
                    End If

                    si = si & $"|{truncated} chars truncated"
                End If
            End If

            Return si
        End Function

        Const ascii_upperbound As Char = ChrW(&H80)

        Public Function truncateDisplayString(si As String, displayWidth As Integer) As String
            If si IsNot Nothing AndAlso displayWidth > 0 Then
                ' 初始化字符计数器
                Dim charCount As Integer = 0
                Dim strlen As Integer = si.Length
                Dim truncated As Integer = strlen - displayWidth

                ' 遍历字符串中的每个字符
                For i As Integer = 0 To strlen - 1
                    ' 检查当前字符是否是ASCII字符
                    If si(i) < ascii_upperbound Then
                        ' ASCII字符，宽度为1
                        charCount += 1
                    Else
                        ' 非ASCII字符（假设为中文字符），宽度为2
                        charCount += 2
                    End If

                    ' 如果字符计数器超过显示宽度，则截断字符串
                    If charCount > displayWidth Then
                        ' 返回截断后的字符串
                        Return si.Substring(0, i) & $"...|{truncated} chars truncated"
                    End If
                Next
            End If

            ' 如果字符串的显示宽度小于或等于设定的显示宽度，返回原始字符串
            Return si
        End Function

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
                            ' 20250227 apply of the more safe display string truncated method
                            Return truncateDisplayString(si, maxWidth)
                            Return simpleTruncatedString(si, maxWidth)
                        End Function) _
                .ToArray
            Dim typeStr As String = $"<{RType.GetRSharpType(type).ToString}>"
            Dim max As String = {colname, typeStr} _
                .JoinIterates(arr) _
                .MaxLengthString(consolePrintWidth:=True)
            Dim maxStrlen As Integer = consoleDevice.RealLength(max, withUtf8Characters:=True)

            arr = arr _
                .Select(Function(str)
                            If str Is Nothing Then
                                str = ""
                            End If

                            Return New String(" "c, maxStrlen - consoleDevice.RealLength(str, True)) & str
                        End Function) _
                .ToArray

            Return New NamedCollection(Of String) With {
                .name = New String(" "c, maxStrlen - consoleDevice.RealLength(colname, True)) & colname,
                .value = {New String(" "c, maxStrlen - typeStr.Length) & typeStr} _
                    .JoinIterates(arr) _
                    .ToArray,
                .description = maxStrlen
            }
        End Function

        <Extension>
        Public Iterator Function ToContent(table As dataframe, maxPrint%, maxWidth%, globalEnv As GlobalEnvironment) As IEnumerable(Of ConsoleTableBaseData)
            Dim nrows As Integer = std.Min(table.nrows, maxPrint)
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

            For Each col As NamedCollection(Of String) In columns
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

        ''' <summary>
        ''' Get console print table data
        ''' </summary>
        ''' <param name="part"></param>
        ''' <param name="nrows"></param>
        ''' <returns></returns>
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
