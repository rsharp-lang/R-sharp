#Region "Microsoft.VisualBasic::ce8f1e71ef68375de4304b5e94416d2f, Library\graphics\grSVG.vb"

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

'   Total Lines: 47
'    Code Lines: 33 (70.21%)
' Comment Lines: 7 (14.89%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 7 (14.89%)
'     File Size: 1.83 KB


' Module grSVG
' 
'     Function: styleByclass
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' the SVG graphics device
''' </summary>
<Package("SVG")>
Module grSVG

    <ExportAPI("parse_svg")>
    Public Function parse(xml As String) As SvgDocument
        Return SvgDocument.Parse(xml)
    End Function

    ''' <summary>
    ''' find svg element by its text data
    ''' </summary>
    ''' <param name="svg"></param>
    ''' <param name="text"></param>
    ''' <returns>
    ''' this function will returns nothing if the given text element 
    ''' is not found inside the given svg document.
    ''' </returns>
    <ExportAPI("findText")>
    Public Function findText(svg As SvgDocument, text As String, Optional ignore_case As Boolean = False) As Object
        Dim find As New List(Of SvgText)

        For Each svgElem As SvgElement In svg.GetElements
            If TypeOf svgElem Is SvgText Then
                Dim textElem As SvgText = svgElem

                If textElem.Text = text OrElse (ignore_case AndAlso String.Equals(textElem.Text, text, StringComparison.OrdinalIgnoreCase)) Then
                    Call find.Add(textElem)
                End If
            End If
        Next

        Return find.ToArray
    End Function

    ''' <summary>
    ''' set styles for the svg text elements
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="color"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("text_style")>
    Public Function text_styles(<RRawVectorArgument>
                                text As Object,
                                Optional color As Object = Nothing,
                                Optional strong As Boolean? = Nothing,
                                Optional env As Environment = Nothing) As Object

        Dim pull As pipeline = pipeline.TryCreatePipeline(Of SvgText)(text, env)
        Dim color_str As String = Nothing

        If Not color Is Nothing Then
            color_str = RColorPalette.getColor(color, [default]:=Nothing, env)
        End If

        If pull.isError Then
            Return pull.getError
        End If

        For Each textElem As SvgText In pull.populates(Of SvgText)(env)
            If Not color_str Is Nothing Then
                textElem.Fill = color_str
            End If
            If Not strong Is Nothing Then
                textElem.SetStyle("font-weight", If(CBool(strong), "bolder", "normal"))
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Add css style by css selector
    ''' </summary>
    ''' <param name="svg"></param>
    ''' <param name="selector$"></param>
    ''' <param name="styles"></param>
    ''' <returns></returns>
    <ExportAPI("styles")>
    Public Function styleByclass(svg As SVGData, selector$,
                                 <RListObjectArgument>
                                 styles As Object,
                                 Optional env As Environment = Nothing) As SVGData

        Dim args = DirectCast(styles, InvokeParameter()).Skip(1).ToArray
        Dim stylesVal As list

        If args.Length = 1 AndAlso Not args(Scan0).haveSymbolName(hasObjectList:=False) AndAlso TypeOf args(Scan0).value Is FunctionInvoke Then
            Dim list As FunctionInvoke = args(Scan0).value

            If TypeOf list.funcName Is Literal AndAlso DirectCast(list.funcName, Literal).Evaluate(env) = "list" Then
                stylesVal = DirectCast(list.Evaluate(env), list)
            Else
                Throw New NotImplementedException
            End If
        Else
            Throw New NotImplementedException
        End If

        Dim cssVal$ = $"{selector} {{
{stylesVal.slots.Select(Function(t) $"{t.Key}: {t.Value};").JoinBy(vbCrLf)}
}}"

        svg.SVG.styles.Add(cssVal)

        Return svg
    End Function
End Module
