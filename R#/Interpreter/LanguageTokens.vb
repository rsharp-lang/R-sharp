Public Enum LanguageTokens As Byte

    undefine = 0
    ''' <summary>
    ''' identifier, value expression, etc.(允许使用小数点作为变量名称的一部分)
    ''' </summary>
    [Object] = 10
    ''' <summary>
    ''' &lt;-
    ''' </summary>
    LeftAssign
    ''' <summary>
    ''' =
    ''' </summary>
    ParameterAssign
    [Operator]
    ''' <summary>
    ''' 使用``:``操作符来调用``.NET``方法
    ''' 使用``$``操作符调用自身的R#方法
    ''' </summary>
    DotNetMethodCall
    ''' <summary>
    ''' ,
    ''' </summary>
    ParameterDelimiter
    ''' <summary>
    ''' |
    ''' </summary>
    Pipeline
    ''' <summary>
    ''' 目标括号对象是一个优先级的改变运算，而非函数调用
    ''' </summary>
    Priority
    ''' <summary>
    ''' ([{
    ''' </summary>
    ParenOpen
    ''' <summary>
    ''' )}]
    ''' </summary>
    ParenClose

    ''' <summary>
    ''' &amp;
    ''' </summary>
    StringContact
    ''' <summary>
    ''' Variable declare init
    ''' </summary>
    Variable
    ''' <summary>
    ''' 字符串值
    ''' </summary>
    [String]
    ''' <summary>
    ''' 数值类型
    ''' </summary>
    Numeric
    ''' <summary>
    ''' 逻辑值类型
    ''' </summary>
    [Boolean]
    Comment
    [Function]
End Enum