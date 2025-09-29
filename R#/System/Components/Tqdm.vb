#Region "Microsoft.VisualBasic::72e9ba89d1c7a9e3653e314b88cfbaea, R#\System\Components\Tqdm.vb"

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

    '   Total Lines: 66
    '    Code Lines: 43 (65.15%)
    ' Comment Lines: 10 (15.15%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 13 (19.70%)
    '     File Size: 2.13 KB


    '     Class tqdmList
    ' 
    '         Function: getKeys, getValue, pullData, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Development.Components

    Public Class tqdmList

        Friend list As list

        Default Public ReadOnly Property Item(name As String) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return list.getByName(name)
            End Get
        End Property

        ''' <summary>
        ''' wrap tqdm from this function
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Iterator Function getKeys() As IEnumerable(Of String)
            Dim bar As Tqdm.ProgressBar = Nothing
            Dim n As Integer = 0
            Dim size As Integer = list.length
            Dim d As Integer = size / 50

            ' 20240215 
            ' for avoid the error: ArgumentOutOfRangeException: Count cannot be less than zero. (Parameter 'count')
            If size <= 0 Then
                Return
            End If

            For Each key As String In Tqdm.Wrap(list.slotKeys, bar:=bar, wrap_console:=App.EnableTqdm)
                If d <= 1 OrElse (n Mod d = 0) Then
                    Call bar.SetLabel(key)
                End If

                n += 1

                Yield key
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getValue(key As Object) As Object
            Return list.getByName(key.ToString)
        End Function

        ''' <summary>
        ''' populate of the data collection with tqdm progress bar
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function pullData() As IEnumerable(Of Object)
            For Each key As String In getKeys()
                Yield list.getByName(key)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return list.ToString
        End Function

    End Class
End Namespace
