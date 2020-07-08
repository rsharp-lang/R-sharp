#Region "Microsoft.VisualBasic::a3eeff51e47ee12b89b8536a7cdc803e, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\SymbolIndexer\SymbolIndexer.vb"

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
    '         Properties: type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: emptyIndexError, Evaluate, getByIndex, getByName, getColumn
    '                   ToString, vectorSubset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

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
            Dim indexer = REnv.asVector(Of Object)(index.Evaluate(envir))

            If indexer.Length = 0 Then
                Return emptyIndexError(Me, envir)
            ElseIf Program.isException(obj) Then
                Return obj
            ElseIf obj Is Nothing Then
                Return Nothing
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
                Return DirectCast(obj, dataframe).sliceByRow(indexer)
            Else
                Return Internal.debug.stop(New NotImplementedException(indexType.ToString), envir)
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
                    Dim strKey As String = Scripting.ToString(key)

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

            If Not objType.ImplementInterface(GetType(RNameIndex)) Then
                If objType.ImplementInterface(Of IDictionary) AndAlso objType.GenericTypeArguments(Scan0) Is GetType(String) Then
                    Dim keys As String() = REnv.asVector(Of String)(indexer)
                    Dim table As IDictionary = DirectCast(obj, IDictionary)

                    If indexer.Length = 1 Then
                        If table.Contains(keys(Scan0)) Then
                            Return table.Item(keys(Scan0))
                        Else
                            Return Nothing
                        End If
                    Else
                        Return Iterator Function() As IEnumerable(Of Object)
                                   For Each key As String In keys
                                       If table.Contains(key) Then
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
                Return DirectCast(obj, RNameIndex).getByName(Scripting.ToString(indexer.GetValue(Scan0)))
            Else
                Return DirectCast(obj, RNameIndex).getByName(Runtime.asVector(Of String)(indexer))
            End If
        End Function

        ''' <summary>
        ''' vec[names/x] or list[names/index]
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="indexer"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Private Function getByIndex(obj As Object, indexer As Array, envir As Environment) As Object
            If obj.GetType Is GetType(list) Then
                Dim list As list = DirectCast(obj, list)

                If Runtime.isVector(Of String)(indexer) Then
                    ' get by names
                    Return list.getByName(Runtime.asVector(Of String)(indexer))
                Else
                    If TypeOf indexer Is Boolean() OrElse MeasureArrayElementType(indexer) Is GetType(Boolean) Then
                        Dim i As New List(Of Integer)

                        For Each flag As SeqValue(Of Boolean) In DirectCast(asVector(Of Boolean)(indexer), Boolean()).SeqIterator(offset:=1)
                            If flag.value Then
                                i.Add(flag.i)
                            End If
                        Next

                        ' get by index
                        Return list.getByIndex(i.ToArray)
                    Else
                        ' get by index
                        Return list.getByIndex(REnv.asVector(Of Integer)(indexer))
                    End If
                End If
            Else
                Return vectorSubset(obj, indexer, envir)
            End If
        End Function

        Private Function vectorSubset(obj As Object, indexer As Array, env As Environment) As Object
            Dim sequence As Array = REnv.asVector(Of Object)(obj)
            Dim Rarray As RIndex

            If sequence.Length = 0 Then
                Return Nothing
            ElseIf sequence.Length = 1 Then
                Dim tmp = sequence.GetValue(Scan0)
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
                vec = Rarray.getByIndex(Which.IsTrue(REnv.asLogical(indexer), offset:=1))
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
