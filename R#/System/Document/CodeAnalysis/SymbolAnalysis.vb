Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development

    ''' <summary>
    ''' analysis of the static symbol reference
    ''' </summary>
    Public Class SymbolAnalysis

        Dim envir As String
        Dim declareSymbols As New Index(Of String)
        Dim parent As SymbolAnalysis

        Public Overrides Function ToString() As String
            Return envir
        End Function

        Public Shared Function GetSymbolReferenceList(code As Expression) As IEnumerable(Of NamedValue(Of PropertyAccess))
            Dim ref As New List(Of NamedValue(Of PropertyAccess))
            Dim context As New Context With {
                .context = New SymbolAnalysis,
                .ref = ref
            }

            Call GetSymbolReferenceList(code, context)

            Return ref
        End Function

        Private Shared Sub GetSymbolReferenceList(code As Expression, context As Context)
            Select Case code.GetType
                Case GetType(BinaryExpression) : Call GetSymbols(DirectCast(code, BinaryExpression), context)


                Case Else
                    Throw New NotImplementedException(code.GetType.FullName)
            End Select
        End Sub

        Private Shared Sub GetSymbols(code As BinaryExpression, context As Context)
            If TypeOf code.left Is SymbolReference Then

            Else
                Call GetSymbolReferenceList(code.left, context)
            End If

            If TypeOf code.right Is SymbolReference Then

            Else
                Call GetSymbolReferenceList(code.right, context)
            End If
        End Sub

        Private Class Context

            Public context As SymbolAnalysis
            Public ref As List(Of NamedValue(Of PropertyAccess))

        End Class
    End Class
End Namespace