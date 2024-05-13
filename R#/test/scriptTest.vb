#Region "Microsoft.VisualBasic::dc3b13f87750dd6533f6b7851c335e4b, R#\Test\scriptTest.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 664 B


    ' Module scriptTest
    ' 
    '     Sub: Main, multipleImportsTest, testSource
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter

Module scriptTest

    Const script$ = "S:\2019\mzCloud\mzcloud_mgf.R"

    Dim R As RInterpreter = New RInterpreter() With {
        .debug = True
    }

    Sub Main()
        Call multipleImportsTest()
    End Sub

    Sub testSource()
        Call R.globalEnvir.packages.InstallLocals("D:\GCModeller\GCModeller\bin\Library\R.base.dll", Nothing)
        Call R.Source(script)
        Call Pause()
    End Sub

    Sub multipleImportsTest()
        R.debug = False
        Call R.Evaluate("source(`E:\GCModeller\src\R-sharp\tutorials\Rscript\multiples\app.R`)")

        Pause()
    End Sub
End Module
