#Region "Microsoft.VisualBasic::0772f9753bc830839879d41c5beda055, R#\Test\interoptest.vb"

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

    '   Total Lines: 95
    '    Code Lines: 66 (69.47%)
    ' Comment Lines: 7 (7.37%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 22 (23.16%)
    '     File Size: 2.90 KB


    ' Module interoptest
    ' 
    '     Sub: enumParameterTest, enumTest, Main, testEnumArgs
    '     Enum eeee
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class TestContainer
    ' 
    '     Properties: name
    ' 
    '     Function: setName, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Interop

Module interoptest

    Dim R As New RInterpreter With {.debug = False}

    Sub Main()
        Call enumParameterTest()
        Call enumTest()

        Call R.Add("x", New TestContainer)
        Call R.Evaluate("x <- as.object(x)")
        Call R.Print("x")

        Call R.Evaluate("x$name <- '9999'")
        Call R.Print("`name value of x is ${x$name}.`")
        Call R.Evaluate("x[['name']] <- '8848'")
        Call R.Print("`name value of x is ${x$name}.`")
        Call R.Print("x")

        ' Call R.Print("do.call")
        Call R.Print("x$setName")

        Call R.Evaluate("x :> do.call(calls = 'setName', newName = 'ABCCCCD')")
        Call R.Print("`name value of x is ${x[['name']]}.`")

        Pause()
    End Sub

    Sub enumTest()
        Dim e As REnum = REnum.GetEnumList(GetType(MSG_TYPES))

        Call Console.WriteLine(ConsolePrinter.enumPrinter.printClass(MSG_TYPES.INF))
        ' Call Console.WriteLine(ConsolePrinter.enumPrinter.defaultValueToString(eeee.a Or eeee.b Or eeee.c Or eeee.d Or eeee.e Or eeee.f Or eeee.g Or eeee.h Or eeee.t, GetType(eeee)))

        Dim method As MethodInfo = GetType(interoptest).GetMethod(NameOf(testEnumArgs))
        Dim type As REnum = REnum.GetEnumList(GetType(eeee))
        Dim flags = eeee.a Or eeee.b Or eeee.c

        Call method.Invoke(Nothing, {flags})
        Call method.Invoke(Nothing, {type.getByIntVal(CInt(flags))})
        Call method.Invoke(Nothing, {eeee.t})
        Call method.Invoke(Nothing, {8})
        Call method.Invoke(Nothing, {type.GetByName("f")})

        Pause()
    End Sub

    Sub enumParameterTest()
        Call R.Add("display", GetType(interoptest).GetMethod(NameOf(testEnumArgs)), target:=Nothing)
        Call R.Evaluate("display('f')")

        Pause()
    End Sub

    Sub testEnumArgs(e As eeee)
        Call Console.WriteLine(ConsolePrinter.defaultValueToString(e, GetType(eeee)))
    End Sub

    Enum eeee As Long
        a = 1
        b = a * 2
        c = b * 2
        d = c * 2
        e = d * 2
        f = e * 2
        g = f * 2
        h = g * 2
        t = h * 2
    End Enum
End Module

Public Class TestContainer

    Public Property name As String

    ''' <summary>
    ''' # Test for R# interop
    ''' </summary>
    ''' <param name="newName">function parameter for set name property value</param>
    ''' <returns>The new name</returns>
    Public Function setName(newName As String) As String
        name = newName
        Return newName
    End Function

    Public Overrides Function ToString() As String
        Return $"Container: {name}"
    End Function

End Class
