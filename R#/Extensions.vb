#Region "Microsoft.VisualBasic::0188cc5da7281265aa1f0e424be66515, R#\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: AsRReturn, GetEncoding, GetObject, GetString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<HideModuleName>
Public Module Extensions

    <DebuggerStepThrough>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsRReturn(Of T)(x As T) As RReturn
        Return New RReturn(x)
    End Function

    ''' <summary>
    ''' Create a specific .NET object from given data
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list">the object property data value collection.</param>
    ''' <returns></returns>
    <Extension>
    Public Function GetObject(Of T As {New, Class})(list As list) As T
        Return RListObjectArgumentAttribute.CreateArgumentModel(Of T)(list.slots)
    End Function

    ''' <summary>
    ''' Get text encoding value
    ''' </summary>
    ''' <param name="val"></param>
    ''' <returns></returns>
    Public Function GetEncoding(val As Object) As Encoding
        If val Is Nothing Then
            Return Encoding.Default
        ElseIf TypeOf val Is Encoding Then
            Return val
        ElseIf TypeOf val Is Encodings Then
            Return DirectCast(val, Encodings).CodePage
        ElseIf val.GetType Like RType.characters Then
            Dim encodingName$ = Scripting.ToString(Runtime.asVector(Of String)(val).AsObjectEnumerator.First)
            Dim encodingVal As Encoding = TextEncodings.ParseEncodingsName(encodingName).CodePage

            Return encodingVal
        Else
            Return Encoding.Default
        End If
    End Function

    <Extension>
    Public Function GetString(list As list, key$, Optional default$ = Nothing) As String
        If Not list.hasName(key) Then
            Return [default]
        Else
            Return Scripting.ToString(Runtime.getFirst(list(key)))
        End If
    End Function
End Module
