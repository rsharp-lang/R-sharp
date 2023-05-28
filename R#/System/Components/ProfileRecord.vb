#Region "Microsoft.VisualBasic::7570e9f9735ea40fd72748c55c6cb814, F:/GCModeller/src/R-sharp/R#//System/Components/ProfileRecord.vb"

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

    '   Total Lines: 59
    '    Code Lines: 40
    ' Comment Lines: 4
    '   Blank Lines: 15
    '     File Size: 1.96 KB


    '     Class ProfileRecord
    ' 
    '         Properties: elapse_time, expression, memory_delta, memory_size, stackframe
    '                     tag
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class ProfilerFrames
    ' 
    '         Properties: profiles, size, timestamp, traceback
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ValueTypes
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Development.Components

    Public Class ProfileRecord

        Public Property tag As Long
        Public Property elapse_time As Long

        ''' <summary>
        ''' memory delta size in bytes unit ``MB``.
        ''' </summary>
        ''' <returns></returns>
        Public Property memory_delta As Double
        Public Property memory_size As Double
        Public Property stackframe As StackFrame
        Public Property expression As String

        Sub New(expr As Expression)
            Dim lines As String() = expr.ToString.LineTokens
            Dim line As String = lines(Scan0).StringReplace("[""]+", "'")

            If lines.Length > 1 Then
                line = line & "..."
            End If

            If line.Length > 64 Then
                line = Mid(line, 1, 61) & "..."
            End If

            expression = line
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{tag.FromUnixTimeStamp.ToString}] elapse_time {TimeSpan.FromTicks(elapse_time).FormatTime}, memory delta {memory_delta.ToString("F2")}MB, memory usage {memory_size.ToString("F2")}MB at {stackframe}"
        End Function

    End Class

    Public Class ProfilerFrames

        Public Property timestamp As Double
        Public Property traceback As StackFrame
        Public Property profiles As ProfileRecord()

        Public ReadOnly Property size As Integer
            Get
                Return profiles.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{profiles.Length} frames at {CLng(timestamp).FromUnixTimeStamp.ToString} | {traceback.ToString}"
        End Function

    End Class
End Namespace
