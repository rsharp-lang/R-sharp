Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class RInteropAttribute : Inherits Attribute
    End Class

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RListObjectArgumentAttribute : Inherits RInteropAttribute

    End Class
End Namespace