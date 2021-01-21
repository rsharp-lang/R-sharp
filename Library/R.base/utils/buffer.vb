
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("buffer", Category:=APICategories.UtilityTools)>
Module buffer

    <ExportAPI("numeric")>
    <RApiReturn(GetType(Double))>
    Public Function numeric(<RRawVectorArgument> stream As Object, Optional networkOrder As Boolean = False, Optional env As Environment = Nothing) As Object
        Dim bytes As pipeline = pipeline.TryCreatePipeline(Of Byte)(stream, env)
        Dim buffer As Byte()

        If stream Is Nothing Then
            Return Nothing
        End If

        If bytes.isError Then
            If TypeOf stream Is Stream Then
                buffer = DirectCast(stream, Stream) _
                    .PopulateBlocks _
                    .IteratesALL _
                    .ToArray
            Else
                Return bytes.getError
            End If
        Else
            buffer = bytes.populates(Of Byte)(env).ToArray
        End If

        If networkOrder AndAlso BitConverter.IsLittleEndian Then
            Return buffer _
                .Split(4) _
                .Select(Function(block)
                            Array.Reverse(block)
                            Return BitConverter.ToSingle(block, Scan0)
                        End Function) _
                .ToArray
        Else
            Return buffer _
               .Split(4) _
               .Select(Function(block)
                           Return BitConverter.ToSingle(block, Scan0)
                       End Function) _
               .ToArray
        End If
    End Function
End Module
