#Region "Microsoft.VisualBasic::d6e036694d1cd410158e1a26495fbb84, E:/GCModeller/src/R-sharp/studio/R-terminal//IntelliSense.vb"

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

    '   Total Lines: 125
    '    Code Lines: 102
    ' Comment Lines: 2
    '   Blank Lines: 21
    '     File Size: 4.48 KB


    ' Class IntelliSense
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AllSymbols, AutoCompletion, GetFileNamesCompletion, packageFunctions
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.LineEdit
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports rLang = SMRUCC.Rsharp.Language.Syntax

Friend Class IntelliSense

    ReadOnly R As RInterpreter

    Sub New(R As RInterpreter)
        Me.R = R
    End Sub

    Private Function AllSymbols() As IEnumerable(Of String)
        Dim globalEnv = R.globalEnvir
        Dim globalSymbols = globalEnv.EnumerateAllSymbols.JoinIterates(globalEnv.EnumerateAllFunctions)
        ' system internal hiddens
        Dim internals = Internal.invoke.getAllInternals
        Dim symbols As IEnumerable(Of String) = globalSymbols _
            .Select(Function(s) s.name) _
            .JoinIterates(internals.Select(Function(s) s.name)) _
            .Distinct

        Return symbols
    End Function

    Private Function packageFunctions(package As String) As IEnumerable(Of String)
        Dim globalEnv = R.globalEnvir
        Dim namespace_loaded = globalEnv.attachedNamespace(package)

        If namespace_loaded Is Nothing Then
            Return {}
        Else
            Return namespace_loaded _
                .EnumerateAllSymbols(enumerateParents:=False) _
                .JoinIterates(namespace_loaded.EnumerateAllFunctions(enumerateParents:=False)) _
                .Select(Function(s) s.name) _
                .Distinct _
                .ToArray
        End If
    End Function

    Private Function GetFileNamesCompletion(s As String, path_str As String) As Completion
        Dim ls As String()

        If path_str.DirectoryExists Then
            Dim is_dot As Boolean = path_str.IsPattern("\.+")

            ls = path_str _
                .EnumerateFiles _
                .Select(Function(f)
                            Dim name As String = f.FileName

                            If is_dot Then
                                Return "/" & name
                            Else
                                Return name
                            End If
                        End Function) _
                .ToArray
        Else
            Dim check_name As String = path_str.BaseName(allowEmpty:=True)
            Dim pos As Integer = check_name.Length

            ls = path_str _
                .ParentPath _
                .EnumerateFiles _
                .Select(Function(f) f.FileName) _
                .Where(Function(f) f.StartsWith(check_name, StringComparison.Ordinal)) _
                .Select(Function(si) si.Substring(pos)) _
                .ToArray
        End If

        Return New Completion(s, ls)
    End Function

    Public Function AutoCompletion(s As String, pos As Integer) As Completion
        Dim prefix As String = Nothing
        Dim ls As String()

        If rLang.IsStringOpen(s, prefix) Then
            Return GetFileNamesCompletion(s, prefix)
        Else
            prefix = s.Substring(0, pos)
        End If

        If prefix.StringEmpty Then
            ls = AllSymbols.ToArray
        ElseIf prefix.EndsWith("::") Then
            ' get package functions
            Dim packageName = prefix.Trim(":"c, " "c).Split({" "c, "+"c, "-"c, "*"c, "/"c, "\"c, "?"c, "&"c}).LastOrDefault

            If packageName.StringEmpty Then
                ls = {}
            Else
                ls = packageFunctions(packageName).ToArray
            End If
        ElseIf rLang.TryRequirePackage(prefix, prefix) Then
            Dim repo = R.globalEnvir.packages.packageRepository
            Dim lib_loc As String = R.globalEnvir.options.lib_loc

            pos = prefix.Length
            ls = repo _
                .enumerateClrBases _
                .JoinIterates(repo.enumerateNugets(lib_loc)) _
                .Where(Function(c) c.StartsWith(prefix, StringComparison.Ordinal)) _
                .Select(Function(c) c.Substring(pos)) _
                .ToArray

            Return New Completion(prefix, ls)
        Else
            If rLang.EndWithIdentifier(prefix, prefix) Then
                pos = prefix.Length
            End If

            ls = AllSymbols _
                .Where(Function(c) c.StartsWith(prefix, StringComparison.Ordinal)) _
                .Select(Function(c) c.Substring(pos)) _
                .ToArray
        End If

        Return New Completion(prefix, ls)
    End Function
End Class
