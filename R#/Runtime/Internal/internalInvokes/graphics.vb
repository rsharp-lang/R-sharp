Imports Microsoft.VisualBasic.CommandLine.Reflection
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

            Return DirectCast(Rlist(args, env), list).invokeGeneric([object], env)
        End Function
    End Module
End Namespace