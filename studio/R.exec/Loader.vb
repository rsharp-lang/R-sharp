#Region "Microsoft.VisualBasic::ce66cc50eee4369a0ae7f031b36e6288, studio\R.exec\Loader.vb"

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

    ' Module Loader
    ' 
    '     Function: Run
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' R# executable file loader
''' </summary>
''' 
<Package("R.executable.loader")>
Public Module Loader

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="base64">
    ''' base64 string
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("run")>
    Public Function Run(Optional base64$ = Nothing, Optional envir As Environment = Nothing) As Object
        Dim binary As Byte()

        If base64.StringEmpty Then
            binary = DirectCast(envir.last, String).Base64RawBytes
        Else
            binary = base64.Base64RawBytes
        End If

        Throw New NotImplementedException
    End Function
End Module
