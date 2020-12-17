Imports System.IO
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File

    Public Class BlockReader

        Public Property Expression As ExpressionTypes
        Public Property type As TypeCodes
        Public Property body As Byte()

        Public Shared Function Read(reader As BinaryReader, i As Long) As BlockReader
            Call reader.BaseStream.Seek(i, SeekOrigin.Begin)


        End Function

    End Class
End Namespace