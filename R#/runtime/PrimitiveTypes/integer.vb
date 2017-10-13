Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.integer"/>
    ''' </summary>
    Public Class [integer] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.integer, GetType(Integer))
            Call MyBase.[New]()

            BinaryOperator1("+") = New BinaryOperator("+", [integer].Add)
        End Sub

        Public Shared Function Add() As RMethodInfo()
            Dim ii As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, Integer, Integer)(x, y), "+")

            Return {ii}
        End Function
    End Class
End Namespace