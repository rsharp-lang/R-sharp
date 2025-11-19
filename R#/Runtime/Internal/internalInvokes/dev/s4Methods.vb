Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].baseOp
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' s4object api function exports
    ''' </summary>
    Public Module s4Methods

        ''' <summary>
        ''' ### Create a Class Definition
        ''' 
        ''' Create a class definition and return a generator function to create objects from the class. Typical usage will be of the style:
        '''
        ''' ```
        ''' MyClass &lt;- setClass("myClass", slots= ...., contains =....);
        '''```
        '''
        ''' where the first argument Is the name Of the New Class And, If supplied, the arguments slots= And contains= specify the slots
        ''' In the New Class And existing classes from which the New Class should inherit. Calls To setClass() are normally found In the 
        ''' source Of a package; When the package Is loaded the Class will be defined In the package's namespace. Assigning the generator 
        ''' function with the name of the class is convenient for users, but not a requirement.
        ''' </summary>
        ''' <param name="Class">character string name for the class.</param>
        ''' <param name="representation"></param>
        ''' <param name="prototype">supplies an object with the default data for the slots in this class. A more flexible approach is to write a method for initialize().</param>
        ''' <param name="contains">A vector specifying existing classes from which this class should inherit. The new class will have all 
        ''' the slots of the superclasses, with the same requirements on the classes of these slots. This argument must be supplied by name, 
        ''' contains=, in the call, for back compatibility with other arguments no longer recommended.
        ''' See the section 'Virtual Classes’ for the special superclass "VIRTUAL".</param>
        ''' <param name="validity"></param>
        ''' <param name="access"></param>
        ''' <param name="where"></param>
        ''' <param name="version"></param>
        ''' <param name="sealed"></param>
        ''' <param name="package"></param>
        ''' <param name="S3methods"></param>
        ''' <param name="slots">The names and classes for the slots in the new class. This argument must be supplied by name, slots=, 
        ''' in the call, for back compatibility with other arguments no longer recommended.
        ''' 
        ''' The argument must be vector With a names attribute, the names being those Of the slots In the New Class. Each element Of the 
        ''' vector specifies an existing Class; the corresponding slot must be from this Class Or a subclass Of it. Usually, this Is a 
        ''' character vector naming the classes. It's also legal for the elements of the vector to be class representation objects, as 
        ''' returned by getClass.
        ''' 
        ''' As a limiting case, the argument may be an unnamed character vector; the elements are taken as slot names And all slots have 
        ''' the unrestricted class "ANY".</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A generator function suitable for creating objects from the class is returned, invisibly. A call to this function generates a 
        ''' call to new for the class. The call takes any number of arguments, which will be passed on to the initialize method. If no 
        ''' initialize method is defined for the class or one of its superclasses, the default method expects named arguments with the name 
        ''' of one of the slots and unnamed arguments that are objects from one of the contained classes.
        ''' 
        ''' Typically the generator Function Is assigned the name Of the Class, For programming clarity. This Is Not a requirement And objects
        ''' from the Class can also be generated directly from New. The advantages Of the generator Function are a slightly simpler And 
        ''' clearer Call, And that the Call will contain the package name Of the Class (eliminating any ambiguity If two classes from 
        ''' different packages have the same name).
        ''' 
        ''' If the Class Is virtual, an attempt To generate an Object from either the generator Or New() will result In an Error.
        ''' </returns>
        ''' <remarks>
        ''' #### Basic Use: Slots and Inheritance
        ''' 
        ''' The two essential arguments other than the class name are slots and contains, defining the explicit slots and the 
        ''' inheritance (superclasses). Together, these arguments define all the information in an object from this class; 
        ''' that is, the names of all the slots and the classes required for each of them.
        ''' 
        ''' The name of the class determines which methods apply directly to objects from this class. The superclass information 
        ''' specifies which methods apply indirectly, through inheritance. See Methods_Details for inheritance in method selection.
        ''' 
        ''' The slots in a class definition will be the union of all the slots specified directly by slots and all the slots in all 
        ''' the contained classes. There can only be one slot with a given name. A class may override the definition of a slot with 
        ''' a given name, but only if the newly specified class is a subclass of the inherited one. For example, if the contained 
        ''' class had a slot a with class "ANY", then a subclass could specify a with class "numeric", but if the original specification 
        ''' for the slot was class "character", the new call to setClass would generate an error.
        ''' 
        ''' Slot names "class" and "Class" are not allowed. There are other slot names with a special meaning; these names start with the
        ''' "." character. To be safe, you should define all of your own slots with names starting with an alphabetic character.
        ''' 
        ''' Some inherited classes will be treated specially—object types, S3 classes and a few special cases—whether inherited directly
        ''' or indirectly. See the next three sections.
        ''' 
        ''' #### Virtual Classes
        ''' 
        ''' Classes exist for which no actual objects can be created, the virtual classes.
        ''' 
        ''' The most common and useful form of virtual class is the class union, a virtual class that is defined in a call to setClassUnion()
        ''' rather than a call to setClass(). This call lists the members of the union—subclasses that extend the new class. Methods that
        ''' are written with the class union in the signature are eligible for use with objects from any of the member classes. Class
        ''' unions can include as members classes whose definition is otherwise sealed, including basic R data types.
        ''' 
        ''' Calls to setClass() will also create a virtual class, either when only the Class argument is supplied (no slots or superclasses) or
        ''' when the contains= argument includes the special class name "VIRTUAL".
        ''' 
        ''' In the latter case, a virtual class may include slots to provide some common behavior without fully defining the object—see 
        ''' the class traceable for an example. Note that "VIRTUAL" does not carry over to subclasses; a class that contains a virtual 
        ''' class is not itself automatically virtual.
        ''' 
        ''' #### Inheriting from Object Types
        ''' 
        ''' In addition to containing other S4 classes, a class definition can contain either an S3 class (see the next section) or a built-in 
        ''' R pseudo-class—one of the R object types or one of the special R pseudo-classes "matrix" and "array". A class can contain at most 
        ''' one of the object types, directly or indirectly. When it does, that contained class determines the “data part” of the class. This
        ''' appears as a pseudo-slot, ".Data" and can be treated as a slot but actually determines the type of objects from this slot.
        ''' 
        ''' Objects from the new class try to inherit the built in behavior of the contained type. In the case of normal R data types, including 
        ''' vectors, functions and expressions, the implementation is relatively straightforward. For any object x from the class, typeof(x) 
        ''' will be the contained basic type; and a special pseudo-slot, .Data, will be shown with the corresponding class. See the "numWithId" 
        ''' example below.
        ''' 
        ''' Classes may also inherit from "vector", "matrix" or "array". The data part of these objects can be any vector data type.
        ''' 
        ''' For an object from any class that does not contain one of these types or classes, typeof(x) will be "S4".
        ''' 
        ''' Some R data types do not behave normally, in the sense that they are non-local references or other objects that are not duplicated. 
        ''' Examples include those corresponding to classes "environment", "externalptr", and "name". These can not be the types for objects 
        ''' with user-defined classes (either S4 or S3) because setting an attribute overwrites the object in all contexts. It is possible to define
        ''' a class that inherits from such types, through an indirect mechanism that stores the inherited object in a reserved slot, ".xData". 
        ''' See the example for class "stampedEnv" below. An object from such a class does not have a ".Data" pseudo-slot.
        ''' 
        ''' For most computations, these classes behave transparently as if they inherited directly from the anomalous type. S3 method dispatch and
        ''' the relevant as.type() functions should behave correctly, but code that uses the type of the object directly will not. For example, 
        ''' as.environment(e1) would work as expected with the "stampedEnv" class, but typeof(e1) is "S4".
        ''' 
        ''' #### Inheriting from S3 Classes
        ''' 
        ''' Old-style S3 classes have no formal definition. Objects are “from” the class when their class attribute contains the character string 
        ''' considered to be the class name.
        ''' 
        ''' Using such classes with formal classes and methods is necessarily a risky business, since there are no guarantees about the content of 
        ''' the objects or about consistency of inherited methods. Given that, it is still possible to define a class that inherits from an S3 
        ''' class, providing that class has been registered as an old class (see setOldClass).
        ''' 
        ''' Broadly speaking, both S3 and S4 method dispatch try to behave sensibly with respect to inheritance in either system. Given an S4 object, 
        ''' S3 method dispatch and the inherits function should use the S4 inheritance information. Given an S3 object, an S4 generic function will 
        ''' dispatch S4 methods using the S3 inheritance, provided that inheritance has been declared via setOldClass. For details, see setOldClass 
        ''' and Section 10.8 of the reference.
        ''' 
        ''' #### Classes and Packages
        ''' 
        ''' Class definitions normally belong to packages (but can be defined in the global environment as well, by evaluating the expression on the 
        ''' command line or in a file sourced from the command line). The corresponding package name is part of the class definition; that is, part 
        ''' of the classRepresentation object holding that definition. Thus, two classes with the same name can exist in different packages, for most
        ''' purposes.
        ''' 
        ''' When a class name is supplied for a slot or a superclass in a call to setClass, a corresponding class definition will be found, looking 
        ''' from the namespace of the current package, assuming the call in question appears directly in the source for the package, as it should 
        ''' to avoid ambiguity. The class definition must be already defined in this package, in the imports directives of the package's DESCRIPTION 
        ''' and NAMESPACE files or in the basic classes defined by the methods package. (The ‘methods’ package must be included in the imports directives
        ''' for any package that uses S4 methods and classes, to satisfy the "CMD check" utility.)
        ''' 
        ''' If a package imports two classes of the same name from separate packages, the packageSlot of the name argument needs to be set to the 
        ''' package name of the particular class. This should be a rare occurrence.
        ''' 
        ''' #### References
        ''' 
        ''' Chambers, John M. (2016) Extending R, Chapman &amp; Hall. (Chapters 9 and 10.)
        ''' </remarks>
        <ExportAPI("setClass")>
        Public Function setClass(Class$,
                                 Optional representation As Object = Nothing,
                                 Optional prototype As list = Nothing,
                                 Optional contains As String = Nothing,
                                 Optional validity As Object = Nothing,
                                 Optional access As Object = Nothing,
                                 Optional where As Object = Nothing,
                                 Optional version As Object = Nothing,
                                 Optional sealed As Object = Nothing,
                                 Optional package As Object = Nothing,
                                 Optional S3methods As Boolean = False,
                                 Optional slots As list = Nothing,
                                 Optional env As Environment = Nothing) As Object

            Dim def As New S4Object With {
                .class_name = [Class],
                .slots = If(slots, Internal.Object.list.empty).AsGeneric(env, [default]:="any"),
                .prototype = If(prototype, Internal.Object.list.empty).slots,
                .contains = contains
            }
            env.globalEnvironment.types([Class]) = def.eval(env.globalEnvironment)
            Return def
        End Function

        ''' <summary>
        ''' ### Generate an Object from a Class
        ''' 
        ''' A call to new returns a newly allocated object from the class identified by the first argument. This call in turn calls 
        ''' the method for the generic function initialize corresponding to the specified class, passing the ... arguments to this 
        ''' method. In the default method for initialize(), named arguments provide values for the corresponding slots and unnamed 
        ''' arguments must be objects from superclasses of this class.
        ''' 
        ''' A call to a generating function for a class (see setClass) will pass its ... arguments to a corresponding call to New().
        ''' </summary>
        ''' <param name="Class">either the name Of a Class, a character String, (the usual Case) Or the Object describing the Class 
        ''' (e.g., the value returned by getClass). Note that the character String passed from a generating Function includes the 
        ''' package name As an attribute, avoiding ambiguity If two packages have identically named classes.</param>
        ''' <param name="args">arguments to specify properties of the new object, to be passed to initialize().</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' #### Initialize Methods
        ''' 
        ''' The generic Function initialize Is Not called directly. A Call To New begins by copying the prototype Object from the Class
        ''' definition, And Then calls initialize() With this Object As the first argument, followed by the ... arguments.
        ''' 
        ''' The interpretation Of the ... arguments In a Call To a generator Function Or To New() can be specialized To particular classes,
        ''' by defining an appropriate method For "initialize".
        ''' 
        ''' In the default method, unnamed arguments in the ... are interpreted as objects from a superclass, And named arguments are 
        ''' interpreted as objects to be assigned into the correspondingly named slots. Explicitly specified slots override inherited 
        ''' information for the same slot, regardless of the order in which the arguments appear.
        ''' 
        ''' The initialize methods Do Not have To have ... As their second argument (see the examples). Initialize methods are often written
        ''' When the natural parameters describing the New Object are Not the names Of the slots. If you Do define such a method, you 
        ''' should include ... As a formal argument, And your method should pass such arguments along via callNextMethod. This helps the 
        ''' definition Of future subclasses Of your Class. If these have additional slots And your method does Not have this argument, it 
        ''' will be difficult For these slots To be included In an initializing Call.
        ''' 
        ''' See initialize-methods For a discussion Of some classes With existing methods.
        ''' 
        ''' Methods for initialize can be inherited only by simple inheritance, since it Is a requirement that the method return an object 
        ''' from the target class. See the simpleInheritanceOnly argument to setGeneric And the discussion in setIs for the general concept.
        ''' 
        ''' Note that the basic vector classes, "numeric", etc. are implicitly defined, so one can use New For these classes. The ... 
        ''' arguments are interpreted As objects Of this type And are concatenated into the resulting vector.
        ''' 
        ''' #### References
        ''' 
        ''' Chambers, John M. (2016) Extending R, Chapman &amp; Hall. (Chapters 9 And 10.)
        ''' </remarks>
        <ExportAPI("new")>
        Public Function new_s4(Class$, <RListObjectArgument> args As list, Optional env As Environment = Nothing) As Object
            Dim def As IRType = env.globalEnvironment.types.TryGetValue([Class])

            If def Is Nothing Then
                Return Message.NullOrStrict(env.strictOption, $"new {[Class]}();", env)
            End If

            If TypeOf def Is S4Object Then
                Return DirectCast(def, S4Object).createObject(args, env)
            Else
                Return DirectCast(def, RType).createObject(args, env)
            End If
        End Function
    End Module
End Namespace