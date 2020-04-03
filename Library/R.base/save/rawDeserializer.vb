Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

Module rawDeserializer

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Friend Function decodeStringVector(value As CDFData) As String()
        Return value.chars.DecodeBase64.LoadJSON(Of String())
    End Function

    Public Iterator Function loadObject(file As netCDFReader, symbols As String()) As IEnumerable(Of NamedValue(Of Object))
        For Each name As String In symbols
            Yield New NamedValue(Of Object) With {
                .Name = name,
                .Value = file.loadObject(name)
            }
        Next
    End Function

    <Extension>
    Public Function loadObject(file As netCDFReader, name As String) As Object
        Dim data = file.getDataVariable(name)
        Dim type As TypeCodes = Integer.Parse(file.getDataVariableEntry(name).FindAttribute("type").value)
        Dim value As Object

        Select Case type
            Case TypeCodes.boolean : value = file.loadElse(name, TypeCodes.boolean)
            Case TypeCodes.dataframe : value = file.loadDataframe(name)
            Case TypeCodes.double : value = file.loadElse(name, TypeCodes.double)
            Case TypeCodes.integer : value = file.loadElse(name, TypeCodes.integer)
            Case TypeCodes.list : value = file.loadList(name)
            Case TypeCodes.string : value = file.loadStrings(name)
            Case Else
                Throw New InvalidProgramException
        End Select

        Return value
    End Function

    <Extension>
    Public Function loadList(file As netCDFReader, symbolRef As String) As Object
        Dim names As String() = file.loadStrings($"{symbolRef}\names")
        Dim list As New list With {.slots = New Dictionary(Of String, Object)}
        Dim slotRef As String
        Dim value As Object

        For Each name As String In names
            slotRef = $"{symbolRef}\slots\{name}"
            value = file.loadObject(slotRef)
            list.slots.Add(name, value)
        Next

        Return list
    End Function

    <Extension>
    Public Function loadDataframe(file As netCDFReader, symbolRef As String) As Object
        Dim data As String() = file.getDataVariable($"{symbolRef}\colnames").decodeStringVector
        Dim rownames As String() = file.getDataVariable($"{symbolRef}\rownames").decodeStringVector
        Dim table As New Rdataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = rownames
        }
        Dim colKey As String
        Dim colVec As Array

        For Each colname As String In data
            colKey = $"{symbolRef}\slots\{colname}"
            colVec = file.loadObject(colKey)
            table.columns.Add(colname, colVec)
        Next

        Return table
    End Function

    <Extension>
    Public Function loadStrings(file As netCDFReader, symbolRef As String) As Object
        Dim data As CDFData = file.getDataVariable(symbolRef)
        Dim strings As String() = data.decodeStringVector

        Return strings
    End Function

    <Extension>
    Public Function loadElse(file As netCDFReader, symbolRef As String, type As TypeCodes) As Object
        Dim data As CDFData = file.getDataVariable(symbolRef)
        Dim value As Object

        Select Case type
            Case TypeCodes.double
                value = REnv.asVector(Of Double)(data.genericValue)
            Case TypeCodes.integer
                value = REnv.asVector(Of Long)(data.genericValue)
            Case TypeCodes.boolean
                value = REnv.asVector(Of Boolean)(data.genericValue)
            Case Else
                Throw New InvalidCastException
        End Select

        Return value
    End Function
End Module
