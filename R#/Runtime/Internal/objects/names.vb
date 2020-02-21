#Region "Microsoft.VisualBasic::fed23d06fd157e5bfd0c1eafa512d2cb, R#\Runtime\Internal\objects\names.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline

Namespace Runtime.Internal.Object

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

                        If objVec _
                            .AsObjectEnumerator _
                            .All(Function(o)
                                     Return o.GetType Is GetType(Group)
                                 End Function) Then

                            Return objVec.AsObjectEnumerator _
                                .Select(Function(g)
                                            Return Scripting.ToString(DirectCast(g, Group).key, "NULL")
                                        End Function) _
                                .ToArray
                        ElseIf objVec _
                            .AsObjectEnumerator _
                            .All(Function(o)
                                     Return o.GetType.ImplementInterface(GetType(INamedValue))
                                 End Function) Then

                            Return objVec.AsObjectEnumerator _
                                .Select(Function(o)
                                            Return DirectCast(o, INamedValue).Key
                                        End Function) _
                                .ToArray
                        End If
                    ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                        Dim keys As vector = DirectCast([object], IDictionary) _
                            .Keys _
                            .DoCall(Function(a)
                                        Return New vector(GetType(String), a, env:=envir)
                                    End Function)

                        Return keys
                    End If

                    Return Internal.stop({"unsupported!", "func: names"}, envir)
            End Select
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="namelist">This method will ensure that the value is a string vector</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function setNames([object] As Object, namelist As Array, envir As Environment) As Object
            namelist = Runtime.asVector(Of String)(namelist)

            ' set names
            Select Case [object].GetType
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).setNames(namelist, envir)
                Case Else
                    Return Internal.stop({"unsupported!", "func: names"}, envir)
            End Select
        End Function

        Public Function getRowNames([object] As Object, envir As Environment) As Object
            If [object].GetType Is GetType(dataframe) Then
                Return DirectCast([object], dataframe).rownames
            Else
                Return getNames([object], envir)
            End If
        End Function

        Public Function setRowNames([object] As Object, namelist As Array, envir As Environment) As Object
            Throw New NotImplementedException
        End Function

        Public Function getColNames([object] As Object, envir As Environment) As Object
            If [object].GetType Is GetType(dataframe) Then
                Return DirectCast([object], dataframe).columns.Keys.ToArray
            Else
                Return getNames([object], envir)
            End If
        End Function

        Public Function setColNames([object] As Object, namelist As Array, envir As Environment) As Object
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
