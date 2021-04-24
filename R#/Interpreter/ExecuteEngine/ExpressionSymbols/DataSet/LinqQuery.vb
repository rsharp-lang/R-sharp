Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.LINQ
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class LinqQuery : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.LinqQuery
            End Get
        End Property

        Dim LINQ As QueryExpression

        Sub New(query As QueryExpression)
            LINQ = query
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return LINQ.Exec(New ExecutableContext(envir))
        End Function

        Friend Shared Function produceSequenceVector(sequence As Object, ByRef isList As Boolean) As Object
            If sequence.GetType Is GetType(list) Then
                sequence = DirectCast(sequence, list).slots
            End If

            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                sequence = DirectCast(sequence, Dictionary(Of String, Object)).ToArray
                isList = True
            ElseIf sequence.GetType.ImplementInterface(GetType(IDictionary)) Then
                With DirectCast(sequence, IDictionary)
                    sequence = (From key In .Keys.AsQueryable
                                Let keyStr As String = any.ToString(key)
                                Let keyVal As Object = .Item(key)
                                Select New KeyValuePair(Of String, Object)(keyStr, keyVal)).ToArray
                    isList = True
                End With
            Else
                sequence = Runtime.asVector(Of Object)(sequence)
            End If

            Return sequence
        End Function
    End Class
End Namespace