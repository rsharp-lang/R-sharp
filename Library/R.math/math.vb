Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.ODESolver
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime.Internal

<Package("base.math")>
Module math

    <ExportAPI("solve.RK4")>
    <RApiReturn(GetType(ODEOutput))>
    Public Function RK4(df As Object,
                        Optional y0# = 0,
                        Optional min# = -100,
                        Optional max# = 100,
                        Optional resolution% = 10000,
                        Optional env As Environment = Nothing) As Object
        Dim y As df

        If df Is Nothing Then
            Return REnv.debug.stop("Missing ``dy/dt``!", env)
        End If

        If TypeOf df Is df Then
            y = DirectCast(df, df)
        ElseIf TypeOf df Is DeclareLambdaFunction Then
            With DirectCast(df, DeclareLambdaFunction).CreateLambda(Of Double, Double)(env)
                y = Function(x, x1) .Invoke(x)
            End With
        Else
            Return REnv.debug.stop(New NotSupportedException, env)
        End If

        Return New ODE With {
            .df = y,
            .y0 = y0,
            .ID = df.ToString
        }.RK4(resolution, min, max)
    End Function
End Module
