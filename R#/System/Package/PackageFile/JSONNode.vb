Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.System.Package.File.Expressions

Namespace System.Package.File

    Public Class JSONNode

        Public Property binary As RBinary
        Public Property callFunc As RCallFunction
        Public Property declareFunc As RFunction
        Public Property require As RImports
        Public Property literal As RLiteral
        Public Property symbol As RSymbol
        Public Property unary As RUnary
        Public Property vector As RVector

        Public Function GetExpression(desc As DESCRIPTION) As Expression
            If Not binary Is Nothing Then
                Return binary.GetExpression(desc)
            ElseIf Not callFunc Is Nothing Then
                Return callFunc.GetExpression(desc)
            ElseIf Not declareFunc Is Nothing Then
                Return declareFunc.GetExpression(desc)
            ElseIf Not require Is Nothing Then
                Return require.GetExpression(desc)
            ElseIf Not literal Is Nothing Then
                Return literal.GetExpression(desc)
            ElseIf Not symbol Is Nothing Then
                Return symbol.GetExpression(desc)
            ElseIf Not unary Is Nothing Then
                Return unary.GetExpression(desc)
            ElseIf Not vector Is Nothing Then
                Return vector.GetExpression(desc)
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Shared Widening Operator CType(x As RExpression) As JSONNode
            If TypeOf x Is RUnary Then
                Return New JSONNode With {.unary = x}
            ElseIf TypeOf x Is RBinary Then
                Return New JSONNode With {.binary = x}
            ElseIf TypeOf x Is RCallFunction Then
                Return New JSONNode With {.callFunc = x}
            ElseIf TypeOf x Is RFunction Then
                Return New JSONNode With {.declareFunc = x}
            ElseIf TypeOf x Is RImports Then
                Return New JSONNode With {.require = x}
            ElseIf TypeOf x Is RLiteral Then
                Return New JSONNode With {.literal = x}
            ElseIf TypeOf x Is RSymbol Then
                Return New JSONNode With {.symbol = x}
            ElseIf TypeOf x Is RVector Then
                Return New JSONNode With {.vector = x}
            Else
                Throw New NotImplementedException
            End If
        End Operator
    End Class
End Namespace