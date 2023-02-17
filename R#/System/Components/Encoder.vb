#Region "Microsoft.VisualBasic::8e4a58c1b38a6724b788c5381aa664ae, D:/GCModeller/src/R-sharp/R#//System/Components/Encoder.vb"

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

    '   Total Lines: 84
    '    Code Lines: 60
    ' Comment Lines: 13
    '   Blank Lines: 11
    '     File Size: 3.34 KB


    '     Module Encoder
    ' 
    '         Function: DigestRSharpObject, GetObject, TryHandleNonVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Development.Components

    Public Module Encoder

        ''' <summary>
        ''' R语言为向量化语言，但是其他的大部分编程语言不是向量化的
        ''' 所以在向量对象这里可能会存在一些bug
        ''' 我们基于大部分的语言所需求的数据都不是向量化的假设
        ''' 在这里将所有只包含有一个元素的向量认为是非向量数据
        ''' </summary>
        ''' <param name="vec"></param>
        ''' <returns></returns>
        Private Function TryHandleNonVector(vec As Array) As Object
            If vec.Length = 1 Then
                Return Encoder.GetObject(vec.GetValue(Scan0))
            Else
                Dim array As New List(Of Object)

                For Each x As Object In vec
                    Call array.Add(Encoder.GetObject(x))
                Next

                Return array.ToArray
            End If
        End Function

        ''' <summary>
        ''' digest R# object as underlying .NET object
        ''' </summary>
        ''' <param name="Robj"></param>
        ''' <returns></returns>
        Public Function GetObject(Robj As Object) As Object
            If (Not Robj Is Nothing) AndAlso Robj.GetType.IsArray Then
                Return TryHandleNonVector(DirectCast(Robj, Array))
            ElseIf TypeOf Robj Is vector Then
                Return TryHandleNonVector(DirectCast(Robj, vector).data)
            ElseIf TypeOf Robj Is list Then
                Dim list As New Dictionary(Of String, Object)

                If Not DirectCast(Robj, list).slots Is Nothing Then
                    For Each slot In DirectCast(Robj, list).slots
                        Call list.Add(slot.Key, Encoder.GetObject(slot.Value))
                    Next
                End If

                Return list
            ElseIf TypeOf Robj Is vbObject Then
                Return Encoder.GetObject(DirectCast(Robj, vbObject).target)
            ElseIf TypeOf Robj Is dataframe Then
                Dim raw = DirectCast(Robj, dataframe).columns
                Dim decode As New Dictionary(Of String, Object)

                For Each slot In raw
                    Call decode.Add(slot.Key, Encoder.GetObject(slot.Value))
                Next

                Return decode
            Else
                Return Robj
            End If
        End Function

        Public Function DigestRSharpObject(any As Object, env As Environment) As Object
            If any Is Nothing Then
                Return Nothing
            ElseIf TypeOf any Is vector Then
                Return DirectCast(any, vector).data
            ElseIf TypeOf any Is list Then
                Return DirectCast(any, list).slots
            ElseIf TypeOf any Is vbObject Then
                Return DirectCast(any, vbObject).target
            ElseIf TypeOf any Is pipeline Then
                Return DirectCast(any, pipeline).populates(Of Object)(env).ToArray
            ElseIf TypeOf any Is dataframe Then
                Return DirectCast(any, dataframe).columns
            Else
                Return any
            End If
        End Function
    End Module
End Namespace
