#Region "Microsoft.VisualBasic::8707d6dbf9e3937dcbf91a3f3480f9c6, E:/GCModeller/src/R-sharp/R#//Runtime/System/TypeCode.vb"

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

    '   Total Lines: 160
    '    Code Lines: 81
    ' Comment Lines: 64
    '   Blank Lines: 15
    '     File Size: 5.14 KB


    '     Enum TypeCodes
    ' 
    '         [boolean], [closure], [dataframe], [double], [integer]
    '         [raw], [string], clr_delegate, environment
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum What
    ' 
    '         [double], [integer], character, complex, int
    '         logical, numeric, raw
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module WhatReader
    ' 
    '         Function: ClassWhat, LoadWhat, ReadWhat
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Numerics
Imports System.Reflection
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    ''' <summary>
    ''' The R# types (byte)
    ''' </summary>
    Public Enum TypeCodes As Byte

        ''' <summary>
        ''' Unknown or invalid
        ''' </summary>
        NA = 0

        ''' <summary>
        ''' Object type in R#, any other CLR type.(使用这个类型来表示没有类型约束)
        ''' </summary>
        [generic] = 1
        ''' <summary>
        ''' 函数类型
        ''' </summary>
        [closure]
        [formula] = 4

        ''' <summary>
        ''' the runtime environment
        ''' </summary>
        environment

        ''' <summary>
        ''' type code for <see cref="RMethodInfo"/> and .NET clr <see cref="MethodInfo"/>
        ''' </summary>
        clr_delegate

#Region "PrimitiveTypes"

        ''' <summary>
        ''' Class type in R#
        ''' </summary>
        ''' <remarks>
        ''' The R# list is the Dictionary type in VB.NET
        ''' 
        ''' R#之中的list类型就是.NET之中的字典类型，对于所有的list类型的R对象而言，
        ''' 尽管他们的属性的数量和名称不相同，但是都是list字典类型
        ''' </remarks>
        [list] = 100
        ''' <summary>
        ''' <see cref="Integer"/> vector
        ''' </summary>
        [integer]
        ''' <summary>
        ''' <see cref="Double"/> numeric vector
        ''' </summary>
        [double]
        ''' <summary>
        ''' <see cref="String"/> vector
        ''' </summary>
        [string]
        ''' <summary>
        ''' <see cref="Boolean"/> vector
        ''' </summary>
        [boolean]
        ''' <summary>
        ''' A data table liked compound type
        ''' </summary>
        [dataframe]
        ''' <summary>
        ''' <see cref="Byte"/>
        ''' </summary>
        [raw]
#End Region
    End Enum

    ''' <summary>
    ''' just enumerate the R# primitive type
    ''' </summary>
    ''' <remarks>
    ''' this enum flag code used for the export functions parameters,
    ''' example as <see cref="Internal.Invokes.file.readBin"/>;
    ''' for compose type, use <see cref="TypeCodes"/>
    ''' </remarks>
    Public Enum What
        numeric
        [double]
        [integer]
        int
        logical
        complex
        character
        raw
    End Enum

    Public Module WhatReader

        ''' <summary>
        ''' convert the function output of <see cref="numeric(Integer, Environment)"/>, 
        ''' <see cref="ints(Integer, Environment)"/> to flag <see cref="What"/>
        ''' </summary>
        ''' <param name="a"></param>
        ''' <returns></returns>
        Public Function ClassWhat(a As Array) As What
            If a Is Nothing Then
                Return Nothing
            Else
                Select Case a.GetType
                    Case GetType(Single) : Return What.numeric
                    Case GetType(Double) : Return What.double
                    Case GetType(Integer), GetType(Long) : Return What.integer
                    Case GetType(Boolean) : Return What.logical
                    Case GetType(Complex) : Return What.complex
                    Case GetType(String), GetType(Char) : Return What.character
                    Case GetType(Byte) : Return What.raw
                    Case Else
                        Throw New NotImplementedException
                End Select
            End If
        End Function

        ''' <summary>
        ''' load what data from byte vector
        ''' </summary>
        ''' <param name="what"></param>
        ''' <returns></returns>
        Public Function LoadWhat(what As What) As Func(Of Byte(), Object)

        End Function

        Public Function ReadWhat(what As What) As Func(Of BinaryReader, Object)
            Select Case what
                Case What.character
                    Return Function(br) br.ReadChar
                Case What.complex
                    Return Function(br)
                               Dim r = br.ReadDouble()
                               Dim i = br.ReadDouble
                               Return New Complex(r, i)
                           End Function
                Case What.double
                    Return Function(br) br.ReadDouble
                Case What.int
                    Return Function(br) br.ReadInt32
                Case What.integer
                    Return Function(br) br.ReadInt32
                Case What.logical
                    Return Function(br) br.ReadBoolean
                Case What.numeric
                    Return Function(br) br.ReadSingle
                Case What.raw
                    Return Function(br) br.ReadByte
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

    End Module
End Namespace
