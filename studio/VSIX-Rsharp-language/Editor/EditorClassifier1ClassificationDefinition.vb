#Region "Microsoft.VisualBasic::e6d132d22e097ef1d88cc6e991bb5c48, studio\VSIX-Rsharp-language\Editor\EditorClassifier1ClassificationDefinition.vb"

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

    ' Module EditorClassifier1ClassificationDefinition
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

''' <summary>
''' Classification type definition export for EditorClassifier1
''' </summary>
Friend Module EditorClassifier1ClassificationDefinition

    ''' <summary>
    ''' Defines the "EditorClassifier1" classification type.
    ''' </summary>
    <Export(GetType(ClassificationTypeDefinition))>
    <Name("EditorClassifier1")>
    Private EditorClassifier1Type As ClassificationTypeDefinition

End Module

