#Region "Microsoft.VisualBasic::77dcc39c5f46ac0103625f8606dc193e, R-sharp\R#\Runtime\Internal\objects\RConversion\castList.vb"

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

    '   Total Lines: 206
    '    Code Lines: 162
    ' Comment Lines: 12
    '   Blank Lines: 32
    '     File Size: 7.76 KB


    '     Module castList
    ' 
    '         Function: CTypeList, dataframe_castList, dictionaryToRList, listInternal, objCastList
    '                   vector_castList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
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

        Public Function vector_castList(vec As Array, args As list, env As Environment) As Object
            If vec.Length = 1 Then
                Return listInternal(vec.GetValue(Scan0), args, env)
            End If

            Dim names As String() = args.getValue(Of String())("names", env)

            If names.IsNullOrEmpty Then
                names = vec.Length _
                    .Sequence _
                    .Select(Function(i) $"X_{i}") _
                    .ToArray
            Else
                names = names.uniqueNames
            End If

            If names.Length <> vec.Length Then
                Return Internal.debug.stop({
                    $"size of names is mis-matched with the dimension of your vector!",
                    $"sizeof_names: {names.Length}",
                    $"dimensionof_vector: {vec.Length}"
                }, env)
            End If

            Dim newList As New list With {
                .slots = New Dictionary(Of String, Object)
            }

            For i As Integer = 0 To vec.Length - 1
                Call newList.add(names(i), vec(i))
            Next

            Return newList
        End Function

        Friend Function listInternal(obj As Object, args As list, env As Environment) As Object
            Dim type As Type = obj.GetType

            If TypeOf obj Is vector Then
                Return vector_castList(DirectCast(obj, vector).data, args, env)
            ElseIf type.IsArray Then
                Return vector_castList(DirectCast(obj, Array), args, env)
            End If
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
                        Return DirectCast(obj, IDictionary).dictionaryToRList
                    Else
                        Return New vbObject(obj).objCastList(args, env)
                    End If
            End Select
        End Function

        <Extension>
        Friend Function dictionaryToRList(dict As IDictionary) As list
            Dim objList As New Dictionary(Of String, Object)
            Dim eleType As RType = RType.any
            Dim type As Type = dict.GetType

            With dict
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
        End Function

        <Extension>
        Private Function dataframe_castList(obj As Object, args As list, env As Environment) As Object
            Dim byRow As Boolean = Vectorization.asLogical(args!byrow)(Scan0)
            Dim names As String = any.ToString(REnv.getFirst(args!names), null:=Nothing)
            Dim df As dataframe = DirectCast(obj, dataframe)

            If byRow Then
                Return df.listByRows(names, env)
            Else
                Return df.listByColumns
            End If
        End Function

        ''' <summary>
        ''' cast .NET object to R# list object
        ''' </summary>
        ''' <param name="vbobj"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <Extension>
        Private Function objCastList(vbobj As vbObject, args As list, env As Environment) As Object
            Dim list As New Dictionary(Of String, Object)

            For Each p As KeyValuePair(Of String, PropertyInfo) In vbobj.properties
                Dim value As Object = p.Value.GetValue(vbobj.target)
                Dim name As String
                Dim ref As Field = p.Value.GetCustomAttribute(Of Field)

                If Not ref Is Nothing Then
                    name = ref.Name
                Else
                    name = p.Key
                End If

                If value Is Nothing Then
                    list.Add(name, Nothing)
                ElseIf TypeOf value Is Array OrElse TypeOf value Is vector Then
                    Dim v As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(value), env)
                    Dim type As Type = v.GetType.GetElementType

                    If Not DataFramework.IsPrimitive(type) Then
                        v = v _
                            .AsObjectEnumerator _
                            .Select(Function(o)
                                        Return listInternal(o, args, env)
                                    End Function) _
                            .ToArray
                    End If

                    list.Add(name, v)
                ElseIf DataFramework.IsPrimitive(value.GetType) Then
                    list.Add(name, value)
                Else
                    value = listInternal(value, args, env)

                    If TypeOf value Is Message Then
                        Return value
                    Else
                        list.Add(name, value)
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
