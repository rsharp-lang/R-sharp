#Region "Microsoft.VisualBasic::818662c4e03f580e4bedf35a8715e045, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\SymbolIndexer\SymbolIndexer.vb"

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

'     Class SymbolIndexer
' 
'         Properties: expressionName, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: doListSubset, emptyIndexError, Evaluate, getByIndex, getByName
'                   getColumn, getDataframeRowRange, listSubset, ToString, vectorSubset
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
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
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime
Imports stdNum = System.Math

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' get/set elements by index 
    ''' (X$name或者X[[name]])
    ''' </summary>
    Public Class SymbolIndexer : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

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
                Dim indexerRaw As Object = index.Evaluate(envir)
                Dim indexer As Array

                If Program.isException(indexerRaw) Then
                    Return indexerRaw
                ElseIf obj.GetType.ImplementInterface(Of RIndexer) AndAlso TypeOf indexerRaw Is Expression Then
                    Return DirectCast(obj, RIndexer).EvaluateIndexer(indexerRaw, env:=envir)
                Else
                    indexer = REnv.asVector(Of Object)(indexerRaw)
                End If

                If indexer.Length = 0 Then
                    If (indexType = SymbolIndexers.nameIndex OrElse indexType = SymbolIndexers.dataframeRanges) Then
                        Return emptyIndexError(Me, envir)
                    Else
                        Return Nothing
                    End If
                End If

                ' now obj is always have values:
                If indexType = SymbolIndexers.nameIndex Then
                    ' a[[name]]
                    ' a$name
                    Return getByName(obj, indexer, envir)
                ElseIf indexType = SymbolIndexers.vectorIndex Then
                    ' a[name]
                    ' a[index]
                    Return getByIndex(obj, indexer, envir)
                ElseIf indexType = SymbolIndexers.dataframeColumns Then
                    Return getColumn(obj, indexer, envir)
                ElseIf indexType = SymbolIndexers.dataframeRows Then
                    Return DirectCast(obj, dataframe).sliceByRow(indexer, envir).Value
                Else
                    Return Internal.debug.stop(New NotImplementedException(indexType.ToString), envir)
                End If
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
                        Dim drop As Boolean = asLogical(opt.value.Evaluate(env))(Scan0)
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
                    Dim x = indexVec.values(Scan0).Evaluate(env)
                    Dim y = asVector(Of Object)(indexVec.values(1).Evaluate(env))
                    Dim result = data.sliceByRow(x, env)

                    If result Like GetType(Message) Then
                        Return result.TryCast(Of Message)
                    Else
                        Return result.TryCast(Of dataframe).projectByColumn(y)
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
                Return obj.projectByColumn(indexer)
            End If
        End Function

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
                    Return Internal.debug.stop("dataframe get by column name only supprts one key element!", envir)
                End If
            ElseIf objType Is GetType(vector) Then
                If indexer.Length = 1 Then
                    Return DirectCast(obj, vector).getByName(any.ToString(indexer.GetValue(Scan0)))
                Else
                    Return Internal.debug.stop("attempt to select more than one element in vectorIndex", envir)
                End If
            ElseIf Not objType.ImplementInterface(GetType(RNameIndex)) Then
                If objType.ImplementInterface(Of IDictionary) AndAlso objType.GenericTypeArguments(Scan0) Is GetType(String) Then
                    Dim keys As String() = REnv.asVector(Of String)(indexer)
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
                        Dim keys As String() = asVector(Of String)(indexer)

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
                         "given: " & objType.FullName
                    }, envir)
                End If
            ElseIf indexer.Length = 1 Then
                Dim i As Object = indexer.GetValue(Scan0)

                If RType.TypeOf(i) Like RType.integers Then
                    Dim names As String() = DirectCast(obj, RNameIndex).getNames

                    Return DirectCast(obj, RNameIndex).getByName(names(CInt(i) - 1))
                Else
                    Return DirectCast(obj, RNameIndex).getByName(any.ToString(i))
                End If
            Else
                Return DirectCast(obj, RNameIndex).getByName(REnv.asVector(Of String)(indexer))
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
                Return listSubset(DirectCast(obj, IDictionary), indexer)
            Else
                Return vectorSubset(obj, indexer, envir)
            End If
        End Function

        Private Shared Function listSubset(list As IDictionary, indexer As Array) As Object
            Dim allKeys = (From x In list.Keys).ToArray

            If REnv.isVector(Of String)(indexer) Then
                ' get by names
                Return doListSubset(list, names:=REnv.asVector(Of String)(indexer))
            Else
                Dim i As New List(Of Object)

                If TypeOf indexer Is Boolean() OrElse MeasureArrayElementType(indexer) Is GetType(Boolean) Then
                    For Each flag As SeqValue(Of Boolean) In DirectCast(asVector(Of Boolean)(indexer), Boolean()).SeqIterator
                        If flag.value Then
                            ' get by index
                            Call i.Add(allKeys(flag.i))
                        End If
                    Next
                Else
                    ' get by index
                    For Each flag As Integer In DirectCast(REnv.asVector(Of Integer)(indexer), Integer())
                        Call i.Add(allKeys(flag - 1))
                    Next
                End If

                Return doListSubset(list, i.ToArray)
            End If
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

                If list.Contains(key:=id) Then
                    subset.slots(id) = list(id)
                Else
                    subset.slots(id) = Nothing
                End If
            Next

            Return subset
        End Function

        Private Shared Function vectorSubset(obj As Object, indexer As Array, env As Environment) As Object
            If TypeOf obj Is Group Then
                Dim group = DirectCast(obj, Group)

                If indexer.Length = 1 Then
                    Return group(DirectCast(asVector(Of Integer)(indexer), Integer())(Scan0) - 1)
                Else
                    Return DirectCast(asVector(Of Integer)(indexer), Integer()) _
                        .Select(Function(i) group(i - 1)) _
                        .ToArray
                End If
            ElseIf TypeOf obj Is Stream Then
                Dim read As Stream = DirectCast(obj, Stream)
                Dim offset As Long() = DirectCast(REnv.asVector(Of Long)(indexer), Long())

                If offset _
                    .SlideWindows(2) _
                    .Where(Function(d) d.Length = 2) _
                    .All(Function(d)
                             Return stdNum.Abs(d.Last - d.First) = 1
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
                ' 20210526 为了避免类型转换带来的性能损耗
                ' 在这里需要手动判断数组或者向量
                ' 最后再执行这个函数来转换数组
                sequence = REnv.asVector(Of Object)(obj)
            End If

            If sequence.Length = 0 Then
                Return Nothing
            ElseIf sequence.Length = 1 Then
                Dim tmp As Object = sequence.GetValue(Scan0)

                If indexer.Length = 1 Then
                    Dim i As Integer = CInt(indexer.GetValue(Scan0))

                    If i > 1 Then
                        Return Nothing
                    ElseIf TypeOf tmp Is list Then
                        Return tmp
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

                        Return DirectCast(asVector(Of Integer)(indexer), Integer()) _
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
                vec = Rarray.getByIndex(which.IsTrue(REnv.asLogical(indexer), offset:=1))
            ElseIf indexer.Length = 1 Then
                Return Rarray.getByIndex(CInt(indexer.GetValue(Scan0)))
            Else
                vec = Rarray.getByIndex(REnv.asVector(Of Integer)(indexer))
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
