#Region "Microsoft.VisualBasic::ffd1b7eeed55cf9b9e34fb953af90641, R-sharp\studio\Rstudio.common\Workdir.vb"

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

    '   Total Lines: 25
    '    Code Lines: 10
    ' Comment Lines: 11
    '   Blank Lines: 4
    '     File Size: 622.00 B


    ' Module Workdir
    ' 
    '     Function: TranslateWorkdir
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

''' <summary>
''' config current workspace directory
''' </summary>
Module Workdir

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <param name="Rscript">
    ''' the absolute full path of the target rscript file
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function TranslateWorkdir(dir As String, Rscript As String) As String
        If dir.ToLower = ("@dir") Then
            Return Rscript.ParentPath
        End If

        Return dir.GetDirectoryFullPath
    End Function

End Module
