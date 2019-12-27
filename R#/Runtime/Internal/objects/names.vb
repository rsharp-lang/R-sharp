#Region "Microsoft.VisualBasic::2252896e733ff6729cd389bfb90d42e1, R#\Runtime\Internal\objects\names.vb"

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

    '     Module names
    ' 
    '         Function: getColNames, getNames, getRowNames, setColNames, setNames
    '                   setRowNames
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline

Namespace Runtime.Internal

    Module names

        Public Function getNames([object] As Object, envir As Environment) As Object
            Dim type As Type = [object].GetType

            ' get names
            Select Case type
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).getNames
                Case GetType(vbObject)
                    Return DirectCast([object], vbObject).getNames
                Case Else
                    If type.IsArray Then
                        Dim objVec As Array = Runtime.asVector(Of Object)([object])

                        If objVec.AsObjectEnumerator.All(Function(o) o.GetType Is GetType(Group)) Then
                            Return objVec.AsObjectEnumerator _
                                .Select(Function(g)
                                            Return Scripting.ToString(DirectCast(g, Group).key, "NULL")
                                        End Function) _
                                .ToArray
                        End If
                    End If
                    Return Internal.stop({"unsupported!", "func: names"}, envir)
            End Select
        End Function

        Public Function setNames([object] As Object, namelist As Array, envir As Environment) As Object
            ' set names
            Select Case [object].GetType
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).setNames(namelist, envir)
                Case Else
                    Return Internal.stop({"unsupported!", "func: names"}, envir)
            End Select
        End Function

        Public Function getRowNames() As Object

        End Function

        Public Function setRowNames() As Object

        End Function

        Public Function getColNames() As Object

        End Function

        Public Function setColNames() As Object

        End Function
    End Module
End Namespace
