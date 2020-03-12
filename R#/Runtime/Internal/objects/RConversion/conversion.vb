#Region "Microsoft.VisualBasic::834574090ae48f43c3d63ae012509942, R#\Runtime\Internal\objects\RConversion\conversion.vb"

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
'         Function: asCharacters, asDataframe, asDate, asInteger, asList
'                   asLogicals, asNumeric, asObject, CastToEnum, CTypeDynamic
'                   isCharacter, listInternal, unlist, unlistOfRList
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set

Namespace Runtime.Internal.Object.Converts

    Module RConversion

        <ExportAPI("as.Date")>
        Public Function asDate(<RRawVectorArgument> obj As Object) As Date()
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
                        Return Internal.debug.stop(New NotImplementedException, env)
                End Select
            End If

            If list.GetType Is GetType(list) Then
                Return DirectCast(list, list).unlistOfRList([typeof], env)
            ElseIf list.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return New list(list).unlistOfRList([typeof], env)
            Else
                Return Internal.debug.stop(New InvalidCastException(list.GetType.FullName), env)
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
        <RApiReturn(GetType(dataframe))>
        Public Function asDataframe(<RRawVectorArgument> x As Object,
                                    <RListObjectArgument> args As Object,
                                    Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return x
            Else
                args = base.Rlist(args, env)

                If Program.isException(args) Then
                    Return args
                End If
            End If

            Dim type As Type = x.GetType

            If makeDataframe.is_ableConverts(type) Then
                Return makeDataframe.createDataframe(type, x, args, env)
            Else
                Return Internal.debug.stop(New InvalidProgramException, env)
            End If
        End Function

        <ExportAPI("as.vector")>
        Public Function asVector(obj As Object, Optional env As Environment = Nothing) As Object
            If obj Is Nothing Then
                Return obj
            End If

            If TypeOf obj Is vector Then
                Return obj
            ElseIf obj.GetType.IsArray Then
                Return New vector With {.data = obj}
            Else
                Dim interfaces = obj.GetType.GetInterfaces

                ' array of <T>
                For Each type In interfaces
                    If type.GenericTypeArguments.Length > 0 AndAlso type.ImplementInterface(Of IEnumerable) Then
                        Dim generic As Type = type.GenericTypeArguments(Scan0)
                        Dim buffer As Object = Activator.CreateInstance(GetType(List(Of )).MakeGenericType(generic))
                        Dim add As MethodInfo = buffer.GetType.GetMethod("Add", BindingFlags.Public Or BindingFlags.Instance)
                        Dim source As IEnumerator = DirectCast(obj, IEnumerable).GetEnumerator

                        source.MoveNext()
                        source = source.Current

                        ' get element count by list
                        Do While source.MoveNext
                            Call add.Invoke(buffer, {source.Current})
                        Loop

                        ' write buffered data to vector
                        Dim vec As Array = Array.CreateInstance(generic, DirectCast(buffer, IList).Count)
                        Dim i As i32 = Scan0

                        For Each x As Object In DirectCast(buffer, IEnumerable)
                            vec.SetValue(x, ++i)
                        Next

                        Return New vector With {.data = vec}
                    End If
                Next

                ' array of object
                For Each type In interfaces
                    If type Is GetType(IEnumerable) Then
                        Dim buffer As New List(Of Object)

                        For Each x As Object In DirectCast(obj, IEnumerable)
                            buffer.Add(x)
                        Next

                        Return New vector With {.data = buffer.ToArray}
                    End If
                Next

                ' obj is not a vector type
                env.AddMessage($"target object of '{obj.GetType.FullName}' can not be convert to a vector.", MSG_TYPES.WRN)

                Return obj
            End If
        End Function

        ''' <summary>
        ''' Cast the raw dictionary object to R# list object
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("as.list")>
        Public Function asList(obj As Object, <RListObjectArgument> args As Object, Optional env As Environment = Nothing) As list
            If obj Is Nothing Then
                Return Nothing
            Else
                Return listInternal(obj, base.Rlist(args, env))
            End If
        End Function

        ''' <summary>
        ''' Cast the given vector or list to integer type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("as.integer")>
        <RApiReturn(GetType(Long))>
        Public Function asInteger(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            If obj Is Nothing Then
                Return 0
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of Long)(obj, env)
            Else
                Return Runtime.asVector(Of Long)(obj)
            End If
        End Function

        <ExportAPI("as.numeric")>
        <RApiReturn(GetType(Double))>
        Public Function asNumeric(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            If obj Is Nothing Then
                Return 0
            End If

            If TypeOf obj Is list Then
                obj = DirectCast(obj, list).slots
            End If

            If obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of Double)(obj, env)
            Else
                Return Runtime.asVector(Of Double)(obj)
            End If
        End Function

        <ExportAPI("as.character")>
        <RApiReturn(GetType(String))>
        Public Function asCharacters(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of String)(obj, env)
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
        <RApiReturn(GetType(Boolean))>
        Public Function asLogicals(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of Boolean)(obj, env)
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
            End If

            Dim objType As Type = obj.GetType

            If objType Is GetType(vbObject) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, vbObject).target

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf objType Is GetType(RDispose) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, RDispose).Value

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf objType Is GetType(list) AndAlso type.ImplementInterface(GetType(IDictionary)) Then
                ' cast R# list object to any dictionary table object???
                Return DirectCast(obj, list).CTypeList(type, env)
            ElseIf type.IsEnum Then
                Return CastToEnum(obj, type, env)
            ElseIf objType Is GetType(Environment) AndAlso type Is GetType(GlobalEnvironment) Then
                ' fix the type mismatch bugs for passing value to 
                ' a API parameter which its data type is a global 
                ' environment.
                Return DirectCast(obj, Environment).globalEnvironment
            ElseIf makeObject.isObjectConversion(type, obj) Then
                Return makeObject.createObject(type, obj, env)
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
