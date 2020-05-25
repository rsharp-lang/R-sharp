Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter

    Public NotInheritable Class ExecutableLoop

        Private Sub New()
        End Sub

        ''' <summary>
        ''' function/forloop/if/else/elseif/repeat/while, etc...
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Shared Function Execute(execQueue As IEnumerable(Of Expression), envir As Environment) As Object
            Dim last As Object = Nothing
            Dim breakLoop As Boolean = False
            Dim debug As Boolean = envir.globalEnvironment.debugMode

            ' The program code loop
            For Each expression As Expression In execQueue
                last = ExecuteCodeLine(expression, envir, breakLoop, debug)

                If breakLoop Then
                    Call configException(envir, last, expression)
                    Exit For
                End If
            Next

            Return last
        End Function

        Private Shared Sub configException(env As Environment, last As Object, expression As Expression)
            If Not last Is Nothing AndAlso Program.isException(last) Then
                Dim err As Message = last

                If err.source Is Nothing Then
                    err.source = expression
                End If

                env.globalEnvironment.lastException = err
            End If
        End Sub

        Private Shared Sub printExpressionDebug(expression As Expression)
            Dim fore As ConsoleColor = Console.ForegroundColor

            Console.ForegroundColor = ConsoleColor.Magenta
            Console.WriteLine(expression.ToString)
            Console.ForegroundColor = fore
        End Sub

        ''' <summary>
        ''' For execute lambda function
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <param name="envir"></param>
        ''' <param name="breakLoop"></param>
        ''' <returns></returns>
        Public Shared Function ExecuteCodeLine(expression As Expression, envir As Environment,
                                               Optional ByRef breakLoop As Boolean = False,
                                               Optional debug As Boolean = False) As Object
            Dim last As Object

            If debug Then
                Call printExpressionDebug(expression)
            End If

            last = expression.Evaluate(envir)

            ' next keyword will break current closure 
            ' and then goto execute next iteration loop
            If TypeOf expression Is ReturnValue Then
                ' return keyword will break the function
                ' current program maybe is a for loop, if closure, etc
                ' so we needs wrap the last value with 
                ' return keyword.
                last = New ReturnValue(New RuntimeValueLiteral(last))
                breakLoop = True
            ElseIf Not last Is Nothing AndAlso last.GetType Is GetType(ReturnValue) Then
                ' the internal closure invoke a returns keyword
                ' so break the current loop
                '
                ' This situation maybe a deep nested closure, example like 
                '
                ' let fun as function() {
                '    for(x in xxx) {
                '       for(y in yyy) {
                '           if (true(x, y)) {
                '              return ooo;
                '           }
                '       }
                '    }
                ' }
                '
                ' Do not break the returns keyword popout chain 
                '
                breakLoop = True
            End If

            If TypeOf expression Is ContinuteFor Then
                breakLoop = True
            ElseIf TypeOf expression Is BreakLoop Then
                breakLoop = True
            End If

            If Not last Is Nothing Then
                If last.GetType Is GetType(Message) Then
                    If DirectCast(last, Message).level = MSG_TYPES.ERR Then
                        ' throw error will break the expression loop
                        breakLoop = True
                        ' populate out this error message to the top stack
                        ' and then print errors
                        Return last
                    ElseIf DirectCast(last, Message).level = MSG_TYPES.DEBUG Then
                    ElseIf DirectCast(last, Message).level = MSG_TYPES.WRN Then
                    Else

                    End If
                ElseIf last.GetType Is GetType(IfBranch.IfPromise) Then
                    envir.ifPromise.Add(last)
                    last = DirectCast(last, IfBranch.IfPromise).Value

                    If envir.ifPromise.Last.Result Then
                        If Not last Is Nothing AndAlso last.GetType Is GetType(ReturnValue) Then
                            breakLoop = True
                        End If
                    End If
                End If
            End If

            Return last
        End Function
    End Class
End Namespace