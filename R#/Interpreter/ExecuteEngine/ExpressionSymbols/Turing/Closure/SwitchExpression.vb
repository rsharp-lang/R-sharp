
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class SwitchExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Switch
            End Get
        End Property

        Dim switch As Expression
        Dim [default] As Expression
        Dim sourceMap As StackFrame
        Dim hash As New Dictionary(Of String, Expression)

        Sub New(switch As Expression, [default] As Expression, sourceMap As StackFrame)
            Me.switch = switch
            Me.default = [default]
            Me.sourceMap = sourceMap
        End Sub

        Public Sub Add(key As String, exp As Expression)
            hash(key) = exp
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim switchVal As Object = switch.Evaluate(envir)
            Dim hasKey As String

            If Program.isException(switchVal) Then
                Return switchVal
            Else
                hasKey = any.ToString(REnv.single(switchVal))
            End If

            If hash.ContainsKey(hasKey) Then
                Return hash(hasKey).Evaluate(envir)
            ElseIf [default] Is Nothing Then
                Return Internal.debug.stop($"no switch for '{hasKey}'!", envir)
            Else
                Return [default].Evaluate(envir)
            End If
        End Function
    End Class
End Namespace