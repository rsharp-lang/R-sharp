#Region "Microsoft.VisualBasic::aa8ce8ce9d14b3c862b13ae4f5ded3d3, R#\Runtime\Internal\objects\RConversion\castList.vb"

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

    '     Module castList
    ' 
    '         Function: CTypeList, listInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Runtime.Internal.Object.Converts

    ''' <summary>
    ''' cast ``R#`` <see cref="list"/> to <see cref="Dictionary(Of String, TValue)"/>
    ''' </summary>
    Module castList

        <Extension>
        Public Function CTypeList(list As list, type As Type, env As Environment) As Object
            Dim table As IDictionary = Activator.CreateInstance(type)
            Dim keyType As Type = type.GenericTypeArguments(Scan0)
            Dim valType As Type = type.GenericTypeArguments(1)
            Dim key As Object
            Dim val As Object

            For Each item In list.slots
                key = Scripting.CTypeDynamic(item.Key, keyType)
                val = RCType.CTypeDynamic(item.Value, valType, env)
                table.Add(key, val)
            Next

            Return table
        End Function

        Friend Function listInternal(obj As Object, args As list) As list
            Dim type As Type = obj.GetType

            Select Case type
                Case GetType(Dictionary(Of String, Object))
                    Return New list With {.slots = obj}
                Case GetType(list)
                    Return obj
                Case GetType(vbObject)
                    ' object property as list data
                    Return DirectCast(obj, vbObject).toList
                Case GetType(dataframe)
                    Dim byRow As Boolean = Runtime.asLogical(args!byrow)(Scan0)

                    If byRow Then
                        Return DirectCast(obj, dataframe).listByRows
                    Else
                        Return DirectCast(obj, dataframe).listByColumns
                    End If
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
        End Function
    End Module
End Namespace
