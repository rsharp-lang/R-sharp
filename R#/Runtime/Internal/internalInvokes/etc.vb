Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module etc

        ''' <summary>
        ''' # The R# License Terms
        ''' 
        ''' The license terms under which R# is distributed.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("license")>
        Public Function license() As <RSuppressPrint> Object
            Call Console.WriteLine(Rsharp.LICENSE.GPL3)
            Return Nothing
        End Function

        ''' <summary>
        ''' # ``R#`` Project Contributors
        ''' 
        ''' The R# Who-is-who, describing who made significant contributions to the development of R#.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("contributors")>
        Public Function contributors() As <RSuppressPrint> Object
            Call Console.WriteLine(My.Resources.contributions)
            Return Nothing
        End Function
    End Module
End Namespace