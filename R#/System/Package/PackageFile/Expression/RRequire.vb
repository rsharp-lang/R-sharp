#Region "Microsoft.VisualBasic::e21b6b38eafc12e488d47b5388f4afb7, R#\System\Package\PackageFile\Expression\RRequire.vb"

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

    '   Total Lines: 110
    '    Code Lines: 81 (73.64%)
    ' Comment Lines: 8 (7.27%)
    '    - Xml Docs: 62.50%
    ' 
    '   Blank Lines: 21 (19.09%)
    '     File Size: 4.20 KB


    '     Class RRequire
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
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    ''' <summary>
    ''' imports语句在脚本模式下可以出现在任意语句块之中，但是
    ''' imports语句在R#程序包之中只允许出现在脚本的最顶层，不允许出现在其他的语句块中
    ''' require语句可以出现在程序包之中的任意语句块之中
    ''' </summary>
    Public Class RRequire : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is Require Then
                Call WriteBuffer(ms, DirectCast(x, Require))
            Else
                Call WriteBuffer(ms, DirectCast(x, [Imports]))
            End If
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As [Imports])
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Imports))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                If x.library Is Nothing Then
                    Call outfile.Write(0)
                Else
                    Dim buf = context.GetBuffer(x.library)

                    Call outfile.Write(buf.Length)
                    Call outfile.Write(buf)
                End If

                If x.packages Is Nothing Then
                    Call outfile.Write(0)
                Else
                    Dim buf = context.GetBuffer(x.packages)

                    Call outfile.Write(buf.Length)
                    Call outfile.Write(buf)
                End If

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As Require)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Require))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                ' get count of the package names
                ' package count should be less than 
                ' 256
                Call outfile.Write(CByte(x.packages.Length))

                For Each pkgName As String In x.packages.Select(AddressOf ValueAssignExpression.GetSymbol)
                    Call outfile.Write(Encoding.ASCII.GetBytes(pkgName))
                    Call outfile.Write(CByte(0))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            If raw.expression = ExpressionTypes.Require Then
                Dim packageNames As String() = buffer.ToArray _
                    .Skip(1) _
                    .Split(Function(b) b = 0, DelimiterLocation.NotIncludes) _
                    .Select(AddressOf Encoding.ASCII.GetString) _
                    .ToArray

                Return New Require(packageNames)
            Else
                Using bin As New BinaryReader(buffer)
                    Dim byte_size As Integer
                    Dim [lib] As Expression = Nothing
                    Dim [pkg] As Expression = Nothing

                    byte_size = bin.ReadInt32

                    If byte_size > 0 Then
                        [lib] = BlockReader.ParseBlock(bin).Parse(desc)
                    End If

                    byte_size = bin.ReadInt32

                    If byte_size > 0 Then
                        pkg = BlockReader.ParseBlock(bin).Parse(desc)
                    End If

                    Return New [Imports](pkg, [lib])
                End Using
            End If
        End Function
    End Class
End Namespace
