#Region "Microsoft.VisualBasic::bb32f4abd5b0811c7aac92b103daf0b7, R#\System\Document\ShellScript\CommandLineDocument.vb"

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

    '   Total Lines: 111
    '    Code Lines: 88 (79.28%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (20.72%)
    '     File Size: 4.49 KB


    '     Class CommandLineDocument
    ' 
    '         Properties: arguments, authors, dependency, info, sourceScript
    '                     title
    ' 
    '         Sub: PrintUsage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language.[Default]
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Development.CommandLine

    Public Class CommandLineDocument

        Public Property arguments As CommandLineArgument()
        Public Property sourceScript As String
        Public Property title As String
        Public Property info As String
        Public Property authors As String()
        Public Property dependency As Dependency()

        Public Sub PrintUsage(dev As TextWriter)
            Dim cli As New List(Of String)
            Dim maxName As String = arguments.Select(Function(a) a.name).MaxLengthString
            Dim valueStr As String

            Call dev.WriteLine()
            Call dev.WriteLine($"  '{sourceScript}' - {title}")
            Call dev.WriteLine()

            For Each line As String In info.LineTokens
                Call dev.WriteLine("  " & line)
            Next

            Call dev.WriteLine()

            For Each arg As CommandLineArgument In arguments
                If arg.defaultValue.StartsWith("<required") Then
                    cli.Add($"{arg.name} <{arg.type}>")
                ElseIf arg.isNumeric Then
                    cli.Add($"[{arg.name} <{arg.type}, default={Strings.Trim(arg.defaultValue).Trim(""""c)}>]")
                Else
                    cli.Add($"[{arg.name} <{arg.type}, default={arg.defaultValue}>]")
                End If
            Next

            Call dev.WriteLine($"SYNOPSIS")
            Call dev.WriteLine($"Rscript ""{sourceScript}"" {cli.JoinBy(" ")}")
            Call dev.WriteLine()
            Call dev.WriteLine("CommandLine Argument Values:")

            Call dev.WriteLine()

            Static none As [Default](Of String) = "-"

            For Each arg As CommandLineArgument In arguments
                Dim prefix As String = $" {arg.name}: {New String(" "c, maxName.Length - arg.name.Length)}"
                Dim descriptionBlock As String = Paragraph _
                    .SplitParagraph(arg.description Or none, 65) _
                    .JoinBy(vbCrLf & New String(" "c, prefix.Length))

                If arg.defaultValue.StartsWith("<required") Then
                    valueStr = ($"<required>")
                ElseIf arg.isNumeric Then
                    valueStr = ($"[{arg.type}, default={Strings.Trim(arg.defaultValue).Trim(""""c)}]")
                Else
                    valueStr = ($"[{arg.type}, default={arg.defaultValue}]")
                End If

                Call dev.WriteLine(prefix & descriptionBlock) ' & valueStr)

                If descriptionBlock.Contains(vbCr) OrElse descriptionBlock.Contains(vbLf) Then
                    Call dev.WriteLine()
                End If
            Next

            If Not authors.IsNullOrEmpty Then
                Call dev.WriteLine()
                Call dev.WriteLine("Authors:")
                Call authors.printContentArray(Nothing, Nothing, 80, dev)
            End If

            If dependency.Length > 0 Then
                Dim requires = dependency.Where(Function(deps) deps.library.StringEmpty).ToArray
                Dim import = dependency.Where(Function(deps) Not deps.library.StringEmpty).ToArray

                Call dev.WriteLine()
                Call dev.WriteLine("Dependency List:")

                If Not requires.IsNullOrEmpty Then
                    Dim allList As String() = requires _
                        .Select(Function(pkg) pkg.packages) _
                        .IteratesALL _
                        .Distinct _
                        .ToArray

                    Call dev.WriteLine()
                    Call dev.WriteLine(" Loading: ")
                    Call allList.printContentArray(Nothing, Nothing, 80, dev)
                End If

                If Not import.IsNullOrEmpty Then
                    Dim allList = import.Select(Function(ref) $"{ref.library}::[{ref.packages.JoinBy(", ")}]").ToArray

                    Call dev.WriteLine()
                    Call dev.WriteLine(" Imports: ")
                    Call allList.printContentArray(Nothing, Nothing, 80, dev)
                End If
            End If

            Call dev.Flush()
        End Sub
    End Class
End Namespace
