#Region "Microsoft.VisualBasic::90a1f2285ce58fc9a4a82c1c224db765, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/devkit//JavaScript.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14
    ' Comment Lines: 3
    '   Blank Lines: 2
    '     File Size: 583 B


    ' Module JavaScript
    ' 
    '     Function: Parse
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.TypeScript

''' <summary>
''' Polyglot
''' </summary>
<Package("javascript")>
Public Module JavaScript

    <ExportAPI("parse")>
    Public Function Parse(script As String, Optional env As Environment = Nothing) As Object
        Dim scriptReader As New TypeScriptLoader
        Dim app As Program = scriptReader.ParseScript(script, env)
        Return app
    End Function
End Module
