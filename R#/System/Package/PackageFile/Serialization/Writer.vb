Imports System.IO
Imports SMRUCC.Rsharp.System.Package.File.Expressions

Namespace System.Package.File

    Public Class Writer : Implements IDisposable

        Dim buffer As StreamWriter
        Dim disposedValue As Boolean

        Sub New(buffer As Stream)
            Me.buffer = New StreamWriter(buffer)
        End Sub

        Public Sub Write(x As RLiteral)
            Call buffer.Write(ExpressionTypes.Literal)
            Call buffer.Write(CByte(x.type))
            Call buffer.Write(x.value)
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call buffer.Flush()
                    Call buffer.Close()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace