#Region "Microsoft.VisualBasic::04f20a2f3c9aacfa430e8f61f3b249c8, R#\Runtime\Environment\SymbolSet.vb"

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

    '   Total Lines: 133
    '    Code Lines: 75 (56.39%)
    ' Comment Lines: 40 (30.08%)
    '    - Xml Docs: 95.00%
    ' 
    '   Blank Lines: 18 (13.53%)
    '     File Size: 4.78 KB


    '     Class SymbolSet
    ' 
    '         Properties: SymbolNames
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Add, CheckSymbolExists, EnumerateKeyTuples, FindSymbol, GetEnumerator
    '                   IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Delete
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime

    ''' <summary>
    ''' A pool set of the R# runtime symbols
    ''' </summary>
    Public Class SymbolSet : Implements IEnumerable(Of Symbol)

        ReadOnly symbols As Dictionary(Of String, Symbol)

        ''' <summary>
        ''' get all variable symbol names inside current symbol list
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SymbolNames As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return symbols.Keys.OrderBy(Function(a) a).ToArray
            End Get
        End Property

        Default Public Property Item(name As String) As Symbol
            Get
                If symbols.ContainsKey(name) Then
                    Return symbols(name)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As Symbol)
                symbols(name) = value
            End Set
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(symbols As IEnumerable(Of Symbol))
            Me.symbols = symbols.ToDictionary(Function(s) s.name)
        End Sub

        ''' <summary>
        ''' make symbol list copy
        ''' </summary>
        ''' <param name="copy"></param>
        Sub New(copy As SymbolSet)
            Me.symbols = New Dictionary(Of String, Symbol)(copy.symbols)
        End Sub

        Sub New()
            Me.symbols = New Dictionary(Of String, Symbol)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function EnumerateKeyTuples() As IEnumerable(Of KeyValuePair(Of String, Symbol))
            Return symbols.AsEnumerable
        End Function

        ''' <summary>
        ''' Check of the given symbol name is existsed inside current symbols collection
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CheckSymbolExists(name As String) As Boolean
            Return symbols.ContainsKey(name)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Delete(name As String)
            Call symbols.Remove(name)
        End Sub

        ''' <summary>
        ''' Add a new symbol into current symbol list
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <returns></returns>
        Public Function Add(symbol As Symbol) As SymbolSet
            symbols(symbol.name) = symbol
            Return Me
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="symbol"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this add function may cause the in-consist name mapping problem. due to the 
        ''' reason of key name parameter value maybe different with the 
        ''' <see cref="Symbol.name"/> value.
        ''' </remarks>
        Public Function Add(name As String, symbol As Symbol) As SymbolSet
            symbols(name) = symbol
            Return Me
        End Function

        ''' <summary>
        ''' try get value from the symbol set dictionary object
        ''' </summary>
        ''' <param name="name">a given symbol name to find associated symbol value</param>
        ''' <returns>
        ''' this function returns nothing if the given symbol name is not found insdie this collection
        ''' </returns>
        Public Function FindSymbol(name As String) As Symbol
            If symbols.ContainsKey(name) Then
                Return symbols(name)
            Else
                Return Nothing
            End If
        End Function

        Public Overrides Function ToString() As String
            Dim const_s As Integer = symbols.Values.Where(Function(s) s.readonly).Count
            Dim let_s As Integer = symbols.Count - const_s

            Return $"{symbols.Count} symbols inside environment, {const_s} binding lock and {let_s} unlock binding."
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Symbol) Implements IEnumerable(Of Symbol).GetEnumerator
            For Each s As Symbol In symbols.Values
                Yield s
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
