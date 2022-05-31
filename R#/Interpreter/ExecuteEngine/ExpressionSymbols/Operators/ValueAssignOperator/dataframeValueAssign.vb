#Region "Microsoft.VisualBasic::a70c4d13bbf55001c3c267e064a7e1b9, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\ValueAssignOperator\dataframeValueAssign.vb"

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

    '   Total Lines: 111
    '    Code Lines: 93
    ' Comment Lines: 2
    '   Blank Lines: 16
    '     File Size: 4.54 KB


    '     Module dataframeValueAssign
    ' 
    '         Function: getArrayFromList, setSingleColumn, ValueAssign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Module dataframeValueAssign

        Public Function ValueAssign(symbolIndex As SymbolIndexer, indexStr As String(), targetObj As dataframe, value As Object, env As Environment) As Message
            If symbolIndex.indexType = SymbolIndexers.dataframeColumns Then
                If indexStr.Length = 1 Then
                    Return setSingleColumn(targetObj, value, indexStr(Scan0), env)
                Else
                    Dim seqVal As Array = Runtime.asVector(Of Object)(value)
                    Dim i As i32 = Scan0
                    Dim err As New Value(Of Message)

                    For Each key As String In indexStr
                        If seqVal.Length = 1 Then
                            If Not err = setSingleColumn(targetObj, value, key, env) Is Nothing Then
                                Return err
                            End If
                        Else
                            If Not err = setSingleColumn(targetObj, seqVal.GetValue(++i), key, env) Is Nothing Then
                                Return err
                            End If
                        End If
                    Next
                End If

                Return Nothing
            Else
                Return Internal.debug.stop(New NotImplementedException, env)
            End If
        End Function

        Private Function setSingleColumn(targetObj As dataframe, value As Object, indexStr$, env As Environment) As Message
            If value Is Nothing Then
                ' removes column
                targetObj.columns.Remove(indexStr)
                Return Nothing
            End If

            Dim valueArray As Array

            If TypeOf value Is vector Then
                valueArray = DirectCast(value, vector).data
            ElseIf TypeOf value Is list Then
                If targetObj.rownames.IsNullOrEmpty Then
                    Return Internal.debug.stop(New InvalidProgramException("unable assign list to a dataframe column without row names as key!"), env)
                Else
                    Dim tmp = DirectCast(value, list).getArrayFromList(targetObj, env)

                    If tmp Like GetType(Message) Then
                        Return tmp.TryCast(Of Message)
                    Else
                        valueArray = tmp.TryCast(Of Array)
                    End If
                End If
            ElseIf Not value.GetType.IsArray Then
                Dim a As Array = Array.CreateInstance(value.GetType, 1)
                a.SetValue(value, Scan0)
                valueArray = a
            Else
                valueArray = value
            End If

            Dim nrows As Integer = targetObj.nrows

            If nrows = 0 Then
                ' is an empty dataframe
                targetObj.columns(indexStr) = valueArray
            ElseIf valueArray.Length = 0 Then
                targetObj.columns.Remove(indexStr)
            ElseIf valueArray.Length <> nrows AndAlso (valueArray.Length <> 1 AndAlso nrows <> 1) Then
                Return Internal.debug.stop({
                    $"the given value is not equal size to the dataframe row numbers!",
                    $"nrows: {nrows}",
                    $"value size: {valueArray.Length}"
                }, env)
            Else
                targetObj.columns(indexStr) = valueArray
            End If

            Return Nothing
        End Function

        <Extension>
        Private Function getArrayFromList(list As list, targetObj As dataframe, env As Environment) As [Variant](Of Array, Message)
            Dim a As Object() = New Object(targetObj.nrows - 1) {}
            Dim i As Integer = Scan0
            Dim setValue As Boolean = False

            For Each rowKey As String In targetObj.rownames
                If list.hasName(rowKey) Then
                    a(i) = list.slots(rowKey)
                    setValue = True
                End If

                i += 1
            Next

            If Not setValue Then
                Return Internal.debug.stop("no row names contains in the given list!", env)
            Else
                Return a
            End If
        End Function
    End Module
End Namespace
