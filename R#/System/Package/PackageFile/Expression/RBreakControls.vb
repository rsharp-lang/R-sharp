#Region "Microsoft.VisualBasic::2bbf56c2ae1f63ceb5ab7d701df988cb, R#\System\Package\PackageFile\Expression\RBreakControls.vb"

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

    '   Total Lines: 44
    '    Code Lines: 34 (77.27%)
    ' Comment Lines: 3 (6.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (15.91%)
    '     File Size: 1.62 KB


    '     Class RBreakControls
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetExpression
    ' 
    '         Sub: WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

Namespace Development.Package.File.Expressions

    ''' <summary>
    ''' break/next
    ''' </summary>
    Public Class RBreakControls : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is BreakLoop OrElse TypeOf x Is ContinuteFor Then
                Using outfile As New BinaryWriter(ms)
                    Call outfile.Write(CInt(x.expressionName))
                    Call outfile.Write(0)
                    Call outfile.Write(CByte(x.type))

                    Call outfile.Write(Encoding.UTF8.GetBytes(x.ToString))

                    Call outfile.Flush()
                    Call saveSize(outfile)
                End Using
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            If raw.expression = ExpressionTypes.Break Then
                Return New BreakLoop
            ElseIf raw.expression = ExpressionTypes.Continute Then
                Return New ContinuteFor
            Else
                Throw New NotImplementedException(raw.expression.ToString)
            End If
        End Function
    End Class
End Namespace
