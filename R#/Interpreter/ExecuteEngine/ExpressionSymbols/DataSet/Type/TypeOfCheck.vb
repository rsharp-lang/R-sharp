#Region "Microsoft.VisualBasic::d4eb18a45c42cca2d4a93d2c710e0686, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/Type/TypeOfCheck.vb"

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

    '   Total Lines: 88
    '    Code Lines: 55
    ' Comment Lines: 18
    '   Blank Lines: 15
    '     File Size: 3.15 KB


    '     Class TypeOfCheck
    ' 
    '         Properties: expressionName, type, typeName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ``typeof x is "export_typename"``
    ''' </summary>
    Public Class TypeOfCheck : Inherits ModeOf

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Binary
            End Get
        End Property

        ''' <summary>
        ''' value of this property is usually comes from <see cref="RTypeExportAttribute"/>
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' binary right
        ''' </remarks>
        Public ReadOnly Property typeName As Expression

        ''' <summary>
        ''' typeof target == typeName?
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="typeName"></param>
        ''' <remarks>
        ''' 关键词总是typeof
        ''' </remarks>
        Public Sub New(target As Expression, typeName As Expression)
            MyBase.New(keyword:="typeof", target)

            Me.typeName = typeName
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim type = MyBase.Evaluate(envir)

            If Program.isException(type) Then
                Return type
            End If

            Dim typeRight As Object = typeName.Evaluate(envir)

            If Program.isException(typeRight) Then
                Return typeRight
            End If

            If TypeOf type Is RType Then
                If TypeOf typeRight Is RType Then
                    Return type Is typeRight
                ElseIf TypeOf typeRight Is String Then
                    Dim type2 As RType = envir.globalEnvironment.GetType(typeRight)

                    If type2 Is Nothing Then
                        Return Internal.debug.stop({
                            $"we are not able to find any information about the data type: '{typeRight}', please imports underlying package namespace at first!",
                            $"type name: {typeRight}"
                        }, envir)
                    Else
                        Return type Is type2 OrElse (type2.mode.IsPrimitive(includeComplexList:=False) AndAlso DirectCast(type, RType).mode = type2.mode)
                    End If
                Else
                    Return Message.InCompatibleType(GetType(String), typeRight.GetType, envir)
                End If
            Else
                Return Message.InCompatibleType(GetType(String), typeRight.GetType, envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"typeof ({target}) is {typeName}"
        End Function

    End Class
End Namespace
