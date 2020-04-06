#Region "Microsoft.VisualBasic::6f2b8aa8e950795884f489f481f77bee, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\ModeOf.vb"

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

    '     Class ModeOf
    ' 
    '         Properties: keyword, target, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class ModeOf : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If keyword = "modeof" Then
                    Return TypeCodes.string
                Else
                    Return TypeCodes.generic
                End If
            End Get
        End Property

        ''' <summary>
        ''' + ``modeof`` for get R# type code
        ''' + ``typeof`` for get VB.NET type
        ''' + ``valueof`` for get VB.NET type of the return value from a .NET api
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property keyword As String
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property target As Expression

        Sub New(keyword$, target As Expression)
            Me.keyword = keyword
            Me.target = target
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = target.Evaluate(envir)

            If keyword = "modeof" Then
                If value Is Nothing Then
                    Return TypeCodes.NA.Description
                Else
                    Return value.GetType.GetRTypeCode.Description
                End If
            ElseIf keyword = "valueof" Then
                If value Is Nothing Then
                    Return RType.GetRSharpType(GetType(Void))
                ElseIf value.GetType Is GetType(Message) Then
                    Return value
                ElseIf value.GetType Is GetType(RMethodInfo) Then
                    Return DirectCast(value, RMethodInfo) _
                        .GetRawDeclares _
                        .DoCall(AddressOf RApiReturnAttribute.GetActualReturnType) _
                        .Select(AddressOf RType.GetRSharpType) _
                        .ToArray
                Else
                    Return Message.InCompatibleType(GetType(RMethodInfo), value.GetType, envir)
                End If
            Else
                If value Is Nothing Then
                    Return RType.GetRSharpType(GetType(Void))
                Else
                    Return RType.GetRSharpType(value.GetType)
                End If
            End If
        End Function
    End Class
End Namespace
