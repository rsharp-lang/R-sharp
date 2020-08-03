#Region "Microsoft.VisualBasic::91d2d5002aa6a1f264c2d81e1f0ae7b6, R#\Extensions.vb"

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
'     Function: AsRReturn, GetEncoding, GetObject, GetString, SafeCreateColumns
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
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
    ''' Get text encoding value, returns <see cref="Encoding.Default"/> by default.
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

    <Extension>
    Public Function SafeCreateColumns(Of T)(data As IEnumerable(Of T), getKey As Func(Of T, String), getArray As Func(Of T, String())) As Dictionary(Of String, Array)
        Dim cols As New Dictionary(Of String, Array)
        Dim key As String
        Dim index As Integer = Scan0

        For Each col As T In data
            key = getKey(col)
            index += 1

            If key Is Nothing Then
                key = $"[, {index}]"
            End If

            If cols.ContainsKey(key) Then
                For i As Integer = 0 To 10000
                    Dim newKey = key & "." & i

                    If Not cols.ContainsKey(newKey) Then
                        key = newKey
                        Exit For
                    End If
                Next
            End If

            cols.Add(key, getArray(col))
        Next

        Return cols
    End Function

    <Extension>
    Public Function EvaluateFramework(Of T, TOut)(env As Environment, x As Object, eval As Func(Of T, TOut)) As Object
        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is list Then
            Return DirectCast(x, list) _
                .AsGeneric(Of T)(env) _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return CObj(eval(a.Value))
                              End Function) _
                .DoCall(Function(list)
                            Return New list With {
                                .slots = list
                            }
                        End Function)
        ElseIf TypeOf x Is vector Then
            With DirectCast(x, vector)
                Return New vector(
                    names:= .getNames,
                    input:= .data.AsObjectEnumerator(Of T).Select(eval).ToArray,
                    type:=RType.GetRSharpType(GetType(TOut)),
                    env:=env
                )
            End With
        ElseIf x.GetType.IsArray Then
            Return New vector(
                input:=DirectCast(x, Array).AsObjectEnumerator(Of T).Select(eval).ToArray,
                type:=RType.GetRSharpType(GetType(TOut))
            )
        ElseIf TypeOf x Is T Then
            Return eval(DirectCast(x, T))
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(T), x.GetType, env), env)
        End If
    End Function
End Module
