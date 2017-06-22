
''' <summary>
''' 创建申明一个内存变量的表达式
''' 
''' ```R
''' var x &lt;- 123;
''' var s &lt;- "Hello world!";
''' ```
''' </summary>
Public Class VariableDeclareExpression : Inherits PrimitiveExpression

    ''' <summary>
    ''' 左边的变量名称
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
    ''' <summary>
    ''' type constraint
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Type As TypeCodes = TypeCodes.generic

    Sub New(name$, type$, initialize As PrimitiveExpression)

    End Sub

    Public Overrides Function ToString() As String
        Return $"Dim {Name} As {Type.Description} = "
    End Function
End Class

Public Class TupleDeclareExpression : Inherits VariableDeclareExpression

    Public Property Members As TupleMember()

End Class

Public Structure TupleMember

    Dim Name$
    Dim Alias$

End Structure