#Region "Microsoft.VisualBasic::5fca2789f5b6de23fd2744a43e2d1693, R#\System\Package\PackageManager.vb"

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

    '   Total Lines: 406
    '    Code Lines: 207
    ' Comment Lines: 137
    '   Blank Lines: 62
    '     File Size: 19.68 KB


    '     Class PackageManager
    ' 
    '         Properties: loadedPackages, packageDocs, packageRepository
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: EnumerateAttachedPackages, FindPackage, GenericEnumerator, getEmpty, getPackageDir
    '                   GetPackageDocuments, hasLibFile, hasLibPackage, installDll, InstallLocals
    '                   installZip
    ' 
    '         Sub: addAttached, (+2 Overloads) Dispose, Flush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports fs = System.IO.Directory
Imports Directory = Microsoft.VisualBasic.FileIO.Directory

Namespace Development.Package

    ''' <summary>
    ''' 这个对象可以枚举出所有已经安装的<see cref="Package"/>对象
    ''' </summary>
    Public Class PackageManager : Implements IDisposable, Enumeration(Of Package)

        ReadOnly pkgDb As LocalPackageDatabase
        ReadOnly config As Options

        Public ReadOnly Property packageDocs As New AnnotationDocs
        Public ReadOnly Property loadedPackages As New Index(Of String)

        Friend ReadOnly attached As New Dictionary(Of String, Package)

        Public ReadOnly Property packageRepository As LocalPackageDatabase
            Get
                Return pkgDb
            End Get
        End Property

        Sub New(config As Options)
            Me.pkgDb = LocalPackageDatabase.Load(config.lib)
            Me.config = config
        End Sub

        Private Sub New(config As Options, pkgDb As LocalPackageDatabase)
            Me.pkgDb = pkgDb
            Me.config = config
        End Sub

        Public Shared Function getEmpty(config As Options) As PackageManager
            Return New PackageManager(config, LocalPackageDatabase.EmptyRepository)
        End Function

        Public Sub addAttached(pkg As Package)
            Call loadedPackages.Add(pkg.namespace)
            Call attached.Add(pkg.namespace, pkg)
        End Sub

        Public Function EnumerateAttachedPackages() As IEnumerable(Of Package)
            Return attached.Values
        End Function

        ''' <summary>
        ''' Check if the given dll module <paramref name="libraryFileName"/> is exists in database or not.
        ''' </summary>
        ''' <param name="libraryFileName"></param>
        ''' <returns></returns>
        Public Function hasLibFile(libraryFileName As String) As Boolean
            Return pkgDb.hasLibFile(libraryFileName)
        End Function

        ''' <summary>
        ''' check if a required zip package is installed or not?
        ''' </summary>
        ''' <param name="pkgName"></param>
        ''' <returns></returns>
        Public Function hasLibPackage(pkgName As String) As Boolean
            Return pkgDb.hasLibPackage(pkgName)
        End Function

        Public Function getPackageDir(pkgName As String) As String
            Return PackageLoader2.GetPackageDirectory(config, pkgName)
        End Function

        ''' <summary>
        ''' Handling the document reading for the CLR module which
        ''' has been registered inside the local package repository
        ''' </summary>
        ''' <param name="pkgName"></param>
        ''' <param name="remarks">
        ''' get the remarks text?
        ''' </param>
        ''' <returns></returns>
        Public Function GetPackageDocuments(pkgName As String, Optional remarks As Boolean = False) As String
            Dim type As Type = Me.FindPackage(pkgName, Nothing, Nothing)?.package
            Dim docs As ProjectType

            If type Is Nothing Then
                Return Nothing
            Else
                docs = packageDocs.GetAnnotations(type)
            End If

            If docs Is Nothing Then
                Return Nothing
            Else
                Return If(remarks, docs.Remarks, docs.Summary)
            End If
        End Function

        ''' <summary>
        ''' Find a dll module package
        ''' </summary>
        ''' <param name="packageName"></param>
        ''' <param name="exception"></param>
        ''' <returns>
        ''' If the package is not exists or load package failure
        ''' then this function returns nothing
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindPackage(packageName As String, env As GlobalEnvironment, ByRef exception As Exception) As Package
            Return pkgDb.FindPackage(packageName, env, exception)
        End Function

        ''' <summary>
        ''' 在调用了这个函数进行包模块的安装之后，需要调用<see cref="Flush()"/>函数更新数据库才可以完成安装
        ''' </summary>
        ''' <param name="pkgFile"></param>
        ''' <returns></returns>
        Public Function InstallLocals(pkgFile As String, ByRef err As Exception) As String()
            If pkgFile.ExtensionSuffix("dll") Then
                Return installDll(dllFile:=pkgFile)
            Else
                Return installZip(zipFile:=pkgFile, err:=err)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="zipFile"></param>
        ''' <returns>returns the symbol names in target zip package file.</returns>
        Private Function installZip(zipFile As String, ByRef err As Exception) As String()
            Dim pkginfo As DESCRIPTION = PackageLoader2.GetPackageIndex(zipFile)
            Dim libDir As String
            Dim libDirOld As String

            ' install success output
            '
            ' * installing to library 'C:/Program Files/R/R-3.6.3/library'
            ' * installing *source* package 'mzkit' ...
            ' ** using staged installation
            ' ** R
            ' ** byte-compile and prepare package for lazy loading
            ' ** help
            ' *** installing help indices
            '   converting help for package 'mzkit'
            '    finding HTML links ...    hello                                   html   ����
            '
            ' ** building package indices
            ' ** testing if installed package can be loaded from temporary location
            ' ** testing if installed package can be loaded from final location
            ' ** testing if installed package keeps a record of temporary installation path
            ' * DONE (mzkit)

            ' install error output
            '
            ' * installing to library 'C:/Program Files/R/R-3.6.3/library'
            ' * installing *source* package 'mzkit' ...
            ' ** using staged installation
            ' ** R
            ' ** byte-compile and prepare package for lazy loading
            ' ** help
            ' *** installing help indices
            '   converting help for package 'mzkit'
            '     hello                                   html  
            '     finding HTML links ... ����
            ' ** building package indices
            ' ** testing if installed package can be loaded from temporary location
            ' Error: package or namespace load failed for 'mzkit':
            '  .onLoad failed in loadNamespace() for 'mzkit', details:
            '   call: fun(libname, pkgname)
            '   error: 1
            ' ����: ����ʧ��
            ' ִֹͣ��
            ' ERROR: loading failed
            ' * removing 'C:/Program Files/R/R-3.6.3/library/mzkit'
            ' * restoring previous 'C:/Program Files/R/R-3.6.3/library/mzkit'
            '
            ' Exited with status 1.

            If pkginfo Is Nothing OrElse pkginfo.Package.StringEmpty Then
                Throw New InvalidProgramException($"the given package file '{zipFile}' is not a valid R# package!")
            Else
                Call Console.WriteLine($"* installing to library '{config.lib_loc.GetDirectoryFullPath}'")
                Call Console.WriteLine($"* installing *source* package '{pkginfo.Package}' ...")

                libDir = PackageLoader2.GetPackageDirectory(config, pkginfo.Package)
                libDirOld = $"{libDir.TrimDIR}.old"

                If libDir = "/" OrElse libDirOld = "/" Then
                    Throw New InvalidProgramException($"we unsure why library path of package '{pkginfo.Package}' is pointed to the root directory?")
                Else
                    Call libDirOld.MakeDir
                End If
            End If

            Call Console.WriteLine("** using staged installation")
            Call Console.WriteLine("** R#")
            Call Console.WriteLine("** byte-compile and prepare package for lazy loading")
            Call Console.WriteLine("** help")
            Call Console.WriteLine("*** installing help indices")

            If libDir.FileExists Then
                Call fs.Move(libDir, libDirOld)
            End If

            Try
                Call UnZip.ImprovedExtractToDirectory(zipFile, libDir, Overwrite.Always)
            Catch ex As Exception
                err = ex
                Return Nothing
            End Try

            For Each file As String In $"{libDir}/package/man".ListFiles
                Call Console.WriteLine($"    {file.FileName}")
            Next

            Call Console.WriteLine($"  converting help for package '{pkginfo.Package}'")
            Call Console.WriteLine("** building package indices")

            Call Console.WriteLine("** testing if installed package can be loaded from temporary location")
            Call Console.WriteLine("** testing if installed package can be loaded from final location")

            If Not PackageLoader2.CheckPackage(libDir) Then
                Call Console.WriteLine("ERROR: loading failed")

                Call Console.WriteLine($"* removing '{libDir.GetDirectoryFullPath}'")
                Call fs.Delete(libDir)

                Call Console.WriteLine($"* restoring previous '{libDirOld.GetDirectoryFullPath}'")
                Call fs.Move(libDirOld, libDir)

                Return Nothing
            Else
                Call fs.Delete(libDirOld, recursive:=True)
            End If

            Call Console.WriteLine("** testing if installed package keeps a record of temporary installation path")

            Dim packageIndex As Dictionary(Of String, PackageInfo) = pkgDb.packages _
                .AsEnumerable _
                .ToDictionary(Function(pkg)
                                  Return pkg.namespace
                              End Function)
            Dim symbolNames As String() = ($"{libDir}/package/manifest/symbols.json") _
                .LoadJsonFile(Of Dictionary(Of String, String)) _
                .Keys _
                .ToArray

            packageIndex(pkginfo.Package) = New PackageInfo With {
                .[namespace] = pkginfo.Package,
                .category = APICategories.ResearchTools,
                .description = pkginfo.Description,
                .publisher = pkginfo.Author,
                .symbols = symbolNames
            }

            pkgDb.packages = packageIndex.Values.ToArray
            pkgDb.system = GetType(LocalPackageDatabase).Assembly.FromAssembly

            Call PackageLoader2.LoadPackage(Directory.FromLocalFileSystem(libDir), pkginfo.Package, GlobalEnvironment.defaultEmpty)
            Call Console.WriteLine()
            Call Console.WriteLine($"* DONE ({pkginfo.Package})")

            Return symbolNames
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dllFile"></param>
        ''' <returns>returns module names in target dll assembly file</returns>
        Private Function installDll(dllFile As String) As String()
            Dim packageIndex As Dictionary(Of String, PackageLoaderEntry) = pkgDb.assemblies _
                .AsEnumerable _
                .ToDictionary(Function(pkg)
                                  Return pkg.namespace
                              End Function)
            Dim names As New List(Of String)

            For Each pkg As Package In PackageLoader.ParsePackages(dll:=dllFile)
                With PackageLoaderEntry.FromLoaderInfo(pkg)
                    ' 新的package信息会覆盖掉旧的package信息
                    packageIndex(.namespace) = .ByRef
                End With

                Call names.Add(pkg.namespace)
                Call $"load: {pkg.info.Namespace}".__INFO_ECHO
            Next

            pkgDb.assemblies = packageIndex.Values.ToArray
            pkgDb.system = GetType(LocalPackageDatabase).Assembly.FromAssembly

            Return names.ToArray
        End Function

        ''' <summary>
        ''' 将程序包数据库更新到硬盘文件之上
        ''' </summary>
        Public Sub Flush()
            ' 20200427 try to fix bugs on linux mono platform when 
            ' initialize the runtime environment in New environment

            ' [root@izbp1anxq3o4vzei3wfddjz R_sharp]# ./R# --setup

            ' [ERROR 4/27/2020 8:40:05 PM]<Print>::System.Exception: Print

            ' System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation.
            ' System.Exception: SMRUCC.Rsharp.System.Package.LocalPackageDatabase
            ' System.InvalidOperationException: There was an error generating the XML document.
            ' System.NullReferenceException: Object reference not set to an instance of an object

            ' at (wrapper managed-to-native) System.Reflection.MonoMethod.InternalInvoke(System.Reflection.MonoMethod,object,object[],System.Exception&)
            ' at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x0006a] in <47d423fd1d4342b9832b2fe1f5d431eb>:0
            ' --- End of inner exception stack trace ---
            ' at System.Xml.Serialization.XmlSerializer.Serialize (System.Xml.XmlWriter xmlWriter, System.Object o, System.Xml.Serialization.XmlSerializerNamespaces namespaces, System.String encodingStyle, System.String id) [0x000f4] in <b29965903c6446b8a29a70b44766d725>:0
            ' at System.Xml.Serialization.XmlSerializer.Serialize (System.Xml.XmlWriter xmlWriter, System.Object o, System.Xml.Serialization.XmlSerializerNamespaces namespaces, System.String encodingStyle) [0x00000] in <b29965903c6446b8a29a70b44766d725>:0
            ' at System.Xml.Serialization.XmlSerializer.Serialize (System.Xml.XmlWriter xmlWriter, System.Object o, System.Xml.Serialization.XmlSerializerNamespaces namespaces) [0x00000] in <b29965903c6446b8a29a70b44766d725>:0
            ' at System.Xml.Serialization.XmlSerializer.Serialize (System.IO.TextWriter textWriter, System.Object o, System.Xml.Serialization.XmlSerializerNamespaces namespaces) [0x00015] in <b29965903c6446b8a29a70b44766d725>:0
            ' at System.Xml.Serialization.XmlSerializer.Serialize (System.IO.TextWriter textWriter, System.Object o) [0x00000] in <b29965903c6446b8a29a70b44766d725>:0
            ' at Microsoft.VisualBasic.XmlExtensions.GetXml (System.Object obj, System.Type type, System.Boolean throwEx, Microsoft.VisualBasic.Text.Xml.XmlEncodings xmlEncoding) [0x00068] in <3242cfcec0124178b1264c731fceb926>:0
            ' --- End of inner exception stack trace ---
            ' at (wrapper managed-to-native) System.Reflection.MonoMethod.InternalInvoke(System.Reflection.MonoMethod,object,object[],System.Exception&)
            ' at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x0006a] in <47d423fd1d4342b9832b2fe1f5d431eb>:0
            ' --- End of inner exception stack trace ---
            ' at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00083] in <47d423fd1d4342b9832b2fe1f5d431eb>:0
            ' at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] in <47d423fd1d4342b9832b2fe1f5d431eb>:0
            ' at Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.handleUnexpectedErrorCalls (System.Object[] callParameters, System.Object target, System.Boolean throw) [0x0000e] in <3242cfcec0124178b1264c731fceb926>:0
            ' --- End of inner exception stack trace ---

            ' [INFOM 4/27/2020 8:40:05 PM] [Log] /root/.local/share/GCModeller/R#/.logs/err/2020-04-27, 20-40-05_0000c.log

            If pkgDb.assemblies Is Nothing Then
                pkgDb.assemblies = New XmlList(Of PackageLoaderEntry) With {
                    .items = {}
                }
            End If
            If pkgDb.system Is Nothing Then
                pkgDb.system = New AssemblyInfo
            End If

            Call pkgDb.GetXml.SaveTo(config.lib)
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Flush()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

        ''' <summary>
        ''' 从<see cref="LocalPackageDatabase.assemblies"/>中枚举出所有的<see cref="PackageLoaderEntry"/>
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GenericEnumerator() As IEnumerator(Of Package) Implements Enumeration(Of Package).GenericEnumerator
            Dim pkg As Package

            For Each loader As PackageLoaderEntry In pkgDb.assemblies.AsEnumerable
                pkg = loader.GetLoader(Nothing, Nothing)

                If pkg Is Nothing Then
                    ' missing from current environment
                    Yield New Package(loader)
                Else
                    Yield pkg
                End If
            Next
        End Function
    End Class
End Namespace
