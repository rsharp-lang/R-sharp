
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

''' <summary>
''' linear programming solver
''' </summary>
<Package("lpSolve")>
Module lpSolve

    <ExportAPI("lp")>
    Public Function lp(direction As OptimizationType, objective As Expression, subjective As Expression())

    End Function

End Module
