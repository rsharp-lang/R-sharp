Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
#Const DEBUG = 0

Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Partial Module CLI

    <ExportAPI("-e")>
    Public Function runExpression(args As CommandLine) As Integer
        Return runExpression(args.Tokens.Skip(1).JoinBy(" "))
    End Function

    Public Function runExpression(expr As String) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim [error] As String = Nothing
        Dim program As RProgram = RProgram.BuildProgram(expr, [error]:=[error])
        Dim result As Object

        If Not [error] Is Nothing Then
            result = REnv.Internal.debug.stop([error], R.globalEnvir)
        Else
#If DEBUG Then
            result = R.Run(program)
#Else
            result = REnv.TryCatch(Function() R.Run(program), debug:=R.debug)
#End If
        End If

        Return Rscript.handleResult(result, R.globalEnvir, program)
    End Function
End Module