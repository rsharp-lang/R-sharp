#Region "Microsoft.VisualBasic::670498b182e641be087f9046b7652405, R-sharp\studio\Rsharp_kit\roxygenNet\Rd\DocParser.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 106
    '    Code Lines: 74
    ' Comment Lines: 17
    '   Blank Lines: 15
    '     File Size: 3.87 KB


    ' Class DocParser
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CreateDoc, ParseDoc
    ' 
    '     Sub: walkChar
    '     Class Escapes
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
