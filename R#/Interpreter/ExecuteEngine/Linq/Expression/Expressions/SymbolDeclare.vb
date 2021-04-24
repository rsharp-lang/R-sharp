Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.My.JavaScript
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

        Public ReadOnly Property isTuple As Boolean
            Get
                If Not tupleNames.IsNullOrEmpty Then
                    Return True
                ElseIf TypeOf symbol Is VectorLiteral Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Dim symbolName As String
        Dim tupleNames As String()

        ''' <summary>
        ''' just create new symbol in the target environment
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns>returns nothing</returns>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            If TypeOf symbol Is Literal Then
                symbolName = any.ToString(DirectCast(symbol, Literal).value)
                context.AddSymbol(symbolName, TypeCodes.generic)
            ElseIf TypeOf symbol Is VectorLiteral Then
                Dim symbols As New List(Of String)

                For Each element As Expression In DirectCast(symbol, VectorLiteral).elements
                    If Not TypeOf element Is Literal Then
                        Return Internal.debug.stop("symbol expression in tuple vector should be symbol or literal text!", context)
                    Else
                        Call symbols.Add(any.ToString(DirectCast(element, Literal).value))
                        Call context.AddSymbol(symbols.Last, TypeCodes.generic)
                    End If
                Next

                tupleNames = symbols.ToArray
            Else
                Return Internal.debug.stop("symbol expression should be symbol or literal text!", context)
            End If

            Return Nothing
        End Function

        Public Function SetValue(value As Object, contex As ExecutableContext) As Message
            If Not isTuple Then
                Call contex.SetSymbol(symbolName, value)
            ElseIf TypeOf value Is JavaScriptObject Then
                For Each name As String In tupleNames
                    Call contex.SetSymbol(name, DirectCast(value, JavaScriptObject)(name))
                Next
            Else
                Return Internal.debug.stop("invalid data type!", contex)
            End If

            Return Nothing
        End Function
    End Class
End Namespace