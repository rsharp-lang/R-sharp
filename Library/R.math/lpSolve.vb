
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' linear programming solver
''' </summary>
<Package("lpSolve")>
Module lpSolve

    <ExportAPI("lp")>
    Public Function lp(direction As OptimizationType, objective As Expression, subjective As Expression(), Optional env As Environment = Nothing)
        If subjective.Length = 1 AndAlso TypeOf subjective(Scan0) Is VectorLiteral Then
            subjective = DirectCast(subjective(Scan0), VectorLiteral).ToArray
        End If

        Dim allSymbols As String() = objective.GetSymbols.ToArray
        Dim obj As Double() = objective.GetVector(allSymbols, env).ToArray
        Dim sbjMatrix As Double()() = subjective _
            .Select(Function(exp)
                        Return DirectCast(exp, BinaryExpression).left _
                            .GetVector(allSymbols, env) _
                            .ToArray
                    End Function) _
            .ToArray
        Dim types As String() = subjective _
            .Select(Function(exp) DirectCast(exp, BinaryExpression).operator) _
            .ToArray
        Dim rightHands As Double() = subjective _
            .Select(Function(exp)
                        Return CDbl(DirectCast(exp, BinaryExpression).right.Evaluate(env))
                    End Function) _
            .ToArray
        Dim lpp As New LPP(
            objectiveFunctionType:=direction.Description,
            variableNames:=allSymbols,
            objectiveFunctionCoefficients:=obj,
            constraintCoefficients:=sbjMatrix,
            constraintTypes:=types,
            constraintRightHandSides:=rightHands,
            objectiveFunctionValue:=0
        )
        Dim solution As LPPSolution = lpp.solve

        Return solution
    End Function

    <Extension>
    Private Iterator Function GetVector(exp As Expression, allSymbols As String(), env As Environment) As IEnumerable(Of Double)

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
End Module
