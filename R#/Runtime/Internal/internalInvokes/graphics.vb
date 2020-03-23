Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module graphics

        ''' <summary>
        ''' Generic function for plotting of R objects. 
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("plot")>
        Public Function plot(<RRawVectorArgument> [object] As Object,
                             <RRawVectorArgument, RListObjectArgument> args As Object,
                             Optional env As Environment = Nothing) As Object

            Dim argumentsVal As Object = base.Rlist(args, env)

            If Program.isException(argumentsVal) Then
                Return argumentsVal
            Else
                Return DirectCast(argumentsVal, list).invokeGeneric([object], env)
            End If
        End Function
    End Module
End Namespace