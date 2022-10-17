#Region "Microsoft.VisualBasic::1ba2fe33cfa9fc74dc54bb3cad3eaea8, R-sharp\studio\Rsharp_kit\roxygenNet\Rd\RDoc\ContentModel.vb"

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

    '   Total Lines: 57
    '    Code Lines: 39
    ' Comment Lines: 0
    '   Blank Lines: 18
    '     File Size: 1.21 KB


    ' Class Doc
    ' 
    '     Properties: Fragments, PlainText
    ' 
    '     Function: GetHtml, GetMarkdown, ToString
    ' 
    ' Class DocFragment
    ' 
    ' 
    ' 
    ' Class PlainText
    ' 
    '     Properties: text
    ' 
    '     Function: ToString
    ' 
    ' Class Code
    ' 
    '     Properties: content
    ' 
    '     Function: ToString
    ' 
    ' Class Link
    ' 
    '     Properties: target
    ' 
    '     Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Doc content model"

Public Class Doc

    Public Property Fragments As DocFragment()

    Public ReadOnly Property PlainText As String
        Get
            Return ToString()
        End Get
    End Property

    Public Function GetMarkdown() As String
        Throw New NotImplementedException
    End Function

    Public Function GetHtml() As String
        Throw New NotImplementedException
    End Function

    Public Overrides Function ToString() As String
        Return Fragments.Select(Function(frag) frag.ToString).JoinBy(" ")
    End Function
End Class

Public MustInherit Class DocFragment

End Class

Public Class PlainText : Inherits DocFragment

    Public Property text As String

    Public Overrides Function ToString() As String
        Return text
    End Function
End Class

Public Class Code : Inherits DocFragment

    Public Property content As DocFragment

    Public Overrides Function ToString() As String
        Return content.ToString
    End Function
End Class

Public Class Link : Inherits DocFragment

    Public Property target As String

    Public Overrides Function ToString() As String
        Return target.ToString
    End Function
End Class

#End Region
