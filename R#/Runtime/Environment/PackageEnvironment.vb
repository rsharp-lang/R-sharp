#Region "Microsoft.VisualBasic::54cb0ca4ab19b8df3ec14d78de2d8c4a, R-sharp\R#\Runtime\Environment\PackageEnvironment.vb"

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


     Code Statistics:

        Total Lines:   46
        Code Lines:    40
        Comment Lines: 0
        Blank Lines:   6
        File Size:     1.79 KB


    '     Class PackageEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: pkgFrame, SetPackage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    Public Class PackageEnvironment : Inherits Environment

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(globalEnv As GlobalEnvironment, packageName As String)
            Call MyBase.New(globalEnv, pkgFrame(packageName), isInherits:=False)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Private Shared Function pkgFrame(pkgName As String) As StackFrame
            Return New StackFrame With {
                .File = pkgName,
                .Line = "n/a",
                .Method = New Method With {
                   .Method = "loadEnvironment",
                   .[Module] = "package",
                   .[Namespace] = pkgName
                }
            }
        End Function

        Public Function SetPackage() As PackageEnvironment

        End Function
    End Class
End Namespace
