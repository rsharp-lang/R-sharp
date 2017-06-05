Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports langToken = Microsoft.VisualBasic.Scripting.TokenIcer.Token(Of R.LanguageTokens)

Public Module TokenIcer

    <Extension> Public Iterator Function Parse(s$) As IEnumerable(Of Statement)
        Dim buffer As New Pointer(Of Char)(Trim(s$))
        Dim QuotOpen As Boolean = False
        Dim tmp As New List(Of Char)
        Dim tokens As New List(Of Token(Of LanguageTokens))
        Dim last As Statement

        Do While Not buffer.EndRead
            Dim c As Char = +buffer

            If QuotOpen Then ' 当前所解析的状态为字符串解析
                If c = ASCII.Quot AndAlso Not tmp.StartEscaping Then
                    ' 当前的字符为双引号，并且不是转义状态，则结束字符串
                    tokens += New langToken With {
                        .Name = LanguageTokens.String,
                        .Value = New String(tmp)
                    }
                    tmp *= 0
                    QuotOpen = False
                Else
                    ' 任然是字符串之中的一部分字符，则继续添加进入tmp之中
                    tmp += c
                End If
            Else
                ' 遇见了字符串的起始的第一个双引号
                If Not QuotOpen AndAlso c = ASCII.Quot Then
                    QuotOpen = True
                Else
                    ' 遇见了语句的结束符号
                    If c = ";"c Then
                        ' 结束当前的statement的解析
                        last = New Statement With {
                            .Tokens = tokens
                        }
                        tokens *= 0

                        Yield last
                    ElseIf c = " "c OrElse c = ASCII.TAB Then
                        ' 遇见了空格，结束当前的token
                        tokens += New langToken With {
                            .Name = LanguageTokens.Identifier,
                            .Value = New String(tmp)
                        }
                        tmp *= 0
                    Else
                        tmp += c
                    End If
                End If
            End If
        Loop
    End Function

    <Extension> Public Function GetSourceTree(s As Statement) As String
        Return s.GetXml
    End Function
End Module

''' <summary>
''' 
''' </summary>
Public Class Statement

    <XmlElement("tokens")>
    Public Tokens As langToken()
    ''' <summary>
    ''' 堆栈
    ''' </summary>
    Public Child As Statement

End Class

Public Enum LanguageTokens

    ''' <summary>
    ''' 允许使用小数点作为变量名称的一部分
    ''' </summary>
    Identifier
    ''' <summary>
    ''' &lt;-
    ''' </summary>
    LeftAssign
    ParameterAssign
    StackOpen
    StackClose
    BracketOpen
    BracketClose
    var
    ''' <summary>
    ''' 字符串值
    ''' </summary>
    [String]

End Enum