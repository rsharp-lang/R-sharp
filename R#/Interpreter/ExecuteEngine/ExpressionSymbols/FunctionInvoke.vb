Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class FunctionInvoke : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
        Public ReadOnly Property funcName As String

        Dim parameters As Expression()
        Dim span As CodeSpan

        Sub New(tokens As Token())
            Dim params = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray

            funcName = tokens(Scan0).text
            span = tokens(Scan0).span
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

        ''' <summary>
        ''' These function create from script text in runtime
        ''' </summary>
        ReadOnly runtimeFuncs As Index(Of Type) = {
            GetType(DeclareNewFunction),
            GetType(DeclareLambdaFunction)
        }

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
            ElseIf funcVar.value.GetType Like runtimeFuncs Then
                ' invoke method create from R# script
                Return DirectCast(funcVar.value, RFunction).Invoke(envir, envir.Evaluate(parameters))
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
                Case "cat"
                    Return Internal.cat(paramVals(Scan0), paramVals.ElementAtOrDefault(1), paramVals.ElementAtOrDefault(2, " "))
                Case "lapply"
                    If paramVals.ElementAtOrDefault(1) Is Nothing Then
                        Return Internal.stop({"Missing apply function!"}, envir)
                    ElseIf Not paramVals(1).GetType.ImplementInterface(GetType(RFunction)) Then
                        Return Internal.stop({"Target is not a function!"}, envir)
                    End If

                    If Program.isException(paramVals(Scan0)) Then
                        Return paramVals(Scan0)
                    ElseIf Program.isException(paramVals(1)) Then
                        Return paramVals(1)
                    End If

                    Return Internal.lapply(paramVals(Scan0), paramVals(1), envir)
                Case Else
                    Return Message.SymbolNotFound(envir, funcName, TypeCodes.closure)
            End Select
        End Function
    End Class
End Namespace