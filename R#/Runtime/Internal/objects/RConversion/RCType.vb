#Region "Microsoft.VisualBasic::d6a23d600f7dcd1248b755fba40b4261, R-sharp\R#\Runtime\Internal\objects\RConversion\RCType.vb"

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

    '   Total Lines: 124
    '    Code Lines: 94
    ' Comment Lines: 12
    '   Blank Lines: 18
    '     File Size: 5.41 KB


    '     Class RCType
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CastToEnum, CTypeDynamic
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Development.Components
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Internal.Object.Converts

    Public NotInheritable Class RCType

        Private Sub New()
        End Sub

        ''' <summary>
        ''' If target <paramref name="type"/> is <see cref="Object"/>, then this function 
        ''' will stop the narrowing conversion from <see cref="vbObject"/> wrapper to 
        ''' object type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <returns>
        ''' an error message will be returns if the conversion error happends
        ''' </returns>
        Public Shared Function CTypeDynamic(obj As Object, type As Type, env As Environment) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf type Is GetType(vbObject) Then
                Return asObject(obj)
            End If

            Dim objType As Type = obj.GetType

            If objType Is type Then
                Return obj
            ElseIf objType.IsArray AndAlso Not type.IsArray Then
                obj = [single](obj)
                objType = obj.GetType
            ElseIf objType Is GetType(vector) AndAlso Not type.IsArray Then
                obj = [single](obj)
                objType = obj.GetType
            End If

            If (objType Is GetType(vector) AndAlso DirectCast(obj, vector).length = 0) OrElse (objType Is GetType(list) AndAlso DirectCast(obj, list).length = 0) Then
                If type Is GetType(String) Then
                    Return Nothing
                End If
            End If

            If type Is GetType(Boolean) AndAlso objType Like RType.characters Then
                Return any.ToString([single](obj)).ParseBoolean
            End If

            If objType Is GetType(vbObject) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, vbObject).target

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf objType Is GetType(RDispose) AndAlso Not type Is GetType(Object) Then
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

            Try
                Return Conversion.CTypeDynamic(obj, type)
            Catch ex As Exception
                Return Internal.debug.stop(ex, env)
            End Try
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

    End Class
End Namespace
