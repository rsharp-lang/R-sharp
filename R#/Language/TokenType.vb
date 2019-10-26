Namespace Language

    Public Enum TokenType
        invalid

        [string]
        comment
        ''' <summary>
        ''' 类似于在VisualBasic中的自定义属性的注解语法
        ''' </summary>
        annotation
        identifier
        keyword
        [operator]
        literal
        ''' <summary>
        ''' 左边的括号与大括号
        ''' </summary>
        open
        ''' <summary>
        ''' 右边的括号与大括号
        ''' </summary>
        close
    End Enum
End Namespace