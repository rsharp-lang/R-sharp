#Region "Microsoft.VisualBasic::d76e3ac5dc289c0aa61a533c683af2b2, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/SymbolIndexer/SymbolIndexer.vb"

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

    '   Total Lines: 686
    '    Code Lines: 518
    ' Comment Lines: 84
    '   Blank Lines: 84
    '     File Size: 30.04 KB


    '     Class SymbolIndexer
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: doListSubset, emptyIndexError, Evaluate, getByIndex, getByName
    '                   getBySymbolIndex, getColumn, getDataframeRowRange, groupSubset, listSubset
    '                   streamView, ToString, translateInteger2keys, translateLogical2keys, vectorSubset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime
Imports std = System.Math

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' get/set elements by index 
    ''' (X$name或者X[[name]])
    ''' </summary>
    Public Class SymbolIndexer : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If TypeOf symbol Is VectorLiteral Then
                    Return symbol.type
                Else
                    Return TypeCodes.generic
                End If
            End Get
        End Property

        Friend ReadOnly index As Expression
        Friend symbol As Expression

        ''' <summary>
        ''' X[[name]]
        ''' </summary>
        Friend ReadOnly indexType As SymbolIndexers

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolIndex
            End Get
        End Property

        Sub New(symbol As Expression, index As Expression, indexType As SymbolIndexers)
            Me.symbol = symbol
            Me.index = index
            Me.indexType = indexType
        End Sub

        ''' <summary>
        ''' symbol$byName
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="byName"></param>
        Sub New(symbol As Expression, byName As Expression)
            Me.symbol = symbol
            Me.index = byName
            Me.indexType = SymbolIndexers.nameIndex
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim obj As Object = symbol.Evaluate(envir)

            If Program.isException(obj) Then
                Return obj
            ElseIf obj Is Nothing Then
                Return Nothing
            End If

            If indexType = SymbolIndexers.dataframeRanges Then
                If Not TypeOf obj Is dataframe Then
                    Return Internal.debug.stop(Message.InCompatibleType(GetType(dataframe), obj.GetType, envir), envir)
                Else
                    Return getDataframeRowRange(data:=DirectCast(obj, dataframe), envir)
                End If
            Else
                Return getBySymbolIndex(obj, envir)
            End If
        End Function

        Private Function getBySymbolIndex(obj As Object, envir As Environment) As Object
            Dim indexerRaw As Object = index.Evaluate(envir)
            Dim indexer As Array
            Dim objType As Type = obj.GetType

            If Program.isException(indexerRaw) Then
                Return indexerRaw
            ElseIf objType.ImplementInterface(Of RIndexer) Then
                If TypeOf indexerRaw Is Expression Then
                    Return DirectCast(obj, RIndexer).EvaluateIndexer(indexerRaw, env:=envir)
                ElseIf TypeOf indexerRaw Is String Then
                    Return DirectCast(obj, RIndexer).EvaluateIndexer(New RuntimeValueLiteral(indexerRaw), env:=envir)
                End If
            End If

#Disable Warning
            indexer = REnv.TryCastGenericArray(REnv.asVector(Of Object)(indexerRaw), env:=envir)
#Enable Warning

            If indexer.Length = 0 Then
                If (indexType = SymbolIndexers.nameIndex OrElse indexType = SymbolIndexers.dataframeRanges) Then
                    Return emptyIndexError(Me, envir)
                Else
                    Return Nothing
                End If
            End If

            ' now obj is always have values:
            If indexType = SymbolIndexers.nameIndex Then
                ' get element value from list by name index syntax
                ' example like:
                '
                '     l[[1]]
                '
                If RType.GetRSharpType(indexer.GetType).mode = TypeCodes.integer AndAlso TypeOf obj IsNot list Then
                    ' a[xxx]
                    Return getByIndex(obj, indexer, envir)
                Else
                    ' a[[name]]
                    ' a$name
                    Return getByName(obj, indexer, envir)
                End If
            ElseIf indexType = SymbolIndexers.vectorIndex Then
                ' a[name]
                ' a[index]
                Return getByIndex(obj, indexer, envir)
            ElseIf indexType = SymbolIndexers.dataframeColumns Then
                If Not TypeOf obj Is dataframe Then
                    Return Message.InCompatibleType(GetType(dataframe), obj.GetType, envir)
                Else
                    Return getColumn(obj, indexer, envir)
                End If
            ElseIf indexType = SymbolIndexers.dataframeRows Then
                If Not TypeOf obj Is dataframe Then
                    Return Message.InCompatibleType(GetType(dataframe), obj.GetType, envir)
                Else
                    Return DirectCast(obj, dataframe) _
                        .sliceByRow(indexer, envir) _
                        .Value
                End If
            Else
                Return Internal.debug.stop(New NotImplementedException(indexType.ToString), envir)
            End If
        End Function

        Private Function getDataframeRowRange(data As dataframe, env As Environment) As Object
            Dim indexVec As VectorLiteral = index

            If indexVec.length = 2 Then
                ' [row, column]
                ' [row, , drop = TRUE]
                If TypeOf indexVec.values(1) Is ValueAssignExpression Then
                    Dim opt As ValueAssignExpression = indexVec(1)

                    If TypeOf opt.targetSymbols(Scan0) Is Literal AndAlso DirectCast(opt.targetSymbols(Scan0), Literal) = "drop" Then
                        Dim drop As Boolean = CLRVector.asLogical(opt.value.Evaluate(env))(Scan0)
                        Dim rowIndex = data.getRowIndex(indexVec.values(Scan0).Evaluate(env))
                        Dim result = data.getRowList(rowIndex, drop:=drop)

                        If TypeOf result Is Dictionary(Of String, Object) Then
                            result = New list With {.slots = result}
                        End If

                        Return result
                    Else
                        Return Internal.debug.stop("invalid options for slice dataframe", env)
                    End If
                Else
                    Dim x = REnv.asVector(Of Object)(indexVec.values(Scan0).Evaluate(env))
                    Dim y = REnv.asVector(Of Object)(indexVec.values(1).Evaluate(env))
                    Dim result = data.sliceByRow(selector:=x, env)

                    If result Like GetType(Message) Then
                        Return result.TryCast(Of Message)
                    Else
                        Return result.TryCast(Of dataframe).projectByColumn(y, env)
                    End If
                End If
            ElseIf indexVec.length = 3 Then
                ' [row, column, drop = TRUE]
                Return Internal.debug.stop(New NotImplementedException, env)
            Else
                Return Internal.debug.stop("invalid expression for subset a dataframe!", env)
            End If
        End Function

        Private Function getColumn(obj As dataframe, indexer As Array, envir As Environment) As Object
            If indexer.Length = 0 Then
                Return Nothing
            ElseIf indexer.Length = 1 Then
                Dim key As Object = indexer.GetValue(Scan0)

                If key Is Nothing Then
                    Return Internal.debug.stop("dataframe index could not be nothing!", envir)
                ElseIf key.GetType Like RType.integers Then
                    Dim strKey As String = obj.getKeyByIndex(CInt(key))

                    If strKey Is Nothing Then
                        Return Internal.debug.stop({"index outside the bounds!", "index: " & key}, envir)
                    Else
                        Return obj.getColumnVector(strKey)
                    End If
                Else
                    Dim strKey As String = any.ToString(key)

                    If obj.columns.ContainsKey(strKey) Then
                        Return obj.getColumnVector(strKey)
                    Else
                        Return Internal.debug.stop({"undefined columns selected", "column key: " & strKey}, envir)
                    End If
                End If
            Else
                ' dataframe projection
                Return obj.projectByColumn(indexer, envir)
            End If
        End Function

        ''' <summary>
        ''' generate the empty index error message for ``R#``
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function emptyIndexError(symbol As SymbolIndexer, env As Environment) As Message
            Return Internal.debug.stop({
                $"attempt to select less than one element in OneIndex!",
                $"SymbolName: {symbol.symbol}",
                $"Index: {symbol.index}"
            }, env)
        End Function

        Private Function getByName(obj As Object, indexer As Array, envir As Environment) As Object
            Dim objType As Type = obj.GetType

            If objType.IsArray AndAlso DirectCast(obj, Array).Length = 1 Then
                obj = DirectCast(obj, Array).GetValue(Scan0)
            End If

            If objType Is GetType(dataframe) Then
                If indexer.Length = 1 Then
                    Return DirectCast(obj, dataframe).getColumnVector(indexer.GetValue(Scan0).ToString)
                Else
                    Return Internal.debug.stop({
                       $"dataframe get by column name only supprts one key element!",
                       $"symbol: {Me.symbol.ToString}",
                       $"indexer: {Me.index.ToString}"
                    }, envir)
                End If
            ElseIf objType Is GetType(vector) Then
                If indexer.Length = 1 Then
                    Return DirectCast(obj, vector).getByName(any.ToString(indexer.GetValue(Scan0)))
                Else
                    Return Internal.debug.stop("attempt to select more than one element in vectorIndex", envir)
                End If
            ElseIf Not objType.ImplementInterface(GetType(RNameIndex)) Then
                If objType.ImplementInterface(Of IDictionary) AndAlso objType.GenericTypeArguments(Scan0) Is GetType(String) Then
                    Dim keys As String() = CLRVector.asCharacter(indexer)
                    Dim table As IDictionary = DirectCast(obj, IDictionary)

                    If indexer.Length = 1 Then
                        If keys(Scan0) IsNot Nothing AndAlso table.Contains(keys(Scan0)) Then
                            Return table.Item(keys(Scan0))
                        Else
                            Return Nothing
                        End If
                    Else
                        Return Iterator Function() As IEnumerable(Of Object)
                                   For Each key As String In keys
                                       If key IsNot Nothing AndAlso table.Contains(key) Then
                                           Yield table.Item(key)
                                       Else
                                           Yield Nothing
                                       End If
                                   Next
                               End Function().ToArray
                    End If
                Else
                    Dim readDefault As PropertyInfo = objType _
                        .GetProperties(PublicProperty) _
                        .Where(Function(p)
                                   Return p.CanRead AndAlso
                                          p.Name = "Item" AndAlso
                                      Not p.GetIndexParameters.IsNullOrEmpty AndAlso
                                          p.GetIndexParameters.Length = 1 AndAlso
                                          p.GetIndexParameters(Scan0).ParameterType Is GetType(String)
                               End Function) _
                        .FirstOrDefault

                    If Not readDefault Is Nothing Then
                        Dim keys As String() = CLRVector.asCharacter(indexer)

                        If indexer.Length = 1 Then
                            Return readDefault.GetValue(obj, {keys(Scan0)})
                        Else
                            Return Iterator Function() As IEnumerable(Of Object)
                                       For Each key As String In keys
                                           Yield readDefault.GetValue(obj, {key})
                                       Next
                                   End Function().ToArray
                        End If
                    End If

                    Return Internal.debug.stop({
                         "Target object can not be indexed by name!",
                         "required: " & GetType(RNameIndex).FullName,
                         "given: " & objType.FullName,
                         "symbol: " & Me.symbol.ToString,
                         "indexer: " & Me.index.ToString
                    }, envir)
                End If
            ElseIf indexer.Length = 1 Then
                Dim i As Object = indexer.GetValue(Scan0)

                If RType.TypeOf(i) Like RType.integers Then
                    Dim names As String() = DirectCast(obj, RNameIndex).getNames
                    Dim offset As Integer = CInt(i) - 1

                    If offset > names.Length - 1 Then
                        Return Internal.debug.stop({
                            $"Error in list[[{i}]] : subscript out of bounds",
                            $"list size: {names.Length}",
                            $"offset: {i}",
                            $"symbol_list: {symbol.ToString}",
                            $"indexer: {Me.index.ToString}"
                        }, envir)
                    End If

                    Return DirectCast(obj, RNameIndex).getByName(names(offset))
                Else
                    Return DirectCast(obj, RNameIndex).getByName(any.ToString(i))
                End If
            Else
                Return DirectCast(obj, RNameIndex).getByName(CLRVector.asCharacter(indexer))
            End If
        End Function

        ''' <summary>
        ''' vec[names/x] or list[names/index]
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="indexer"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Shared Function getByIndex(obj As Object, indexer As Array, envir As Environment) As Object
            If obj.GetType Is GetType(list) Then
                obj = DirectCast(obj, list).slots
            End If

            If obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return listSubset(DirectCast(obj, IDictionary), indexer, env:=envir)
                ' ElseIf obj.GetType.ImplementInterface(GetType(IReadOnlyDictionary)) Then
            Else
                Return vectorSubset(obj, indexer, envir)
            End If
        End Function

        Private Shared Function listSubset(list As IDictionary, indexer As Array, env As Environment) As Object
            Dim allKeys = (From x In list.Keys).ToArray
            Dim subsetKeys As Array

            If REnv.isVector(Of String)(indexer) Then
                ' get by names
                subsetKeys = CLRVector.asCharacter(indexer)
            Else
                If indexer.Length >= list.Count Then
                    env.AddMessage($"the index size({indexer.Length}) is greater than the size({list.Count}) of the target list!", MSG_TYPES.WRN)
                End If

                ' translate the bool vector or integer vector 
                ' to the subset of list all keys character vector
                If TypeOf indexer Is Boolean() OrElse MeasureArrayElementType(indexer) Is GetType(Boolean) Then
                    subsetKeys = translateLogical2keys(indexer, allKeys)
                Else
                    Try
                        subsetKeys = translateInteger2keys(indexer, allKeys)
                    Catch ex As Exception
                        Return Internal.debug.stop(ex, env)
                    End Try
                End If
            End If

            Return doListSubset(list, subsetKeys)
        End Function

        Private Shared Function translateInteger2keys(indexer As Array, allkeys As Object()) As Array
            Dim i As New List(Of Object)

            ' get by index
            ' the string index key is already handling
            ' at the code above
            ' so make the index to integer at here
            For Each flag As Integer In CLRVector.asInteger(indexer)
                If flag >= allkeys.Length Then
                    Call i.Add(Nothing)
                Else
                    Call i.Add(allkeys(flag - 1))
                End If
            Next

            Return i.ToArray
        End Function

        Private Shared Function translateLogical2keys(indexer As Array, allkeys As Object()) As Array
            Dim i As New List(Of Object)

            For Each flag As SeqValue(Of Boolean) In CLRVector.asLogical(indexer).SeqIterator
                If flag.value Then
                    ' get by index
                    If flag.i >= allkeys.Length Then
                        Call i.Add(Nothing)
                    Else
                        Call i.Add(allkeys(flag.i))
                    End If
                End If
            Next

            Return i.ToArray
        End Function

        Private Shared Function doListSubset(list As IDictionary, names As Array) As Object
            Dim subset As New list() With {
                .slots = New Dictionary(Of String, Object)
            }

            ' will skip of the key name with value NULL
            '
            For Each id As Object In From key As Object
                                     In names.AsObjectEnumerator
                                     Where Not key Is Nothing

                If (id Is Nothing) OrElse list.Contains(key:=id) Then
                    subset.slots(id) = list(id)
                Else
                    subset.slots(id) = Nothing
                End If
            Next

            Return subset
        End Function

        Private Shared Function streamView(read As Stream, offset As Long()) As Byte()
            If offset _
                .SlideWindows(2) _
                .Where(Function(d) d.Length = 2) _
                .All(Function(d)
                         Return std.Abs(d.Last - d.First) = 1
                     End Function) Then

                Dim buffer As Byte()

                ' is a asc/desc range
                If offset(1) - offset(0) > 0 Then
                    buffer = New Byte(offset.Last - offset.First) {}

                    ' asc
                    read.Seek(offset(0) - 1, SeekOrigin.Begin)
                    read.Read(buffer, Scan0, buffer.Length)
                Else
                    ' desc
                    Throw New NotImplementedException
                End If

                Return buffer
            Else
                Dim buffer As New List(Of Byte)

                For Each i As Long In offset
                    Call read.Seek(i - 1, SeekOrigin.Begin)
                    Call buffer.Add(CByte(read.ReadByte))
                Next

                Return buffer.ToArray
            End If
        End Function

        Private Shared Function groupSubset(group As Group, indexer As Array, genericIndex As Object)
            If TypeOf genericIndex Is Boolean() Then
                Dim idx As Integer() = DirectCast(genericIndex, Boolean()) _
                    .Select(Function(f, i) (f, i)) _
                    .Where(Function(t) t.f = True) _
                    .Select(Function(t) t.i) _
                    .ToArray

                If indexer.Length = 1 Then
                    Return group(idx(Scan0))
                Else
                    Return idx _
                        .Select(Function(i) group(i)) _
                        .ToArray
                End If
            Else
                If indexer.Length = 1 Then
                    Return group(CLRVector.asInteger(indexer)(Scan0) - 1)
                Else
                    Return CLRVector.asInteger(indexer) _
                        .Select(Function(i) group(i - 1)) _
                        .ToArray
                End If
            End If
        End Function

        Private Shared Function vectorSubset(obj As Object, indexer As Array, env As Environment) As Object
            If TypeOf obj Is Group Then
                Dim group = DirectCast(obj, Group)
                Dim genericIndex = REnv.TryCastGenericArray(indexer, env)

                Return groupSubset(group, indexer, genericIndex)
            ElseIf TypeOf obj Is Stream Then
                Dim read As Stream = DirectCast(obj, Stream)
                Dim offset As Long() = CLRVector.asLong(indexer)

                Return streamView(read, offset)
            End If

            Dim sequence As Array
            Dim Rarray As RIndex

            If obj.GetType.IsArray Then
                sequence = obj
            ElseIf TypeOf obj Is vector Then
                sequence = DirectCast(obj, vector).data
            ElseIf TypeOf obj Is RMethodInfo OrElse TypeOf obj Is DeclareLambdaFunction OrElse TypeOf obj Is DeclareNewFunction Then
                Return Internal.debug.stop({$"object of type 'closure' is not subsettable", $"closure: {obj}"}, env)
            Else
                Dim type As Type = obj.GetType
                Dim item As PropertyInfo = type.GetProperties _
                    .Where(Function(p)
                               Return p.Name = "Item" AndAlso
                                      p.CanRead AndAlso
                                      p.GetIndexParameters.Count = 1
                           End Function) _
                    .FirstOrDefault

                If Not item Is Nothing Then
                    Dim argType As Type = item.GetIndexParameters()(Scan0).ParameterType
                    Dim index As Object = RCType.CTypeDynamic(indexer, argType, env)

                    Return item.GetValue(obj, {index})
                Else
                    ' 20210526 为了避免类型转换带来的性能损耗
                    ' 在这里需要手动判断数组或者向量
                    ' 最后再执行这个函数来转换数组
                    sequence = REnv.asVector(Of Object)(obj)
                End If
            End If

            If sequence.Length = 0 Then
                Return Nothing
            ElseIf sequence.Length = 1 Then
                Dim tmp As Object = sequence.GetValue(Scan0)

                If indexer.Length = 1 Then
                    Dim idx As Object = indexer.GetValue(Scan0)

                    If idx Is Nothing Then
                        idx = False
                    End If

                    If TypeOf idx Is Integer OrElse TypeOf idx Is Long Then
                        If CInt(idx) > 1 Then
                            Return Nothing
                        ElseIf TypeOf tmp Is list Then
                            Return tmp
                        End If
                    ElseIf TypeOf idx Is Boolean Then
                        If CBool(idx) Then
                            Return tmp
                        Else
                            Return Nothing
                        End If
                    ElseIf TypeOf idx Is String Then
                        If TypeOf tmp Is list Then
                            Return DirectCast(tmp, list).getByName(CStr(idx))
                        ElseIf TypeOf tmp Is dataframe Then
                            ' do field projection
                            '
                            ' a = data.frame(x = 1:5, d = 2:6)
                            ' > a["x"]
                            '   x
                            ' 1 1
                            ' 2 2
                            ' 3 3
                            ' 4 4
                            ' 5 5
                            '
                            Dim v As Array = DirectCast(tmp, dataframe)(CStr(idx))
                            Dim newDf As New dataframe With {
                                .columns = New Dictionary(Of String, Array) From {
                                    {idx, v}
                                },
                                .rownames = DirectCast(tmp, dataframe).rownames
                            }

                            Return newDf
                        Else
                            Return Internal.debug.stop(New NotImplementedException, env)
                        End If
                    Else
                        Return Internal.debug.stop(New NotImplementedException(idx.GetType.FullName), env)
                    End If
                End If

                Dim type As RType = RType.GetRSharpType(tmp.GetType)

                If type.raw.ImplementInterface(GetType(RIndex)) Then
                    Rarray = tmp

                    '' by element index
                    'If Not sequence.GetType.ImplementInterface(GetType(RIndex)) Then
                    '    Return Internal.debug.stop("Target object can not be indexed!", envir)
                    'End If
                Else
                    Dim count As Integer
                    Dim item As Func(Of Integer, Object)

                    If Not (type.getCount Is Nothing OrElse type.getItem Is Nothing) Then
                        count = type.getCount.GetValue(tmp)
                        item = Function(i)
                                   If i > count Then
                                       Return Nothing
                                   ElseIf i < 0 Then
                                       i = count + i
                                   End If

                                   Return type.getItem.GetValue(tmp, {i - 1})
                               End Function

                        Return CLRVector.asInteger(indexer) _
                            .Select(item) _
                            .ToArray
                    Else
                        Rarray = New vector With {.data = sequence}
                    End If
                End If
            Else
                Rarray = New vector With {.data = sequence}
            End If

            ' 20200429 但indexer的长度为1个元素，并且类型为boolean逻辑值的时候
            ' 假设If indexer.Length = 1 Then分支在前面，则indexer逻辑值会被强制转换为integer
            ' 可能会产生错误：
            '
            ' System.IndexOutOfRangeException: Index has to be between upper and lower bound of the array.
            '
            ' at System.Array.GetValue(System.Int32 index) [0x0002a] in <d50a1f2f14b642d2b936cb144b307343>:0
            ' at SMRUCC.Rsharp.Runtime.Internal.Object.vector.getByIndex(System.Int32 i) [0x0003e] in <e6d99a935c2a4c6a89f70a13664e6034>:0
            '
            Dim vec As Array

            If REnv.isVector(Of Boolean)(indexer) Then
                vec = Rarray.getByIndex(which.IsTrue(CLRVector.asLogical(indexer), offset:=1))
            ElseIf indexer.Length = 1 Then
                Return Rarray.getByIndex(CInt(indexer.GetValue(Scan0)))
            Else
                vec = Rarray.getByIndex(CLRVector.asInteger(indexer))
            End If

            If vec.Length = 0 Then
                Return New vector With {.data = {}}
            Else
                ' 20200617 但vec是空集合的时候
                ' 得到的type是void类型
                ' 会报错：无法创建一个void类型的数组
                Return New vector(REnv.MeasureRealElementType(vec), vec, env)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{symbol}[{index}]"
        End Function
    End Class
End Namespace
