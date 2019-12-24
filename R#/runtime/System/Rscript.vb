#Region "Microsoft.VisualBasic::f868da3257bcff14c1274c0aaa4802d3, R#\Runtime\System\Rscript.vb"

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

    '     Class Rscript
    ' 
    '         Properties: fileName, script, source
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FromFile, FromText, (+2 Overloads) GetRawText, GetSourceDirectory, GetTokens
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Runtime.Components

    ''' <summary>
    ''' An Rscript source wrapper
    ''' </summary>
    Public Class Rscript

        ''' <summary>
        ''' If the script is load from a text file, then property value of <see cref="source"/> is the file location
        ''' Otherwise this property is value nothing
        ''' </summary>
        ''' <returns></returns>
        Public Property source As String

        ''' <summary>
        ''' The script text
        ''' </summary>
        ''' <returns></returns>
        Public Property script As String

        Public ReadOnly Property fileName As String
            Get
                If source.StringEmpty Then
                    Return Nothing
                Else
                    Return source.FileName
                End If
            End Get
        End Property

        <DebuggerStepThrough>
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Get language <see cref="Scanner"/> tokens
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        Public Function GetTokens() As Token()
            Return New Scanner(script).GetTokens.ToArray
        End Function

        Public Function GetSourceDirectory() As String
            If source.StringEmpty Then
                Return App.CurrentDirectory
            Else
                Return source.ParentPath
            End If
        End Function

        <DebuggerStepThrough>
        Public Shared Function FromFile(path As String) As Rscript
            Return New Rscript With {
                .source = path.GetFullPath,
                .script = .source.ReadAllText
            }
        End Function

        <DebuggerStepThrough>
        Public Shared Function FromText(text As String) As Rscript
            Return New Rscript With {
                .source = Nothing,
                .script = text
            }
        End Function

        Public Function GetRawText(tokenSpan As IEnumerable(Of Token)) As String
            With tokenSpan.OrderBy(Function(t) t.span.start).ToArray
                Dim left = .First.span.start
                Dim right = .Last.span.stops

                Return script.Substring(left, right - left)
            End With
        End Function

        Public Function GetRawText(span As IntRange) As String
            Return script.Substring(span.Min, span.Length)
        End Function

        Public Overrides Function ToString() As String
            If source.StringEmpty Then
                Return "<in_memory> " & script
            Else
                Return $"<{source.FileName}> " & script.LineTokens.First & "..."
            End If
        End Function
    End Class
End Namespace
