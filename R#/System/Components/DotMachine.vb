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
        ''' the smallest positive floating-point number x such that 1 + x != 1. 
        ''' It equals double.base ^ ulp.digits if either double.base is 2 or 
        ''' double.rounding is 0; otherwise, it is (double.base ^ double.ulp.digits) / 2. 
        ''' Normally 2.220446e-16.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property double_eps As Double = Double.Epsilon
        ''' <summary>
        ''' a small positive floating-point number x such that 1 - x != 1. It equals 
        ''' double.base ^ double.neg.ulp.digits if double.base is 2 or double.rounding 
        ''' is 0; otherwise, it is (double.base ^ double.neg.ulp.digits) / 2. Normally
        ''' 1.110223e-16. As double.neg.ulp.digits is bounded below by -(double.digits + 3),
        ''' double.neg.eps may not be the smallest number that can alter 1 by subtraction.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property double_neg_eps As Double = 0.0000000000000001110223

        Public ReadOnly Property double_xmin As Double = Double.MinValue
        Public ReadOnly Property double_xmax As Double = Double.MaxValue

        ''' <summary>
        ''' the largest integer which can be represented. Always 2^{31} - 1 = 21474836472
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property integer_max As Integer = Integer.MaxValue
        Public ReadOnly Property uinteger_max As Long = UInteger.MaxValue

        Public Function toList() As list Implements ICTypeList.toList
            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"integer.max", integer_max},
                    {"uinteger.max", uinteger_max}
                }
            }
        End Function
    End Class
End Namespace