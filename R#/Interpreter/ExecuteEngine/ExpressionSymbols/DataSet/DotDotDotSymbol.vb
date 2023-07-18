Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ... symbol for the R function
    ''' </summary>
    Public Class DotDotDotSymbol : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.list
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.DotDotDot
            End Get
        End Property

        Public Const dddSymbolName As String = "!...{dot-dot-dot}"

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim ddd As Symbol = envir.FindSymbol(dddSymbolName, [inherits]:=True)

            If ddd Is Nothing Then
                Return list.empty
            Else
                Return ddd.value
            End If
        End Function

        Public Overrides Function ToString() As String
            Return "..."
        End Function
    End Class
End Namespace