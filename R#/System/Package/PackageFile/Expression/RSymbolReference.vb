#Region "Microsoft.VisualBasic::3f68152223851eeafc76e597e271d045, E:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Expression/RSymbolReference.vb"

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

    '   Total Lines: 105
    '    Code Lines: 87
    ' Comment Lines: 0
    '   Blank Lines: 18
    '     File Size: 4.54 KB


    '     Class RSymbolReference
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetExpression, parseFunctionRef, parseSymbol
    ' 
    '         Sub: (+4 Overloads) WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Annotation
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Development.Package.File.Expressions

    Public Class RSymbolReference : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is SymbolReference Then
                Call WriteBuffer(ms, DirectCast(x, SymbolReference))
            ElseIf TypeOf x Is NamespaceFunctionSymbolReference Then
                Call WriteBuffer(ms, DirectCast(x, NamespaceFunctionSymbolReference))
            ElseIf TypeOf x Is HomeSymbol Then
                Call WriteBuffer(ms, DirectCast(x, HomeSymbol))
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Sub

        Private Overloads Sub WriteBuffer(ms As MemoryStream, x As HomeSymbol)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.SymbolReference))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Encoding.ASCII.GetBytes("@HOME"))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Private Overloads Sub WriteBuffer(ms As MemoryStream, x As NamespaceFunctionSymbolReference)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.SymbolNamespaceReference))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(sourceMap:=CType(x, IRuntimeTrace).stackFrame))
                Call outfile.Write(Encoding.ASCII.GetBytes(x.namespace))
                Call outfile.Write(CByte(0))
                Call outfile.Write(context.GetBuffer(x.symbol))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As SymbolReference)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.SymbolReference))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Encoding.ASCII.GetBytes(x.symbol))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Private Function parseSymbol(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Dim symbolName As String = Encoding.ASCII.GetString(buffer.ToArray)

            If symbolName = "@HOME" Then
                Return New HomeSymbol
            Else
                Return New SymbolReference(symbolName)
            End If
        End Function

        Private Function parseFunctionRef(io As BinaryReader, desc As DESCRIPTION) As Expression
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(io, desc)
            Dim ns As String = StreamHelper _
                .ReadZEROBlock(io) _
                .DoCall(Function(bytes)
                            Return Encoding.ASCII.GetString(bytes.ToArray)
                        End Function)
            Dim symbol As Expression = BlockReader.ParseBlock(io).Parse(desc)

            Return New NamespaceFunctionSymbolReference(ns, symbol, sourceMap)
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using io As New BinaryReader(buffer)
                Select Case raw.expression
                    Case ExpressionTypes.SymbolReference : Return parseSymbol(buffer, desc)
                    Case ExpressionTypes.SymbolNamespaceReference : Return parseFunctionRef(io, desc)
                    Case Else
                        Throw New NotImplementedException(raw.expression.Description)
                End Select
            End Using
        End Function
    End Class
End Namespace
