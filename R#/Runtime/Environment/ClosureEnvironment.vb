Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime

    Public Class ClosureEnvironment : Inherits Environment

        ReadOnly parent_context As Environment
        ReadOnly closure_context As Environment

        Sub New(parent As Environment, closure_context As Environment)
            Call MyBase.New(parent, parent.stackFrame, isInherits:=False)

            Me.parent_context = parent
            Me.closure_context = closure_context
        End Sub

        ''' <summary>
        ''' find in closure internal context at first 
        ''' and then find on the parent context
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[inherits]"></param>
        ''' <returns>
        ''' this function returns nothing if the target symbol 
        ''' is not found in the environment context
        ''' </returns>
        Public Overrides Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            Dim symbol As Symbol = MyBase.FindSymbol(name, [inherits]:=False)

            If symbol Is Nothing Then
                symbol = closure_context.FindSymbol(name, [inherits])
            End If

            If symbol Is Nothing Then
                symbol = parent_context.FindSymbol(name, [inherits])
            End If

            Return symbol
        End Function
    End Class
End Namespace