Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class FunctionInvoke : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim funcName As String
        Dim parameters As Expression()

        Sub New(tokens As Token())
            Dim params = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray

            funcName = tokens(Scan0).text
            parameters = params _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .Select(Function(param)
                            Return Expression.CreateExpression(param)
                        End Function) _
                .ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim paramVals As Object() = parameters _
                .Select(Function(a) a.Evaluate(envir)) _
                .ToArray
            ' 当前环境中的函数符号的优先度要高于
            ' 系统环境下的函数符号
            Dim funcVar As Variable = envir.FindSymbol(funcName)

            If funcVar Is Nothing Then
                ' 可能是一个系统的内置函数
                Return invokeInternals(envir, funcName, paramVals)
            Else
                Return DirectCast(funcVar.value, RMethodInfo).Invoke(envir, paramVals)
            End If
        End Function

        Private Shared Function invokeInternals(envir As Environment, funcName$, paramVals As Object()) As Object
            Select Case funcName
                Case "length" : Return DirectCast(paramVals(Scan0), Array).Length
                Case "round"
                    Dim x = paramVals(Scan0)
                    Dim decimals As Integer = Runtime.getFirst(paramVals(1))

                    If x.GetType.IsInheritsFrom(GetType(Array)) Then
                        Return (From element As Object In DirectCast(x, Array).AsQueryable Select Math.Round(CDbl(element), decimals)).ToArray
                    Else
                        Return Math.Round(CDbl(x), decimals)
                    End If
                Case "print"
                    Dim x = paramVals(Scan0)
                    Dim valueType As Type = x.GetType
                    Dim isString As Boolean = valueType Is GetType(String) OrElse valueType Is GetType(String())
                    Dim toString = Function(o As Object) As String
                                       If isString Then
                                           Return $"""{Scripting.ToString(o, "NULL")}"""
                                       Else
                                           Return Scripting.ToString(o, "NULL")
                                       End If
                                   End Function

                    If x.GetType.IsInheritsFrom(GetType(Array)) Then
                        Dim xVec As Array = DirectCast(x, Array)

                        Call Console.WriteLine($"[{xVec.Length}] " & (From element As Object In xVec.AsQueryable Select toString(element)).JoinBy(vbTab))
                    Else
                        Call Console.WriteLine("[1] " & toString(x))
                    End If

                    Return x
            End Select

            Throw New NotImplementedException
        End Function
    End Class
End Namespace