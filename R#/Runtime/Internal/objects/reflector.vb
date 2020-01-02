#Region "Microsoft.VisualBasic::6ed286d16a6f2e199575e194f5e6baac, R#\Runtime\Internal\objects\reflector.vb"

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

'     Module reflector
' 
'         Function: GetStructure, strVector
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Internal.Object

    Module reflector

        Public Function GetStructure(x As Object, env As GlobalEnvironment, indent$) As String
            If x Is Nothing Then
                Return "<NULL>"
            ElseIf x.GetType Is GetType(list) Then
                x = DirectCast(x, list).slots
            End If

            Dim type As Type = x.GetType
            Dim code As TypeCodes = type.GetRTypeCode

            If type.ImplementInterface(GetType(IDictionary)) Then
                Dim list As IDictionary = x
                Dim value As Object
                Dim sb As New StringBuilder
                Dim i As i32 = 1

                Call sb.AppendLine("List of " & list.Count)

                For Each slotKey As Object In list.Keys
                    value = list(slotKey)

                    If ++i = CInt(Val(slotKey.ToString)) Then
                        slotKey = $"[{slotKey}]"
                    End If

                    sb.AppendLine($"{indent}$ {slotKey}: {GetStructure(value, env, indent & "..")}")
                Next

                Return sb.ToString
            ElseIf Runtime.IsPrimitive(code, includeComplexList:=False) Then
                Return strVector(Runtime.asVector(Of Object)(x), type, env)
            Else
                Return classPrinter.printClass(x)
            End If
        End Function

        Private Function strVector(a As Array, type As Type, env As GlobalEnvironment) As String
            Dim typeCode$

            If type Like BinaryExpression.integers Then
                typeCode = "int"
            ElseIf type Like BinaryExpression.characters Then
                typeCode = "chr"
            ElseIf type Like BinaryExpression.floats Then
                typeCode = "num"
            Else
                typeCode = "logical"
            End If

            If a.Length = 1 Then
                Return $"{typeCode} {printer.ValueToString(a.GetValue(Scan0), env)}"
            ElseIf a.Length <= 6 Then
                Return $"{typeCode} [1:{a.Length}] {printer.getStrings(a, env).JoinBy(" ")}"
            Else
                Return $"{typeCode} [1:{a.Length}] {printer.getStrings(a, env).Take(6).JoinBy(" ")} ..."
            End If
        End Function
    End Module
End Namespace
