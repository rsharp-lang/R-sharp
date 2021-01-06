#Region "Microsoft.VisualBasic::52f4b9a008a44f36b1c0792684ef61fa, studio\VSIX-Rsharp-language\Editor\EditorClassifier1Provider.vb"

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

    ' Class EditorClassifier1Provider
    ' 
    '     Function: GetClassifier
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

''' <summary>
''' Classifier provider. It adds the classifier to the set of classifiers.
''' </summary>
<Export(GetType(IClassifierProvider))>
<ContentType("text")> '' This classifier applies to all text files.
Friend NotInheritable Class EditorClassifier1Provider
    Implements IClassifierProvider

    ''' <summary>
    ''' Import the classification registry to be used for getting a reference
    ''' to the custom classification type later.
    ''' </summary>
    <Import()>
    Private classificationRegistry As IClassificationTypeRegistryService

    ''' <summary>
    ''' Gets a classifier for the given text buffer.
    ''' </summary>
    ''' <param name="buffer">The <see cref="ITextBuffer"/> to classify.</param>
    ''' <returns>A classifier for the text buffer, Or null if the provider cannot do so in its current state.</returns>
    Public Function GetClassifier(ByVal buffer As ITextBuffer) As IClassifier Implements IClassifierProvider.GetClassifier

        Return buffer.Properties.GetOrCreateSingletonProperty(Of EditorClassifier1)(Function() New EditorClassifier1(Me.classificationRegistry))

    End Function

End Class

