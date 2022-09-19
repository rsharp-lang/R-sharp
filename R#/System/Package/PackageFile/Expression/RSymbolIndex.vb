#Region "Microsoft.VisualBasic::854b43a79ab8617fccdfa5cec1fe35b1, R-sharp\R#\System\Package\PackageFile\Expression\RSymbolIndex.vb"

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

'   Total Lines: 68
'    Code Lines: 55
' Comment Lines: 0
'   Blank Lines: 13
'     File Size: 2.83 KB


'     Class RSymbolIndex
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetExpression
' 
'         Sub: (+3 Overloads) WriteBuffer
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    Public Class RSymbolIndex : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is DotNetObject Then
                Call WriteBuffer(ms, DirectCast(x, DotNetObject))
            ElseIf TypeOf x Is VectorLoop Then
                Call WriteBuffer(ms, DirectCast(x, VectorLoop))
            Else
                Call WriteBuffer(ms, DirectCast(x, SymbolIndexer))
            End If
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As DotNetObject)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.DotNetMemberReference))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(x.object))
                Call outfile.Write(context.GetBuffer(x.member))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As SymbolIndexer)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.SymbolIndex))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(x.indexType)
                Call outfile.Write(context.GetBuffer(x.symbol))
                Call outfile.Write(context.GetBuffer(x.index))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As VectorLoop)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.VectorLoop))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(x.symbol))
                Call outfile.Write(context.GetBuffer(x.index))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                If raw.expression = ExpressionTypes.DotNetMemberReference Then
                    Dim symbol As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                    Dim index As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                    Return New DotNetObject(symbol, index)
                ElseIf raw.expression = ExpressionTypes.VectorLoop Then
                    Dim symbol As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                    Dim index As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                    Return New VectorLoop(symbol, index)
                Else
                    Dim indexType As SymbolIndexers = bin.ReadByte
                    Dim symbol As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                    Dim index As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                    Return New SymbolIndexer(symbol, index, indexType)
                End If
            End Using
        End Function
    End Class
End Namespace
