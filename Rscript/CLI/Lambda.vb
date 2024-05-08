#Region "Microsoft.VisualBasic::5cbbc47e25433a9a802e950f429cea4e, E:/GCModeller/src/R-sharp/Rscript//CLI/Lambda.vb"

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

    '   Total Lines: 165
    '    Code Lines: 128
    ' Comment Lines: 11
    '   Blank Lines: 26
    '     File Size: 6.67 KB


    ' Module CLI
    ' 
    '     Function: execLambda, getLambdaArguments, invokeLambda
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports any = Microsoft.VisualBasic.Scripting

Partial Module CLI

    ' Rscript --lambda biodeepMSMS::doMSMSalignment --SetDllDirectory /usr/local/bin

    <ExportAPI("--lambda")>
    <Description("Execute R# function with parameters")>
    <Usage("--lambda <delegate_name> [--request </path/to/del_func_parameters.json, default=""./.r_env/run.json""> --SetDllDirectory <dll_directory> --attach <pkg_directory> --debug]")>
    Public Function execLambda(args As CommandLine) As Integer
        Dim SetDllDirectory As String = args("--SetDllDirectory")
        Dim renv As New RInterpreter
        Dim del_func As String = args.SingleValue
        Dim request_argv As String = args("--request") Or "./.r_env/run.json".GetFullPath
        Dim options_argv As String = args("--options") Or "./.r_env/options.json".GetFullPath
        Dim attach As String = args("--attach")
        Dim debugMode As Boolean = args.IsTrue("--debug")

        If Not SetDllDirectory.StringEmpty Then
            Call renv.globalEnvir.options.setOption("SetDllDirectory", SetDllDirectory)
        End If
        If attach.DirectoryExists Then
            Dim err As Message = PackageLoader2.Hotload(attach.GetDirectoryFullPath, renv.globalEnvir)

            If Not err Is Nothing Then
                Return Rscript.handleResult(err, renv.globalEnvir, Nothing)
            End If
        End If

        Dim func As NamedValue(Of String) = del_func.GetTagValue("::", trim:=True, failureNoName:=True)

        ' load primary base libraries
        Call LoadLibrary(renv, ignoreMissingStartupPackages:=True, "base", "utils", "grDevices", "math", "stats")

        ' set options
        Dim opts_val = renv.getLambdaArguments(file:=options_argv)
        Dim opts As list = TryCast(opts_val, list)

        If opts Is Nothing Then
            If TypeOf opts_val Is Message Then
                Return handleResult(opts_val, renv.globalEnvir)
            End If

            opts = New list With {
                .slots = New Dictionary(Of String, Object)
            }
        End If

        For Each name As String In opts.getNames
            Call renv.globalEnvir.options.setOption(
                opt:=name,
                value:=any.ToString(opts.getByName(name)),
                env:=renv.globalEnvir
            )
        Next

        If Not func.Name.StringEmpty Then
            Call renv.LoadLibrary(func.Name, silent:=False)
        End If

        Dim callable As Symbol = renv.globalEnvir.FindFunction(del_func)
        Dim result As Object
        Dim run As Object = renv.getLambdaArguments(file:=request_argv)

        If callable Is Nothing OrElse callable.value Is Nothing Then
            result = Internal.debug.stop({
                $"can not found the symbol({del_func}) could be invoke!",
                $"del_func: {del_func}"
            }, renv.globalEnvir)
        ElseIf Not callable.value.GetType.ImplementInterface(Of RFunction) Then
            result = Message.InCompatibleType(
                require:=GetType(RFunction),
                given:=callable.value.GetType,
                envir:=renv.globalEnvir
            )
        ElseIf TypeOf run Is Message Then
            result = run
        Else
            opts = DirectCast(run, list)

            For Each arg As NamedValue(Of String) In args
                Dim name As String = CommandLine.TrimNamePrefix(arg.Name)

                If Not opts.hasName(name) Then
                    Call opts.add(name, arg.Value)
                End If
            Next

            result = renv.globalEnvir.invokeLambda(run, callable.value, debugMode)
        End If

        Return handleResult(result, renv.globalEnvir)
    End Function

    ''' <summary>
    ''' this function will ensure that the parameter value is a list
    ''' </summary>
    ''' <param name="renv"></param>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <Extension>
    Private Function getLambdaArguments(renv As RInterpreter, file As String) As Object
        Call renv.LoadLibrary("JSON", silent:=False)

        If Not file.FileExists Then
            ' returns empty list, means the target function has no parameter inputs
            Return New list With {.slots = New Dictionary(Of String, Object)}
        Else
            Dim val As Object = renv.Evaluate($"JSON::json_decode(readText('{file}'));")

            If TypeOf val Is Message Then
                Return val
            End If

            If val Is Nothing OrElse Not TypeOf val Is list Then
                Return New list With {.slots = New Dictionary(Of String, Object) From {{"$0", val}}}
            Else
                Return val
            End If
        End If
    End Function

    <Extension>
    Private Function invokeLambda(env As Environment, argv As list, del_func As RFunction, debugModel As Boolean) As Object
        Dim args As NamedValue(Of Expression)() = del_func.getArguments.ToArray
        Dim run As InvokeParameter() = New InvokeParameter(args.Length - 1) {}

        If debugModel Then
            Call base.print("(debug) get input parameter names:",, env)
            Call base.print(argv.getNames,, env)
            Call base.print("(debug) the target lambda function required parameters:",, env)
            Call base.print(args.Keys,, env)
            Call base.print("from lambda function:",, env)
            Call base.print(del_func,, env)
        End If

        For i As Integer = 0 To args.Length - 1
            Dim name As String = args(i).Name
            Dim isOptional As Boolean = Not args(i).Value Is Nothing
            Dim index As String = $"${i}"

            If argv.hasName(name) Then
                run(i) = New InvokeParameter(name, argv.getByName(name), i)
            ElseIf isOptional Then
                run(i) = New InvokeParameter(name, args(i).Value.Evaluate(env), i)
            ElseIf argv.hasName(index) Then
                run(i) = New InvokeParameter(index, argv.getByName(index), i)
            Else
                ' missing the required parameter
                Return Internal.debug.stop({
                    $"missing the required parameter({name}) with no default value!",
                    $"parameter: {name}"
                }, env)
            End If
        Next

        Return del_func.Invoke(env, run)
    End Function
End Module
