#Region "Microsoft.VisualBasic::5745a310fae6a58e68a2ecee708cc1c8, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/names.vb"

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

'   Total Lines: 253
'    Code Lines: 201
' Comment Lines: 14
'   Blank Lines: 38
'     File Size: 9.97 KB


'     Module names
' 
'         Function: checkChar, getArrayNames, getColNames, getNames, getRowNames
'                   makeNames, setColNames, (+2 Overloads) setNames, setRowNames, uniqueNames
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object

    Public Module names

        <Extension>
        Private Function checkChar(c As Char, allow_ As Boolean) As Char
            If c = "_"c AndAlso allow_ Then
                Return c
            End If

            If c >= "a"c AndAlso c <= "z"c Then
                Return c
            ElseIf c >= "A"c AndAlso c <= "Z"c Then
                Return c
            ElseIf c >= "0"c AndAlso c <= "9"c Then
                Return c
            Else
                Return "."c
            End If
        End Function

        <Extension>
        Public Function makeNames(nameList As IEnumerable(Of String), Optional unique As Boolean = False, Optional allow_ As Boolean = True) As String()
            Dim nameAll As New List(Of String)

            For Each name As String In nameList
                If name Is Nothing Then
                    name = "X"
                End If

                name = name _
                    .Select(Function(c) c.checkChar(allow_)) _
                    .CharString

                Call nameAll.Add(name)
            Next

            If unique Then
                Return nameAll.uniqueNames
            Else
                Return nameAll.ToArray
            End If
        End Function

        <Extension>
        Public Iterator Function uniqueNames(Of T As {INamedValue, Class})(data As IEnumerable(Of T)) As IEnumerable(Of T)
            Dim pool = data.SafeQuery.ToArray
            Dim names As String() = pool.Keys.uniqueNames

            For i As Integer = 0 To names.Length - 1
                pool(i).Key = names(i)
                Yield pool(i)
            Next
        End Function

        ''' <summary>
        ''' makes the name string unique by adding an additional numeric suffix
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns></returns>
        <Extension>
        Public Function uniqueNames(names As IEnumerable(Of String), <Out> Optional ByRef duplicated As String() = Nothing) As String()
            Dim nameUniques As New Dictionary(Of String, Counter)
            Dim duplicates As New List(Of String)

            For Each name As String In names
RE0:
                If nameUniques.ContainsKey(name) Then
                    nameUniques(name).Hit()
                    duplicates.Add(name)
                    name = name & "_" & nameUniques(name).Value
                    GoTo RE0
                Else
                    nameUniques.Add(name, Scan0)
                End If
            Next

            Erase duplicated

            If duplicates.Any Then
                duplicated = duplicates.ToArray
            End If

            Return nameUniques.Keys.ToArray
        End Function

        Public Function getNames([object] As Object, envir As Environment) As Object
            Dim type As Type = [object].GetType

            ' get names
            Select Case type
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).getNames
                Case GetType(vbObject)
                    Return DirectCast([object], vbObject).getNames
                Case GetType(vector)
                    Return DirectCast([object], vector).getNames
                Case Else
                    If type.ImplementInterface(Of IReflector) Then
                        Return DirectCast([object], IReflector).getNames
                    ElseIf type.IsArray Then
                        Dim objVec As Array = CLRVector.asObject([object])
                        Dim names As String() = getArrayNames(objVec)

                        If Not names Is Nothing Then
                            Return names
                        End If

                    ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                        Dim keys As vector = DirectCast([object], IDictionary) _
                            .Keys _
                            .DoCall(Function(a)
                                        Return New vector(GetType(String), a, env:=envir)
                                    End Function)

                        Return keys
                    End If

                    Return Internal.debug.stop({"unsupported!", "func: names", "type: " & type.FullName}, envir)
            End Select
        End Function

        <Extension>
        Private Function getArrayNames(objvec As Array) As String()
            If objvec _
                .AsObjectEnumerator _
                .All(Function(o)
                         Return o.GetType Is GetType(Group)
                     End Function) Then

                Return objvec.AsObjectEnumerator _
                    .Select(Function(g)
                                Return Scripting.ToString(DirectCast(g, Group).key, "NULL")
                            End Function) _
                    .ToArray
            ElseIf objvec _
                .AsObjectEnumerator _
                .All(Function(o)
                         Return o.GetType.ImplementInterface(GetType(INamedValue))
                     End Function) Then

                Return objvec.AsObjectEnumerator _
                    .Select(Function(o)
                                Return DirectCast(o, INamedValue).Key
                            End Function) _
                    .ToArray
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="names">This method will ensure that the value is a string vector</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function setNames([object] As Object, names As Array, env As Environment) As Object
            Dim raw_namelist As String() = CLRVector.asCharacter(names)
            Dim duplicated As String() = Nothing
            Dim namelist As String() = raw_namelist.uniqueNames(duplicated)

            If Not duplicated.IsNullOrEmpty Then
                ' 20240407 duplicated name was found in names list
                Call env.AddMessage({
                    $"{duplicated.Length} duplcated names was found! All these duplicated names({duplicated.Distinct.GetJson}) has been convert to unique names.",
                    $"duplicated names: {duplicated.Distinct.GetJson}"
                }, MSG_TYPES.WRN)
            End If

            ' set names
            Select Case [object].GetType
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).setNames(namelist, env)
                Case GetType(vector)
                    Return DirectCast([object], vector).setNames(namelist, env)
                Case Else
                    If [object].GetType.ImplementInterface(Of IDictionary) Then
                        Return DirectCast([object], IDictionary).setNames(namelist, env)
                    End If

                    Return Internal.debug.stop({
                        "set names for the given type of object is unsupported! please consider convert it as vector at first...",
                        "func: names",
                        "objtype: " & [object].GetType.FullName
                    }, env)
            End Select
        End Function

        <Extension>
        Private Function setNames(dict As IDictionary, namelist As String(), env As Environment) As Object
            Dim oldkeys = dict.Keys.ToArray(Of String)

            For i As Integer = 0 To dict.Count - 1
                Dim oldKey = oldkeys(i)
                Dim newKey = namelist(i)
                Dim value = dict(oldKey)

                dict.Remove(oldKey)
                dict.Add(newKey, value)
            Next

            Return dict
        End Function

        ''' <summary>
        ''' try to get the dataframe rownames
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function getRowNames([object] As Object, envir As Environment) As Object
            If [object].GetType Is GetType(dataframe) Then
                Return DirectCast([object], dataframe).rownames
            ElseIf [object].GetType.ImplementInterface(Of IdataframeReader) Then
                Return DirectCast([object], IdataframeReader).getRowNames
            Else
                ' get column names?
                Return getNames([object], envir)
            End If
        End Function

        Public Function setRowNames([object] As Object, namelist As Array, envir As Environment) As Object
            If TypeOf [object] Is dataframe Then
                Dim data As dataframe = DirectCast([object], dataframe)

                If data.nrows <> namelist.Length Then
                    Return Internal.debug.stop({
                        "row size is not matched on set new rownames!",
                        "nrows: " & data.nrows,
                        "given rownames size: " & namelist.Length
                    }, envir)
                Else
                    data.rownames = CLRVector.asCharacter(namelist)
                    Return data.rownames
                End If
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Function getColNames([object] As Object, envir As Environment) As Object
            If [object].GetType Is GetType(dataframe) Then
                Return DirectCast([object], dataframe).columns.Keys.ToArray
            Else
                Return getNames([object], envir)
            End If
        End Function

        Public Function setColNames([object] As Object, namelist As Array, envir As Environment) As Object
            If TypeOf [object] Is dataframe Then
                With DirectCast([object], dataframe)
                    Dim [raw] = .columns.ToArray
                    Dim [new] As New Dictionary(Of String, Array)
                    Dim names As String() = CLRVector.asCharacter(namelist)

                    If raw.Length <> names.Length Then
                        Return Internal.debug.stop(
                            {
                                $"Error in names(x) <- value:",
                                $"'names' attribute [{names.Length}] must be the same length as the vector [{raw.Length}]",
                                $"set column names error!"
                            }, envir)
                    End If

                    For i As Integer = 0 To names.Length - 1
                        [new].Add(names(i), raw(i).Value)
                    Next

                    .columns = [new]
                End With
            Else
                Return Internal.debug.stop(New NotImplementedException, envir)
            End If

            Return [object]
        End Function
    End Module
End Namespace
