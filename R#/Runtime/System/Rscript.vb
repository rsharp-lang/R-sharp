#Region "Microsoft.VisualBasic::393a9b9ba0b87a8630ae8da716c12572, R#\Runtime\System\Rscript.vb"

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

    '   Total Lines: 164
    '    Code Lines: 102
    ' Comment Lines: 41
    '   Blank Lines: 21
    '     File Size: 5.66 KB


    '     Class Rscript
    ' 
    '         Properties: fileName, script, source
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AutoHandleScript, FromFile, FromText, GetByLineNumber, GetByLineRange
    '                   GetFileNameDisplay, (+2 Overloads) GetRawText, GetSourceDirectory, (+2 Overloads) GetTokens, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Text
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
                    Return $"<in_memory_&{script.GetHashCode.ToHexString}>"
                Else
                    Return source.FileName
                End If
            End Get
        End Property

        <DebuggerStepThrough>
        Private Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetByLineNumber(line As Integer) As String
            Return script.LineTokens.Skip(line - 1).FirstOrDefault
        End Function

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

        Public Shared Function GetTokens(text As String) As Token()
            Return FromText(text).GetTokens
        End Function

        ''' <summary>
        ''' R script from local file
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <DebuggerStepThrough>
        Public Shared Function FromFile(path As String) As Rscript
            Return New Rscript With {
                .source = path.GetFullPath,
                .script = .source.ReadAllText
            }
        End Function

        Public Shared Function GetFileNameDisplay(handle As String) As String
            If Not handle.FileExists Then
                Return $"<in_memory_&{handle.GetHashCode.ToHexString}>"
            Else
                Return handle.FileName
            End If
        End Function

        ''' <summary>
        ''' auto handle Rscript text from:
        ''' 
        ''' + plain text input
        ''' + local file
        ''' + network file on a web server
        ''' 
        ''' </summary>
        ''' <param name="handle"></param>
        ''' <returns></returns>
        Public Shared Function AutoHandleScript(handle As String) As Rscript
            If handle.FileExists Then
                Return FromFile(handle)
            ElseIf handle.IsURLPattern Then
                ' download to a temp location
                ' and then create Rscript object from the temp file
                Dim tmpfile As String = TempFileSystem.GetAppSysTempFile("", sessionID:=App.PID.ToHexString, prefix:=handle.BaseName) & "/" & handle.FileName
                Dim Rscript As Rscript

                ' download script file in silent
                handle.GET(echo:=False).SaveTo(tmpfile)
                Rscript = FromFile(tmpfile)

                Return Rscript
            Else
                Return FromText(handle)
            End If
        End Function

        ''' <summary>
        ''' R script from in memory plain text input
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRawText(span As IntRange) As String
            Return script.Substring(span.Min, span.Interval)
        End Function

        Public Overrides Function ToString() As String
            If source.StringEmpty Then
                Return "<in_memory> " & script
            Else
                Return $"<{source.FileName}> " & script.LineTokens.First & "..."
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetByLineRange(region As IntRange) As String
            Return script _
                .LineTokens _
                .Skip(region.Min - 1) _
                .Take(region.Interval) _
                .JoinBy(ASCII.LF)
        End Function
    End Class
End Namespace
