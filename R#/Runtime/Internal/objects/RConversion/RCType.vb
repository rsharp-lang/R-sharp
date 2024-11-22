﻿#Region "Microsoft.VisualBasic::42dea2af50fa35d27c921a19c5501afe, R#\Runtime\Internal\objects\RConversion\RCType.vb"

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
    '    Code Lines: 186 (73.52%)
    ' Comment Lines: 31 (12.25%)
    '    - Xml Docs: 32.26%
    ' 
    '   Blank Lines: 36 (14.23%)
    '     File Size: 11.99 KB


    '     Class RCType
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CastToEnum, castUnsure, CTypeDynamic, GetCType, hasInterfaceCast
    '                   hasTypeCast, IsNALiteralValue, NADefault
    ' 
    '         Sub: AddCType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Internal.Object.Converts

    Public NotInheritable Class RCType

        Friend ReadOnly castFunc As New Dictionary(Of Type, Dictionary(Of Type, Func(Of Object, Object)))

        Shared ReadOnly interfaceCast As New RCType
        Shared ReadOnly typeCast As New RCType

        Private Sub New()
        End Sub

        Shared Sub New()
            Call interfaceCast.AddCType(GetType(ISequenceData(Of Char, String)), GetType(String), Function(seq) DirectCast(seq, ISequenceData(Of Char, String)).SequenceData)
            ' get script file path from the symbol object
            Call typeCast.AddCType(GetType(MagicScriptSymbol), GetType(String), Function(script) DirectCast(script, MagicScriptSymbol).fullName)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCType(from As Type, [to] As Type) As Func(Of Object, Object)
            Return castFunc(from)([to])
        End Function

        Public Sub AddCType(from As Type, [to] As Type, cast As Func(Of Object, Object))
            If Not castFunc.ContainsKey(from) Then
                castFunc.Add(from, New Dictionary(Of Type, Func(Of Object, Object)))
            End If

            castFunc(from)([to]) = cast
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function hasInterfaceCast([interface] As Type, [to] As Type) As Boolean
            Return interfaceCast.castFunc.ContainsKey([interface]) AndAlso interfaceCast.castFunc([interface]).ContainsKey([to])
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function hasTypeCast(objType As Type, [to] As Type) As Boolean
            Return typeCast.castFunc.ContainsKey(objType) AndAlso typeCast.castFunc(objType).ContainsKey([to])
        End Function

        ''' <summary>
        ''' If target <paramref name="type"/> is <see cref="Object"/>, then this function 
        ''' will stop the narrowing conversion from <see cref="vbObject"/> wrapper to 
        ''' object type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type">Target data type to cast in R# runtime</param>
        ''' <returns>
        ''' an error <see cref="Message"/> will be returns if the conversion error happends
        ''' </returns>
        Public Shared Function CTypeDynamic(obj As Object, type As Type, env As Environment) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf type Is GetType(vbObject) Then
                Return asObject(obj)
            ElseIf obj.GetType Is GetType(vbObject) Then
                obj = DirectCast(obj, vbObject).target
            End If
RE0:
            Dim objType As Type = obj.GetType

            If objType Is type Then
                Return obj
            ElseIf objType.IsArray AndAlso Not type.IsArray Then
                obj = [single](obj)

                If obj Is Nothing AndAlso DataFramework.IsNumericType(type) Then
                    Return CTypeDynamic(0, type, env)
                Else
                    objType = obj.GetType
                End If
            ElseIf objType Is GetType(vector) AndAlso Not type.IsArray Then
                obj = [single](obj)
                objType = obj.GetType
            End If

            If (objType Is GetType(vector) AndAlso DirectCast(obj, vector).length = 0) OrElse
                (objType Is GetType(list) AndAlso DirectCast(obj, list).length = 0) Then

                If type Is GetType(String) Then
                    Return Nothing
                End If
            End If

            If type Is GetType(Boolean) AndAlso objType Like RType.characters Then
                Return any.ToString([single](obj)).ParseBoolean
            End If

            If objType Is GetType(RDispose) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, RDispose).Value

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf objType Is GetType(AutoFileSave) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, AutoFileSave).data

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf objType Is GetType(list) AndAlso type.ImplementInterface(GetType(IDictionary)) Then
                ' cast R# list object to any dictionary table object???
                Return DirectCast(obj, list).CTypeList(type, env)
            ElseIf objType.ImplementInterface(GetType(IDictionary)) AndAlso type Is GetType(list) Then
                Dim list As New list With {
                    .slots = New Dictionary(Of String, Object)
                }
                Dim raw As IDictionary = DirectCast(obj, IDictionary)

                For Each itemKey As Object In raw.Keys
                    list.slots.Add(itemKey.ToString, raw.Item(itemKey))
                Next

                Return list
            ElseIf type.IsEnum Then
                Return CastToEnum(obj, type, env)
            ElseIf objType Is GetType(Environment) AndAlso type Is GetType(GlobalEnvironment) Then
                ' fix the type mismatch bugs for passing value to 
                ' a API parameter which its data type is a global 
                ' environment.
                Return DirectCast(obj, Environment).globalEnvironment
            ElseIf makeObject.isObjectConversion(type, obj) Then
                Return makeObject.createObject(type, obj, env)
            ElseIf objType.IsArray AndAlso type.IsArray Then
                Return Runtime.asVector(obj, type.GetElementType, env)
            ElseIf type.IsArray AndAlso type.GetElementType Is objType Then
                Dim array As Array = Array.CreateInstance(objType, 1)
                array.SetValue(obj, Scan0)
                Return array
            End If

            ' unsure about this type cast bugs
            ' Error in <globalEnvironment> -> InitializeEnvironment -> for_loop_[1517] -> "insert" -> insert
            ' 1. InvalidCastException: Conversion from type 'vector' to type 'String' is not valid.
            ' 2. stackFrames:
            '      at Microsoft.VisualBasic.CompilerServices.Conversions.ObjectUserDefinedConversion(Object Expression, type TargetType)
            '      at Microsoft.VisualBasic.CompilerServices.Conversions.ChangeType(Object Expression, type TargetType, Boolean Dynamic)
            '      at SMRUCC.Rsharp.Runtime.Internal.Object.Converts.RCType.CTypeDynamic(Object obj, type type, Environment env) in D:\GCModeller\src\R-sharp\R#\Runtime\Internal\objects\RConversion\RCType.vb:line 156
            ' 
            ' R# Source: Call "insert"(& kb, [&lipid]:&LM_ID, "lipidmaps", &meta, "selfReference" <- False)

            ' Query.R#_interop: .insert at graphR.dll: line <unknown>
            ' SMRUCC/R#.call_function."insert" at 02.lipidmaps.R:line 90
            ' SMRUCC/R#.forloop.for_loop_[1517] at 02.lipidmaps.R:line 36
            ' SMRUCC/R#.n/a.InitializeEnvironment at 02.lipidmaps.R:line 0
            ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a
            If TypeOf obj Is vector Then
                obj = DirectCast(obj, vector).data
                GoTo RE0
            ElseIf obj Is GetType(Void) AndAlso type IsNot GetType(Type) Then
                Dim warn As String = $"literal NA has been cast to nothing while required of target should be {type.FullName}!"

                ' cast NA to nothing
                If env Is Nothing Then
                    Call warn.Warning
                Else
                    Call env.AddMessage(warn, MSG_TYPES.WRN)
                End If

                Return Nothing
            End If

            Try
                Return castUnsure(obj, objType, type, env)
            Catch ex As Exception
                Return Internal.debug.stop(ex, env, suppress:=True)
            End Try
        End Function

        Private Shared Function castUnsure(obj As Object, objType As Type, type As Type, env As Environment)
            If obj.GetType.IsArray Then
                If obj.GetType.GetElementType Is type Then
                    If DirectCast(obj, Array).Length = 0 Then
                        Return Nothing
                    ElseIf DirectCast(obj, Array).Length > 1 Then
                        Call env.AddMessage("target array contains multiple elements, while the target conversion type is a single scalar element...")
                    End If

                    Return DirectCast(obj, Array).GetValue(Scan0)
                ElseIf type Is GetType(list) Then
                    Dim list As New list With {.slots = New Dictionary(Of String, Object)}
                    Dim arr As Array = obj

                    ' enable simple array cast to list
                    For i As Integer = 0 To arr.Length - 1
                        Call list.add($"X_{list.length + 1}", arr(i))
                    Next

                    Return list
                Else
                    Return Conversion.CTypeDynamic(obj, type)
                End If
            ElseIf hasTypeCast(objType, type) Then
                Return typeCast.GetCType(objType, [to]:=type)(obj)
            Else
                ' create type cast cache at here?
                For Each i As Type In objType.GetInterfaces
                    If hasInterfaceCast(i, type) Then
                        Call typeCast.AddCType(objType, [to]:=type, cast:=interfaceCast.castFunc(i)(type))
                        Return interfaceCast.castFunc(i)(type)(obj)
                    End If
                Next

                Return Conversion.CTypeDynamic(obj, type)
            End If
        End Function

        Public Shared Function CastToEnum(obj As Object, type As Type, env As Environment) As Object
            Dim REnum As REnum = REnum.GetEnumList(type)

            If obj.GetType Is GetType(String) Then
                If REnum.hasName(obj) Then
                    Return REnum.GetByName(obj)
                Else
                    Return debug.stop($"Can not convert string '{obj}' to enum type: {REnum.raw.FullName}", env)
                End If
            ElseIf obj.GetType.GetRTypeCode = TypeCodes.integer Then
                Return REnum.getByIntVal(obj)
            Else
                Return debug.stop($"Can not convert type '{obj.GetType.FullName}' to enum type: {REnum.raw.FullName}", env)
            End If
        End Function

        Public Shared Function NADefault(type As RType) As Object
            Select Case type.mode
                Case TypeCodes.boolean : Return False
                Case TypeCodes.double : Return Double.NaN
                Case TypeCodes.integer : Return Long.MinValue
                Case TypeCodes.string : Return ""
                Case Else
                    Return Nothing
            End Select
        End Function

        Public Shared Function IsNALiteralValue(obj As Object) As Boolean
            If obj Is Nothing OrElse Not TypeOf obj Is Type Then
                Return False
            Else
                Return obj Is GetType(Void)
            End If
        End Function
    End Class
End Namespace
