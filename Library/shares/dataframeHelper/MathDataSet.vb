#If MATH_DATASET Then

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.DataFrame
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports FeatureFrame = Microsoft.VisualBasic.Math.DataFrame.DataFrame
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

Module MathDataSet

    ''' <summary>
    ''' cast the scibasic.net dataframe object to R# dataframe object
    ''' </summary>
    ''' <param name="features"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Friend Function toDataframe(features As FeatureFrame, args As list, env As Environment) As Rdataframe
        Return New Rdataframe With {
            .columns = features.features _
                .ToDictionary(Function(v) v.Key,
                              Function(v)
                                  Return v.Value.vector
                              End Function),
            .rownames = features.rownames
        }
    End Function

    ''' <summary>
    ''' cast the R# dataframe object to <see cref="FeatureFrame"/> type object in scibasic.net framework.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function toFeatureSet(x As Rdataframe, Optional env As Environment = Nothing) As Object
        Dim featureSet As New Dictionary(Of String, FeatureVector)
        Dim general As Array

        For Each name As String In x.columns.Keys
            general = x(columnName:=name)
            general = TryCastGenericArray(general, env)

            If Not FeatureVector.CheckSupports(general.GetType.GetElementType) Then
                Return Internal.debug.stop($"Not supports '{name}'!", env)
            End If

            featureSet(name) = FeatureVector.FromGeneral(name, general)
        Next

        Return New FeatureFrame With {
            .rownames = x.getRowNames,
            .features = featureSet
        }
    End Function
End Module
#End If


