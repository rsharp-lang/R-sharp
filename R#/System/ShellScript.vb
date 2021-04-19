Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    ''' <summary>
    ''' the R# shell script commandline arguments helper module
    ''' </summary>
    Public Class ShellScript

        ReadOnly Rscript As Program
        ReadOnly arguments As New List(Of NamedValue(Of String))

        Public ReadOnly Property message As String

        Sub New(Rscript As Rscript)
            Me.Rscript = Program.CreateProgram(Rscript, [error]:=message)
        End Sub

        Private Sub AnalysisTree(expr As Expression)
            If expr Is Nothing OrElse TypeOf expr Is Literal Then
                Return
            End If

            Select Case expr.GetType
                Case GetType([Imports]) : Call analysisTree(DirectCast(expr, [Imports]))
                Case GetType(BinaryOrExpression) : Call analysisTree(DirectCast(expr, BinaryOrExpression))
                Case GetType(DeclareNewSymbol) : Call analysisTree(DirectCast(expr, DeclareNewSymbol))

                Case Else
                    Throw New NotImplementedException(expr.GetType.FullName)
            End Select
        End Sub

        Private Sub analysisTree(expr As DeclareNewSymbol)
            Call AnalysisTree(expr.m_value)
        End Sub

        Private Sub analysisTree(expr As [Imports])
            Call AnalysisTree(expr.packages)
            Call AnalysisTree(expr.library)
        End Sub

        Private Sub analysisTree(expr As BinaryOrExpression)
            Dim left As Expression = expr.left
            Dim right As Expression = expr.right

            If TypeOf left Is ArgumentValue Then
                Dim name As String = DirectCast(left, ArgumentValue).name.ToString

                If TypeOf right Is FunctionInvoke Then
                    Call New NamedValue(Of String) With {
                        .Name = name,
                        .Description = parseInfo(right)
                    }.DoCall(AddressOf arguments.Add)
                Else
                    Call New NamedValue(Of String) With {
                        .Name = name,
                        .Value = parseDefault(right)
                    }.DoCall(AddressOf arguments.Add)
                End If
            Else
                Call AnalysisTree(left)
                Call AnalysisTree(right)
            End If
        End Sub

        Private Function parseDefault(def As Expression) As String

        End Function

        Private Function parseInfo(calls As FunctionInvoke) As String

        End Function

        Public Function AnalysisAllCommands() As ShellScript
            For Each line As Expression In Rscript
                Call AnalysisTree(line)
            Next

            Return Me
        End Function

        Public Shared Widening Operator CType(Rscript As Rscript) As ShellScript
            Return New ShellScript(Rscript)
        End Operator
    End Class
End Namespace