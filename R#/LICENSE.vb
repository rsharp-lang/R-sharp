﻿#Region "Microsoft.VisualBasic::e45e7e5b2b62603a56189142806c3de8, R#\LICENSE.vb"

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

    '   Total Lines: 45
    '    Code Lines: 16 (35.56%)
    ' Comment Lines: 24 (53.33%)
    '    - Xml Docs: 58.33%
    ' 
    '   Blank Lines: 5 (11.11%)
    '     File Size: 1.54 KB


    ' Module LICENSE
    ' 
    '     Properties: GPL3
    ' 
    '     Sub: GithubRepository
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics.CodeAnalysis
Imports System.Runtime.CompilerServices

' ignores of the gdi+ warning message
<Assembly: SuppressMessage("", "CA1416")>

''' <summary>
''' Information about the ``R#`` interpreter code module.
''' </summary>
Public Module LICENSE

    '''<summary>
    '''  Looks up a localized string similar to                     GNU GENERAL PUBLIC LICENSE
    '''                       Version 3, 29 June 2007
    '''
    ''' Copyright (C) 2007 Free Software Foundation, Inc. &lt;http://fsf.org/&gt;
    ''' Everyone is permitted to copy and distribute verbatim copies
    ''' of this license document, but changing it is not allowed.
    '''
    '''                            Preamble
    '''
    '''  The GNU General Public License is a free, copyleft license for
    '''software and other kinds of works.
    '''
    '''  The licenses for most software and other practical works are designed
    '''to take away yo [rest of string was truncated]&quot;;.
    '''</summary>
    Public ReadOnly Property GPL3 As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return My.Resources.gpl
        End Get
    End Property

    Public Const githubURL$ = "https://github.com/SMRUCC/R-sharp"

    ''' <summary>
    ''' https://github.com/SMRUCC/R-sharp
    ''' </summary>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub GithubRepository()
        Call Process.Start(LICENSE.githubURL)
    End Sub
End Module
