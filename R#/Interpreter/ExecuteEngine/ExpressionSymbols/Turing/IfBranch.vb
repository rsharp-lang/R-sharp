Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Interpreter.ExecuteEngine

    Public Class IfBranch : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim ifTest As Expression
        Dim trueClosure As DeclareNewFunction

        Friend Class IfPromise

            Public ReadOnly Property Result As Boolean
            Public ReadOnly Property Value As Object
            Public Property assignTo As Expression

            Sub New(value As Object, result As Boolean)
                Me.Value = value
                Me.Result = result
            End Sub

            Public Function DoValueAssign(envir As Environment) As Object
                ' 没有变量需要进行closure的返回值设置
                ' 则跳过
                If assignTo Is Nothing Then
                    Return Value
                End If

                Select Case assignTo.GetType
                    Case GetType(ValueAssign)
                        Return DirectCast(assignTo, ValueAssign).DoValueAssign(envir, Value)
                    Case Else
                        Throw New NotImplementedException
                End Select
            End Function
        End Class

        Sub New(tokens As IEnumerable(Of Token))
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.close)

            ifTest = Expression.CreateExpression(blocks(Scan0).Skip(1))
            trueClosure = New DeclareNewFunction With {
                .funcName = "if_closure_internal",
                .params = {},
                .body = blocks(2) _
                    .Skip(1) _
                    .DoCall(AddressOf ClosureExpression.ParseExpressionTree)
            }
        End Sub

        Sub New(ifTest As Expression, trueClosure As ClosureExpression)
            Me.ifTest = ifTest
            Me.trueClosure = New DeclareNewFunction With {
                .funcName = "if_closure_internal",
                .params = {},
                .body = trueClosure
            }
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim test As Object = ifTest.Evaluate(envir)

            If test Is Nothing Then
                Return New Message With {
                    .Message = {
                        $"missing value where TRUE/FALSE needed"
                    },
                    .MessageLevel = MSG_TYPES.ERR,
                    .EnvironmentStack = envir.getEnvironmentStack,
                    .Trace = devtools.ExceptionData.GetCurrentStackTrace
                }
            End If

            If True = Runtime.asLogical(test)(Scan0) Then
                Return New IfPromise(trueClosure.Invoke(envir, {}), True)
            Else
                Return New IfPromise(Nothing, False)
            End If
        End Function
    End Class
End Namespace