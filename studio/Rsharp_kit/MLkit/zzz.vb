#Region "Microsoft.VisualBasic::6d4b1297ce59091c9d564e967c6219c1, studio\Rsharp_kit\MLkit\zzz.vb"

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

    '   Total Lines: 30
    '    Code Lines: 24 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (20.00%)
    '     File Size: 906 B


    ' Class zzz
    ' 
    '     Sub: onLoad
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics.CodeAnalysis
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Runtime.Interop

<Assembly: RPackageModule>

<Assembly: SuppressMessage("", "CA1416")>
<Assembly: SuppressMessage("", "SYSLIB0021")>
<Assembly: SuppressMessage("", "SYSLIB0022")>
<Assembly: SuppressMessage("", "SYSLIB0003")>
<Assembly: SuppressMessage("", "SYSLIB0004")>

Public Class zzz

    Public Shared Sub onLoad()
        Call GetType(zzz).Assembly _
            .FromAssembly _
            .AppSummary("Welcome to the SMRUCC Machine Learning toolkit!", "", App.StdOut)

        Call clustering.Main()
        Call CNNTools.Main()
        Call NLP.Main()
        Call aprioriRules.Main()
        Call Manifold.Main()
        Call bitmap_func.Main()

        Call Console.WriteLine()
    End Sub
End Class
