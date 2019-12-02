#Region "Microsoft.VisualBasic::9ec83e7bdbe50299d1dcfd27f5b588fc, R#\test\scriptTest.vb"

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

    ' Module scriptTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter

Module scriptTest

    Const script$ = "E:\GCModeller\src\GCModeller\engine\Rscript\enrichment_result_union.R"

    Dim R As RInterpreter = New RInterpreter() With {
        .debug = True
    }

    Sub Main()
        Call R.globalEnvir.packages.InstallLocals("E:\GCModeller\GCModeller\bin\R.base.dll")
        Call R.Source(script)
        Call Pause()
    End Sub
End Module

