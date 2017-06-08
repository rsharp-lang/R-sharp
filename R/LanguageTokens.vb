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
    ''' :
    ''' </summary>
    methodCall
    ''' <summary>
    ''' ,
    ''' </summary>
    ParameterDelimiter
    ''' <summary>
    ''' |
    ''' </summary>
    Pipeline

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
    [Function]
End Enum