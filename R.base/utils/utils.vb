#Region "Microsoft.VisualBasic::7cc1d6b53f552f8d8f80b5c47e726edf, R.base\utils\utils.vb"

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

' Module utils
' 
'     Function: writecsv
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.dataframe

<Package("utils", Category:=APICategories.UtilityTools, Description:="")>
Public Module utils

    <ExportAPI("write.csv")>
    Public Function writecsv(data As Object, file$, envir As Environment) As Object
        If data Is Nothing Then
            Return Internal.stop("Empty dataframe object!", envir)
        End If

        Dim type As Type = data.GetType

        If type Is GetType(Rdataframe) Then
            Dim matrix As String()() = data.GetTable
            Dim dataframe As New File(matrix.Select(Function(r) New RowObject(r)))

            Return dataframe.Save(path:=file)
        ElseIf type Is GetType(File) Then
            Return DirectCast(data, File).Save(path:=file)
        ElseIf type Is GetType(DataFrame) Then
            Return DirectCast(data, DataFrame).Save(path:=file)
        ElseIf type Is GetType(EntityObject()) OrElse type.ImplementInterface(GetType(IEnumerable(Of EntityObject))) Then
            Return DirectCast(data, IEnumerable(Of EntityObject)).SaveTo(path:=file)
        ElseIf type Is GetType(DataSet()) OrElse type.ImplementInterface(GetType(IEnumerable(Of DataSet))) Then
            Return DirectCast(data, IEnumerable(Of DataSet)).SaveTo(path:=file)
        Else
            Return Message.InCompatibleType(GetType(File), type, envir)
        End If
    End Function
End Module

