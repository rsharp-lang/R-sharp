Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package.File.Expressions

Namespace System.Package.File

    Public Class Writer : Implements IDisposable

        Dim buffer As BinaryWriter
        Dim disposedValue As Boolean

        Public Const Magic As String = "SMRUCC/R#"

        Sub New(buffer As Stream)
            Me.buffer = New BinaryWriter(buffer)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>
        ''' 函数返回表达式的长度
        ''' </returns>
        Public Function Write(x As Expression) As Integer
            Select Case x.GetType
                Case GetType(Literal)
                    Return Write(DirectCast(x, Literal))
            End Select
        End Function

        Private Function save(x As Literal) As Byte()
            Using ms As New MemoryStream, outfile As New BinaryWriter(ms)
                Call outfile.Write(DirectCast(ExpressionTypes.Literal, Integer))
                Call outfile.Write(0)
                Call outfile.Write(DirectCast(x.type, Byte))

                Select Case x.type
                    Case TypeCodes.boolean : Call outfile.Write(CType(If(DirectCast(x.value, Boolean), 1, 0), Byte))
                    Case TypeCodes.double : Call outfile.Write(CType(x.value, Double))
                    Case TypeCodes.integer : Call outfile.Write(CType(x.value, Long))
                    Case Else
                        Call outfile.Write(Scripting.ToString(x.value))
                End Select

                Call outfile.Flush()

                Dim totalLength As Integer = ms.Length
                Dim dataSize As Integer = totalLength - 4 - 4

                Call outfile.Seek(4, SeekOrigin.Begin)
                Call outfile.Write(dataSize)
                Call outfile.Flush()

                Return ms.ToArray
            End Using
        End Function

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