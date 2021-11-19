Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("Matrix")>
Module RMatrix

    ''' <summary>
    ''' ## 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="nrow"></param>
    ''' <param name="ncol"></param>
    ''' <param name="byrow"></param>
    ''' <param name="dimnames"></param>
    ''' <param name="sparse"></param>
    ''' <returns></returns>
    <ExportAPI("Matrix")>
    Public Function Matrix(<RRawVectorArgument>
                           Optional data As Object = Nothing,
                           Optional nrow As Integer = 1,
                           Optional ncol As Integer = 1,
                           Optional byrow As Boolean = False,
                           Optional dimnames As String() = Nothing,
                           Optional sparse As Boolean = False) As GeneralMatrix

        If TypeOf data Is vector Then
            data = DirectCast(data, vector).data
        End If

        If TypeOf data Is Double() Then
            If byrow Then
            Else

            End If
        End If

        If sparse Then
        Else

        End If
    End Function
End Module
