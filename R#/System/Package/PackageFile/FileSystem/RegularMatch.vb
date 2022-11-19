#Region "Microsoft.VisualBasic::9ba827881d2a845843018044c3c1be44, R-sharp\R#\System\Package\PackageFile\FileSystem\RegularMatch.vb"

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

    '   Total Lines: 29
    '    Code Lines: 17
    ' Comment Lines: 6
    '   Blank Lines: 6
    '     File Size: 892 B


    '     Class RegularMatch
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: isMatch, Norm, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Development.Package.File

    Public Class RegularMatch : Implements IFilePredicate

        ReadOnly filepath As String

        ''' <summary>
        ''' normal file path matched
        ''' </summary>
        ''' <param name="pattern">
        ''' a relative file path
        ''' </param>
        Sub New(pattern As String)
            filepath = Norm(pattern)
        End Sub

        Private Shared Function Norm(relpath As String) As String
            Return relpath.Replace("\", "/").StringReplace("[/]{2,}", "/").Trim("/"c, "."c)
        End Function

        Public Function isMatch(relpath As String) As Boolean Implements IFilePredicate.isMatch
            Return Norm(relpath).StartsWith(filepath)
        End Function

        Public Overrides Function ToString() As String
            Return $"(file_path) {filepath}"
        End Function
    End Class
End Namespace
