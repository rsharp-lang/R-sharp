Public Enum LanguageTokens

    undefine
    ''' <summary>
    ''' identifier, value expression, etc.(允许使用小数点作为变量名称的一部分)
    ''' </summary>
    [Object]
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
    ''' :
    ''' </summary>
    methodCall
    ''' <summary>
    ''' ,
    ''' </summary>
    ParameterDelimiter

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
    var
    ''' <summary>
    ''' 字符串值
    ''' </summary>
    [String]
    Comment

End Enum