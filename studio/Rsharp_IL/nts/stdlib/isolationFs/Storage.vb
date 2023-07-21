#Region "Microsoft.VisualBasic::d8d74c4bc930aca975b99ed131a85b84, D:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//stdlib/isolationFs/Storage.vb"

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


    ' Code Statistics:

    '   Total Lines: 54
    '    Code Lines: 40
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 1.70 KB


    '     Class Storage
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: clear, getItem, getStoragePath, removeItem, setItem
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd.isolationFs

    Public Class Storage

        ReadOnly fs As String

        Sub New(dir As String)
            fs = dir.GetDirectoryFullPath
        End Sub

        Public Overrides Function ToString() As String
            Return fs
        End Function

        Private Function getStoragePath(key As String) As String
            Return $"{fs}/{key.NormalizePathString(False)}_{key.MD5}.json"
        End Function

        Public Function clear() As Object
            For Each file As String In fs.ListFiles("*.json")
                Call file.DeleteFile
            Next

            Return True
        End Function

        Public Function removeItem(keyname As String) As Object
            Return getStoragePath(keyname).DeleteFile
        End Function

        <ExportAPI("setItem")>
        Public Function setItem(key As String, <RRawVectorArgument> value As Object,
                                Optional env As Environment = Nothing) As Object

            Dim fileName As String = getStoragePath(key)
            Dim json As String = jsonlite.toJSON(value, env)

            Return json.SaveTo(fileName)
        End Function

        <ExportAPI("getItem")>
        Public Function getItem(key As String, Optional env As Environment = Nothing) As Object
            Dim fileName As String = getStoragePath(key)
            Dim json As String = fileName.ReadAllText
            Dim obj = jsstd.JSON.parse(json, env)

            Return obj
        End Function
    End Class
End Namespace
