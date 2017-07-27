Namespace Runtime.CodeDOM

    ''' <summary>
    ''' Declare a function
    ''' 
    ''' ```
    ''' func &lt;- function(a,b, c as integer = a - 99 + b / 2) {
    '''    return [a * 2, b ^ 3, mean({a, b, c})];
    ''' }
    ''' ```
    ''' </summary>
    Public Class FunctionExpression : Inherits Closure

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
        ''' <summary>
        ''' 当存在表达式初始化的时候为可选参数，反之为必须参数
        ''' </summary>
        ''' <returns></returns>
        Public Property Parameters As VariableDeclareExpression()

    End Class

    Public Class Closure : Inherits PrimitiveExpression


    End Class

    Public Class ForLoop : Inherits Closure

    End Class

    Public Class IfBranch : Inherits Closure

    End Class

    Public Class DoLoop : Inherits Closure

    End Class
End Namespace