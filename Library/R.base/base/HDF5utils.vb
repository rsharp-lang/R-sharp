#Region "Microsoft.VisualBasic::47440870ae73bedb3925d46b6b41fe68, Library\R.base\base\HDF5utils.vb"

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

    ' Module HDF5utils
    ' 
    '     Function: openHDF5
    ' 
    ' /********************************************************************************/

#End Region

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
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(String), file.GetType, env), env)
        End If
    End Function
End Module
