#Region "Microsoft.VisualBasic::9f6fe74df587f1ea3d7400ba817104af, R-sharp\Library\R.base\save\rawDeserializer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


     Code Statistics:

        Total Lines:   111
        Code Lines:    94
        Comment Lines: 0
        Blank Lines:   17
        File Size:     4.32 KB


    ' Module rawDeserializer
    ' 
    '     Function: decodeStringVector, loadDataframe, loadElse, loadList, (+2 Overloads) loadObject
    '               loadStrings
    ' 
    ' /********************************************************************************/

#End Region

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
    Friend Function decodeStringVector(value As ICDFDataVector) As String()
        Return DirectCast(value, chars).CharString.DecodeBase64.LoadJSON(Of String())
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
        Dim data As ICDFDataVector = file.getDataVariable(symbolRef)
        Dim strings As String() = data.decodeStringVector

        Return strings
    End Function

    <Extension>
    Public Function loadElse(file As netCDFReader, symbolRef As String, type As TypeCodes) As Object
        Dim data As ICDFDataVector = file.getDataVariable(symbolRef)
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
