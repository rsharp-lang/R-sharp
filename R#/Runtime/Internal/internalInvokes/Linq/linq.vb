#Region "Microsoft.VisualBasic::81941280b58776c423acd983afc0244e, R#\Runtime\Internal\internalInvokes\Linq\linq.vb"

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

    '     Module linq
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: first, groupBy, projectAs, where
    ' 
    '         Sub: pushEnvir
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes.LinqPipeline

    Module linq

        Sub New()
            Call Internal.invoke.add("groupBy", AddressOf linq.groupBy)
            Call Internal.invoke.add("first", AddressOf linq.first)
            Call Internal.invoke.add("which", AddressOf linq.where)
            Call Internal.invoke.add("projectAs", AddressOf linq.projectAs)
        End Sub

        Friend Sub pushEnvir()
            ' do nothing
        End Sub

        Private Function projectAs(envir As Environment, params As Object()) As Object
            Dim sequence As Array = Runtime.asVector(Of Object)(params(Scan0))
            Dim project As RFunction = params(1)
            Dim result As Object() = sequence _
                .AsObjectEnumerator _
                .Select(Function(o)
                            Dim arg As New InvokeParameter() With {
                                .value = New RuntimeValueLiteral(o)
                            }

                            Return project.Invoke(envir, {arg})
                        End Function) _
                .ToArray

            Return result
        End Function

        ''' <summary>
        ''' The which test filter
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="params"></param>
        ''' <returns></returns>
        Private Function where(envir As Environment, params As Object()) As Object
            Dim sequence As Array = Runtime.asVector(Of Object)(params(Scan0))
            Dim test As RFunction = params(1)
            Dim pass As Boolean
            Dim arg As InvokeParameter
            Dim filter As New List(Of Object)

            For Each item As Object In sequence
                arg = New InvokeParameter() With {
                    .value = New RuntimeValueLiteral(item)
                }
                pass = Runtime.asLogical(test.Invoke(envir, {arg}))(Scan0)

                If pass Then
                    Call filter.Add(item)
                End If
            Next

            Return filter.ToArray
        End Function

        Private Function first(envir As Environment, params As Object()) As Object
            Dim sequence As Array = Runtime.asVector(Of Object)(params(Scan0))
            Dim test As RFunction = params(1)
            Dim pass As Boolean
            Dim arg As InvokeParameter

            For Each item As Object In sequence
                arg = New InvokeParameter() With {
                    .value = New RuntimeValueLiteral(item)
                }
                pass = Runtime.asLogical(test.Invoke(envir, {arg}))(Scan0)

                If pass Then
                    Return item
                End If
            Next

            Return Nothing
        End Function

        Private Function groupBy(envir As Environment, params As Object()) As Object
            Dim sequence As Array = Runtime.asVector(Of Object)(params(Scan0))
            Dim getKey As RFunction = params(1)
            Dim result = sequence.AsObjectEnumerator _
                .GroupBy(Function(o)
                             Dim arg As New InvokeParameter() With {
                                .value = New RuntimeValueLiteral(o)
                             }

                             Return getKey.Invoke(envir, {arg})
                         End Function) _
                .Select(Function(group)
                            Return New Group With {
                                .key = group.Key,
                                .group = group.ToArray
                            }
                        End Function) _
                .ToArray

            Return result
        End Function
    End Module
End Namespace
