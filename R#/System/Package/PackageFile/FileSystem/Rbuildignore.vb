#Region "Microsoft.VisualBasic::dcbdedab7efc30b39f09cfd6877413c5, R-sharp\R#\System\Package\PackageFile\FileSystem\Rbuildignore.vb"

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

    '   Total Lines: 69
    '    Code Lines: 36
    ' Comment Lines: 22
    '   Blank Lines: 11
    '     File Size: 2.31 KB


    '     Interface IFilePredicate
    ' 
    '         Function: isMatch
    ' 
    '     Class Rbuildignore
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreatePatterns, IsFileIgnored
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace Development.Package.File

    Friend Interface IFilePredicate
        Function isMatch(relpath As String) As Boolean
    End Interface

    ''' <summary>
    ''' file handler of ``.Rbuildignore``
    ''' </summary>
    Public Class Rbuildignore

        ReadOnly patterns As IFilePredicate()

        Private Sub New(patterns As IEnumerable(Of IFilePredicate))
            Me.patterns = patterns.ToArray
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="relpath">
        ''' a relative path to the directory of DESCRIPTION meta data file.
        ''' </param>
        ''' <returns></returns>
        Public Function IsFileIgnored(relpath As String) As Boolean
            ' test on all pattern match
            For Each pattern As IFilePredicate In patterns
                If pattern.isMatch(relpath) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' create a file name matches pattern
        ''' 
        ''' 1. a regular expression pattern should start with symbol ``^`` and ends with the symbol ``$``
        ''' 2. a relative file path pattern just required a relative file path to a specific file
        ''' 3. comment line start with the ``#`` symbol
        ''' </summary>
        ''' <param name="file">
        ''' a text file its file path or the content data text itself
        ''' </param>
        ''' <returns></returns>
        Public Shared Function CreatePatterns(file As String) As Rbuildignore
            Dim list As String() = file _
                .LineIterators _
                .Where(Function(line) Not line.StringEmpty) _
                .Where(Function(line) Not line.StartsWith("#")) _
                .ToArray
            Dim patterns As New List(Of IFilePredicate)

            For Each line As String In list
                If line.StartsWith("^") AndAlso line.EndsWith("$") Then
                    Call patterns.Add(New PatternMatch(line))
                Else
                    Call patterns.Add(New RegularMatch(line))
                End If
            Next

            Return New Rbuildignore(patterns)
        End Function

    End Class
End Namespace
