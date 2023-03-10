Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]

Partial Module CLI

    ' Rscript --lambda biodeepMSMS::doMSMSalignment --SetDllDirectory /usr/local/bin

    <ExportAPI("--lambda")>
    <Description("Execute R# function with parameters")>
    <Usage("--lambda <delegate_name> [--SetDllDirectory <dll_directory>]")>
    Public Function execLambda(args As CommandLine) As Integer
        Dim SetDllDirectory As String = args("--SetDllDirectory")
        Dim renv As New RInterpreter
        Dim del_func As String = args.SingleValue

        If Not SetDllDirectory.StringEmpty Then
            Call renv.globalEnvir.options.setOption("SetDllDirectory", SetDllDirectory)
        End If

        Dim func As NamedValue(Of String) = del_func.GetTagValue("::", trim:=True, failureNoName:=True)

        If Not func.Name.StringEmpty Then
            Call renv.LoadLibrary(func.Name, silent:=False)
        End If

        Dim callable As Symbol = renv.globalEnvir.FindFunction(del_func)
        Dim result As Object

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
        Else
            result = renv.globalEnvir.invokeLambda(callable.value)
        End If

        Return handleResult(result, renv.globalEnvir)
    End Function

    <Extension>
    Private Function invokeLambda(env As Environment, del_func As RFunction) As Object

    End Function
End Module
