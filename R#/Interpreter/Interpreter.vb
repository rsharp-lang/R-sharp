#Region "Microsoft.VisualBasic::e97cb4464072ef39b44a03e0b22ce5ad, R#\Interpreter\Interpreter.vb"

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

    '     Class RInterpreter
    ' 
    '         Properties: globalEnvir, Rsharp
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Evaluate, Source
    ' 
    '         Sub: PrintMemory
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Interpreter.Language
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    ''' <summary>
    ''' The R# language interpreter
    ''' </summary>
    Public Class RInterpreter

        ''' <summary>
        ''' Global runtime environment.(全局环境)
        ''' </summary>
        Public ReadOnly Property globalEnvir As New Environment

        Public Const LastVariableName$ = ".Last"

        Sub New()
            Call globalEnvir.Push(LastVariableName, Nothing, TypeCodes.generic)
        End Sub

        Public Sub PrintMemory(Optional dev As TextWriter = Nothing)
            Dim table$()() = globalEnvir _
                .Variables _
                .Values _
                .Select(Function(v)
                            Dim vector = v.ToVector
                            Dim value$ = vector _
                                .Select(Function(x) CStrSafe(x)) _
                                .JoinBy(", ")

                            Return {
                                v.Name,
                                v.TypeCode.ToString & $" ({v.TypeOf.FullName})",
                                $"[{vector.Length}] {value}"
                            }
                        End Function) _
                .ToArray

            With dev Or Console.Out.AsDefault
                Call table.Print(dev:= .ByRef, distance:=3)
            End With
        End Sub

        ''' <summary>
        ''' Run R# script program from text data.
        ''' </summary>
        ''' <param name="script$"></param>
        ''' <returns></returns>
        Public Function Evaluate(script$) As Object
            Return Codes.TryParse(script).RunProgram(globalEnvir)
        End Function

        ''' <summary>
        ''' Run script file.
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function Source(path$, args As IEnumerable(Of NamedValue(Of Object))) As Object

        End Function

        Public Shared ReadOnly Property Rsharp As New RInterpreter

        Public Shared Function Evaluate(script$, ParamArray args As NamedValue(Of Object)()) As Object
            SyncLock Rsharp
                With Rsharp
                    If Not args.IsNullOrEmpty Then
                        For Each x In args
                            Call .globalEnvir.Push(x.Name, x.Value, NameOf(TypeCodes.generic))
                        Next
                    End If

                    Return .Evaluate(script)
                End With
            End SyncLock
        End Function
    End Class
End Namespace
