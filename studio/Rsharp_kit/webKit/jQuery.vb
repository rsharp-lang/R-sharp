﻿
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Vectorization

<Package("jQuery")>
Public Class jQuery : Implements RIndexer, RIndex

    Dim page As InnerPlantText

    Public ReadOnly Property length As Integer Implements RIndex.length
        Get
            If TypeOf page Is HtmlDocument Then
                Return DirectCast(page, HtmlDocument).HtmlElements.TryCount
            ElseIf TypeOf page Is HtmlElement Then
                Return DirectCast(page, HtmlElement).HtmlElements.TryCount
            Else
                Return 1
            End If
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(tag As InnerPlantText)
        page = tag
    End Sub

    Public Function EvaluateIndexer(expr As Expression, env As Environment) As Object Implements RIndexer.EvaluateIndexer
        Dim q As String = CLRVector.asCharacter(expr.Evaluate(env)).First

        If q = "innerHTML" Then
            Return page.GetHtmlText.Trim.GetValue.Trim
        ElseIf q = "innerText" Then
            Return page.GetPlantText.Trim
        Else
            Dim sel As New CSSSelector(q)
            Dim parse = sel.Parse(page, isArray:=q.First <> "#"c, New Engine)

            Return New jQuery With {.page = parse}
        End If
    End Function

    <ExportAPI("load")>
    Public Shared Function load(url As String) As jQuery
        Return New jQuery With {.page = HtmlDocument.LoadDocument(url.GET)}
    End Function

    Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
        If TypeOf page Is HtmlDocument Then
            Return New jQuery(DirectCast(page, HtmlDocument).HtmlElements.ElementAtOrDefault(i - 1))
        ElseIf TypeOf page Is HtmlElement Then
            Return New jQuery(DirectCast(page, HtmlElement).HtmlElements.ElementAtOrDefault(i - 1))
        Else
            Return Me
        End If
    End Function

    Public Function getByIndex(i() As Integer) As Array Implements RIndex.getByIndex
        Return i.Select(AddressOf getByIndex).ToArray
    End Function

    Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
        Throw New NotImplementedException()
    End Function

    Public Function setByindex(i() As Integer, value As Array, envir As Environment) As Object Implements RIndex.setByindex
        Throw New NotImplementedException()
    End Function
End Class