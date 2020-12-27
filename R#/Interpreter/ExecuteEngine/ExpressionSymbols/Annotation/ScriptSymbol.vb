Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.System.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    Public Class ScriptSymbol : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Annotation
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim script As Symbol = envir.FindSymbol("!script", [inherits]:=True)

            If script Is Nothing Then
                Return Nothing
            Else
                Return DirectCast(DirectCast(script.value, vbObject).target, MagicScriptSymbol).fullName
            End If
        End Function

        Public Overrides Function ToString() As String
            Return "@script"
        End Function
    End Class
End Namespace