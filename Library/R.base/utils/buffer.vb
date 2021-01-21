Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rsharp = SMRUCC.Rsharp

<Package("buffer", Category:=APICategories.UtilityTools)>
Module buffer

    <ExportAPI("float")>
    <RApiReturn(GetType(Double))>
    Public Function float(<RRawVectorArgument> stream As Object, Optional networkOrder As Boolean = False, Optional sizeOf As Integer = 32, Optional env As Environment = Nothing) As Object
        If sizeOf = 32 Then
            Return env.numberFramework(stream, networkOrder, 4, AddressOf BitConverter.ToSingle)
        ElseIf sizeOf = 64 Then
            Return env.numberFramework(stream, networkOrder, 8, AddressOf BitConverter.ToDouble)
        Else
            Return Internal.debug.stop($"the given size value '{sizeOf}' is invalid!", env)
        End If
    End Function

    <Extension>
    Private Function numberFramework(Of T)(env As Environment, <RRawVectorArgument> stream As Object, networkOrder As Boolean, width As Integer, fromBlock As Func(Of Byte(), Integer, T)) As Object
        Dim buffer As [Variant](Of Byte(), Message) = Rsharp.Buffer(stream, env)

        If buffer Is Nothing Then
            Return Nothing
        ElseIf buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Dim bytes As Byte() = buffer.TryCast(Of Byte())

        If networkOrder AndAlso BitConverter.IsLittleEndian Then
            Return bytes _
                .Split(width) _
                .Select(Function(block)
                            Array.Reverse(block)
                            Return fromBlock(block, Scan0)
                        End Function) _
                .ToArray
        Else
            Return bytes _
               .Split(width) _
               .Select(Function(block)
                           Return fromBlock(block, Scan0)
                       End Function) _
               .ToArray
        End If
    End Function

    <ExportAPI("integer")>
    <RApiReturn(GetType(Integer))>
    Public Function toInteger(<RRawVectorArgument> stream As Object, Optional networkOrder As Boolean = False, Optional sizeOf As Integer = 32, Optional env As Environment = Nothing) As Object
        If sizeOf = 32 Then
            Return env.numberFramework(stream, networkOrder, 4, AddressOf BitConverter.ToInt32)
        ElseIf sizeOf = 64 Then
            Return env.numberFramework(stream, networkOrder, 8, AddressOf BitConverter.ToInt64)
        Else
            Return Internal.debug.stop($"the given size value '{sizeOf}' is invalid!", env)
        End If
    End Function
End Module
