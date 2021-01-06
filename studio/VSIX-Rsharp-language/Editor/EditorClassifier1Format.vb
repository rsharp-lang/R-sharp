#Region "Microsoft.VisualBasic::c720a42a4bd33746884f3138d575a1cc, studio\VSIX-Rsharp-language\Editor\EditorClassifier1Format.vb"

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

    ' Class EditorClassifier1Format
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel.Composition
Imports System.Windows.Media
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

''' <summary>
''' Defines an editor format for our EditorClassifier1 type that has a purple background
''' and is underlined.
''' </summary>
<Export(GetType(EditorFormatDefinition))>
<ClassificationType(ClassificationTypeNames:="EditorClassifier1")>
<Name("EditorClassifier1")>
<UserVisible(True)>
<Order(After:=Priority.Default)>
Friend NotInheritable Class EditorClassifier1Format
    Inherits ClassificationFormatDefinition

    ''' <summary>
    ''' Initializes a new instance of the <see cref="EditorClassifier1Format"/> class.
    ''' </summary>
    Public Sub New()

        Me.DisplayName = "EditorClassifier1"
        Me.BackgroundColor = Colors.BlueViolet
        Me.TextDecorations = System.Windows.TextDecorations.Underline

    End Sub

End Class

