Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Internal

    Delegate Function RFuncInvoke(envir As Environment, funcName$, paramVals As Object()) As Object

    Module invoke

        ReadOnly index As New Dictionary(Of String, RFuncInvoke)

        Sub New()

        End Sub

        ''' <summary>
        ''' Invoke the runtime internal functions
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="funcName$"></param>
        ''' <param name="paramVals"></param>
        ''' <returns></returns>
        Public Function invokeInternals(envir As Environment, funcName$, paramVals As Object()) As Object
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
    End Module
End Namespace