#Region "Microsoft.VisualBasic::7421a4e83a193df8938f76a7c42ba8ec, R#\Interpreter\ExecuteEngine\Linq\Query\ProjectionExpression.vb"

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

    '     Class ProjectionExpression
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' from ... select ...
    ''' </summary>
    Public Class ProjectionExpression : Inherits QueryExpression

        Dim opt As Options

        Friend ReadOnly project As OutputProjection

        Public Overrides ReadOnly Property name As String
            Get
                Return "from ... [select ...]"
            End Get
        End Property

        Sub New(symbol As SymbolDeclare, sequence As Expression, exec As IEnumerable(Of Expression), proj As OutputProjection, opt As Options)
            Call MyBase.New(symbol, sequence, exec)

            Me.opt = opt
            Me.project = proj

            If proj Is Nothing Then
                ' 当不存在投影表达式的时候
                ' 默认返回所有的symbol
                If symbol.isTuple Then
                    Dim fields = DirectCast(symbol.symbol, VectorLiteral).elements _
                        .Select(Function(a)
                                    Dim name As String = a.ToString

                                    If TypeOf a Is Literal Then
                                        name = any.ToString(DirectCast(a, Literal).value)
                                    ElseIf TypeOf a Is SymbolReference Then
                                        name = DirectCast(a, SymbolReference).symbolName
                                    End If

                                    Return New NamedValue(Of Expression)(name, a)
                                End Function) _
                        .ToArray

                    project = New OutputProjection(fields)
                Else
                    project = New OutputProjection({New NamedValue(Of Expression)("*", symbol.symbol)})
                End If
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns>
        ''' array of <see cref="JavaScriptObject"/>
        ''' </returns>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim projections As New List(Of JavaScriptObject)
            Dim closure As New ExecutableContext(New Environment(context, context.stackFrame, isInherits:=False))
            Dim skipVal As Boolean
            Dim err As Message = symbol.Exec(closure)

            If Not err Is Nothing Then
                Return err
            Else
                dataset = GetDataSet(context)
            End If

            If TypeOf dataset Is ErrorDataSet Then
                Return DirectCast(dataset, ErrorDataSet).message
            End If

            For Each item As Object In dataset.PopulatesData()
                err = symbol.SetValue(item, closure)

                If Not err Is Nothing Then
                    Return err
                End If

                For Each line As Expression In executeQueue
                    If TypeOf line Is WhereFilter Then
                        skipVal = Not DirectCast(line.Exec(closure), Boolean)

                        If skipVal Then
                            Exit For
                        End If
                    End If
                Next

                If Not skipVal Then
                    projections.Add(project.Exec(closure))
                End If
            Next

            Return opt.RunOptionPipeline(projections, context).ToArray
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"FROM {symbol} IN {sequence}")

            For Each line In executeQueue
                Call sb.AppendLine(line.ToString)
            Next

            Call sb.AppendLine(project.ToString)
            Call sb.AppendLine(opt.ToString)

            Return sb.ToString
        End Function
    End Class
End Namespace
