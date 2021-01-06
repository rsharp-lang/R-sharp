#Region "Microsoft.VisualBasic::948601cce8d47028104b899fe5884191, studio\VSIX-Rsharp-language\Editor\EditorClassifier1.vb"

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

    ' Class EditorClassifier1
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetClassificationSpans
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification

''' <summary>
''' Classifier that classifies all text as an instance of the "EditorClassifier1" classification type.
''' </summary>
Friend NotInheritable Class EditorClassifier1
    Implements IClassifier

    ''' <summary>
    ''' Classification type
    ''' </summary>
    Private ReadOnly classificationType As IClassificationType

    ''' <summary>
    ''' Initializes a new instance of the <see cref="EditorClassifier1"/> class.
    ''' </summary>
    ''' <param name="registry">Classification registry.</param>
    Friend Sub New(ByVal registry As IClassificationTypeRegistryService)

        Me.classificationType = registry.GetClassificationType("EditorClassifier1")

    End Sub

    ''' <summary>
    ''' An event that occurs when the classification of a span of text has changed.
    ''' </summary>
    ''' <remarks>
    ''' This event gets raised if a non-text change would affect the classification in some way,
    ''' for example typing /* would cause the classification to change in C# without directly
    ''' affecting the span.
    ''' </remarks>
    Public Event ClassificationChanged As EventHandler(Of ClassificationChangedEventArgs) Implements IClassifier.ClassificationChanged

    ''' <summary>
    ''' This method scans the given SnapshotSpan for potential matches for this classification.
    ''' In this instance, it classifies everything and returns each span as a new ClassificationSpan.
    ''' </summary>
    ''' <param name="span">The span currently being classified.</param>
    ''' <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
    Public Function GetClassificationSpans(ByVal span As SnapshotSpan) As IList(Of ClassificationSpan) Implements IClassifier.GetClassificationSpans

        ' Create a list to hold the results
        Dim result As New List(Of ClassificationSpan) From {
            New ClassificationSpan(New SnapshotSpan(span.Snapshot, New span(span.Start, span.Length)), Me.classificationType)
        }

        Return result

    End Function

End Class

