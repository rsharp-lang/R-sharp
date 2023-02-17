#Region "Microsoft.VisualBasic::a4bcf138c5573eaf6c7b48040858b036, D:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/FileSystem/PatternMatch.vb"

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

    '   Total Lines: 33
    '    Code Lines: 20
    ' Comment Lines: 7
    '   Blank Lines: 6
    '     File Size: 1.14 KB


    '     Class PatternMatch
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: isMatch, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports regexp = System.Text.RegularExpressions.Regex

Namespace Development.Package.File

    Public Class PatternMatch : Implements IFilePredicate

        ReadOnly r As regexp

        ''' <summary>
        ''' create a new regular expression pattern match object
        ''' </summary>
        ''' <param name="pattern">
        ''' a regular expression pattern
        ''' </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(pattern As String)
            r = New regexp(pattern, RegexOptions.Compiled Or RegexOptions.Multiline)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function isMatch(relpath As String) As Boolean Implements IFilePredicate.isMatch
            Return r.Match(relpath).Success
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"(regexp) /{r.ToString}/ig"
        End Function
    End Class
End Namespace
