#Region "Microsoft.VisualBasic::d21ae8acb71dc9d9c401d3a56d126eca, R-sharp\R#\Runtime\Internal\objects\RConversion\castList.vb"

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

    '   Total Lines: 145
    '    Code Lines: 116
    ' Comment Lines: 5
    '   Blank Lines: 24
    '     File Size: 5.80 KB


    '     Module castList
    ' 
    '         Function: CTypeList, dataframe_castList, listInternal, objCastList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.CType
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

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
                key = any.CTypeDynamic(item.Key, keyType)
                val = RCType.CTypeDynamic(item.Value, valType, env)
                table.Add(key, val)
            Next

            Return table
        End Function

        Friend Function listInternal(obj As Object, args As list, env As Environment) As Object
            Dim type As Type = obj.GetType

            If type.ImplementInterface(Of ICTypeList) Then
                Return DirectCast(obj, ICTypeList).toList
            End If

            Select Case type
                Case GetType(Dictionary(Of String, Object))
                    Return New list With {.slots = obj}
                Case GetType(list)
                    Return obj
                Case GetType(vbObject)
                    ' object property as list data
                    Return DirectCast(obj, vbObject).objCastList(args, env)
                Case GetType(dataframe)
                    Return dataframe_castList(obj, args, env)
                Case Else
                    ' 将字典对象转换为列表对象
                    If type.ImplementInterface(GetType(IDictionary)) Then
                        Dim objList As New Dictionary(Of String, Object)
                        Dim eleType As RType = RType.any

                        With DirectCast(obj, IDictionary)
                            For Each key As Object In .Keys
                                Call objList.Add(any.ToString(key), .Item(key))
                            Next
                        End With

                        If Not type.GetGenericArguments.ElementAtOrDefault(1) Is Nothing Then
                            eleType = RType.GetRSharpType(type.GetGenericArguments()(1))
                        End If

                        Return New list With {
                            .slots = objList,
                            .elementType = eleType
                        }
                    Else
                        Return New vbObject(obj).objCastList(args, env)
                    End If
            End Select
        End Function

        <Extension>
        Private Function dataframe_castList(obj As Object, args As list, env As Environment) As Object
            Dim byRow As Boolean = REnv.asLogical(args!byrow)(Scan0)
            Dim names As String = any.ToString(REnv.getFirst(args!names), null:=Nothing)

            If byRow Then
                Dim list As list = DirectCast(obj, dataframe).listByRows

                If Not names.StringEmpty Then
                    If DirectCast(obj, dataframe).hasName(names) Then
                        obj = list.setNames(DirectCast(obj, dataframe).columns(names), env)

                        If TypeOf obj Is Message Then
                            Return obj
                        End If
                    Else
                        Return Internal.debug.stop({
                            $"undefined column '{names}' that selected for used as list names!",
                            $"column: {names}"
                        }, env)
                    End If
                End If

                Return list
            Else
                Return DirectCast(obj, dataframe).listByColumns
            End If
        End Function

        <Extension>
        Private Function objCastList(vbobj As vbObject, args As list, env As Environment) As Object
            Dim list As New Dictionary(Of String, Object)

            For Each p As KeyValuePair(Of String, PropertyInfo) In vbobj.properties
                Dim value As Object = p.Value.GetValue(vbobj.target)

                If value Is Nothing Then
                    list.Add(p.Key, Nothing)
                ElseIf TypeOf value Is Array OrElse TypeOf value Is vector Then
                    list.Add(p.Key, value)
                Else
                    value = listInternal(value, args, env)

                    If TypeOf value Is Message Then
                        Return value
                    Else
                        list.Add(p.Key, value)
                    End If
                End If
            Next

            If vbobj.type.haveDynamicsProperty Then
                Dim dynamic As IDynamicsObject = vbobj.target

                For Each name As String In dynamic _
                    .GetNames _
                    .Where(Function(nameKey)
                               Return Not list.ContainsKey(nameKey)
                           End Function)

                    Call list.Add(name, dynamic.GetItemValue(name))
                Next
            End If

            Return New list With {.slots = list}
        End Function
    End Module
End Namespace
