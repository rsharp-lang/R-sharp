#Region "Microsoft.VisualBasic::7aff32807ffdb20f9407f9704b803c03, G:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Expression/RBinary.vb"

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

    '   Total Lines: 102
    '    Code Lines: 90
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 4.58 KB


    '     Class RBinary
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetExpression, getOperator, left, right
    ' 
    '         Sub: WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    Public Class RBinary : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Binary))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Encoding.ASCII.GetBytes(getOperator(x)))
                Call outfile.Write(CByte(0))
                Call outfile.Write(context.GetBuffer(left(x)))
                Call outfile.Write(context.GetBuffer(right(x)))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Private Shared Function left(x As Expression) As Expression
            If TypeOf x Is BinaryBetweenExpression Then
                Return DirectCast(x, BinaryBetweenExpression).collectionSet
            ElseIf TypeOf x Is BinaryInExpression Then
                Return DirectCast(x, BinaryInExpression).left
            ElseIf TypeOf x Is BinaryOrExpression Then
                Return DirectCast(x, BinaryOrExpression).left
            ElseIf TypeOf x Is BinaryExpression Then
                Return DirectCast(x, BinaryExpression).left
            ElseIf TypeOf x Is TypeOfCheck Then
                Return DirectCast(x, TypeOfCheck).target
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Private Shared Function right(x As Expression) As Expression
            If TypeOf x Is BinaryBetweenExpression Then
                Return DirectCast(x, BinaryBetweenExpression).range
            ElseIf TypeOf x Is BinaryInExpression Then
                Return DirectCast(x, BinaryInExpression).right
            ElseIf TypeOf x Is BinaryOrExpression Then
                Return DirectCast(x, BinaryOrExpression).right
            ElseIf TypeOf x Is BinaryExpression Then
                Return DirectCast(x, BinaryExpression).right
            ElseIf TypeOf x Is TypeOfCheck Then
                Return DirectCast(x, TypeOfCheck).typeName
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Const CheckType As String = "is_type"

        Private Shared Function getOperator(x As Expression) As String
            If TypeOf x Is BinaryBetweenExpression Then
                Return "between"
            ElseIf TypeOf x Is BinaryInExpression Then
                Return "in"
            ElseIf TypeOf x Is BinaryOrExpression Then
                Return "||"
            ElseIf TypeOf x Is BinaryExpression Then
                Return DirectCast(x, BinaryExpression).operator
            ElseIf TypeOf x Is TypeOfCheck Then
                Return CheckType
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim op As String = StreamHelper.ReadZEROBlock(bin) _
                    .DoCall(Function(bytes)
                                Return Encoding.ASCII.GetString(bytes.ToArray)
                            End Function)
                Dim left As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim right As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Select Case op
                    Case "between" : Return New BinaryBetweenExpression(left, right)
                    Case "in" : Return New BinaryInExpression(left, right)
                    Case "||" : Return New BinaryOrExpression(left, right)
                    Case CheckType : Return New TypeOfCheck(left, right)
                    Case Else
                        Return New BinaryExpression(left, right, op)
                End Select
            End Using
        End Function
    End Class
End Namespace
