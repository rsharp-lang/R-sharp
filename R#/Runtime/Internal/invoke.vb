Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports Microsoft.VisualBasic.Language.C

Namespace Runtime.Internal

    Delegate Function RFuncInvoke(envir As Environment, funcName$, paramVals As Object()) As Object

    Module invoke

        ReadOnly index As New Dictionary(Of String, RFuncInvoke)

        Sub New()

        End Sub

        Public Function Rdataframe(envir As Environment, parameters As List(Of Expression)) As Object
            Dim dataframe As New dataframe With {
                .columns = InvokeParameter _
                    .CreateArguments(envir, InvokeParameter.Create(parameters)) _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return Runtime.asVector(Of Double)(a.Value)
                                  End Function)
            }

            Return dataframe
        End Function

        Public Function Rlist(envir As Environment, parameters As List(Of Expression)) As Object
            Dim list As New Dictionary(Of String, Object)
            Dim slot As Expression
            Dim key As String
            Dim value As Object

            For i As Integer = 0 To parameters.Count - 1
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

            Return New list With {.slots = list}
        End Function

        ''' <summary>
        ''' Invoke the runtime internal functions
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="funcName$"></param>
        ''' <param name="paramVals"></param>
        ''' <returns></returns>
        Public Function invokeInternals(envir As Environment, funcName$, paramVals As Object()) As Object
            Select Case funcName
                Case "length"
                    Return DirectCast(paramVals(Scan0), Array).Length
                Case "round"
                    Dim x As Object = paramVals(Scan0)
                    Dim decimals As Integer = Runtime.getFirst(paramVals(1))

                    If x.GetType.IsInheritsFrom(GetType(Array)) Then
                        Return (From element As Object In DirectCast(x, Array).AsQueryable Select Math.Round(CDbl(element), decimals)).ToArray
                    Else
                        Return Math.Round(CDbl(x), decimals)
                    End If
                Case "get"
                    Return base.get(paramVals(Scan0), envir)
                Case "print"
                    Return Internal.print(paramVals(Scan0), envir)
                Case "stop"
                    Return Internal.stop(paramVals(Scan0), envir)
                Case "warning"
                    Return Internal.warning(paramVals(Scan0), envir)
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
                Case "require"
                    Dim libraryNames As String() = paramVals _
                        .Select(AddressOf Scripting.ToString) _
                        .ToArray

                    Throw New NotImplementedException

                Case "sprintf"
                    Dim format As Array = Runtime.asVector(Of String)(paramVals(Scan0))
                    Dim arguments = paramVals.Skip(1).ToArray
                    Dim sprintf As Func(Of String, Object(), String) = AddressOf CLangStringFormatProvider.sprintf
                    Dim result As String() = format _
                        .AsObjectEnumerator _
                        .Select(Function(str)
                                    Return sprintf(Scripting.ToString(str, "NULL"), arguments)
                                End Function) _
                        .ToArray

                    Return result
                Case Else
                    Return Message.SymbolNotFound(envir, funcName, TypeCodes.closure)
            End Select
        End Function
    End Module
End Namespace