#Region "Microsoft.VisualBasic::62f85ac2cb60bee588b271641f81ec78, R#\System\Components\Encoder.vb"

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

    '     Module Encoder
    ' 
    '         Function: DigestRSharpObject, GetObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace System.Components

    Module Encoder

        ''' <summary>
        ''' digest R# object as underlying .NET object
        ''' </summary>
        ''' <param name="Robj"></param>
        ''' <returns></returns>
        Public Function GetObject(Robj As Object) As Object
            If TypeOf Robj Is vector Then
                Dim array As New List(Of Object)

                For Each x As Object In DirectCast(Robj, vector).data
                    Call array.Add(Encoder.GetObject(x))
                Next

                Return array.ToArray
            ElseIf TypeOf Robj Is list Then
                Dim list As New Dictionary(Of String, Object)

                For Each slot In DirectCast(Robj, list).slots
                    Call list.Add(slot.Key, Encoder.GetObject(slot.Value))
                Next

                Return list
            ElseIf TypeOf Robj Is vbObject Then
                Return Encoder.GetObject(DirectCast(Robj, vbObject).target)
            ElseIf TypeOf Robj Is dataframe Then
                Return DirectCast(Robj, dataframe).columns
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
