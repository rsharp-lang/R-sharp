Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class DeclareNewFunction : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Dim funcName$
        Dim params As DeclareNewVariable()
        Dim body As Program

        Sub New(code As List(Of Token()))
            Dim [declare] = code(4)
            Dim parts = [declare].SplitByTopLevelDelimiter(TokenType.close)
            Dim paramPart = parts(Scan0).Skip(1).ToArray
            Dim bodyPart = parts(2).Skip(1).ToArray

            funcName = code(1)(Scan0).text

            Call getParameters(paramPart)
            Call getExecBody(bodyPart)
        End Sub

        Shared ReadOnly [let] As New List(Of Token) From {
            New Token With {.name = TokenType.keyword, .text = "let"}
        }

        Private Sub getParameters(tokens As Token())
            Dim parts = tokens.SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .ToArray

            params = parts _
                .Select(Function(t)
                            Return New DeclareNewVariable([let] + t)
                        End Function) _
                .ToArray
        End Sub

        Private Sub getExecBody(tokens As Token())
            Throw New NotImplementedException
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace