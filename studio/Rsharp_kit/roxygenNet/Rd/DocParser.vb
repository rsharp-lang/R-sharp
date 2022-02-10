Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework

Public Class DocParser : Inherits RDocParser

    ''' <summary>
    ''' 标签栈的名称缓存
    ''' </summary>
    Dim nameBuffer As New List(Of Char)
    Dim parserEscape As New Escapes
    Dim doc As New RDoc With {
        .comments = ""
    }

    Dim textWriter As Dictionary(Of String, Action(Of String)) = GetType(RDoc) _
        .GetProperties(PublicProperty) _
        .Where(Function(p) p.PropertyType Is GetType(String)) _
        .ToDictionary(Function(p) p.Name.ToLower,
                      Function(p) As Action(Of String)
                          Return Sub(text As String)
                                     Call p.SetValue(doc, text)
                                 End Sub
                      End Function)

    Dim contentWriter As Dictionary(Of String, Action(Of Doc)) = GetType(RDoc) _
        .GetProperties(PublicProperty) _
        .Where(Function(p) p.PropertyType Is GetType(Doc)) _
        .ToDictionary(Function(p) p.Name,
                      Function(p) As Action(Of Doc)
                          Return Sub(doc As Doc)
                                     Call p.SetValue(Me.doc, doc)
                                 End Sub
                      End Function)

    Public Class Escapes

        ''' <summary>
        ''' Rd文件之中的注释为单行注释
        ''' </summary>
        Public docComment As Boolean
        Public stackOpen As Boolean
        Public stackNameParser As Boolean

    End Class

    Private Sub New(text As Pointer(Of Char))
        Me.text = text
    End Sub

    Public Function CreateDoc() As RDoc
        Do While Not text.EndRead
            Call walkChar(++text)
        Loop

        Return doc
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="rd">The text content of ``*.Rd`` file.</param>
    ''' <returns></returns>
    Public Shared Function ParseDoc(rd As String) As RDoc
        Return New DocParser(text:=New Pointer(Of Char)(rd.SolveStream)).CreateDoc
    End Function

    Protected Overrides Sub walkChar(c As Char)
        If c = "%"c AndAlso buffer = 0 Then
            ' 以%符号起始，并且缓存为空，则说明是一条注释文本
            parserEscape.docComment = True
        ElseIf c = "{"c AndAlso parserEscape.stackNameParser Then
            ' 结束解析标签名称
            Dim stackName As New String(nameBuffer.PopAll)

            Select Case stackName
                Case "name", "alias"
                    ' 纯文本内容
                    textWriter(stackName)(New PlainTextParser(text).GetCurrentText)
                Case "usage", "examples"
                    textWriter(stackName)(New RcodeParser(text).GetCurrentText)
                Case "title", "details", "description"
                    contentWriter(stackName)(New ContentParser(text).GetCurrentContent)
                Case "arguments"

                Case Else
                    Throw New NotImplementedException(stackName)
            End Select

        ElseIf c = ASCII.CR OrElse c = ASCII.LF Then
            If parserEscape.docComment Then
                ' 单行注释
                doc.comments = doc.comments & New String(buffer.PopAll)
            End If
        ElseIf parserEscape.stackNameParser Then
            nameBuffer += c
        ElseIf parserEscape.docComment Then
            buffer += c
        ElseIf c = "\"c Then
            ' 不是注释部分的文本
            ' 则通过\符号起始的是一个标签栈
            parserEscape.stackNameParser = True
        End If
    End Sub
End Class

Public MustInherit Class RDocParser

    Protected text As Pointer(Of Char)
    Protected buffer As New List(Of Char)

    Protected MustOverride Sub walkChar(c As Char)

End Class

Public Class ContentParser : Inherits RDocParser

    Friend Sub New(text As Pointer(Of Char))
        Me.text = text
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        Throw New NotImplementedException()
    End Sub

    ''' <summary>
    ''' 可能包含有纯文本以及以下的标签：``\code{}``,``\link{}``,``\enumerate{}``
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCurrentContent() As Doc
        Throw New NotImplementedException
    End Function
End Class

Public Class PlainTextParser : Inherits RDocParser

    Protected endContent As Boolean = False

    Friend Sub New(text As Pointer(Of Char))
        Me.text = text
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        If c = "}"c Then
            endContent = True
        Else
            buffer += c
        End If
    End Sub

    Public Function GetCurrentText() As String
        Do While Not endContent
            Call walkChar(++text)
        Loop

        Return New String(buffer)
    End Function
End Class

''' <summary>
''' ``examples``标签里面为R代码，所以可能会出现字符串和代码注释
''' 因为字符串或者注释之中可能会存在``{}``符号，所以会需要额外的
''' 信息来解决这个问题
''' </summary>
Public Class RcodeParser : Inherits PlainTextParser

    ''' <summary>
    ''' 因为R代码示例之中任然可能含有``{}``符号，所以会需要使用一个栈对象来确定文本结束的位置
    ''' </summary>
    Dim codeStack As New Stack(Of Char)
    Dim commentEscape As Boolean
    Dim stringEscape As Boolean

    Friend Sub New(text As Pointer(Of Char))
        Call MyBase.New(text)
        Call Me.codeStack.Push("{")
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        If c = "#"c Then
            If stringEscape Then
                ' Do nothing
            ElseIf Not commentEscape Then
                commentEscape = True
            End If
        ElseIf c = """"c Then
            If commentEscape Then
                ' Do nothing
            ElseIf Not stringEscape Then
                stringEscape = True
            ElseIf stringEscape Then
                stringEscape = False
            End If
        End If

        If (Not commentEscape) AndAlso (Not stringEscape) Then
            If c = "{"c Then
                codeStack.Push("{"c)
            ElseIf c = "}"c Then
                codeStack.Pop()
            End If
        End If

        If codeStack.Count = 0 Then
            endContent = True
        Else
            buffer += c
        End If
    End Sub
End Class