#Region "Microsoft.VisualBasic::73f145f1f783546beeb6a77ef9f05163, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/webKit//jQuery.vb"

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

    '   Total Lines: 87
    '    Code Lines: 72
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 3.16 KB


    ' Class jQuery
    ' 
    '     Properties: length
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: EvaluateIndexer, (+2 Overloads) getByIndex, load, setByindex, setByIndex
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

<Package("jQuery")>
<RPolyglotSymbol("$")>
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
    Public Shared Function load(url As String, Optional proxy As IHttpGet = Nothing) As jQuery
        Dim doc As String

        If proxy Is Nothing Then
            doc = url.GET
        Else
            doc = proxy.GetText(url)
        End If

        Return New jQuery With {.page = HtmlDocument.LoadDocument(doc)}
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
