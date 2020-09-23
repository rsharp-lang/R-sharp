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