
Imports System.Reflection
Imports System.Runtime.CompilerServices

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

        End Sub
    End Module
End Namespace