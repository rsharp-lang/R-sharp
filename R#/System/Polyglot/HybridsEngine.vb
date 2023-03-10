#Region "Microsoft.VisualBasic::a2eac7bc77fb7191f22a3bebca631b5d, D:/GCModeller/src/R-sharp/R#//System/HybridsEngine.vb"

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

'   Total Lines: 136
'    Code Lines: 97
' Comment Lines: 12
'   Blank Lines: 27
'     File Size: 5.27 KB


'     Class HybridsEngine
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: CanHandle, LoadScript, ParseScript, (+2 Overloads) Register
' 
'     Class ScriptLoader
' 
' 
' 
'     Class HybridsREngine
' 
'         Properties: SuffixName
' 
'         Function: LoadScript, ParseScript
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime

Namespace Development.Polyglot

    Public Class HybridsEngine

        ReadOnly engine As New Dictionary(Of String, ScriptLoader)
        ReadOnly suffix As New List(Of String)

        Sub New()
            Call Register(New HybridsREngine)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CanHandle(scriptfile As String) As Boolean
            Return scriptfile.ExtensionSuffix(suffix.ToArray)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadScript(scriptfile As String, env As Environment) As Object
            Dim key As String = scriptfile.ExtensionSuffix.ToLower
            Dim loader As ScriptLoader = engine(key)

            Return loader.LoadScript(scriptfile, env)
        End Function

        Public Function ParseScript(scriptfile As String, env As Environment) As Object
            Dim key As String = scriptfile.ExtensionSuffix.ToLower
            Dim loader As ScriptLoader = engine(key)

            Return loader.ParseScript(scriptfile, env)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dllpath">
        ''' dll full path
        ''' </param>
        ''' <returns></returns>
        Public Function Register(dllpath As String) As HybridsEngine
            Dim types As Type() = deps.LoadAssemblyOrCache(dllFile:=dllpath).GetTypes

            For Each type As Type In types
                If type.IsInheritsFrom(GetType(ScriptLoader)) Then
                    Call Me.Register(DirectCast(Activator.CreateInstance(type), ScriptLoader))
                End If
            Next

            Return Me
        End Function

        Public Function Register(loader As ScriptLoader) As HybridsEngine
            engine(loader.SuffixName.ToLower) = loader
            suffix.Add(loader.SuffixName.ToLower)

            Return Me
        End Function
    End Class
End Namespace
