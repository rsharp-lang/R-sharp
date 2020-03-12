# create a new VB.NET object with list 
# as property value for the 
# initialization

imports "makeObject" from "test.exe";

# Public Class arguments
    # Public Property a As String
    # Public Property b As Integer()
    # Public Property c As Boolean()
# End Class

print("the test api function required a VB.NET object, which can be created from a given R# object list:");
print(debug_echo);

list(a = "9875", b = [23,43,24,2,3], c = FALSE) :> debug_echo;