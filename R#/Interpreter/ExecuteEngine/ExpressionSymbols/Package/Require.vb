#Region "Microsoft.VisualBasic::2714f87a4e7105ebe09a8078e63d88a1, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/Package/Require.vb"

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

    '   Total Lines: 197
    '    Code Lines: 66
    ' Comment Lines: 114
    '   Blank Lines: 17
    '     File Size: 9.59 KB


    '     Class Require
    ' 
    '         Properties: expressionName, options, packages, type
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Evaluate, getOptions, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    ''' <summary>
    ''' ## Loading/Attaching and Listing of Packages
    ''' 
    ''' library and require load and attach add-on packages.
    ''' 
    ''' library and require can only load/attach an installed package, 
    ''' and this is detected by having a ‘DESCRIPTION’ file containing
    ''' a Built: field.
    ''' Under Unix-alikes, the code checks that the package was 
    ''' installed under a similar operating system As given by
    ''' R.version$platform (the canonical name Of the platform under 
    ''' which R was compiled), provided it contains compiled code. 
    ''' Packages which Do Not contain compiled code can be Shared 
    ''' between Unix-alikes, but Not To other OSes because Of potential 
    ''' problems With line endings And OS-specific help files. If 
    ''' Sub-architectures are used, the OS similarity Is Not checked 
    ''' since the OS used To build may differ (e.g. i386-pc-linux-gnu
    ''' code can be built On an x86_64-unknown-linux-gnu OS).
    ''' The package name given To library And require must match the 
    ''' name given In the package's ‘DESCRIPTION’ file exactly, even on 
    ''' case-insensitive file systems such as are common on Windows 
    ''' and macOS.
    ''' </summary>
    ''' 
    Public Class Require : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Require
            End Get
        End Property

        ''' <summary>
        ''' the name of a package, given as a name or 
        ''' literal character string, or a character 
        ''' string, depending on whether ``character.only``
        ''' is FALSE (default) or TRUE.
        ''' </summary>
        ''' <returns></returns>
        Public Property packages As Expression()
        Public Property options As ValueAssignExpression()

        ''' <summary>
        ''' Loading/Attaching and Listing of Packages
        ''' </summary>
        ''' <param name="names"></param>
        Sub New(names As IEnumerable(Of Expression))
            packages = names.ToArray
            options = packages _
                .Where(Function(exp) TypeOf exp Is ValueAssignExpression) _
                .Select(Function(exp) DirectCast(exp, ValueAssignExpression)) _
                .ToArray
            packages = packages _
                .Where(Function(exp) Not TypeOf exp Is ValueAssignExpression) _
                .ToArray
        End Sub

        ''' <summary>
        ''' Loading/Attaching and Listing of Packages
        ''' </summary>
        ''' <param name="packageName"></param>
        Sub New(packageName As String)
            packages = {New Literal(packageName)}
        End Sub

        ''' <summary>
        ''' Loading/Attaching and Listing of Packages
        ''' </summary>
        ''' <param name="names"></param>
        Sub New(names As IEnumerable(Of String))
            packages = names.Select(Function(name) New Literal(name)).ToArray
        End Sub

        Private Function getOptions(env As Environment) As Dictionary(Of String, Object)
            Dim opts As New Dictionary(Of String, Object)

            For Each opt As ValueAssignExpression In options.SafeQuery
                Dim name As String = ValueAssignExpression.GetSymbol(opt.targetSymbols(Scan0))
                Dim value As Object = opt.value.Evaluate(env)

                opts(name) = value
            Next

            Return opts
        End Function

        ''' <summary>
        ''' require returns (invisibly) a logical indicating whether the 
        ''' required package is available.
        ''' 
        ''' library(package) and require(package) both load the namespace
        ''' of the package with name package and attach it on the search
        ''' list. require is designed for use inside other functions; it 
        ''' returns FALSE and gives a warning (rather than an error as
        ''' ``library()`` does by default) if the package does not exist. 
        ''' Both functions check and update the list of currently attached 
        ''' packages and do not reload a namespace which is already loaded. 
        ''' (If you want to reload such a package, call detach(unload = TRUE)
        ''' or unloadNamespace first.) If you want to load a package 
        ''' without attaching it on the search list, see requireNamespace.
        ''' To suppress messages during the loading of packages use 
        ''' suppressPackageStartupMessages: this will suppress all messages 
        ''' from R itself but Not necessarily all those from package authors.
        ''' If library Is called With no package Or help argument, it lists 
        ''' all available packages In the libraries specified by Lib.loc,
        ''' And returns the corresponding information In an Object Of Class
        ''' "libraryIQR". (The Structure Of this Class may change In future
        ''' versions.) Use .packages(all = True) To obtain just the names 
        ''' Of all available packages, And installed.packages() For more 
        ''' information.
        ''' library(help = somename) computes basic information about the package 
        ''' somename, And returns this in an object of class "packageInfo". 
        ''' (The structure of this class may change in future versions.) When 
        ''' used with the default value (NULL) for lib.loc, the attached 
        ''' packages are searched before the libraries.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' Normally library returns (invisibly) the list of attached packages,
        ''' but TRUE or FALSE if logical.return is TRUE. When called as library() 
        ''' it returns an object of class "libraryIQR", and for library(help=),
        ''' one of class "packageInfo".
        ''' require returns(invisibly) a logical indicating whether the required 
        ''' package Is available.
        ''' </returns>
        ''' <remarks>
        ''' ### Conflicts
        ''' 
        ''' Handling of conflicts depends on the setting of the conflicts.policy 
        ''' option. If this option Is Not set, then conflicts result in warning 
        ''' messages if the argument warn.conflicts Is TRUE. If the option Is set
        ''' to the character string "strict", then all unresolved conflicts signal
        ''' errors. Conflicts can be resolved using the mask.ok, exclude, And
        ''' include.only arguments to library And require. Defaults for mask.ok 
        ''' And exclude can be specified using conflictRules.
        ''' If the conflicts.policy Option Is Set To the String "depends.ok" Then
        ''' conflicts resulting from attaching declared dependencies will Not
        ''' produce errors, but other conflicts will. This Is likely To be the
        ''' best setting For most users wanting some additional protection against
        ''' unexpected conflicts.
        ''' The policy can be tuned further by specifying the conflicts.policy Option 
        ''' As a named list With the following fields
        ''' 
        ''' + error: logical; if TRUE treat unresolved conflicts as errors.
        ''' + warn: logical; unless FALSE issue a warning message when conflicts are found.
        ''' + generics.ok: logical; if TRUE ignore conflicts created by defining S4 
        '''                generics for functions on the search path.
        ''' + depends.ok: logical; if TRUE do Not treat conflicts with required 
        '''               packages as errors.
        ''' + can.mask: character vector Of names Of packages that are allowed To be
        '''             masked. These would typically be base packages attached by
        '''             Default.
        ''' </remarks>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As New List(Of String)
            Dim [global] As GlobalEnvironment = envir.globalEnvironment
            Dim options = getOptions(envir)
            Dim pkgName As String
            Dim message As Message
            Dim quietly As Boolean = options.TryGetValue("quietly")

            For Each name As Expression In packages
                pkgName = ValueAssignExpression.GetSymbol(name)
                message = [global].LoadLibrary(pkgName, silent:=quietly)

                If Not message Is Nothing AndAlso Not quietly Then
                    Call Internal.debug.PrintMessageInternal(message, envir.globalEnvironment)
                End If
            Next

            Return names.ToArray
        End Function

        ''' <summary>
        ''' Loading/Attaching and Listing of Packages
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"require({packages.JoinBy(", ")})"
        End Function
    End Class
End Namespace
