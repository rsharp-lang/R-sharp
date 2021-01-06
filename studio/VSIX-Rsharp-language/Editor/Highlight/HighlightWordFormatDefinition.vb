#Region "Microsoft.VisualBasic::128b883e79477bbdb9f804e744505123, studio\VSIX-Rsharp-language\Editor\Highlight\HighlightWordFormatDefinition.vb"

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

    ' Class HighlightWordFormatDefinition
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel.Composition
Imports System.Windows.Media
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

<Export(GetType(EditorFormatDefinition))>
<Name(HighlightWordFormatDefinition.FormatName)>
<UserVisible(True)>
Public Class HighlightWordFormatDefinition : Inherits MarkerFormatDefinition

    Public Const FormatName As String = "MarkerFormatDefinition/HighlightWordFormatDefinition"

    Sub New()
        BackgroundColor = Colors.LightBlue
        ForegroundColor = Colors.DarkBlue
        DisplayName = "keyword"
        ZOrder = 5
    End Sub
End Class

