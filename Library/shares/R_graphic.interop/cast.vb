Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module cast

    ''' <summary>
    ''' cast the gdi size data to R# tuple list object
    ''' </summary>
    ''' <param name="size"></param>
    ''' <returns>
    ''' a tuple list obejct that contains two slot elements: w for width and h for height
    ''' </returns>
    <Extension>
    Public Function size_toList(size As Size) As list
        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"w", size.Width},
                {"h", size.Height}
            }
        }
    End Function

End Module
