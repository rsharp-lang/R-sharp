#Region "Microsoft.VisualBasic::74937f4da12a3001037688f37638a5ee, D:/GCModeller/src/R-sharp/R#//Runtime/Environment/EnvironmentExtensions.vb"

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

    '   Total Lines: 54
    '    Code Lines: 46
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 2.12 KB


    '     Module EnvironmentExtensions
    ' 
    '         Properties: globalStackFrame
    ' 
    '         Function: CreateMagicScriptSymbol, GetEnvironmentStackTraceString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter

<Assembly: InternalsVisibleTo("njl")>
<Assembly: InternalsVisibleTo("nts")>

Namespace Runtime

    <HideModuleName>
    Module EnvironmentExtensions

        Public ReadOnly Property globalStackFrame As StackFrame
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New StackFrame With {
                    .File = "<globalEnvironment>",
                    .Line = "n/a",
                    .Method = New Method With {
                        .Method = "<globalEnvironment>",
                        .[Module] = "global",
                        .[Namespace] = "SMRUCC/R#"
                    }
                }
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetEnvironmentStackTraceString(env As Environment) As String
            Return env.parent?.ToString & "\" & env.stackFrame.Method.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateMagicScriptSymbol(filepath As String, R As RInterpreter) As MagicScriptSymbol
            Dim commandLine As New Dictionary(Of String, String())

            For Each arguments In App.CommandLine.ParameterList.GroupBy(Function(a) a.Name.ToLower)
                commandLine.Add(arguments.Key, arguments.Select(Function(a) a.Value).ToArray)
            Next

            Return New MagicScriptSymbol With {
                .dir = filepath.ParentPath,
                .file = filepath.FileName,
                .fullName = filepath.GetFullPath,
                .startup_time = Now.ToString,
                .debug = R.debug,
                .log4vb_redirect = R.globalEnvir.log4vb_redirect,
                .silent = R.silent,
                .commandLine = commandLine,
                .commandArguments = App.CommandLine.Tokens
            }
        End Function
    End Module
End Namespace
