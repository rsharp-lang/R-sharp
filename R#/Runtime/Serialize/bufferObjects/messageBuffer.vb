#Region "Microsoft.VisualBasic::7ba04c672c838db0a1a75f95130d972c, R-sharp\R#\Runtime\Serialize\bufferObjects\messageBuffer.vb"

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

    '   Total Lines: 153
    '    Code Lines: 116
    ' Comment Lines: 3
    '   Blank Lines: 34
    '     File Size: 5.48 KB


    '     Class messageBuffer
    ' 
    '         Properties: code, environmentStack, level, message, source
    '                     trace
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateBuffer, GetErrorMessage, getValue
    ' 
    '         Sub: Serialize
    '         Class TextExpression
    ' 
    '             Properties: expressionName, text, type
    ' 
    '             Function: Evaluate, ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Runtime.Serialize

    ''' <summary>
    ''' the error message
    ''' </summary>
    Public Class messageBuffer : Inherits BufferObject

        Public Property message As String()
        Public Property level As MSG_TYPES
        Public Property environmentStack As StackFrame()
        Public Property trace As StackFrame()
        Public Property source As String

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.message
            End Get
        End Property

        Sub New(message As Message, Optional source As String = "unknown")
            Me.message = message.message
            Me.level = message.level
            Me.source = If(message.source?.ToString, source)
            Me.environmentStack = message.environmentStack
            Me.trace = message.trace
        End Sub

        Sub New()
        End Sub

        Public Class TextExpression : Inherits Expression

            Public Overrides ReadOnly Property type As TypeCodes
                Get
                    Return TypeCodes.string
                End Get
            End Property

            Public Overrides ReadOnly Property expressionName As ExpressionTypes
                Get
                    Return ExpressionTypes.Literal
                End Get
            End Property

            Public Property text As String

            Public Overrides Function Evaluate(envir As Environment) As Object
                Return text
            End Function

            Public Overrides Function ToString() As String
                Return text
            End Function
        End Class

        Public Function GetErrorMessage() As Message
            Return New Message With {
                .environmentStack = environmentStack,
                .level = level,
                .message = message,
                .trace = trace,
                .source = New TextExpression With {
                    .text = source
                }
            }
        End Function

        Public Shared Function CreateBuffer(buffer As Stream) As messageBuffer
            Dim level As MSG_TYPES
            Dim int_buf As Byte() = New Byte(3) {}
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim bytes As Byte()

            buffer.Read(int_buf, Scan0, int_buf.Length)
            level = CType(BitConverter.ToInt32(int_buf, Scan0), MSG_TYPES)

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim source As String = text.GetString(bytes)

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim message As String() = RawStream.GetData(bytes, TypeCode.String)

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim env As StackFrame() = New TraceBuffer(bytes).StackTrace

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim trace As StackFrame() = New TraceBuffer(bytes).StackTrace

            Return New messageBuffer With {
                .environmentStack = env,
                .level = level,
                .message = message,
                .source = source,
                .trace = trace
            }
        End Function

        Public Overrides Sub Serialize(buffer As Stream)
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim bytes As Byte()

            Call buffer.Write(BitConverter.GetBytes(CInt(level)), Scan0, 4)

            bytes = text.GetBytes(source)
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)

            bytes = RawStream.GetBytes(message)
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)

            Dim trace As New TraceBuffer With {.StackTrace = environmentStack}

            bytes = trace.Serialize
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)

            trace = New TraceBuffer With {.StackTrace = Me.trace}

            bytes = trace.Serialize
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)

            buffer.Flush()
        End Sub

        Public Overrides Function getValue() As Object
            Return GetErrorMessage()
        End Function
    End Class
End Namespace
