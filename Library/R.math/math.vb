Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.ODESolver
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime.Internal
Imports stdVec = Microsoft.VisualBasic.Math.LinearAlgebra.Vector

<Package("base.math")>
Module math

    Sub New()
        REnv.Object.Converts.makeDataframe.addHandler(GetType(ODEsOut), AddressOf create_deSolve_DataFrame)
    End Sub

    Private Function create_deSolve_DataFrame(x As ODEsOut, env As Environment) As dataframe
        Dim data As New dataframe

        For Each v In x.y
            data.columns.Add(v.Key, v.Value.ToArray)
        Next

        data.rownames = x.x _
            .Select(Function(d) CStr(d)) _
            .ToArray

        Return data
    End Function

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

    <ExportAPI("deSolve")>
    <RApiReturn(GetType(ODEsOut))>
    Public Function RK4(system As DeclareLambdaFunction(), y0 As list, a#, b#,
                        Optional resolution% = 10000,
                        Optional env As Environment = Nothing) As Object

        Dim vector As var() = New var(system.Length - 1) {}
        Dim i As i32 = Scan0
        Dim solve As Func(Of Double)
        Dim names As New Dictionary(Of String, Variable)

        For Each v As NamedValue(Of Object) In y0.namedValues
            Call env.Push(v.Name, CDbl(v.Value), TypeCodes.double)
            Call names.Add(v.Name, env.FindSymbol(v.Name, [inherits]:=False))
        Next

        For Each formula As DeclareLambdaFunction In system
            Dim lambda As Func(Of Double, Double) = formula.CreateLambda(Of Double, Double)(env)
            Dim name As String = formula.parameterNames(Scan0)
            Dim ref As Variable = names(name)

            solve = Function() lambda(DirectCast(ref.value, Double())(Scan0))
            vector(++i) = New var(solve) With {
                .Name = name,
                .Value = y0.getByName(.Name)
            }
        Next

        Dim df = Sub(dx#, ByRef dy As stdVec)
                     For Each x As var In vector
                         dy(x) = x.Evaluate()
                     Next
                 End Sub
        Dim ODEs As New GenericODEs(vector, df)
        Dim result As ODEsOut = ODEs.Solve(n:=resolution, a:=a, b:=b)

        Return result
    End Function
End Module
