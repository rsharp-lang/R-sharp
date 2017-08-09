#Region "Microsoft.VisualBasic::89941403c327204fdc0636aa5773a8f2, ..\R-sharp\R#\Interpreter\Interpreter.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    ''' <summary>
    ''' The R# language interpreter
    ''' </summary>
    Public Class Interpreter

        ''' <summary>
        ''' Global runtime environment.(全局环境)
        ''' </summary>
        Public ReadOnly Property globalEnvir As New Environment

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

        Public Shared ReadOnly Property Rsharp As New Interpreter

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
