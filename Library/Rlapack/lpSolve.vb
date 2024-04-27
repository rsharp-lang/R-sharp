#Region "Microsoft.VisualBasic::ee6474c66a5779db2a8388deedbca18f, G:/GCModeller/src/R-sharp/Library/Rlapack//lpSolve.vb"

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

    '   Total Lines: 208
    '    Code Lines: 172
    ' Comment Lines: 11
    '   Blank Lines: 25
    '     File Size: 8.98 KB


    ' Module lpSolve
    ' 
    '     Function: alignVector, GetSymbols, GetVector, isSimple, linprog
    '               lp, lpMax, lpMin
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' Solve Linear/Integer Programs 
''' </summary>
<Package("lpSolve")>
Module lpSolve

    <ExportAPI("lp.min")>
    Public Function lpMin(objective As Expression, subjective As Expression(), Optional env As Environment = Nothing) As Object
        Return lp(objective, subjective, OptimizationType.MIN, env)
    End Function

    <ExportAPI("lp.max")>
    Public Function lpMax(objective As Expression, subjective As Expression(), Optional env As Environment = Nothing) As Object
        Return lp(objective, subjective, OptimizationType.MAX, env)
    End Function

    ''' <summary>
    ''' Linear and Integer Programming
    ''' </summary>
    ''' <param name="direction">Character string giving direction of optimization: "min" (default) or "max."</param>
    ''' <param name="objective"></param>
    ''' <param name="subjective"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("lp")>
    Public Function lp(objective As Expression,
                       subjective As Expression(),
                       Optional direction As OptimizationType = OptimizationType.MIN,
                       Optional env As Environment = Nothing) As Object

        If subjective.Length = 1 AndAlso TypeOf subjective(Scan0) Is VectorLiteral Then
            subjective = DirectCast(subjective(Scan0), VectorLiteral).ToArray
        End If

        Dim allSymbols As Index(Of String) = objective.GetSymbols.Indexing
        Dim obj As Double() = objective _
            .GetVector(allSymbols, env) _
            .alignVector(allSymbols)
        Dim sbjMatrix As Double()() = subjective _
            .Select(Function(exp)
                        Dim target As Expression

                        If TypeOf exp Is ValueAssignExpression Then
                            target = DirectCast(exp, ValueAssignExpression).targetSymbols(Scan0)
                        Else
                            target = DirectCast(exp, BinaryExpression).left
                        End If

                        Return target _
                            .GetVector(allSymbols, env) _
                            .alignVector(allSymbols)
                    End Function) _
            .ToArray
        Dim types As String() = subjective _
            .Select(Function(exp)
                        If TypeOf exp Is ValueAssignExpression Then
                            Return "="
                        Else
                            Return DirectCast(exp, BinaryExpression).operator
                        End If
                    End Function) _
            .ToArray
        Dim rightHands As Double() = subjective _
            .Select(Function(exp)
                        Dim value As Expression

                        If TypeOf exp Is ValueAssignExpression Then
                            value = DirectCast(exp, ValueAssignExpression).value
                        Else
                            value = DirectCast(exp, BinaryExpression).right
                        End If

                        Return CDbl(value.Evaluate(env))
                    End Function) _
            .ToArray
        Dim lpp As New LPP(
            objectiveFunctionType:=direction.Description,
            variableNames:=allSymbols.Objects,
            objectiveFunctionCoefficients:=obj,
            constraintCoefficients:=sbjMatrix,
            constraintTypes:=types,
            constraintRightHandSides:=rightHands,
            objectiveFunctionValue:=0
        )
        Dim solution As LPPSolution = lpp.solve(showProgress:=False)
        Dim lppResult As New list

        lppResult.add("feasible", solution.failureMessage.StringEmpty)
        lppResult.add("solution", allSymbols.Objects.ToDictionary(Function(name) name, Function(name) solution.GetSolution(name)))
        lppResult.add("objective", solution.ObjectiveFunctionValue)
        lppResult.add("slack", solution.slack _
                      .Select(Function(x, i) (x, i)) _
                      .ToDictionary(Function(j) j.i,
                                    Function(i)
                                        Dim info As New Dictionary(Of String, Object)
                                        Dim j As Integer = i.i

                                        info.Add("slack", solution.slack(j))

                                        If solution.slack(j) = 0.0 Then
                                            info.Add("binding", True)
                                            info.Add("shadowPrice", solution.shadowPrice(j))
                                        ElseIf solution.slack(j) > 0 Then
                                            info.Add("shadowPrice", Double.NaN)
                                            info.Add("binding", False)
                                        Else
                                            info.Add("shadowPrice", Double.NaN)
                                            info.Add("binding", False)
                                        End If

                                        Return info
                                    End Function))
        lppResult.add("reduced_cost", solution.GetReducedCost)

        Return lppResult
    End Function

    <Extension>
    Private Function alignVector(vector As IEnumerable(Of NamedValue(Of Double)), allSymbols As Index(Of String)) As Double()
        Dim table = vector.ToDictionary(Function(a) a.Name, Function(a) a.Value)
        Dim vec As Double() = New Double(allSymbols.Count - 1) {}

        For Each i In allSymbols
            If table.ContainsKey(i.value) Then
                vec(i) = table(i.value)
            End If
        Next

        Return vec
    End Function

    <Extension>
    Private Function isSimple(exp As Expression) As Boolean
        Return TypeOf exp Is Literal OrElse TypeOf exp Is SymbolReference
    End Function

    <Extension>
    Private Iterator Function GetVector(exp As Expression, allSymbols As Index(Of String), env As Environment) As IEnumerable(Of NamedValue(Of Double))
        If TypeOf exp Is BinaryExpression Then
            Dim bin = DirectCast(exp, BinaryExpression)
            Dim left = bin.left
            Dim right = bin.right

            If left.isSimple AndAlso right.isSimple Then
                Dim symbol As String
                Dim coef As Double

                If TypeOf left Is Literal Then
                    coef = left.Evaluate(env)
                    symbol = DirectCast(right, SymbolReference).symbol
                Else
                    coef = right.Evaluate(env)
                    symbol = DirectCast(left, SymbolReference).symbol
                End If

                Yield New NamedValue(Of Double)(symbol, coef)
            Else
                For Each x In left.GetVector(allSymbols, env)
                    Yield x
                Next
                For Each x In right.GetVector(allSymbols, env)
                    Yield x
                Next
            End If
        ElseIf TypeOf exp Is SymbolReference Then
            Yield New NamedValue(Of Double)(DirectCast(exp, SymbolReference).symbol, 1)
        Else
            Throw New NotImplementedException
        End If
    End Function

    <Extension>
    Private Iterator Function GetSymbols(exp As Expression) As IEnumerable(Of String)
        If TypeOf exp Is SymbolReference Then
            Yield DirectCast(exp, SymbolReference).symbol
            Return
        End If
        If TypeOf exp Is BinaryExpression Then
            For Each x As String In DirectCast(exp, BinaryExpression).left.GetSymbols
                Yield x
            Next
            For Each x As String In DirectCast(exp, BinaryExpression).right.GetSymbols
                Yield x
            Next
        End If
    End Function

    <ExportAPI("linprog")>
    Public Function linprog(f As vector,
                            A As matrix,
                            b As vector,
                            Optional direction As OptimizationType = OptimizationType.MIN,
                            Optional env As Environment = Nothing) As Object
        Throw New NotImplementedException
    End Function
End Module
