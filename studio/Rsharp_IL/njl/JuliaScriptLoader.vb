﻿#Region "Microsoft.VisualBasic::7bebaff7f0490b0ba74659dd3130bf07, studio\Rsharp_IL\njl\JuliaScriptLoader.vb"

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

    '   Total Lines: 70
    '    Code Lines: 53 (75.71%)
    ' Comment Lines: 6 (8.57%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (15.71%)
    '     File Size: 2.73 KB


    ' Class JuliaScriptLoader
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: LoadScript, ParseScript
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports njl.Language
Imports SMRUCC.Rsharp.Development.Polyglot
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

' https://stackoverflow.com/questions/55970064/calling-local-julia-package-from-c

Public Class JuliaScriptLoader : Inherits ScriptLoader

    Public Overrides ReadOnly Iterator Property SuffixNames As IEnumerable(Of String)
        Get
            Yield "jl"
        End Get
    End Property

    Shared Sub New()
        Call RInternal.invoke.pushEnvir(GetType(njl.math))
        Call RInternal.invoke.pushEnvir(GetType(njl.io))
    End Sub

    Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object
        Dim R As RInterpreter = env.globalEnvironment.Rscript
        ' 20200213 因为source函数是创建了一个新的环境容器
        ' 所以函数无法被导入到全局环境之中
        ' 在这里imports关键词操作则是使用全局环境
        Dim script As MagicScriptSymbol = CreateMagicScriptSymbol(scriptfile, R)
        Dim Rscript As Rscript = Rscript.FromFile(scriptfile)
        Dim stackframe As New StackFrame With {
            .File = Rscript.fileName,
            .Line = 0,
            .Method = New Method With {
                .Method = MethodBase.GetCurrentMethod.Name,
                .[Module] = "n/a",
                .[Namespace] = "SMRUCC/njl"
            }
        }

        Call env.setStackInfo(stackframe)

        If env.FindSymbol("!script") Is Nothing Then
            env.Push("!script", New vbObject(script), [readonly]:=False)
        Else
            env.FindSymbol("!script").setValue(New vbObject(script), env)
        End If

        Dim program = Rscript.ParseJlScript(R.debug)

        If program Is Nothing Then
            ' there are syntax error in the external script
            ' for current imports action
            Return RInternal.debug.stop("".Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB), env)
        Else
            Return program.Execute(env)
        End If
    End Function

    Public Overrides Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)
        Dim Rscript As Rscript = Rscript.FromFile(scriptfile)
        Dim program As Program = Rscript.ParseJlScript(debug:=env.globalEnvironment.debugMode)

        Return program
    End Function
End Class
