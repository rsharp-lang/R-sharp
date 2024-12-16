#If DATAMINING_DATASET Then

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

Module DataMiningDataSet

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns>
    ''' returns the column name of the the class id, nothing will be returns if the class label is not presented inside the data fields.
    ''' </returns>
    <Extension>
    Private Function CheckClassLabel(df As Rdataframe) As String
        Dim class_col As String = Nothing

        For Each col As String In {"class", "cluster", "group", "type", "Class", "Cluster", "Group", "Type"}
            If df.checkClassLabels(col) Then
                class_col = col
                Exit For
            End If
        Next

        Return class_col
    End Function

    Public Function getRawMatrix(x As Object, check_class As Boolean, env As Environment) As [Variant](Of Message, IEnumerable(Of Double()))
        Dim fetch As IEnumerable(Of Double())

        If x Is Nothing Then
            Return RInternal.debug.stop("the given dataset should not be nothing!", env)
        End If

        If x.GetType.IsArray Then
#Disable Warning
            Select Case REnv.MeasureArrayElementType(x)
                Case GetType(DataSet)
                    fetch = DirectCast(REnv.asVector(Of DataSet)(x), DataSet()).populateRows
                Case GetType(EntityClusterModel)
                    fetch = DirectCast(REnv.asVector(Of EntityClusterModel)(x), EntityClusterModel()).populateRows
                Case Else
                    Return REnv.Internal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
            End Select
#Enable Warning
        ElseIf TypeOf x Is Rdataframe Then
            fetch = DirectCast(x, Rdataframe).populateRows(check_class)
        Else
            Return RInternal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
        End If

        Return New [Variant](Of Message, IEnumerable(Of Double()))(fetch)
    End Function

    <Extension>
    Private Iterator Function populateRows(df As Rdataframe, check_class As Boolean) As IEnumerable(Of Double())
        Dim class_col As String = If(check_class, df.CheckClassLabel, Nothing)

        If Not class_col.StringEmpty Then
            df = New Rdataframe With {
                .columns = New Dictionary(Of String, Array)(df.columns),
                .rownames = df.rownames
            }
            df.columns.Remove(class_col)
        End If

        ' col names maybe has been updated
        Dim colNames As String() = df.columns.Keys.ToArray

        For Each r As NamedCollection(Of Object) In df.forEachRow(colNames)
            Yield CLRVector.asNumeric(r.value)
        Next
    End Function

    <Extension>
    Private Iterator Function populateRows(x As DataSet()) As IEnumerable(Of Double())
        Dim cols As String() = x.PropertyNames

        For Each xi As DataSet In x
            Yield xi(cols)
        Next
    End Function

    <Extension>
    Private Iterator Function populateRows(x As EntityClusterModel()) As IEnumerable(Of Double())
        Dim cols As String() = x.PropertyNames

        For Each xi As EntityClusterModel In x
            Yield xi(cols)
        Next
    End Function

    Public Function getDataModel(x As Object, check_class As Boolean, env As Environment) As [Variant](Of Message, EntityClusterModel())
        Dim model As EntityClusterModel()

        If x Is Nothing Then
            Return RInternal.debug.stop("the given dataset should not be nothing!", env)
        End If

        If x.GetType.IsArray Then
#Disable Warning
            Select Case REnv.MeasureArrayElementType(x)
                Case GetType(DataSet)
                    model = DirectCast(REnv.asVector(Of DataSet)(x), DataSet()).ToKMeansModels
                Case GetType(EntityClusterModel)
                    model = REnv.asVector(Of EntityClusterModel)(x)
                Case Else
                    Return REnv.Internal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
            End Select
#Enable Warning
        ElseIf TypeOf x Is Rdataframe Then
            model = DirectCast(x, Rdataframe).CastDataFrame(check_class).ToArray
        Else
            Return RInternal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
        End If

        Return model
    End Function

    <Extension>
    Private Iterator Function CastDataFrame(df As Rdataframe, check_class As Boolean) As IEnumerable(Of EntityClusterModel)
        Dim class_labels As String() = Nothing
        Dim class_col As String = If(check_class, df.CheckClassLabel, Nothing)

        If Not class_col.StringEmpty Then
            class_labels = CLRVector.asCharacter(df(class_col))
            df = New Rdataframe With {
                .columns = New Dictionary(Of String, Array)(df.columns),
                .rownames = df.rownames
            }
            df.columns.Remove(class_col)
        End If

        ' col names maybe has been updated
        Dim colNames As String() = df.columns.Keys.ToArray
        Dim index As Integer = 0

        For Each r As NamedCollection(Of Object) In df.forEachRow
            Yield New EntityClusterModel With {
                .ID = r.name,
                .Properties = colNames _
                    .SeqIterator _
                    .ToDictionary(Function(c) c.value,
                                    Function(i)
                                        Return CType(r(i), Double)
                                    End Function),
                .Cluster = class_labels.ElementAtOrNull(index)
            }

            index += 1
        Next
    End Function

    <Extension>
    Private Function checkClassLabels(df As Rdataframe, col As String) As Boolean
        Return df.hasName(col) AndAlso CLRVector _
            .asCharacter(df(col)) _
            .All(Function(str)
                     Return str.IsPattern("((class)|(cluster)|(group)).*\d+")
                 End Function)
    End Function
End Module
#End If