#Region "Microsoft.VisualBasic::258c3c8c70e0c9c98f9bafc45aef6ada, R#\System\Package\zzz.vb"

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

    '   Total Lines: 86
    '    Code Lines: 49 (56.98%)
    ' Comment Lines: 26 (30.23%)
    '    - Xml Docs: 76.92%
    ' 
    '   Blank Lines: 11 (12.79%)
    '     File Size: 3.07 KB


    '     Module zzz
    ' 
    '         Function: CheckArguments
    ' 
    '         Sub: RunZzz, TryRunZzzOnLoad
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Development.Package

    ''' <summary>
    ''' R# ``zzz.R`` magic trick
    ''' </summary>
    Module zzz

        ' config quietly for clr assembly module loader
        ' options(onload_quietly = TRUE);

        ''' <summary>
        ''' It's a file where one usually puts actions on load of the package. 
        ''' It is tradition/convention that it's called zzz.R and could be 
        ''' called anything.R
        ''' 
        ''' You only need To include this If you want you package To Do something 
        ''' out Of the ordinary When it loads. Keep looking at what people put 
        ''' In there And you'll begin to get a sense of what they're used for.
        ''' </summary>
        ''' <param name="package">
        ''' module named ``zzz`` andalso contains a entry method named ``onLoad``
        ''' </param>
        <Extension>
        Public Sub TryRunZzzOnLoad(package As Assembly, quietly As Boolean)
            Static assemblyLoaded As New Index(Of String)

            If Not package.ToString Like assemblyLoaded Then
                Call package.RunZzz(quietly)
                Call assemblyLoaded.Add(package.ToString)
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="package"></param>
        ''' <param name="quietly">
        ''' config quietly for clr assembly module loader
        ''' </param>
        <Extension>
        Private Sub RunZzz(package As Assembly, quietly As Boolean)
            ' find zzz module
            ' and then find onLoad method in zzz module
            Dim zzz As Type = package _
                .GetTypes _
                .Where(Function(a) a.Name = "zzz") _
                .FirstOrDefault

            If zzz Is Nothing Then
                Return
            End If

            Dim onLoad As MethodInfo = zzz _
                .GetMethods(BindingFlags.Public Or BindingFlags.Static) _
                .Where(Function(m) m.Name = "onLoad") _
                .Where(Function(m)
                           Return CheckArguments(m)
                       End Function) _
                .FirstOrDefault

            If Not onLoad Is Nothing Then
                If onLoad.GetParameters.IsNullOrEmpty Then
                    Call onLoad.Invoke(Nothing, {})
                Else
                    Call onLoad.Invoke(Nothing, New Object() {quietly})
                End If
            End If
        End Sub

        Private Function CheckArguments(m As MethodInfo) As Boolean
            Dim args = m.GetParameters

            If args.IsNullOrEmpty Then
                Return True
            ElseIf args.Length = 1 AndAlso args(0).ParameterType Is GetType(Boolean) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Module
End Namespace
