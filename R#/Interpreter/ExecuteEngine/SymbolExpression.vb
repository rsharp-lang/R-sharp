#Region "Microsoft.VisualBasic::d58753533c464f513d3f34e0782c515d, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/SymbolExpression.vb"

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
    '    Code Lines: 20
    ' Comment Lines: 18
    '   Blank Lines: 6
    '     File Size: 1.54 KB


    '     Class SymbolExpression
    ' 
    '         Sub: AddCustomAttributes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

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

        ''' <summary>
        ''' the annotation data from the attribute annotation, example as:
        ''' 
        ''' ```R
        ''' [@name "value"]
        ''' [@name1 "value2"]
        ''' const symbol = ...
        ''' ```
        ''' </summary>
        Protected Friend ReadOnly attributes As New Dictionary(Of String, String())

        ''' <summary>
        ''' The symbol name of current symbol object
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function GetSymbolName() As String

        ''' <summary>
        ''' Get all attribute name that tagged with current symbol object.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetAttributeNames() As IEnumerable(Of String)
            Return attributes.Keys
        End Function

        ''' <summary>
        ''' Get values that associated with the current symbol object
        ''' </summary>
        ''' <param name="name">the attribute name string for get the value</param>
        ''' <returns></returns>
        Public Function GetAttributeValue(name As String) As IEnumerable(Of String)
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

        ''' <summary>
        ''' Just set value, this function use for the package parser
        ''' </summary>
        ''' <param name="attrs"></param>
        Friend Sub SetAttributes(attrs As Dictionary(Of String, String()))
            For Each attr As KeyValuePair(Of String, String()) In attrs.SafeQuery
                attributes(attr.Key) = attr.Value
            Next
        End Sub
    End Class
End Namespace
