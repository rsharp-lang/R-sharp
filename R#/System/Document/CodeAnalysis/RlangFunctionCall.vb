Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Namespace Development.CodeAnalysis

    Public Class RlangFunctionCall

        ReadOnly func As DeclareNewFunction

        Sub New(f As DeclareNewFunction)
            func = f
        End Sub

        Public Function GetScript(args As Dictionary(Of String, Object), env As Environment) As String
            Dim caller As New StringBuilder
            Dim pass As New List(Of String)

            For Each arg As DeclareNewSymbol In func.parameters
                If args.ContainsKey(arg.getName(0)) Then
                    Dim val As Object = args(arg.getName(0))
                    Dim script As String = ValueScript(val, env)

                    Call pass.Add(script)
                ElseIf arg.value IsNot Nothing Then
                    ' use optional default value
                    Dim defaultVal As New RlangTranslator(New ClosureExpression(arg.value))

                    Call pass.Add(defaultVal.GetScript(env).Trim(";"c))
                Else
                    Throw New MissingPrimaryKeyException($"missing parameter value of '{arg.getName(0)}'")
                End If
            Next

            Call caller.AppendLine("# call")
            Call caller.AppendLine($"{func.funcName}({pass.JoinBy(", " & vbLf)});")

            Return caller.ToString
        End Function

        Private Function ValueScript(val As Object, env As Environment) As String
            If val Is Nothing Then
                Return "NULL"
            ElseIf TypeOf val Is vector OrElse val.GetType.IsArray Then
                Dim raw_vec As Array = If(TypeOf val Is vector, DirectCast(val, vector).data, DirectCast(val, Array))
                Dim vec As Array = UnsafeTryCastGenericArray(raw_vec)
                Dim script As String = VectorScript(vec, env)

                Return script
            ElseIf TypeOf val Is list Then
                Return ListScript(DirectCast(val, list).slots, env)
            ElseIf TypeOf val Is dataframe Then
                Return TableScript(DirectCast(val, dataframe), env)
            Else
                Dim literal As Literal = Literal.FromAnyValue(val)
                Dim script As String = literal.GetNativeRScript
                Return script
            End If
        End Function

        Private Function TableScript(df As dataframe, env As Environment) As String

        End Function

        Private Function ListScript(list As Dictionary(Of String, Object), env As Environment) As String
            Dim slots As New List(Of String)

            For Each name As String In list.Keys
                Dim val As Object = list(name)
                Dim script As String = ValueScript(val, env)

                Call slots.Add($"{name} = {script}")
            Next

            Return $"list({slots.JoinBy(", " & vbLf)})"
        End Function

        Private Function VectorScript(vec As Array, env As Environment) As String
            Dim vals As New List(Of String)

            For Each xi As Object In vec.AsObjectEnumerator
                Call vals.Add(ValueScript(xi, env))
            Next

            Return $"c({vals.JoinBy(", ")})"
        End Function
    End Class
End Namespace