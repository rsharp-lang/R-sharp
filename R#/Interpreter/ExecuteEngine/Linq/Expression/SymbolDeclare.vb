Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' declare a new temp symbol: ``LET x = ...``
    ''' </summary>
    Public Class SymbolDeclare : Inherits LinqKeywordExpression

        ''' <summary>
        ''' is a <see cref="Literal"/> or <see cref="VectorLiteral"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property symbol As Expression
        Public Property typeName As String

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "LET"
            End Get
        End Property

        ''' <summary>
        ''' just create new symbol in the target environment
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns>returns nothing</returns>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            If TypeOf symbol Is Literal Then
                Call context.AddSymbol(any.ToString(DirectCast(symbol, Literal).value), TypeCodes.generic)
            ElseIf TypeOf symbol Is RunTimeValueExpression AndAlso TypeOf DirectCast(symbol, RunTimeValueExpression).R Is VectorLiteral Then
                For Each element In DirectCast(DirectCast(symbol, RunTimeValueExpression).R, VectorLiteral)
                    If Not TypeOf element Is DataSets.Literal Then
                        Return Internal.debug.stop("symbol expression in tuple vector should be symbol or literal text!", context)
                    Else
                        Call context.AddSymbol(any.ToString(DirectCast(element, DataSets.Literal).value), TypeCodes.generic)
                    End If
                Next
            Else
                Return Internal.debug.stop("symbol expression should be symbol or literal text!", context)
            End If

            Return Nothing
        End Function

        Public Sub SetValue(value As Object, contex As ExecutableContext)

        End Sub
    End Class
End Namespace