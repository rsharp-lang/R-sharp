#Region "Microsoft.VisualBasic::121a8526f32611213b8fa36587e0d087, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\BinaryBetweenExpression.vb"

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

    '     Class BinaryBetweenExpression
    ' 
    '         Properties: type
    ' 
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class BinaryBetweenExpression : Inherits Expression

        Dim collectionSet As Expression
        Dim range As Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        Sub New(collectionSet As Expression, range As Expression)
            Me.collectionSet = collectionSet
            Me.range = range
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim rangeVal As Object() = asVector(Of Object)(range.Evaluate(envir))
            Dim min As IComparable = rangeVal(Scan0)
            Dim max As IComparable = rangeVal(1)
            Dim values As Object() = asVector(Of Object)(collectionSet.Evaluate(envir))
            Dim flags As Boolean() = values _
                .Select(Function(a)
                            Dim x As IComparable = DirectCast(a, IComparable)
                            Dim cmin As Boolean = x.CompareTo(min) >= 0
                            Dim cmax As Boolean = x.CompareTo(max) <= 0

                            Return cmin AndAlso cmax
                        End Function) _
                .ToArray

            Return flags
        End Function
    End Class
End Namespace
