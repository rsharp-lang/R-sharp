#Region "Microsoft.VisualBasic::0f893da8d6a1727672bdb8bd806d0a6f, R#\Runtime\Interop\MethodInvoke.vb"

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

    '     Class MethodInvoke
    ' 
    '         Function: Invoke, ToString
    ' 
    '     Class RuntimeValueLiteral
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    Friend Class MethodInvoke

        Public method As MethodInfo
        Public target As Object

        Public ReadOnly Property isStatic As Boolean
            Get
                Return target Is Nothing
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Invoke(parameters As Object()) As Object
            Return method.Invoke(target, parameters)
        End Function

        <DebuggerStepThrough>
        Public Overrides Function ToString() As String
            Return $"{Scripting.ToString(target, "NULL")}::{method.ToString}"
        End Function

    End Class

    ''' <summary>
    ''' Literal of any .NET runtime value object
    ''' </summary>
    Public Class RuntimeValueLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If value Is Nothing Then
                    Return TypeCodes.NA
                Else
                    Return value.GetType.GetRTypeCode
                End If
            End Get
        End Property

        Public ReadOnly Property value As Object

        Sub New(value As Object)
            Me.value = value
        End Sub

        <DebuggerStepThrough>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return value
        End Function

        Public Overrides Function ToString() As String
            If value Is Nothing Then
                Return "NULL"
            Else
                Return value.ToString
            End If
        End Function
    End Class
End Namespace
