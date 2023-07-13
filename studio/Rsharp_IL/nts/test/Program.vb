#Region "Microsoft.VisualBasic::78f2496a78e6ca2f88036cd314f5d6b4, G:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts/test//Program.vb"

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

    '   Total Lines: 38
    '    Code Lines: 13
    ' Comment Lines: 19
    '   Blank Lines: 6
    '     File Size: 2.74 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.TypeScript
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Program
    Sub Main(args As String())
        Dim ts = New TypeScriptLoader
        Dim _global As Environment = GlobalEnvironment.defaultEmpty
        Dim println = _global.WriteLineHandler

        Call TypeScriptLoader.setup_jsEnv(_global.globalEnvironment)

        ' Dim script1 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test1.js", GlobalEnvironment.defaultEmpty)
        'Dim script3 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test3.js", GlobalEnvironment.defaultEmpty)
        ' Dim script4 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test4.js", GlobalEnvironment.defaultEmpty)
        ' Dim script5 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test5.js", GlobalEnvironment.defaultEmpty)
        ' Dim script6 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test6.js", GlobalEnvironment.defaultEmpty)
        ' Dim script7 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\run_test.js", _global)
        'Dim script71 As RProgram = ts.ParseScript("\GCModeller\src\R-sharp\test\jsTest\json_test.js", _global)
        'Dim script8 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test_for.js", GlobalEnvironment.defaultEmpty)
        ' Dim script9 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\invoke_test.js", _global)
        ' Dim script9 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\create_function.js", _global)
        ' Dim script11 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\function2.js", _global)
        'Dim script12 As RProgram = ts.ParseScript("\GCModeller\src\R-sharp\test\jsTest\imports.js", _global)
        ' Dim script13 = ts.ParseScript("require(GCModeller);", _global)
        ' Dim script14 As RProgram = ts.ParseScript("\GCModeller\src\interops\RNA-Seq\test.js", _global)
        ' Dim script15 As RProgram = ts.ParseScript("var linear = lm(Ct ~ At, data = data.frame(Ct, At), weights = 1 / (At ^ 2))", _global)
        ' Dim script16 As RProgram = ts.ParseScript("throw 'error message!'; var x= [1,2,3]", _global)
        ' Dim script17 As RProgram = ts.ParseScript("\GCModeller\src\R-sharp\test\jsTest\polyglot_stack\stack_test.js", _global)
        ' Dim script18 As RProgram = ts.ParseScript("doc['.stattbl']", _global)
        Dim script19 As RProgram = ts.ParseScript("import {jQuery, Html} from 'webKit'; 'strict'", _global)

        ' Call println(script9.Execute(_global))


        Console.WriteLine("Hello World!")
    End Sub
End Module
