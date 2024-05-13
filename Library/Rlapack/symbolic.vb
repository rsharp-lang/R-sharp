#Region "Microsoft.VisualBasic::27d877b4bb795a9a91117cb5d7980589, Library\Rlapack\symbolic.vb"

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

    '   Total Lines: 126
    '    Code Lines: 94
    ' Comment Lines: 14
    '   Blank Lines: 18
    '     File Size: 5.37 KB


    ' Module symbolic
    ' 
    '     Function: fit, fit2, formulaDataFrame, lambda, ParseExpression
    '               ParseMathML, PolynomialParse
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.factory
Imports Microsoft.VisualBasic.MIME.application.xml.MathML
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' math symbolic expression for R#
''' </summary>
<Package("symbolic", Category:=APICategories.ResearchTools)>
Module symbolic

    Friend Sub Main()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(EvolutionResult()), AddressOf formulaDataFrame)
    End Sub

    Private Function formulaDataFrame(formula As EvolutionResult(), args As list, env As Environment) As dataframe
        Dim df As New dataframe With {.columns = New Dictionary(Of String, Array)}

        Call df.add("formula", formula.Select(Function(f) f.result.toStringExpression))
        Call df.add("error", formula.Select(Function(f) f.fitness))
        Call df.add("time", formula.Select(Function(f) f.time))
        Call df.add("epochs", formula.Select(Function(f) f.epochs))

        Return df
    End Function

    ''' <summary>
    ''' Parse a math formula
    ''' </summary>
    ''' <param name="expressions">a list of character vector which are all represents some math formula expression.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parse")>
    <RApiReturn(GetType(Expression))>
    Public Function ParseExpression(<RRawVectorArgument> expressions As Object, Optional env As Environment = Nothing) As Object
        Dim str As pipeline = pipeline.TryCreatePipeline(Of String)(expressions, env)

        If str.isError Then
            Return str.getError
        End If

        Return str _
            .populates(Of String)(env) _
            .Select(AddressOf ScriptEngine.ParseExpression) _
            .DoCall(AddressOf Internal.Object.vector.asVector)
    End Function

    <ExportAPI("parse.mathml")>
    Public Function ParseMathML(mathml As String) As LambdaExpression
        Return LambdaExpression.FromMathML(mathml)
    End Function

    ''' <summary>
    ''' convert the formula expression to the mathml lambda expression model
    ''' </summary>
    ''' <param name="formula"></param>
    ''' <returns></returns>
    <ExportAPI("lambda")>
    <RApiReturn(GetType(LambdaExpression))>
    Public Function lambda(formula As Object, Optional env As Environment = Nothing) As Object
        If TypeOf formula Is FormulaExpression Then
            Return Compiler.GetLambda(DirectCast(formula, FormulaExpression))
        ElseIf TypeOf formula Is DeclareLambdaFunction Then
            Return Compiler.GetLambda(DirectCast(formula, DeclareLambdaFunction))
        ElseIf TypeOf formula Is String Then
            Return Compiler.GetLambda(DirectCast(formula, String))
        Else
            Return Message.InCompatibleType(GetType(DeclareLambdaFunction), formula.GetType, env)
        End If
    End Function

    <ExportAPI("as.polynomial")>
    Public Function PolynomialParse(expression As String) As Polynomial
        Return Polynomial.Parse(expression)
    End Function

    <ExportAPI("gp_fit")>
    <RApiReturn(GetType(EvolutionResult))>
    Public Function fit(x As Double(), y As Double(),
                        <RRawVectorArgument(TypeCodes.string)>
                        Optional symbols As Object = "1|2|12|E|PI|Tau|Sin|Cos|Log|Exp|Pow",
                        Optional threshold As Double = 0.05,
                        Optional epochs As Integer = 10,
                        Optional env As Environment = Nothing) As Object

        Dim factory As New ExpressionFactory
        Dim tokens As String() = CLRVector.asCharacter(symbols)
        Dim evolution As New Evolution() With {.ExpressionFactory = factory}
        Dim data As DataPoint() = x _
            .Select(Function(xi, i) New DataPoint(xi, y(i))) _
            .ToArray
        Dim config As GPConfiguration = GPConfiguration.createDefaultConfig()
        config.objective = ObjectiveFunction.MAE
        config.fitnessThreshold = threshold
        config.initTreeDepth = 4

        Dim results As New List(Of EvolutionResult)

        For i As Integer = 0 To epochs
            results.Add(evolution.evolveTreeFor(data, config))
        Next

        Return results _
            .OrderBy(Function(a) a.fitness) _
            .ToArray
    End Function

    <ExportAPI("ga_fit")>
    <RApiReturn(GetType(EvolutionResult))>
    Public Function fit2(x As Double(), y As Double(), Optional env As Environment = Nothing) As Object

    End Function
End Module
