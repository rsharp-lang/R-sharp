Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace System.Package.File.Expressions

    ''' <summary>
    ''' the R expression writer/reader
    ''' </summary>
    ''' <remarks>
    ''' expression: [ExpressionTypes, i32][dataSize, i32][TypeCodes, byte][expressionData, bytes]
    '''              4                     4              1                ...
    ''' </remarks>
    Public MustInherit Class RExpression

        Protected ReadOnly context As Writer

        Sub New(context As Writer)
            Me.context = context
        End Sub

        Public MustOverride Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
        Public MustOverride Sub WriteBuffer(ms As MemoryStream, x As Expression)

        Public Function GetBuffer(x As Expression) As Byte()
            Using ms As New MemoryStream
                Call WriteBuffer(ms, x)
                Return ms.ToArray
            End Using
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="outfile"></param>
        ''' <remarks>
        ''' expression: [ExpressionTypes, i32][dataSize, i32][TypeCodes, byte][expressionData, bytes]
        '''              4                     4              1                ...
        ''' </remarks>
        Protected Shared Sub saveSize(outfile As BinaryWriter)
            Dim totalLength As Integer = outfile.BaseStream.Length
            Dim dataSize As Integer = totalLength - 4 - 4 - 1

            Call outfile.Seek(4, SeekOrigin.Begin)
            Call outfile.Write(dataSize)
            Call outfile.Flush()
        End Sub
    End Class
End Namespace