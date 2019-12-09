Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    Public Class RReturn(Of T) : Inherits Value(Of T)

        Public Property messages As New List(Of Message)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(x As T) As RReturn(Of T)
            Return New RReturn(Of T) With {.Value = x}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(msg As Message) As RReturn(Of T)
            Return New RReturn(Of T) With {
                .messages = New List(Of Message) From {msg}
            }
        End Operator
    End Class
End Namespace