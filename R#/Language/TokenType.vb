Namespace Language

    ''' <summary>
    ''' Language syntax token types
    ''' </summary>
    Public Enum TokenType As Byte
        invalid
        newLine

        comment
        ''' <summary>
        ''' 类似于在VisualBasic中的自定义属性的注解语法
        ''' </summary>
        annotation
        identifier
        ''' <summary>
        ''' 必须要使用``;``作为表达式的结束分隔符
        ''' </summary>
        terminator
        comma
        keyword
        [operator]
        ''' <summary>
        ''' :
        ''' </summary>
        sequence
        ''' <summary>
        ''' ?
        ''' </summary>
        iif

        ''' <summary>
        ''' 字符串插值语法，与javascript脚本之中的字符串插值保持一致
        ''' </summary>
        stringInterpolation
        ''' <summary>
        ''' 命令行调用语法
        ''' </summary>
        cliShellInvoke

        ''' <summary>
        ''' NULL, NA, Inf
        ''' </summary>
        missingLiteral
        stringLiteral
        numberLiteral
        integerLiteral
        booleanLiteral

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