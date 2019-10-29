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

        Public Overrides Function ToString() As String
            Return $"Call {funcName}({parameters.JoinBy(", ")})"
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' 当前环境中的函数符号的优先度要高于
            ' 系统环境下的函数符号
            Dim funcVar As Variable = envir.FindSymbol(funcName)

            If funcVar Is Nothing Then
                ' 可能是一个系统的内置函数
                If funcName = "list" Then
                    Dim list As New Dictionary(Of String, Object)
                    Dim slot As Expression
                    Dim key As String
                    Dim value As Object

                    For i As Integer = 0 To parameters.Length - 1
                        slot = parameters(i)

                        If TypeOf slot Is ValueAssign Then
                            ' 不支持tuple
                            key = DirectCast(slot, ValueAssign).targetSymbols(Scan0)
                            value = DirectCast(slot, ValueAssign).value.Evaluate(envir)
                        Else
                            key = i + 1
                            value = slot.Evaluate(envir)
                        End If

                        Call list.Add(key, value)
                    Next

                    Return list
                Else
                    Return invokeInternals(envir, funcName, envir.Evaluate(parameters))
                End If
            ElseIf funcVar.value.GetType Is GetType(DeclareNewFunction) Then
                ' invoke method create from R# script
                Return DirectCast(funcVar.value, DeclareNewFunction).Invoke(envir, envir.Evaluate(parameters))
            Else
                ' invoke .NET method
                Return DirectCast(funcVar.value, RMethodInfo).Invoke(envir, envir.Evaluate(parameters))
            End If
        End Function

        ''' <summary>
        ''' Invoke the runtime internal functions
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="funcName$"></param>
        ''' <param name="paramVals"></param>
        ''' <returns></returns>
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
                    Return Internal.print(paramVals(Scan0))
                Case "stop"
                    Return Internal.stop(paramVals(Scan0), envir)
            End Select

            Throw New NotImplementedException
        End Function
    End Class
End Namespace