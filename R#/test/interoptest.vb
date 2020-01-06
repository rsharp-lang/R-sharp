#Region "Microsoft.VisualBasic::b829c3b7f70266b344f2e9a80fe11ffd, R#\test\interoptest.vb"

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

' Module interoptest
' 
'     Sub: Main
' 
' Class TestContainer
' 
'     Properties: name
' 
'     Function: setName, ToString
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Internal

Module interoptest

    Dim R As New RInterpreter With {.debug = False}

    Sub Main()

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

        Pause()
    End Sub
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
