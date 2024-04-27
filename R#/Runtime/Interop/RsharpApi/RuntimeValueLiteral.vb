#Region "Microsoft.VisualBasic::f0c4c4c57a048af4992757b11163c707, G:/GCModeller/src/R-sharp/R#//Runtime/Interop/RsharpApi/RuntimeValueLiteral.vb"

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

    '   Total Lines: 55
    '    Code Lines: 36
    ' Comment Lines: 11
    '   Blank Lines: 8
    '     File Size: 1.63 KB


    '     Class RuntimeValueLiteral
    ' 
    '         Properties: expressionName, type, value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    ''' <summary>
    ''' Literal of any .NET clr runtime value object
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

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Literal
            End Get
        End Property

        ''' <summary>
        ''' Any .NET clr runtime object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property value As Object

        ''' <summary>
        ''' Create a new .NET clr object literal value
        ''' </summary>
        ''' <param name="value"></param>
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
