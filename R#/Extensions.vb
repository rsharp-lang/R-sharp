#Region "Microsoft.VisualBasic::344fba8b739ce8de60041c4e329155fb, R#\Extensions.vb"

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
'     Function: AsRReturn, GetObject
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
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

    <Extension>
    Public Function GetObject(Of T As {New, Class})(list As list) As T
        Return RListObjectArgumentAttribute.CreateArgumentModel(Of T)(list.slots)
    End Function

    Public Function GetEncoding(val As Object) As Encoding
        If val Is Nothing Then
            Return Encoding.Default
        ElseIf TypeOf val Is Encoding Then
            Return val
        ElseIf TypeOf val Is Encodings Then
            Return DirectCast(val, Encodings).CodePage
        ElseIf val.GetType Like BinaryExpression.characters Then
            Dim encodingName$ = Scripting.ToString(Runtime.asVector(Of String)(val).AsObjectEnumerator.First)
            Dim encodingVal As Encoding = TextEncodings.ParseEncodingsName(encodingName).CodePage

            Return encodingVal
        Else
            Return Encoding.Default
        End If
    End Function
End Module
