#Region "Microsoft.VisualBasic::1d357e8c99819962e2bdcf76fd3bf9ef, R-sharp\Library\base\base\HDF5utils.vb"

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

    '   Total Lines: 32
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 1.25 KB


    ' Module HDF5utils
    ' 
    '     Function: getData, openHDF5
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.HDF5
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

<Package("HDF5.utils")>
Module HDF5utils

    <ExportAPI("open.hdf5")>
    Public Function openHDF5(file As Object, Optional env As Environment = Nothing) As Object
        If file Is Nothing Then
            Return Internal.debug.stop("file for open can not be nothing!", env)
        ElseIf TypeOf file Is String Then
            If DirectCast(file, String).FileExists Then
                Return New HDF5File(DirectCast(file, String))
            Else
                Return Internal.debug.stop("the given file is not found on your filesystem!", env)
            End If
        ElseIf TypeOf file Is Stream Then
            Return New HDF5File(DirectCast(file, Stream))
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(String), file.GetType, env), env)
        End If
    End Function

    <ExportAPI("getData")>
    Public Function getData(hdf5 As hdf5file, path As String) As Object
        Return hdf5(path).data
    End Function
End Module
