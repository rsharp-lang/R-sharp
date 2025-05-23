﻿#Region "Microsoft.VisualBasic::4b47bdc4880f3b2c3829bdfdee7961a1, R#\Interpreter\ExecuteEngine\SymbolExpression.vb"

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

    '   Total Lines: 139
    '    Code Lines: 74 (53.24%)
    ' Comment Lines: 49 (35.25%)
    '    - Xml Docs: 95.92%
    ' 
    '   Blank Lines: 16 (11.51%)
    '     File Size: 5.10 KB


    '     Class SymbolExpression
    ' 
    '         Function: GetAttributeNames, GetAttributes, GetAttributeValue, hasAttribute
    ' 
    '         Sub: AddCustomAttribute, AddCustomAttributes, SetAttributes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' An expression that will create a new symbol in R# environment
    ''' </summary>
    ''' <remarks>
    ''' the base type of 
    ''' 
    ''' 1. <see cref="DeclareNewSymbol"/>
    ''' 2. <see cref="DeclareLambdaFunction"/>
    ''' 3. <see cref="DeclareNewFunction"/>
    ''' 4. <see cref="UsingClosure"/>
    ''' </remarks>
    Public MustInherit Class SymbolExpression : Inherits Expression
        Implements IAttributeReflector

        ''' <summary>
        ''' the annotation data from the attribute annotation, example as:
        ''' 
        ''' ```R
        ''' [@name "value"]
        ''' [@name1 "value2"]
        ''' const symbol = ...
        ''' ```
        ''' </summary>
        Protected Friend attributes As Dictionary(Of String, String())

        ''' <summary>
        ''' The symbol name of current symbol object
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function GetSymbolName() As String

        Public Function GetAttributes() As Dictionary(Of String, String())
            If attributes Is Nothing Then
                attributes = New Dictionary(Of String, String())
            End If

            Return attributes
        End Function

        ''' <summary>
        ''' does a specific attribute exists in current symbol?
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function hasAttribute(name As String) As Boolean
            If attributes Is Nothing Then
                Return False
            Else
                Return attributes.ContainsKey(name)
            End If
        End Function

        ''' <summary>
        ''' Get all attribute name that tagged with current symbol object.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetAttributeNames() As IEnumerable(Of String) Implements IAttributeReflector.getAttributeNames
            If attributes Is Nothing Then
                Return New String() {}
            End If
            Return attributes.Keys
        End Function

        ''' <summary>
        ''' Get values that associated with the current symbol object
        ''' </summary>
        ''' <param name="name">the attribute name string for get the value</param>
        ''' <returns>returns empty string collection if the attribute name is not exists</returns>
        Public Function GetAttributeValue(name As String) As IEnumerable(Of String)
            If attributes Is Nothing Then
                Return New String() {}
            End If

            If attributes.ContainsKey(name) Then
                Return attributes(name)
            Else
                Return New String() {}
            End If
        End Function

        ''' <summary>
        ''' Add custom attribute data into current symbol object
        ''' </summary>
        ''' <param name="attrs"></param>
        ''' <remarks>
        ''' Call this method at script parser or expression parser code
        ''' </remarks>
        Protected Friend Overridable Sub AddCustomAttributes(attrs As IEnumerable(Of NamedValue(Of String())))
            If attributes Is Nothing Then
                attributes = New Dictionary(Of String, String())
            End If

            For Each attr As NamedValue(Of String()) In attrs
                If attributes.ContainsKey(attr.Name) Then
                    attributes(attr.Name) = attributes(attr.Name) _
                        .JoinIterates(attr.Value) _
                        .ToArray
                Else
                    Call attributes.Add(attr.Name, attr.Value)
                End If
            Next
        End Sub

        Public Sub AddCustomAttribute(name As String, ParamArray value As String())
            If attributes Is Nothing Then
                attributes = New Dictionary(Of String, String())
            End If

            If attributes.ContainsKey(name) Then
                attributes(name) = attributes(name) _
                    .JoinIterates(value) _
                    .ToArray
            Else
                Call attributes.Add(name, value)
            End If
        End Sub

        ''' <summary>
        ''' Just set value, this function use for the package parser
        ''' </summary>
        ''' <param name="attrs"></param>
        ''' <remarks>
        ''' use the <see cref="AddCustomAttribute(String, String())"/> if want to 
        ''' add a specific single attribute data
        ''' </remarks>
        Friend Sub SetAttributes(attrs As Dictionary(Of String, String()))
            If attributes Is Nothing Then
                attributes = New Dictionary(Of String, String())
            End If

            For Each attr As KeyValuePair(Of String, String()) In attrs.SafeQuery
                attributes(attr.Key) = attr.Value
            Next
        End Sub
    End Class
End Namespace
