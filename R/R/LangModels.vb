Imports System.Xml.Serialization
Imports langToken = Microsoft.VisualBasic.Scripting.TokenIcer.Token(Of R.LanguageTokens)

''' <summary>
''' 
''' </summary>
Public Class Statement

    <XmlElement("tokens")>
    Public Property Tokens As langToken()
    ''' <summary>
    ''' if/for/do/function堆栈
    ''' </summary>
    Public Property Child As Statement()

End Class

Public Enum LanguageTokens

    undefine
    ''' <summary>
    ''' 允许使用小数点作为变量名称的一部分
    ''' </summary>
    Identifier
    ''' <summary>
    ''' &lt;-
    ''' </summary>
    LeftAssign
    ParameterAssign
    methodCall
    StackOpen
    StackClose
    BracketOpen
    BracketClose
    StringContact
    ''' <summary>
    ''' Variable declare init
    ''' </summary>
    var
    ''' <summary>
    ''' 字符串值
    ''' </summary>
    [String]

End Enum