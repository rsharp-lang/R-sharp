
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class TryCatchExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.TryCatch
            End Get
        End Property

        Dim sourceMap As StackFrame
        Dim [try] As Expression
        Dim [catch] As Expression
        Dim exception As SymbolReference

        Sub New([try] As Expression, [catch] As Expression, sourceMap As StackFrame)
            Me.try = [try]
            Me.catch = [catch]
            Me.sourceMap = sourceMap

            If TypeOf [try] Is DeclareLambdaFunction Then
                With DirectCast([try], DeclareLambdaFunction)
                    Me.exception = New SymbolReference(.parameterNames(Scan0))
                    Me.try = .closure
                End With
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim tryVal As Object = [try].Evaluate(envir)

            If Program.isException(tryVal) Then
                If [catch] Is Nothing Then
                    ' returns a try-error
                    Return New TryError(tryVal, sourceMap)
                Else
                    Using closureEnv As New Environment(envir, sourceMap, isInherits:=False)
                        closureEnv.Push(exception.symbol, New TryError(tryVal, sourceMap), [readonly]:=False)
                        Return [catch].Evaluate(closureEnv)
                    End Using
                End If
            Else
                ' no error
                Return tryVal
            End If
        End Function
    End Class
End Namespace