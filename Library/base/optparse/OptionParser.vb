#Region "Microsoft.VisualBasic::2479a8b70e4cb672dacbd99233e4a316, Library\base\optparse\OptionParser.vb"

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

    '   Total Lines: 79
    '    Code Lines: 68 (86.08%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (13.92%)
    '     File Size: 2.89 KB


    ' Class OptionParser
    ' 
    '     Properties: add_help_option, dependency, description, epilogue, option_list
    '                 title, usage
    ' 
    '     Function: GetDocument, getOptions
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Internal.Object.baseOp

Public Class OptionParser

    Public Property usage As String
    Public Property option_list As OptionParserOption()
    Public Property add_help_option As Boolean = True
    Public Property description As String
    Public Property epilogue As String
    Public Property title As String
    Public Property dependency As String()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDocument() As CommandLineDocument
        Dim deps As New Dependency With {.packages = dependency}

        Return New CommandLineDocument With {
            .arguments = option_list _
                .Select(Function(a) a.CreateArgumentInternal) _
                .ToArray,
            .info = description,
            .title = title,
            .sourceScript = App.CommandLine.Name,
            .dependency = {deps}
        }
    End Function

    Public Function getOptions(args As CommandLine,
                               env As Environment,
                               Optional convert_hyphens_to_underscores As Boolean = False) As list

        Dim opts As list = list.empty
        Dim opt_val As Object

        For Each opt As OptionParserOption In option_list.SafeQuery
            Dim value = (From name As String
                         In opt.opt_str
                         Let val = args.GetString(name)
                         Where Not val.StringEmpty
                         Select val).FirstOrDefault

            If value Is Nothing Then
                If opt.opt_str.Any(Function(o) args.HavebFlag(o)) Then
                    If opt.action = "store_true" Then
                        opt_val = True
                    ElseIf opt.action = "store_false" Then
                        opt_val = False
                    Else
                        opt_val = Nothing
                    End If
                ElseIf opt.isLogical Then
                    If opt.action = "store_true" Then
                        opt_val = False
                    Else
                        opt_val = True
                    End If
                Else
                    opt_val = opt.default
                End If
            Else
                opt_val = s4Methods.conversionValue(value, opt.type, env)
            End If

            For Each name As String In opt.opt_names(convert_hyphens_to_underscores)
                Call opts.add(name, opt_val)
            Next
        Next

        Return opts
    End Function

End Class
