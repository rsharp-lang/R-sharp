Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language

    Public Class Token : Inherits CodeToken(Of TokenType)

        Public ReadOnly Property literal As Object
            Get
                Select Case name
                    Case TokenType.stringLiteral
                        Return text
                    Case TokenType.booleanLiteral
                        Return text.ParseBoolean
                    Case TokenType.numberLiteral
                        Return text.ParseDouble
                    Case Else
                        Throw New InvalidCastException(ToString)
                End Select
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(name As TokenType, Optional value$ = Nothing)
            Call MyBase.New(name, value)
        End Sub

        Public Shared Widening Operator CType(type As TokenType) As Token
            Return New Token(type)
        End Operator
    End Class
End Namespace