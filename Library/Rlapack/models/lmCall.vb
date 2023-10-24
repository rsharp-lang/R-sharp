#Region "Microsoft.VisualBasic::8eb3bd15d7ffc7c17b3fe6249e7aa9c6, D:/GCModeller/src/R-sharp/Library/Rlapack//models/lmCall.vb"

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

    '   Total Lines: 132
    '    Code Lines: 108
    ' Comment Lines: 0
    '   Blank Lines: 24
    '     File Size: 4.37 KB


    ' Class lmCall
    ' 
    '     Properties: data, equation, formula, lm, name
    '                 R2, variables, weights
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CreateFormulaCall, Predicts, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Security.Principal
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Public Class lmCall

    Public Property lm As IFitted
    Public Property formula As FormulaExpression
    Public Property name As String
    Public Property variables As String()
    Public Property data As String
    Public Property weights As String

    Public ReadOnly Property R2 As Double
        Get
            Return lm.R2
        End Get
    End Property

    Public ReadOnly Property equation As String
        Get
            Return formula.ToString
        End Get
    End Property

    Public ReadOnly Property factors As Double()
        Get
            Return lm.Polynomial.Factors
        End Get
    End Property

    Public ReadOnly Property summary As list
        Get
            If TypeOf lm Is WeightedFit Then
                Return weightLmSummary(lm)
            Else
                Return Nothing
            End If
        End Get
    End Property

    Sub New(name As String, variables As String())
        Me.name = name
        Me.variables = variables
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Predicts(x As Double()) As Double
        Return lm.GetY(x)
    End Function

    Const format As String = "G7"

    Private Function weightLmSummary(wf As WeightedFit) As list
        Dim formula = DirectCast(DirectCast(lm, WeightedFit).Polynomial, Polynomial)

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"F", wf.FisherF},
                {"factors", wf.Polynomial.Factors},
                {"f(x)", formula.ToString(variables, format)}
            }
        }
    End Function

    Private Function weightLmString(wf As WeightedFit) As String
        Dim formula = DirectCast(DirectCast(lm, WeightedFit).Polynomial, Polynomial)

        Return $"
Call:
lm(formula = {formula.ToString(variables, format)}, data = <{data}>, weights = {weights})

Coefficients:
(Intercept)            {variables(Scan0)}  
  {formula.Factors(Scan0).ToString(format)}    {formula.Factors(1).ToString(format)}

R2: {lm.R2}
F-statistic: {wf.FisherF}
"
    End Function

    Public Overrides Function ToString() As String
        If TypeOf lm Is WeightedFit Then
            Return weightLmString(lm)
        ElseIf TypeOf lm Is FitResult Then
            Dim formula = DirectCast(DirectCast(lm, FitResult).Polynomial, Polynomial)

            Return $"
Call:
lm(formula = {formula.ToString(variables, "G3")}, data = <{data}>)

Coefficients:
(Intercept)            {variables(Scan0)}    
  {formula.Factors(Scan0).ToString("G4")}    {formula.Factors(1).ToString("G4")}  

"
        ElseIf TypeOf lm Is LogisticFit Then
            Dim formula = DirectCast(DirectCast(lm, LogisticFit).Polynomial, Polynomial)

            Return $"
Call:
glm(formula = {formula.ToString(variables, "G3")}, family = binomial(link = ""logit""), data = <{data}>)

Coefficients:
(Intercept)            {variables(Scan0)}    
  {formula.Factors(Scan0).ToString("G4")}    {formula.Factors(1).ToString("G4")}  

"
        Else
            Dim formula = DirectCast(DirectCast(lm, MLRFit).Polynomial, MultivariatePolynomial)

            Return $"
Call:
lm(formula = {formula.ToString(variables, "G3")}, data = <{data}>)

Coefficients:
(Intercept)            {variables(Scan0)}    
  {formula.Factors(Scan0).ToString("G4")}    {formula.Factors(1).ToString("G4")}  

"
        End If
    End Function

    Public Function CreateFormulaCall() As Expression
        If TypeOf lm Is MLRFit Then
            Dim poly = DirectCast(lm.Polynomial, MultivariatePolynomial)
            Dim exp As Expression = New Literal(poly.Factors(Scan0))

            For i As Integer = 1 To poly.Factors.Length - 1
                exp = New BinaryExpression(
                    exp,
                    New BinaryExpression(
                        New Literal(poly.Factors(i)),
                        New SymbolReference(variables(i - 1)),
                        "*"),
                    "+")
            Next

            Return exp
        Else
            Dim linear = DirectCast(lm.Polynomial, Polynomial)
            Dim exp As Expression = New Literal(linear.Factors(Scan0))
            Dim singleSymbol As String = variables(Scan0)

            For i As Integer = 1 To linear.Factors.Length - 1
                exp = New BinaryExpression(
                    exp,
                    New BinaryExpression(
                        New Literal(linear.Factors(i)),
                        New BinaryExpression(
                            New SymbolReference(singleSymbol),
                            New Literal(i),
                            "^"),
                        "*"),
                    "+")
            Next

            Return exp
        End If
    End Function
End Class
