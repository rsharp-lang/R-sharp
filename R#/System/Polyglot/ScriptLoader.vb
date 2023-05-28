#Region "Microsoft.VisualBasic::a889898b92193b6d0ea286442d28c07b, F:/GCModeller/src/R-sharp/R#//System/Polyglot/ScriptLoader.vb"

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

    '   Total Lines: 16
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 573 B


    '     Class ScriptLoader
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development.Polyglot

    Public MustInherit Class ScriptLoader

        Public MustOverride ReadOnly Property SuffixNames As IEnumerable(Of String)

        Public MustOverride Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)
        Public MustOverride Function LoadScript(scriptfile As String, env As Environment) As Object

    End Class
End Namespace
