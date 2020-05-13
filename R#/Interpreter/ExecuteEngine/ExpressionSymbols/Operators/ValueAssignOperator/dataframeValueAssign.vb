#Region "Microsoft.VisualBasic::9653f4aa06b9a6a7b878c2db3859badd, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\ValueAssignOperator\dataframeValueAssign.vb"

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

    '     Module dataframeValueAssign
    ' 
    '         Function: ValueAssign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
                    If value Is Nothing Then
                        ' removes column
                        targetObj.columns.Remove(indexStr(Scan0))
                    Else
                        If Not value.GetType.IsArray Then
                            Dim a As Array = Array.CreateInstance(value.GetType, 1)
                            a.SetValue(value, Scan0)
                            value = a
                        End If

                        targetObj.columns(indexStr(Scan0)) = value
                    End If
                Else
                    Dim seqVal As Array = Runtime.asVector(Of Object)(value)
                    Dim i As i32 = Scan0

                    For Each key As String In indexStr
                        If seqVal.Length = 1 Then
                            targetObj.columns(key) = value
                        Else
                            targetObj.columns(key) = seqVal.GetValue(++i)
                        End If
                    Next
                End If

                Return Nothing
            Else
                Return Internal.debug.stop(New NotImplementedException, env)
            End If
        End Function
    End Module
End Namespace
