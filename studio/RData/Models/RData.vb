#Region "Microsoft.VisualBasic::587ad7513b22c9f14c9b647058b78c0d, studio\RData\Models\RData.vb"

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

    '   Total Lines: 28
    '    Code Lines: 14 (50.00%)
    ' Comment Lines: 9 (32.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (17.86%)
    '     File Size: 897 B


    '     Class RData
    ' 
    '         Function: ParseFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList

Namespace Struct

    ''' <summary>
    ''' Data contained in a R file.
    ''' </summary>
    Public Class RData

        Public versions As RVersions
        Public extra As RExtraInfo
        Public [object] As RObject

        ''' <summary>
        ''' a wrapper function of <see cref="Reader.ParseData"/>.
        ''' </summary>
        ''' <param name="rdafile"></param>
        ''' <param name="expand_altrep"></param>
        ''' <returns></returns>
        Public Shared Function ParseFile(rdafile As String, Optional expand_altrep As Boolean = True) As RData
            Using buffer As Stream = rdafile.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return Reader.ParseData(buffer, expand_altrep)
            End Using
        End Function

    End Class
End Namespace
