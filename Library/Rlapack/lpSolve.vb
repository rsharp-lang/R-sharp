#Region "Microsoft.VisualBasic::33d5039224eb1452a7fae060225e21b0, Library\Rlapack\lpSolve.vb"

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

'   Total Lines: 218
'    Code Lines: 175 (80.28%)
' Comment Lines: 16 (7.34%)
'    - Xml Docs: 93.75%
' 
'   Blank Lines: 27 (12.39%)
'     File Size: 9.24 KB


' Module lpSolve
' 
'     Function: alignVector, GetSymbols, GetVector, isSimple, linprog
'               lp, lpMax, lpMin, solve_LSAP
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' Solve Linear/Integer Programs 
''' </summary>
<Package("lpSolve")>
Module lpSolve

    <ExportAPI("lp.min")>
    Public Function lpMin(<RLazyExpression> objective As Expression, Optional env As Environment = Nothing) As Object
        Return lp_objective_func(OptimizationType.MIN, objective, env)
    End Function

    <ExportAPI("lp.max")>
    Public Function lpMax(<RLazyExpression> objective As Expression, Optional env As Environment = Nothing) As Object
        Return lp_objective_func(OptimizationType.MAX, objective, env)
    End Function

    Private Function lp_objective_func(type As OptimizationType, objective As Expression, env As Environment) As Object
        Dim allSymbols As Index(Of String) = Nothing
        Dim obj As Double() = Nothing

        If TypeOf objective Is BinaryExpression Then
            ' x1+x2+x3+...
            allSymbols = objective.GetSymbols.Indexing
            obj = objective _
                .GetVector(allSymbols, env) _
                .alignVector(allSymbols)
        ElseIf TypeOf objective Is VectorLiteral Then
            ' [....]
            allSymbols = Nothing
            obj = CLRVector.asNumeric(objective.Evaluate(env))
        End If

        If obj Is Nothing Then
            Return RInternal.debug.stop("the given objective function should be an binary math expression or a numeric vector!", env)
        End If

        Return New lp_objective With {
            .factors = obj,
            .symbols = If(allSymbols Is Nothing, Nothing, allSymbols.Objects),
            .type = type
        }
    End Function

    <ExportAPI("subject_to")>
    Public Function subject_to(<RRawVectorArgument, RLazyExpression, RListObjectArgument> subject As list, Optional env As Environment = Nothing) As Object
        Dim subjects As BinaryExpression() = subject.data.Select(Function(e) DirectCast(e, BinaryExpression)).ToArray
        Dim allSymbols As Index(Of String) = subjects.Select(Function(b) b.left.GetSymbols).IteratesALL.Indexing
        Dim matrix As Double()() = subjects _
            .Select(Function(b) b.left _
            .GetVector(allSymbols, env) _
            .alignVector(allSymbols)) _
            .ToArray
        Dim c As Double() = subjects.Select(Function(b) CLRVector.asNumeric(b.right.Evaluate(env))(0)).ToArray
        Dim op As String() = subjects.Select(Function(b) b.operator).ToArray

        Return New lp_subject With {
            .constraints = c,
            .types = op,
            .matrix = matrix,
            .symbols = allSymbols.Objects
        }
    End Function

    Private Class lp_objective

        Public type As OptimizationType
        Public symbols As String()
        Public factors As Double()

    End Class

    Private Class lp_subject

        Public symbols As String()
        Public matrix As Double()()
        Public constraints As Double()
        Public types As String()

        Public Iterator Function alignMatrix(obj As String()) As IEnumerable(Of Double())
            Dim cols As Index(Of String) = symbols

            For Each row As Double() In matrix
                Yield obj.Select(Function(name) row(cols(name))).ToArray
            Next
        End Function

    End Class

    ''' <summary>
    ''' Linear and Integer Programming
    ''' </summary>
    ''' <param name="type">Character string giving direction of optimization: "min" (default) or "max."</param>
    ''' <param name="objective">the objective function, for apply of the optimization analysis of this function min value or max value</param>
    ''' <param name="subjective">the subjective matrix</param>
    ''' <param name="env"></param>
    ''' <returns>the lpp solution, is a tuple list that contains the data fields:
    ''' 
    ''' objective - the objective function value
    ''' solution - a list of the variable value for make the solution
    ''' </returns>
    <ExportAPI("lp")>
    Public Function lp(<RLazyExpression> objective As Expression,
                       <RLazyExpression, RRawVectorArgument>
                       subjective As Expression,
                       Optional type As OptimizationType? = Nothing,
                       Optional env As Environment = Nothing) As Object

        Dim lp_subj As lp_subject = Nothing
        Dim lp_obj As lp_objective = Nothing
        Dim subjects As Expression() = Nothing

        If TypeOf subjective Is SymbolReference Then
            Dim val = subjective.Evaluate(env)

            If TypeOf val Is lp_subject Then
                lp_subj = val
            Else
                subjects = val
            End If
        ElseIf TypeOf subjective Is VectorLiteral Then
            subjects = subjective.Evaluate(env)
        Else
            Return RInternal.debug.stop("invalid subjective matrix!", env)
        End If

        If subjects.TryCount = 1 Then
            lp_subj = subjects(Scan0).Evaluate(env)
        End If

        Dim allSymbols As Index(Of String) = Nothing
        Dim obj As Double() = Nothing

        If TypeOf objective Is BinaryExpression Then
            allSymbols = objective.GetSymbols.Indexing
            obj = objective _
                .GetVector(allSymbols, env) _
                .alignVector(allSymbols)
        Else
            Dim [object] As Object = objective.Evaluate(env)

            If TypeOf [object] Is lp_objective Then
                lp_obj = [object]
            ElseIf DataFramework.IsNumericCollection([object].GetType) Then
                lp_obj = New lp_objective With {
                    .factors = CLRVector.asNumeric([object])
                }
            Else
                Return Message.InCompatibleType(GetType(lp_objective), [object].GetType, env)
            End If
        End If

        If Not lp_obj Is Nothing Then
            obj = lp_obj.factors
            allSymbols = lp_obj.symbols

            If type Is Nothing Then
                type = lp_obj.type
            End If
        End If

        Dim sbjMatrix As Double()()
        Dim types As String()
        Dim rhs As Double()

        If lp_subj Is Nothing AndAlso lp_obj Is Nothing Then
            sbjMatrix = subjects.subjectiveMatrix(allSymbols, env).ToArray
            types = subjects.op.ToArray
            rhs = subjects.constraintBoundaries(env).ToArray
        ElseIf lp_subj IsNot Nothing AndAlso lp_obj IsNot Nothing Then
            allSymbols = lp_obj.symbols
            obj = lp_obj.factors

            If type Is Nothing Then
                type = lp_obj.type
            End If

            sbjMatrix = lp_subj.alignMatrix(allSymbols.Objects).ToArray
            types = lp_subj.types
            rhs = lp_subj.constraints
        ElseIf lp_subj Is Nothing AndAlso lp_obj IsNot Nothing Then
            ' 情况3: 约束条件数据为空，但目标函数数据不为空
            ' 使用lp_obj的目标函数数据，从subjects表达式数组构建约束条件
            obj = lp_obj.factors
            allSymbols = lp_obj.symbols

            If type Is Nothing Then
                type = lp_obj.type
            End If

            ' 从subjects构建约束矩阵，类似第一个分支的逻辑
            If subjects IsNot Nothing Then
                sbjMatrix = subjects.subjectiveMatrix(allSymbols, env).ToArray
                types = subjects.op.ToArray
                rhs = subjects.constraintBoundaries(env).ToArray
            Else
                Return RInternal.debug.stop("Constraint data is missing! Please provide a valid constraint expression.", env)
            End If
        ElseIf lp_subj IsNot Nothing AndAlso lp_obj Is Nothing Then
            ' 情况4: 目标函数数据为空，但约束条件数据不为空
            ' 检查是否已通过二元表达式解析设置了obj和allSymbols
            If obj Is Nothing OrElse allSymbols Is Nothing Then
                Return RInternal.debug.stop("Objective function data is missing! Please provide a valid objective function expression.", env)
            End If

            ' 使用已设置的obj和allSymbols，结合lp_subj的约束数据
            sbjMatrix = lp_subj.alignMatrix(allSymbols.Objects).ToArray
            types = lp_subj.types
            rhs = lp_subj.constraints
        Else
            ' 错误处理：缺少构建约束的必要数据
            Return RInternal.debug.stop("Constraint data is missing! Please provide a valid constraint expression or an lp_subject object.", env)
        End If

        Dim lpp As New LPP(
            objectiveFunctionType:=CType(type, OptimizationType).Description,
            variableNames:=allSymbols.Objects,
            objectiveFunctionCoefficients:=obj,
            constraintCoefficients:=sbjMatrix,
            constraintTypes:=types,
            constraintRightHandSides:=rhs,
            objectiveFunctionValue:=0
        )
        Dim solution As LPPSolution = lpp.solve(showProgress:=False)
        Dim lppResult As New list
        Dim slack As list = list.empty
        Dim slack_size = solution.slack.TryCount

        For i As Integer = 0 To slack_size - 1
            Dim info As New Dictionary(Of String, Object)
            Dim j As Integer = i

            Call info.Add("slack", solution.slack(j))

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

            Call slack.add($"#{i + 1}", info)
        Next

        If Not solution.failureMessage.StringEmpty(, True) Then
            Call env.AddMessage(solution.failureMessage, MSG_TYPES.WRN)
        End If

        Call lppResult.add("feasible", solution.failureMessage.StringEmpty)
        Call lppResult.add("solution", New list(allSymbols.Objects _
             .ToDictionary(Function(name) name,
                           Function(name)
                               Return solution.GetSolution(name)
                           End Function)))
        Call lppResult.add("objective", solution.ObjectiveFunctionValue)
        Call lppResult.add("slack", slack)
        Call lppResult.add("reduced_cost", solution.GetReducedCost)

        Return lppResult
    End Function

    <Extension>
    Private Iterator Function constraintBoundaries(subjective As Expression(), env As Environment) As IEnumerable(Of Double)
        For Each exp As Expression In subjective
            Dim value As Expression

            If TypeOf exp Is ValueAssignExpression Then
                value = DirectCast(exp, ValueAssignExpression).value
            Else
                value = DirectCast(exp, BinaryExpression).right
            End If

            Yield CDbl(value.Evaluate(env))
        Next
    End Function

    <Extension>
    Private Iterator Function op(subjective As Expression()) As IEnumerable(Of String)
        For Each exp As Expression In subjective
            If TypeOf exp Is ValueAssignExpression Then
                Yield "="
            Else
                Yield DirectCast(exp, BinaryExpression).operator
            End If
        Next
    End Function

    <Extension>
    Private Iterator Function subjectiveMatrix(subjective As Expression(), allSymbols As Index(Of String), env As Environment) As IEnumerable(Of Double())
        For Each exp As Expression In subjective
            Dim target As Expression

            If TypeOf exp Is ValueAssignExpression Then
                target = DirectCast(exp, ValueAssignExpression).targetSymbols(Scan0)
            Else
                target = DirectCast(exp, BinaryExpression).left
            End If

            Yield target _
                .GetVector(allSymbols, env) _
                .alignVector(allSymbols)
        Next
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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x">the cost matrix for the assignments</param>
    ''' <returns></returns>
    <ExportAPI("solve_LSAP")>
    Public Function solve_LSAP(x As Object) As Object

    End Function
End Module
