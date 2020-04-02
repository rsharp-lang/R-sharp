Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components

Module rawDeserializer

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Friend Function decodeStringVector(value As CDFData) As String()
        Return value.chars.DecodeBase64.LoadJSON(Of String())
    End Function

    Public Iterator Function loadObject(file As netCDFReader, symbols As String()) As IEnumerable(Of NamedValue(Of Object))
        For Each name As String In symbols
            Dim data = file.getDataVariable(name)
            Dim type As TypeCodes = Integer.Parse(file.getDataVariableEntry(name).FindAttribute("type").value)
            Dim value As Object

            Select Case type
                Case TypeCodes.boolean : value = loadElse()
                Case TypeCodes.dataframe : value = loadDataframe()
                Case TypeCodes.double : value = loadElse()
                Case TypeCodes.integer : value = loadElse()
                Case TypeCodes.list : value = loadList()
                Case TypeCodes.string : value = loadStrings()
                Case Else
                    Throw New InvalidProgramException
            End Select
        Next
    End Function

    Public Function loadList() As Object

    End Function

    Public Function loadDataframe() As Object

    End Function

    Public Function loadStrings() As Object

    End Function

    Public Function loadElse() As Object

    End Function
End Module
