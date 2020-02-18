#Region "Microsoft.VisualBasic::bd0dffe8cf8e5ed984bd537bbe4435ca, R#\Runtime\Internal\objects\conversion.vb"

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
'         Function: asCharacters, asInteger, asList, asLogicals, asNumeric
'                   asObject, CastToEnum, CTypeDynamic, listInternal, unlist
'                   unlistOfRList
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set

Namespace Runtime.Internal.Object.Converts

    Module RConversion

        <ExportAPI("as.Date")>
        Public Function asDate(<RRawVectorArgument> obj As Object) As Object
            Return Rset _
                .getObjectSet(obj) _
                .Select(Function(o)
                            If TypeOf o Is Date Then
                                Return CDate(o)
                            Else
                                Return Date.Parse(Scripting.ToString(o))
                            End If
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Cast .NET object to R# object
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("as.object")>
        Public Function asObject(<RRawVectorArgument> obj As Object) As Object
            If obj Is Nothing Then
                Return Nothing
            Else
                Dim type As Type = obj.GetType

                Select Case type
                    Case GetType(vbObject), GetType(vector), GetType(list)
                        Return obj
                    Case GetType(RReturn)
                        Return asObject(DirectCast(obj, RReturn).Value)
                    Case Else
                        If type.IsArray Then
                            Return Runtime.asVector(Of Object)(obj) _
                                .AsObjectEnumerator _
                                .Select(Function(o) New vbObject(o)) _
                                .ToArray
                        Else
                            Return New vbObject(obj, type)
                        End If
                End Select
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="[typeof]">element type of the array</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("unlist")>
        Public Function unlist(list As Object, Optional [typeof] As Object = Nothing, Optional env As Environment = Nothing) As Object
            If list Is Nothing Then
                Return Nothing
            End If
            If Not [typeof] Is Nothing Then
                Select Case [typeof].GetType
                    Case GetType(Type) ' do nothing
                    Case GetType(RType)
                        [typeof] = DirectCast([typeof], RType).raw
                    Case GetType(String)
                        Dim RType As RType = DirectCast([typeof], String) _
                            .GetRTypeCode _
                            .DoCall(AddressOf RType.GetType)

                        If RType.isArray Then
                            [typeof] = RType.raw.GetElementType
                        Else
                            [typeof] = RType.raw
                        End If
                    Case Else
                        Return Internal.stop(New NotImplementedException, env)
                End Select
            End If

            If list.GetType Is GetType(list) Then
                Return DirectCast(list, list).unlistOfRList([typeof], env)
            ElseIf list.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return New list(list).unlistOfRList([typeof], env)
            Else
                Return Internal.stop(New InvalidCastException(list.GetType.FullName), env)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rlist"></param>
        ''' <param name="[typeof]">element type of the array</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <Extension>
        Private Function unlistOfRList(rlist As list, [typeof] As Type, env As Environment) As Object
            Dim data As New List(Of Object)
            Dim names As New List(Of String)
            Dim vec As vector

            For Each name As String In rlist.getNames
                Dim a As Array = Runtime.asVector(Of Object)(rlist.slots(name))

                If a.Length = 1 Then
                    data.Add(a.GetValue(Scan0))
                    names.Add(name)
                Else
                    Dim i As i32 = 1

                    For Each item As Object In a
                        data.Add(item)
                        names.Add($"{name}{++i}")
                    Next
                End If
            Next

            If [typeof] Is Nothing Then
                vec = New vector(names, data.ToArray, env)
            Else
                vec = New vector([typeof], data.AsEnumerable, env)
                vec.setNames(names.ToArray, env)
            End If

            Return vec
        End Function

        ''' <summary>
        ''' ### Coerce to a Data Frame
        ''' 
        ''' Functions to check if an object is a data frame, or coerce it if possible.
        ''' </summary>
        ''' <param name="x">any R object.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("as.data.frame")>
        Public Function asDataframe(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return x
            End If

            Dim type As Type = x.GetType

            If makeDataframe.is_ableConverts(type) Then
                Return makeDataframe.createDataframe(type, x, env)
            Else
                Return Internal.stop(New InvalidProgramException, env)
            End If
        End Function

        ''' <summary>
        ''' Cast the raw dictionary object to R# list object
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("as.list")>
        Public Function asList(obj As Object, <RListObjectArgument> args As Object, Optional envir As Environment = Nothing) As list
            If obj Is Nothing Then
                Return Nothing
            Else
                Return listInternal(obj, base.Rlist(args, envir))
            End If
        End Function

        Private Function listInternal(obj As Object, args As list) As list
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

        ''' <summary>
        ''' Cast the given vector or list to integer type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("as.integer")>
        Public Function asInteger(<RRawVectorArgument> obj As Object) As Object
            If obj Is Nothing Then
                Return 0
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of Long)(obj)
            Else
                Return Runtime.asVector(Of Long)(obj)
            End If
        End Function

        <ExportAPI("as.numeric")>
        Public Function asNumeric(<RRawVectorArgument> obj As Object) As Object
            If obj Is Nothing Then
                Return 0
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of Double)(obj)
            Else
                Return Runtime.asVector(Of Double)(obj)
            End If
        End Function

        <ExportAPI("as.character")>
        Public Function asCharacters(<RRawVectorArgument> obj As Object) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of String)(obj)
            Else
                Return Runtime.asVector(Of String)(obj)
            End If
        End Function

        <ExportAPI("is.character")>
        Public Function isCharacter(<RRawVectorArgument> obj As Object) As Boolean
            If obj Is Nothing Then
                Return False
            ElseIf obj.GetType Is GetType(vector) Then
                obj = DirectCast(obj, vector).data
            ElseIf obj.GetType Is GetType(list) Then
                ' 只判断list的value
                obj = DirectCast(obj, list).slots.Values.ToArray
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                obj = DirectCast(obj, IDictionary).Values.AsSet.ToArray
            End If

            If obj.GetType Like BinaryExpression.characters Then
                    Return True
                ElseIf obj.GetType.IsArray AndAlso DirectCast(obj, Array).AsObjectEnumerator.All(Function(x) x.GetType Like BinaryExpression.characters) Then
                    Return True
                Else
                    Return False
            End If
        End Function

        <ExportAPI("as.logical")>
        Public Function asLogicals(<RRawVectorArgument> obj As Object) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of Boolean)(obj)
            Else
                Return Runtime.asLogical(obj)
            End If
        End Function

        ''' <summary>
        ''' If target <paramref name="type"/> is <see cref="Object"/>, then this function 
        ''' will stop the narrowing conversion from <see cref="vbObject"/> wrapper to 
        ''' object type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function CTypeDynamic(obj As Object, type As Type, env As Environment) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf type Is GetType(vbObject) Then
                Return asObject(obj)
            ElseIf obj.GetType Is GetType(vbObject) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, vbObject).target

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf obj.GetType Is GetType(RDispose) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, RDispose).Value

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf type.IsEnum Then
                Return CastToEnum(obj, type, env)
            End If

            Return Conversion.CTypeDynamic(obj, type)
        End Function

        Public Function CastToEnum(obj As Object, type As Type, env As Environment) As Object
            Dim REnum As REnum = REnum.GetEnumList(type)

            If obj.GetType Is GetType(String) Then
                If REnum.hasName(obj) Then
                    Return REnum.GetByName(obj)
                Else
                    Return debug.stop($"Can not convert string '{obj}' to enum type: {REnum.raw.FullName}", env)
                End If
            ElseIf obj.GetType.GetRTypeCode = TypeCodes.integer Then
                Return REnum.getByIntVal(obj)
            Else
                Return debug.stop($"Can not convert type '{obj.GetType.FullName}' to enum type: {REnum.raw.FullName}", env)
            End If
        End Function
    End Module
End Namespace
