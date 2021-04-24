Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class TakeItems : Inherits PipelineKeyword

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "Take"
            End Get
        End Property

        ''' <summary>
        ''' this expression should produce an integer value
        ''' </summary>
        Dim n As Expression

        Sub New(n As Expression)
            Me.n = n
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            Return n.Exec(context)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Exec(result As IEnumerable(Of JavaScriptObject), context As ExecutableContext) As IEnumerable(Of JavaScriptObject)
            Return result.Take(count:=CInt(Exec(context)))
        End Function

        Public Overrides Function ToString() As String
            Return $"TAKE {n}"
        End Function
    End Class
End Namespace