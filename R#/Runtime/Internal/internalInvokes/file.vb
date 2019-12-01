#Region "Microsoft.VisualBasic::27e23e05fec2b281699a16087176a463, R#\Runtime\Internal\internalInvokes\file.vb"

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

'     Module file
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: exists, normalizeFileName, readLines, saveImage, setwd
'                   writeLines
' 
'         Sub: pushEnvir
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' ## File Manipulation
    ''' 
    ''' These functions provide a low-level interface to the computer's file system.
    ''' </summary>
    Module file

        Sub New()
            Call Internal.invoke.add("file.exists", AddressOf file.exists)
            Call Internal.invoke.add("readLines", AddressOf file.readLines)
            Call Internal.invoke.add("writeLines", AddressOf file.writeLines)
            Call Internal.invoke.add("setwd", AddressOf file.setwd)
            Call Internal.invoke.add("normalize.filename", AddressOf file.normalizeFileName)
            Call Internal.invoke.add("basename", AddressOf file.basename)
        End Sub

        Friend Sub pushEnvir()
            ' do nothing
        End Sub

        Private Function basename(envir As Environment, params As Object()) As Object
            If params.IsNullOrEmpty Then
                Return Internal.stop("no file names provided!", envir)
            End If

            Dim fileNames As String() = Runtime.asVector(Of String)(params(Scan0))
            Dim withExtensionName As Boolean = Runtime.asLogical(params.ElementAtOrDefault(1))(Scan0)

            If withExtensionName Then
                ' get fileName
                Return fileNames.Select(AddressOf FileName).ToArray
            Else
                Return fileNames.Select(Function(file) file.BaseName).ToArray
            End If
        End Function

        Friend Function normalizeFileName(envir As Environment, params As Object()) As String()
            Return params.SafeQuery _
                .Select(Function(val)
                            Return Runtime.asVector(Of Double)(val) _
                                .AsObjectEnumerator _
                                .Select(Function(file)
                                            Return Scripting.ToString(file).NormalizePathString(False)
                                        End Function)
                        End Function) _
                .IteratesALL _
                .ToArray
        End Function

        Friend Function exists(envir As Environment, params As Object()) As Boolean()
            Return params.SafeQuery _
                .Select(Function(val)
                            If val Is Nothing Then
                                Return False
                            Else
                                Return Scripting _
                                    .ToString(Runtime.getFirst(val)) _
                                    .DoCall(AddressOf FileExists)
                            End If
                        End Function) _
                .ToArray
        End Function

        Friend Function readLines(envir As Environment, params As Object()) As String()
            Return Scripting.ToString(Runtime.getFirst(params(Scan0))).ReadAllLines
        End Function

        ' writeLines(text, con = stdout(), sep = "\n", useBytes = FALSE)
        Friend Function writeLines(envir As Environment, params As Object()) As Object
            Dim text = Runtime.asVector(Of String)(params(Scan0))
            Dim con$ = Scripting.ToString(Runtime.getFirst(params(1)))

            If con.StringEmpty Then
                Call text.AsObjectEnumerator _
                    .JoinBy(vbCrLf) _
                    .DoCall(AddressOf Console.WriteLine)
            Else
                Call text.AsObjectEnumerator _
                    .JoinBy(vbCrLf) _
                    .SaveTo(con)
            End If

            Return text
        End Function

        Friend Function setwd(envir As Environment, paramVals As Object()) As Object
            Dim dir As String() = Runtime.asVector(Of String)(paramVals(Scan0))

            If dir.Length = 0 Then
                Return invoke.missingParameter(NameOf(setwd), "dir", envir)
            ElseIf dir(Scan0).StringEmpty Then
                Return invoke.invalidParameter("cannot change working directory due to the reason of NULL value provided!", NameOf(setwd), "dir", envir)
            Else
                App.CurrentDirectory = PathMapper.GetMapPath(dir(Scan0))
            End If

            Return App.CurrentDirectory
        End Function
    End Module
End Namespace
