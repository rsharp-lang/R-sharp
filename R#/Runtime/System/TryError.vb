Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    ''' <summary>
    ''' try-error
    ''' </summary>
    Public Class TryError

        Public ReadOnly Property [error] As Message
        Public ReadOnly Property stackframe As StackFrame

        Sub New(err As Message, stackframe As StackFrame)
            Me.error = err
            Me.stackframe = stackframe
        End Sub

        Public Function traceback() As vector
            Return New vector With {
                .data = [error].environmentStack _
                    .Select(Function(line) line.ToString) _
                    .ToArray,
                .elementType = RType.GetRSharpType(GetType(String))
            }
        End Function

        Public Overrides Function ToString() As String
            Return [error].message.JoinBy("; ")
        End Function
    End Class
End Namespace