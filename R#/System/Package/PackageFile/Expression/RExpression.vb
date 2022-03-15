#Region "Microsoft.VisualBasic::f1ebc617f330dd167762cfd32c50911a, R-sharp\R#\System\Package\PackageFile\Expression\RExpression.vb"

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


     Code Statistics:

        Total Lines:   48
        Code Lines:    25
        Comment Lines: 15
        Blank Lines:   8
        File Size:     1.75 KB


    '     Class RExpression
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetBuffer
    ' 
    '         Sub: saveSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Development.Package.File.Expressions

    ''' <summary>
    ''' the R expression writer/reader
    ''' </summary>
    ''' <remarks>
    ''' expression: [ExpressionTypes, i32][dataSize, i32][TypeCodes, byte][expressionData, bytes]
    '''              4                     4              1                ...
    ''' </remarks>
    Public MustInherit Class RExpression

        Protected ReadOnly context As Writer

        Sub New(context As Writer)
            Me.context = context
        End Sub

        Public MustOverride Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
        Public MustOverride Sub WriteBuffer(ms As MemoryStream, x As Expression)

        Public Function GetBuffer(x As Expression) As Byte()
            Using ms As New MemoryStream
                Call WriteBuffer(ms, x)
                Return ms.ToArray
            End Using
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="outfile"></param>
        ''' <remarks>
        ''' expression: [ExpressionTypes, i32][dataSize, i32][TypeCodes, byte][expressionData, bytes]
        '''              4                     4              1                ...
        ''' </remarks>
        Protected Shared Sub saveSize(outfile As BinaryWriter)
            Dim totalLength As Integer = outfile.BaseStream.Length
            Dim dataSize As Integer = totalLength - 4 - 4 - 1

            Call outfile.Seek(4, SeekOrigin.Begin)
            Call outfile.Write(dataSize)
            Call outfile.Flush()
        End Sub
    End Class
End Namespace
