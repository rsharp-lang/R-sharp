#Region "Microsoft.VisualBasic::cb5cd5e68f05ee302f7c20da3f151a31, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Annotation\DataDir.vb"

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

    '   Total Lines: 107
    '    Code Lines: 76 (71.03%)
    ' Comment Lines: 14 (13.08%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 17 (15.89%)
    '     File Size: 4.20 KB


    '     Class AnnotationSymbol
    ' 
    '         Properties: expressionName, type
    ' 
    '         Function: CheckSymbolText, CreateSymbol, ToString
    ' 
    '     Class DataDir
    ' 
    '         Properties: symbol
    ' 
    '         Function: Evaluate, PackageDir
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports LibDir = Microsoft.VisualBasic.FileIO.Directory
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    Public MustInherit Class AnnotationSymbol : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Annotation
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' must be prefixed with symbol "@"
        ''' </remarks>
        Public MustOverride ReadOnly Property symbol As String

        Public Overrides Function ToString() As String
            Return symbol
        End Function

        Public Shared Function CheckSymbolText(symbol As String) As Boolean
            Select Case Strings.LCase(symbol)
                Case "@stop", "@script", "@home", "@host", "@dir", "@datadir", "@profile" : Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Shared Function CreateSymbol(symbol As String) As AnnotationSymbol
            Select Case Strings.LCase(symbol)
                Case "@stop" : Return New BreakPoint
                Case "@script" : Return New ScriptSymbol
                Case "@home" : Return New HomeSymbol
                Case "@host" : Return New HostSymbol
                Case "@dir" : Return New ScriptFolder
                Case "@datadir" : Return New DataDir
                Case "@profile" : Throw New NotImplementedException(symbol)

                Case Else
                    Throw New NotImplementedException(symbol)
            End Select
        End Function

    End Class

    ''' <summary>
    ''' @datadir
    ''' 
    ''' directory path of the /data in current package
    ''' </summary>
    Public Class DataDir : Inherits AnnotationSymbol

        Public Overrides ReadOnly Property symbol As String
            Get
                Return "@datadir"
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' REnv.declare_function.R_invoke$paletteer_colors
            For Each frame As StackFrame In envir.stackTrace
                Dim fun_p As Method = frame.Method

                If fun_p.Module = "declare_function" AndAlso fun_p.Method.StartsWith("R_invoke$") Then
                    Dim libdir = PackageDir(envir, fun_p.Namespace)

                    If TypeOf libdir Is Message Then
                        Return libdir
                    Else
                        Return DirectCast(libdir, IFileSystemEnvironment).GetFullPath("/data/")
                    End If
                End If
            Next

            Return RInternal.debug.stop("@datadir symbol must be declared inside a R package function", envir)
        End Function

        Public Shared Function PackageDir(env As Environment, package As String) As Object
            ' 优先从已经加载的程序包位置进行加载操作
            If env.globalEnvironment.attachedNamespace.hasNamespace(package) Then
                Return env.globalEnvironment.attachedNamespace(package).libpath
            ElseIf Not RFileSystem.PackageInstalled(package, env) Then
                Return Internal.debug.stop({$"we could not found any installed package which is named '{package}'!", $"package: {package}"}, env)
            Else
                Return LibDir.FromLocalFileSystem($"{RFileSystem.GetPackageDir(env)}/{package}")
            End If
        End Function
    End Class
End Namespace
