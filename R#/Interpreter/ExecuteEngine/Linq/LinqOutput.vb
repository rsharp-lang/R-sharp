#Region "Microsoft.VisualBasic::c83a9e9b4c88b7e53cf38d33ead19fd5, R#\Interpreter\ExecuteEngine\Linq\LinqOutput.vb"

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

    '     Module LinqOutput
    ' 
    '         Function: getOutputSort, populateOutput
    ' 
    '     Class LinqOutputUnit
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Interpreter.ExecuteEngine.LINQ

    Module LinqOutput

        <Extension>
        Public Function getOutputSort(linq As LinqExpression) As (indexBy As Expression, desc As Boolean)
            Dim sort As FunctionInvoke = linq.output.Where(Function(fun) fun.funcName.ToString = "sort").FirstOrDefault

            If sort Is Nothing Then
                Return Nothing
            Else
                Return (sort.parameters(Scan0), DirectCast(sort.parameters(1).Evaluate(Nothing), Boolean))
            End If
        End Function

        <Extension>
        Public Function populateOutput(linq As LinqExpression, result As List(Of LinqOutputUnit)) As Object
            Dim sort = linq.getOutputSort

            If sort.indexBy Is Nothing Then
                Return result.ToDictionary(Function(a) a.key, Function(a) a.value)
            ElseIf sort.desc Then
                Return result.OrderByDescending(Function(a) a.sortKey).ToDictionary(Function(a) a.key, Function(a) a.value)
            Else
                Return result.OrderBy(Function(a) a.sortKey).ToDictionary(Function(a) a.key, Function(a) a.value)
            End If
        End Function
    End Module

    Friend Class LinqOutputUnit

        Public key As String
        Public sortKey As Object
        Public value As Object

        Public Overrides Function ToString() As String
            Return $"[{key}] {Scripting.ToString(value, "null")}"
        End Function

    End Class
End Namespace
