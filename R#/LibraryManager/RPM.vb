#Region "Microsoft.VisualBasic::8cde9e958b3a9b42f8c6938aed1b83a7, R#\LibraryManager\RPM.vb"

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

    '     Class RPM
    ' 
    '         Function: [Imports], LibraryLoad
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Library

    ''' <summary>
    ''' ``R#`` package manager
    ''' </summary>
    Public Class RPM

        ''' <summary>
        ''' Imports dotnet namespace and module API.
        ''' </summary>
        ''' <param name="namespace$"></param>
        ''' <returns></returns>
        Public Function [Imports](namespace$) As Dictionary(Of String, String())

        End Function

        ''' <summary>
        ''' Load R# package
        ''' </summary>
        ''' <param name="package$"></param>
        ''' <returns></returns>
        Public Function LibraryLoad(package$) As String()

        End Function
    End Class
End Namespace
