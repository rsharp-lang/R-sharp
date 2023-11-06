#Region "Microsoft.VisualBasic::ab9cdf0c2901abfaf519b762d5ccabca, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/MemberValueAssign.vb"

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

'   Total Lines: 61
'    Code Lines: 50
' Comment Lines: 0
'   Blank Lines: 11
'     File Size: 2.44 KB


'     Class MemberValueAssign
' 
'         Properties: expressionName, memberReference, type, value
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' a[xxx] = value
    ''' </summary>
    Public Class MemberValueAssign : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return value.type
            End Get
        End Property

        Public ReadOnly Property memberReference As SymbolIndexer
        Public ReadOnly Property value As Expression

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolMemberAssign
            End Get
        End Property

        Sub New(member As SymbolIndexer, value As Expression)
            Me.memberReference = member
            Me.value = value
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = Me.value.Evaluate(envir)

            If Program.isException(value) Then
                Return value
            End If

            Return setByNameIndex(memberReference, envir, value)
        End Function

        Friend Shared Function setByNameIndex(symbolIndex As SymbolIndexer, envir As Environment, value As Object) As Message
            ' evaluate data object and then check for errors
            Dim targetObj As Object = symbolIndex.symbol.Evaluate(envir)
            Dim index As Object = symbolIndex.index.Evaluate(envir)

            If Program.isException(index) Then
                Return index
            ElseIf True = CBool(base.isEmpty(index)) Then
                If Not index Is Nothing Then
                    If TypeOf index Is String OrElse TypeOf index Is String() Then
                        index = CLRVector.asCharacter(index).First
                    Else
                        Return SymbolIndexer.emptyIndexError(symbolIndex, envir)
                    End If
                Else
                    Return SymbolIndexer.emptyIndexError(symbolIndex, envir)
                End If
            End If

            If targetObj Is Nothing Then
                Return Internal.debug.stop({"Target symbol is nothing!", $"SymbolName: {symbolIndex.symbol}"}, envir)
            ElseIf TypeOf targetObj Is Message Then
                Return targetObj
            End If

            If TypeOf index Is vector Then
                index = DirectCast(index, vector).data
            End If

            Dim targetType As Type = targetObj.GetType

            If symbolIndex.indexType = SymbolIndexers.vectorIndex AndAlso index.GetType Like RType.integers Then
                ' integer index
                Return setVectorElements(targetObj, CLRVector.asInteger(index), value, envir)
                ' logical index
            ElseIf symbolIndex.indexType = SymbolIndexers.vectorIndex AndAlso index.GetType Like RType.logicals Then
                Dim flags As Boolean() = CLRVector.asLogical(index)
                Dim indexVals As Integer() = flags _
                    .Select(Function(b, i) (b, i + 1)) _
                    .Where(Function(flag) flag.b) _
                    .Select(Function(flag) flag.Item2) _
                    .ToArray

                Return setVectorElements(targetObj, indexVals, value, envir)
            ElseIf symbolIndex.indexType = SymbolIndexers.dataframeColumns Then
                If targetType IsNot GetType(dataframe) Then
                    Return Internal.debug.stop({
                        $"Error in {symbolIndex.symbol}[, ""{symbolIndex.index}""] = *value: incorrect number of subscripts on matrix",
                        $"typeof_symbol: {targetType.FullName}",
                        $"symbol: {symbolIndex.symbol.ToString}"
                    }, envir)
                End If
            End If

            ' character name index
            Dim indexStr As String() = CLRVector.asCharacter(index)
            Dim result As Object

            If Not targetType.ImplementInterface(GetType(RNameIndex)) Then
                If targetType Is GetType(dataframe) Then
                    Return dataframeValueAssign.ValueAssign(symbolIndex, indexStr, DirectCast(targetObj, dataframe), value, envir)
                ElseIf targetType.ImplementInterface(Of IDataIndex) Then
                    ' 优先单个元素值？
                    If Not value Is Nothing Then
                        If TypeOf value Is vector AndAlso DirectCast(value, vector).length = 1 Then
                            value = DirectCast(value, vector).getByIndex(1)
                        ElseIf value.GetType.IsArray AndAlso DirectCast(value, Array).Length = 1 Then
                            value = DirectCast(value, Array).GetValue(Scan0)
                        End If
                    End If

                    Call DirectCast(targetObj, IDataIndex).SetByIndex(indexStr(Scan0), value)

                    If indexStr.Length > 1 Then
                        Call envir.AddMessage($"the index name is more then one element!", MSG_TYPES.WRN)
                    End If

                    Return Nothing
                Else
                    Return Internal.debug.stop({
                        $"Target symbol can not be indexed by name!",
                        $"SymbolName: {symbolIndex.symbol}",
                        $"type: {targetObj.GetType.FullName}"
                    }, envir)
                End If
            End If

            If indexStr.IsNullOrEmpty Then
                Return SymbolIndexer.emptyIndexError(symbolIndex, envir)
            End If

            If symbolIndex.indexType = SymbolIndexers.nameIndex Then
                ' a[[x]] <- v
                ' a$x <- v
                result = DirectCast(targetObj, RNameIndex).setByName(indexStr(Scan0), value, envir)

                If indexStr.Length > 1 Then
                    envir.AddMessage($"'{symbolIndex.index}' contains multiple index, only use first index key value...")
                End If
            Else
                ' set null to a specific list slot element
                If value Is Nothing OrElse value.GetType.IsArray Then
                    result = DirectCast(targetObj, RNameIndex).setByName(
                        names:=indexStr,
                        value:=DirectCast(value, Array),
                        envir:=envir
                    )
                Else
                    Return Internal.debug.stop({
                        $"invalid data value source! it should be a vector type!",
                        $"given: {value.GetType.FullName}",
                        $"symbol indexer: {symbolIndex.indexType.Description}"
                    }, envir)
                End If
            End If

            If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
                Return DirectCast(result, Message)
            Else
                Return Nothing
            End If
        End Function

        Private Shared Function setVectorElements(ByRef target As Object, index As Integer(), value As Object, env As Environment) As Message
            If index.IsNullOrEmpty Then
                ' vector content no changed
                Return Nothing
            End If

            If target.GetType Is GetType(vector) Then
                target = DirectCast(target, vector).data
            End If

            Dim targetVector As Array = DirectCast(target, Array)
            Dim getValue As Func(Of Object)

            If value Is Nothing Then
                getValue = Function() Nothing
            Else
                Dim valueVec As Array = renv.asVector(Of Object)(value)
                Dim i As i32 = Scan0

                If valueVec.Length = 1 Then
                    value = valueVec.GetValue(Scan0)
                    getValue = Function() value
                Else

                    getValue = Function() valueVec.GetValue(++i)
                End If
            End If

            Dim elementType As Type = MeasureArrayElementType(targetVector)
            Dim val As Object

            For Each i As Integer In DirectCast(index, Integer())
                val = getValue()

                If elementType IsNot GetType(Object) Then
                    ' generic array should do type cast for the
                    ' value
                    val = RCType.CTypeDynamic(val, elementType, env)
                End If

                If Program.isException(val) Then
                    Return val
                End If

                ' 动态调整数组的大小
                If targetVector.Length >= i Then
                    targetVector.SetValue(val, i - 1)
                Else
                    Dim newVec As Array = Array.CreateInstance(elementType, i - 1)

                    Array.ConstrainedCopy(targetVector, Scan0, newVec, Scan0, targetVector.Length)
                    newVec.SetValue(val, i - 1)
                    targetVector = newVec
                End If
            Next

            If target.GetType Is GetType(vector) Then
                DirectCast(target, vector).data = targetVector
            Else
                target = targetVector
            End If

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return $"{memberReference} <- {value}"
        End Function
    End Class
End Namespace
