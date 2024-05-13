#Region "Microsoft.VisualBasic::d9f943472a95d353edba96819f6dfea6, R#\System\Document\CodeAnalysis\@Export\FunctionTree.vb"

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

    '   Total Lines: 49
    '    Code Lines: 32
    ' Comment Lines: 7
    '   Blank Lines: 10
    '     File Size: 1.47 KB


    '     Class FunctionTree
    ' 
    '         Properties: FunctionTrace, MySelf, Symbol1
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetNode, SortName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree

Namespace Development.CodeAnalysis

    Public Class FunctionTree : Inherits TreeNodeBase(Of FunctionTree)

        Public Overrides ReadOnly Property MySelf As FunctionTree
            Get
                Return Me
            End Get
        End Property

        Public Property Symbol1 As SymbolTypeDefine
        Public Property FunctionTrace As New List(Of String)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' the return value always not null
        ''' </returns>
        Public Function GetNode(name As String) As FunctionTree
            Dim find = ChildNodes.Where(Function(t) t.Name = name).FirstOrDefault

            If find Is Nothing Then
                find = New FunctionTree(name)
                AddChild(find)
            End If

            Return find
        End Function

        Public Function SortName() As FunctionTree
            ChildNodes = ChildNodes _
                .OrderBy(Function(a) a.Name) _
                .Select(Function(a) a.SortName) _
                .AsList

            Return Me
        End Function
    End Class
End Namespace
