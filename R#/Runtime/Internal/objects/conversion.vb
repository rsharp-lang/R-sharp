#Region "Microsoft.VisualBasic::8175659ef54a52847a9d2b60900ab4ae, R#\Runtime\Internal\objects\conversion.vb"

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

'     Module RConversion
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: asObject, CTypeDynamic
' 
'         Sub: pushEnvir
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Runtime.Internal

    Module RConversion

        <ExportAPI("as.object")>
        Public Function asObject(obj As Object) As Object
            If obj Is Nothing Then
                Return Nothing
            Else
                Dim type As Type = obj.GetType

                Select Case type
                    Case GetType(vbObject), GetType(vector), GetType(list)
                        Return obj
                    Case Else
                        If type.IsArray Then
                            Return Runtime.asVector(Of Object)(obj) _
                                .AsObjectEnumerator _
                                .Select(Function(o) New vbObject(o)) _
                                .ToArray
                        Else
                            Return New vbObject(obj)
                        End If
                End Select
            End If
        End Function

        <ExportAPI("as.list")>
        Public Function asList(obj As Object) As list
            If obj Is Nothing Then
                Return Nothing
            Else
                Dim type As Type = obj.GetType

                Select Case type
                    Case GetType(Dictionary(Of String, Object))
                        Return New list With {.slots = obj}
                    Case GetType(list)
                        Return obj
                    Case Else
                        If type.ImplementInterface(GetType(IDictionary)) Then
                            Dim objList As New Dictionary(Of String, Object)

                            With DirectCast(obj, IDictionary)
                                For Each key As Object In .Keys
                                    Call objList.Add(Scripting.ToString(key), .Item(key))
                                Next
                            End With

                            Return New list With {.slots = objList}
                        Else
                            Throw New NotImplementedException
                        End If
                End Select
            End If
        End Function

        <ExportAPI("as.numeric")>
        Public Function asNumeric(obj As Object) As Object
            If obj Is Nothing Then
                Return 0
            Else
                Return Runtime.asVector(Of Double)(obj)
            End If
        End Function

        Public Function CTypeDynamic(obj As Object, type As Type) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf type Is GetType(vbObject) Then
                Return asObject(obj)
            ElseIf obj.GetType Is GetType(vbObject) Then
                obj = DirectCast(obj, vbObject).target
            End If

            Return Conversion.CTypeDynamic(obj, type)
        End Function
    End Module
End Namespace
