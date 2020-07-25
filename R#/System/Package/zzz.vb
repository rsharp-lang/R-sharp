
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace System.Package

    ''' <summary>
    ''' R# ``zzz.R`` magic trick
    ''' </summary>
    Module zzz

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
        Public Sub TryRunZzzOnLoad(package As Assembly)
            Static assemblyLoaded As New Index(Of String)

            If Not package.ToString Like assemblyLoaded Then
                Call package.RunZzz
                Call assemblyLoaded.Add(package.ToString)
            End If
        End Sub

        <Extension>
        Private Sub RunZzz(package As Assembly)
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
                .Where(Function(m) m.GetParameters.IsNullOrEmpty) _
                .FirstOrDefault

            If Not onLoad Is Nothing Then
                Call onLoad.Invoke(Nothing, {})
            End If
        End Sub
    End Module
End Namespace