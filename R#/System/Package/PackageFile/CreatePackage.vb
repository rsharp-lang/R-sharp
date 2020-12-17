#Region "Microsoft.VisualBasic::613f58f298b19db32735ad8ef158aa82, R#\System\Package\PackageFile\CreatePackage.vb"

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

'     Module CreatePackage
' 
'         Function: Build
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package.File.Expressions

Namespace System.Package.File

    Public Module CreatePackage

        ''' <summary>
        ''' build a R# package file
        ''' </summary>
        ''' <param name="target">
        ''' the target directory that contains the necessary 
        ''' files for create a R# package file.</param>
        ''' <param name="outfile">the output zip file stream</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Build(desc As DESCRIPTION, target As String, outfile As Stream) As Message
            Dim srcR As String = $"{target}/R".GetDirectoryFullPath
            Dim file As New PackageModel With {
                .info = desc,
                .symbols = New Dictionary(Of String, Expression),
                .assembly = $"{target}/assembly".EnumerateFiles("*.dll")
            }
            Dim loading As New List(Of Expression)

            For Each script As String In srcR.ListFiles("*.R")
                Dim error$ = Nothing
                Dim exec As Program = Program.CreateProgram(Rscript.FromFile(script), [error]:=[error])

                If Not [error].StringEmpty Then
                    Return New Message With {
                        .level = MSG_TYPES.ERR,
                        .message = {[error]}
                    }
                Else
                    For Each line As Expression In exec
                        If TypeOf line Is DeclareNewSymbol Then
                            Dim var As DeclareNewSymbol = line

                            If var.isTuple Then
                                Return New Message With {.message = {"top level declare new symbol is not allows tuple!"}, .level = MSG_TYPES.ERR}
                            Else
                                file.symbols.Add(var.names(Scan0), var)
                            End If
                        ElseIf TypeOf line Is DeclareNewFunction Then
                            Dim fun As DeclareNewFunction = line

                            file.symbols.Add(fun.funcName, fun)
                        ElseIf TypeOf line Is [Imports] OrElse TypeOf line Is Require Then
                            loading.Add(line)
                        Else
                            Return New Message With {
                                .level = MSG_TYPES.ERR,
                                .message = {$"'{line.GetType.Name}' is not allow in top level script when create a R# package!"}
                            }
                        End If
                    Next
                End If
            Next

            file.loading = loading.DoCall(AddressOf Dependency.GetDependency).ToArray
            file.Flush(outfile)

            Return Nothing
        End Function
    End Module
End Namespace
