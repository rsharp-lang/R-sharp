Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop.CType

Namespace Development.Components

    ''' <summary>
    ''' ### Numerical Characteristics of the Machine
    ''' 
    ''' .Machine is a variable holding information on the numerical 
    ''' characteristics of the machine R is running on, such as the 
    ''' largest double or integer and the machine's precision.
    ''' </summary>
    ''' <remarks>
    ''' The algorithm is based on Cody's (1988) subroutine MACHAR. 
    ''' As all current implementations of R use 32-bit integers and 
    ''' use IEC 60559 floating-point (double precision) arithmetic, 
    ''' the "integer" and "double" related values are the same for 
    ''' almost all R builds.
    '''
    ''' Note that On most platforms smaller positive values than 
    ''' .Machine$Double.xmin can occur. On a typical R platform the 
    ''' smallest positive Double Is about 5E-324.
    ''' </remarks>
    Public Class DotMachine : Implements ICTypeList

        ''' <summary>
        ''' the largest integer which can be represented. Always 2^{31} - 1 = 21474836472
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property integer_max As Integer = Integer.MaxValue

        Public Function toList() As list Implements ICTypeList.toList
            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"integer.max", integer_max}
                }
            }
        End Function
    End Class
End Namespace