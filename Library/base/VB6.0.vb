Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' Microsoft VisualBasic 6.0 primitive functions
''' </summary>
<Package("Microsoft.VisualBasic")>
Module VB6

    ''' <summary>
    ''' Returns the numbers contained in a string as a numeric value of appropriate type.
    ''' </summary>
    ''' <param name="x">
    ''' Required. Any valid String expression, Object variable, or Char value. If Expression
    ''' is of type Object, its value must be convertible to String or an System.ArgumentException
    ''' error occurs.
    ''' </param>
    ''' <returns>
    ''' The numbers contained in a string as a numeric value of appropriate type.
    ''' </returns>
    <ExportAPI("Val")>
    Public Function conversion_val(<RRawVectorArgument> x As Object) As Double()
        Return CLRVector.asCharacter(x) _
            .SafeQuery _
            .Select(Function(str) Conversion.Val(str)) _
            .ToArray
    End Function

End Module
