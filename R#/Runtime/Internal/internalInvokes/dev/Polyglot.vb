#Region "Microsoft.VisualBasic::674290e23cc5876abf9eec0493144741, R#\Runtime\Internal\internalInvokes\dev\Polyglot.vb"

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

    '   Total Lines: 20
    '    Code Lines: 16 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (20.00%)
    '     File Size: 710 B


    '     Module Polyglot
    ' 
    '         Function: assembly, register
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module Polyglot

        <ExportAPI("register")>
        Public Function register(<RRawVectorArgument> file_types As Object, assembly As Assembly, Optional env As Environment = Nothing) As Object
            Throw New NotImplementedException
        End Function

        <ExportAPI("assembly")>
        <RApiReturn(GetType(Assembly))>
        Public Function assembly(filename As String, Optional env As Environment = Nothing) As Object
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
