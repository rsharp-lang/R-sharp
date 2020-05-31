Namespace Runtime.Interop

    ''' <summary>
    ''' 标注这个程序集是一个``R#``程序包
    ''' </summary>
    <AttributeUsage(AttributeTargets.Assembly, AllowMultiple:=False, Inherited:=True)>
    Public Class RPackageModuleAttribute : Inherits RInteropAttribute
    End Class
End Namespace