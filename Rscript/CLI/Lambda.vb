Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports any = Microsoft.VisualBasic.Scripting

Partial Module CLI

    ' Rscript --lambda biodeepMSMS::doMSMSalignment --SetDllDirectory /usr/local/bin

    <ExportAPI("--lambda")>
    <Description("Execute R# function with parameters")>
    <Usage("--lambda <delegate_name> [--request </path/to/del_func_parameters.json, default=""./.r_env/run.json""> --SetDllDirectory <dll_directory>]")>
    Public Function execLambda(args As CommandLine) As Integer
        Dim SetDllDirectory As String = args("--SetDllDirectory")
        Dim renv As New RInterpreter
        Dim del_func As String = args.SingleValue
        Dim request_argv As String = args("--request") Or "./.r_env/run.json".GetFullPath
        Dim options_argv As String = args("--options") Or "./.r_env/options.json".GetFullPath

        If Not SetDllDirectory.StringEmpty Then
            Call renv.globalEnvir.options.setOption("SetDllDirectory", SetDllDirectory)
        End If

        Dim func As NamedValue(Of String) = del_func.GetTagValue("::", trim:=True, failureNoName:=True)

        ' load primary base libraries
        Call LoadLibrary(renv, ignoreMissingStartupPackages:=True, "base", "utils", "grDevices", "math", "stats")

        ' set options
        Dim opts As list = renv.getLambdaArguments(file:=options_argv)

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

            result = renv.globalEnvir.invokeLambda(run, callable.value)
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
    Private Function invokeLambda(env As Environment, argv As list, del_func As RFunction) As Object
        Dim args As NamedValue(Of Expression)() = del_func.getArguments.ToArray
        Dim run As InvokeParameter() = New InvokeParameter(args.Length - 1) {}

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
