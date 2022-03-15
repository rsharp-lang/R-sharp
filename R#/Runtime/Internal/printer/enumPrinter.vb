#Region "Microsoft.VisualBasic::260763df5936849c29d756edd1654d10, R-sharp\R#\Runtime\Internal\printer\enumPrinter.vb"

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


     Code Statistics:

        Total Lines:   50
        Code Lines:    32
        Comment Lines: 8
        Blank Lines:   10
        File Size:     1.80 KB


    '     Module enumPrinter
    ' 
    '         Function: defaultValueToString, printClass, printEnumValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.ConsolePrinter

    Public Module enumPrinter

        ReadOnly printerCache As New Dictionary(Of Type, Func(Of Object, String))

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj">
        ''' This object should be an enum value.
        ''' </param>
        ''' <returns></returns>
        Public Function printClass(obj As Object) As String
            Return printerCache.ComputeIfAbsent(obj.GetType, AddressOf printEnumValue)(obj)
        End Function

        Public Function printEnumValue(enumType As Type) As Func(Of Object, String)
            Dim type As REnum = REnum.GetEnumList(enumType)
            Dim base As Type = type.baseType

            Return Function(obj) As String
                       Dim describ$ = DirectCast(obj, [Enum]).Description
                       Dim print$ = $"{{{base.Name.ToLower} {type.IntValue(obj)}}} {obj.ToString}"

                       If describ = obj.ToString Then
                           Return print
                       Else
                           Return $"{print} #{describ}"
                       End If
                   End Function
        End Function

        Public Function defaultValueToString([default] As Object, type As Type) As String
            Dim s As String = [default].ToString

            If s.IsPattern("\d+") Then
                ' is flag combinations
                s = GetAllEnumFlags([default], type) _
                    .Select(Function(flag) flag.ToString) _
                    .JoinBy("|")
            End If

            Return $"[{s}]"
        End Function
    End Module
End Namespace
