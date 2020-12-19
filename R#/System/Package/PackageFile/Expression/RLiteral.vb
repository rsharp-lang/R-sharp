#Region "Microsoft.VisualBasic::4fc81c5e70c1b00f0db8de2dc9006f06, R#\System\Package\PackageFile\Expression\RLiteral.vb"

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

'     Class RLiteral
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetExpression
' 
'         Sub: (+2 Overloads) WriteBuffer
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RLiteral : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is Literal Then
                Call WriteBuffer(ms, DirectCast(x, Literal))
            ElseIf TypeOf x Is Regexp Then
                Call WriteBuffer(ms, DirectCast(x, Regexp))
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As Regexp)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(DirectCast(ExpressionTypes.SymbolRegexp, Integer))
                Call outfile.Write(0)
                Call outfile.Write(DirectCast(x.type, Byte))

                Call outfile.Write(Encoding.UTF8.GetBytes(x.pattern))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As Literal)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(DirectCast(ExpressionTypes.Literal, Integer))
                Call outfile.Write(0)
                Call outfile.Write(DirectCast(x.type, Byte))

                Select Case x.type
                    Case TypeCodes.boolean : Call outfile.Write(CType(If(DirectCast(x.value, Boolean), 1, 0), Byte))
                    Case TypeCodes.double : Call outfile.Write(CType(x.value, Double))
                    Case TypeCodes.integer : Call outfile.Write(CType(x.value, Long))
                    Case Else
                        Call outfile.Write(Encoding.UTF8.GetBytes(Scripting.ToString(x.value)))
                End Select

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Private Shared Function readRegexp(bin As BinaryReader) As Regexp
            Return New Regexp(Encoding.UTF8.GetString(bin.ReadBytes(bin.BaseStream.Length)))
        End Function

        Private Shared Function readLiteral(bin As BinaryReader, type As TypeCodes) As Literal
            Select Case type
                Case TypeCodes.boolean : Return New Literal(If(bin.ReadByte = 0, False, True))
                Case TypeCodes.double : Return New Literal(bin.ReadDouble)
                Case TypeCodes.integer : Return New Literal(bin.ReadInt64)
                Case Else
                    Return New Literal(Encoding.UTF8.GetString(bin.ReadBytes(bin.BaseStream.Length)))
            End Select
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression

        End Function
    End Class
End Namespace
