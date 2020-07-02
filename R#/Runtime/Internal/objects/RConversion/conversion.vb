#Region "Microsoft.VisualBasic::c2bb14e04d9ec887a366889a7c9b0265, R#\Runtime\Internal\objects\RConversion\conversion.vb"

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
    '                   asLogicals, asNumeric, asObject, asRaw, asVector
    '                   isCharacter, unlist, unlistOfRList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
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

            Dim listType As Type = list.GetType

            If listType.IsArray Then
                Return list
            ElseIf DataFramework.IsPrimitive(listType) Then
                Return list
            ElseIf listType Is GetType(list) Then
                Return DirectCast(list, list).unlistOfRList([typeof], env)
            ElseIf listType.ImplementInterface(GetType(IDictionary)) Then
                Return New list(list).unlistOfRList([typeof], env)
            Else
                ' Return Internal.debug.stop(New InvalidCastException(list.GetType.FullName), env)
                ' is a single uer defined .NET object 
                Return list
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
                type = makeDataframe.tryTypeLineage(type)

                If type Is Nothing Then
                    Return Internal.debug.stop(New InvalidProgramException("missing api for handle of data: " & type.FullName), env)
                Else
                    Return makeDataframe.createDataframe(type, x, args, env)
                End If
            End If
        End Function

        <ExportAPI("as.vector")>
        Public Function asVector(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
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
        ''' <param name="args">
        ''' for dataframe type:
        ''' 
        ''' ``byRow``: logical, default is FALSE, means cast dataframe to list directly by column hash table values
        ''' </param>
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
            ElseIf obj.GetType.IsArray Then
                Dim type As Type = MeasureRealElementType(obj)

                If type Is GetType(String) Then
                    ' 20200427 try to fix bugs on linux platform 
                    ' 
                    ' Error in <globalEnvironment> -> InitializeEnvironment -> str_pad -> str_pad -> as.integer -> as.integer
                    ' 1. TargetInvocationException: Exception has been thrown by the target of an invocation.
                    ' 2. DllNotFoundException: kernel32
                    ' 3. stackFrames: 
                    ' at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00083] in <47d423fd1d4342b9832b2fe1f5d431eb>:0 
                    ' at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] in <47d423fd1d4342b9832b2fe1f5d431eb>:0 
                    ' at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo.Invoke (System.Object[] parameters, SMRUCC.Rsharp.Runtime.Environment env) [0x00073] in <f2d41b7896b5443b8d9c40b31555c1b7>:0 

                    ' R# source: mapId <- Call str_pad(Call as.integer(Call /\d+/(&mapId)), 5, left, 0)

                    ' RConversion.R#_interop::.as.integer at REnv.dll:line <unknown>
                    ' SMRUCC/R#.call_function.as.integer at renderMap_CLI.R:line 17
                    ' stringr.R#_interop::.str_pad at REnv.dll:line <unknown>
                    ' SMRUCC/R#.call_function.str_pad at renderMap_CLI.R:line 17
                    ' SMRUCC/R#.n/a.InitializeEnvironment at renderMap_CLI.R:line 0
                    ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a
                    Return DirectCast(Runtime.asVector(Of String)(obj), String()) _
                        .Select(AddressOf Long.Parse) _
                        .ToArray
                Else
                    Return Runtime.asVector(Of Long)(obj)
                End If
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

            If obj.GetType Like RType.characters Then
                Return True
            ElseIf obj.GetType.IsArray AndAlso DirectCast(obj, Array).AsObjectEnumerator.All(Function(x) x.GetType Like RType.characters) Then
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

        <ExportAPI("as.raw")>
        Public Function asRaw(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Byte()

        End Function

        ''' <summary>
        ''' running pipeline function in linq pipeline mode
        ''' </summary>
        ''' <param name="seq">any kind of object sequence in R# environment</param>
        ''' <returns></returns>
        <ExportAPI("as.pipeline")>
        Public Function asPipeline(<RRawVectorArgument> seq As Object) As pipeline
            Dim type As RType = Nothing
            Dim sequence = Rset.getObjectSet(seq, elementType:=type)

            Return New pipeline(sequence, type)
        End Function
    End Module
End Namespace
