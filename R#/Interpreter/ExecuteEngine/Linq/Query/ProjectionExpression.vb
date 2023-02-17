#Region "Microsoft.VisualBasic::df72b2e14d8f961204e161a26d064f7f, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/Query/ProjectionExpression.vb"

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

    '   Total Lines: 139
    '    Code Lines: 98
    ' Comment Lines: 12
    '   Blank Lines: 29
    '     File Size: 4.62 KB


    '     Class ProjectionExpression
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AssembleMultipleSource, AssembleSingleSource, Exec, ToString
    ' 
    '         Sub: FixProjection
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
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

        Friend project As OutputProjection

        Public Overrides ReadOnly Property name As String
            Get
                Return "from ... [select ...]"
            End Get
        End Property

        Sub New(symbol As SymbolDeclare, sequence As Expression, exec As IEnumerable(Of Expression), proj As OutputProjection, opt As Options)
            Call MyBase.New(symbol, sequence, exec)

            Me.opt = opt
            Me.project = proj
        End Sub

        Public Sub FixProjection()
            If Not project Is Nothing Then
                Return
            ElseIf joins.Count = 0 Then
                project = AssembleSingleSource()
            Else
                project = AssembleMultipleSource()
            End If
        End Sub

        Private Function AssembleMultipleSource() As OutputProjection
            Dim fields As New List(Of NamedValue(Of Expression))

            Call fields.AddRange(source.EnumerateFields(addSymbol:=True))

            For Each join As DataLeftJoin In joins
                Call fields.AddRange(join.EnumerateFields)
            Next

            Return New OutputProjection(fields)
        End Function

        Private Function AssembleSingleSource() As OutputProjection
            ' 当不存在投影表达式的时候
            ' 默认返回所有的symbol
            Return New OutputProjection(source.EnumerateFields(addSymbol:=False))
        End Function

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
            Dim symbol As SymbolDeclare = source.symbol
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

            If projections.Count = 0 Then
                Return projections.ToArray
            Else
                Dim output = opt.RunOptionPipeline(projections, context).SafeQuery.ToArray

                If opt.message Is Nothing Then
                    Return output
                Else
                    Return opt.message
                End If
            End If
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"FROM {source.symbol} IN {source.sequence}")

            For Each join As DataLeftJoin In joins
                Call sb.AppendLine(join.ToString)
            Next

            For Each line In executeQueue
                Call sb.AppendLine(line.ToString)
            Next

            Call sb.AppendLine(project.ToString)
            Call sb.AppendLine(opt.ToString)

            Return sb.ToString
        End Function
    End Class
End Namespace
