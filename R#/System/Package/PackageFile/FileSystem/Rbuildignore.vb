#Region "Microsoft.VisualBasic::7c8e52ecd6069e769f1b43a2f8a74a43, R-sharp\R#\System\Package\PackageFile\FileSystem\Rbuildignore.vb"

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

    '   Total Lines: 60
    '    Code Lines: 32
    ' Comment Lines: 18
    '   Blank Lines: 10
    '     File Size: 1.92 KB


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

    ''' <summary>
    ''' file handler of ``.Rbuildignore``
    ''' </summary>
    Public Class Rbuildignore

        ReadOnly patterns As Predicate(Of String)()

        Sub New(patterns As IEnumerable(Of Predicate(Of String)))
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
            For Each pattern As Predicate(Of String) In patterns
                If pattern(relpath) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="file">
        ''' a text file its file path or the content data text itself
        ''' </param>
        ''' <returns></returns>
        Public Shared Function CreatePatterns(file As String) As Rbuildignore
            Dim list As String() = file _
                .LineIterators _
                .Where(Function(line) Not line.StringEmpty) _
                .ToArray
            Dim patterns As New List(Of Predicate(Of String))

            For Each line As String In list
                If line.StartsWith("^") AndAlso line.EndsWith("$") Then
                    patterns.Add(AddressOf New PatternMatch(line).isMatch)
                Else
                    patterns.Add(AddressOf New RegularMatch(line).isMatch)
                End If
            Next

            Return New Rbuildignore(patterns)
        End Function

    End Class
End Namespace
