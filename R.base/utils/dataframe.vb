Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' The sciBASIC.NET dataframe api
''' </summary>
<Package("dataframe", Category:=APICategories.UtilityTools)>
Module dataframe

    <ExportAPI("dataset.colnames")>
    Public Function colnames(dataset As Array, envir As Environment) As Object
        Dim baseElement As Type = dataset.GetValue(Scan0).GetType

        If baseElement Is GetType(EntityObject) Then
            Return dataset.AsObjectEnumerator _
                .Select(Function(d)
                            Return DirectCast(d, EntityObject)
                        End Function) _
                .PropertyNames
        ElseIf baseElement Is GetType(DataSet) Then
            Return dataset.AsObjectEnumerator _
                .Select(Function(d)
                            Return DirectCast(d, DataSet)
                        End Function) _
                .PropertyNames
        Else
            Return Internal.debug.stop(New InvalidProgramException, envir)
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <param name="cols"></param>
    ''' <returns></returns>
    <ExportAPI("dataset.project")>
    Public Function project(dataset As Array, cols As String(), envir As Environment) As Object
        Dim baseElement As Type = dataset.GetValue(Scan0).GetType

        If baseElement Is GetType(EntityObject) Then
            Return dataset.AsObjectEnumerator _
                .Select(Function(d)
                            Dim row As EntityObject = DirectCast(d, EntityObject)
                            Dim subset As New EntityObject With {
                                .ID = row.ID,
                                .Properties = row.Properties.Subset(cols)
                            }

                            Return subset
                        End Function) _
                .ToArray
        ElseIf baseElement Is GetType(DataSet) Then
            Return dataset.AsObjectEnumerator _
                .Select(Function(d)
                            Dim row As DataSet = DirectCast(d, DataSet)
                            Dim subset As New DataSet With {
                                .ID = row.ID,
                                .Properties = row.Properties.Subset(cols)
                            }

                            Return subset
                        End Function) _
                .ToArray
        Else
            Return Internal.debug.stop(New InvalidProgramException, envir)
        End If
    End Function

    <ExportAPI("read.dataframe")>
    Public Function readDataSet(file$, Optional mode$ = "numeric|character") As Object
        Dim readMode = mode.Split("|"c).First

        Select Case readMode.ToLower
            Case "numeric"
                Return DataSet.LoadDataSet(file).ToArray
            Case "character"
                Return EntityObject.LoadDataSet(file).ToArray
            Case Else
                Return utils.read_csv(file)
        End Select
    End Function
End Module
