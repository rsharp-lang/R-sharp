Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("optparse")>
Module optparse

    ''' <summary>
    ''' Functions to enable our OptionParser to recognize specific command line options.
    ''' 
    ''' add_option adds a option to a prexisting OptionParser instance whereas make_option is used to 
    ''' create a list of OptionParserOption instances that will be used in the option_list argument 
    ''' of the OptionParser function to create a new OptionParser instance.
    ''' </summary>
    ''' <param name="opt_str">A character vector containing the string of the desired long flag comprised of “–” followed by a letter and then a sequence of alphanumeric characters and optionally a string of the desired short flag comprised of the “-” followed by a letter.</param>
    ''' <param name="action">A character string that describes the action optparse should take when it encounters an option, either “store”, “store_true”, “store_false”, or “callback”. An action of “store” signifies that optparse should store the specified following value if the option is found on the command string. “store_true” stores TRUE if the option is found and “store_false” stores FALSE if the option is found. “callback” stores the return value produced by the function specified in the callback argument. If callback is not NULL then the default is “callback” else “store”.</param>
    ''' <param name="type">A character string that describes specifies which data type should be stored, either “logical”, “integer”, “double”, “complex”, or “character”. Default is “logical” if action %in% c("store_true", store_false), typeof(default) if action == "store" and default is not NULL and “character” if action == "store" and default is NULL. “numeric” will be converted to “double”.</param>
    ''' <param name="dest">A character string that specifies what field in the list returned by parse_args should optparse store option values. Default is derived from the long flag in opt_str.</param>
    ''' <param name="default">The default value optparse should use if it does not find the option on the command line.</param>
    ''' <param name="help">A character string describing the option to be used by print_help in generating a usage message. %default will be substituted by the value of default.</param>
    ''' <param name="metavar">A character string that stands in for the option argument when printing help text. Default is the value of dest.</param>
    ''' <param name="callback">A function that executes after the each option value is fully parsed. It's value is assigned to the option and its arguments are the option S4 object, the long flag string, the value of the option, the parser S4 object, and ....</param>
    ''' <param name="callback_args">A list of additional arguments passed to callback function (via do.call).</param>
    ''' <returns>Both make_option and add_option return instances of class <see cref="OptionParserOption"/>.</returns>
    Public Function make_option(opt_str$(),
                                Optional action As String = null,
                                Optional type As String = null,
                                Optional dest As String = null,
                                <RRawVectorArgument>
                                Optional [default] As Object = null,
                                Optional help As String = "",
                                Optional metavar As String = null,
                                Optional callback As Object = null,
                                Optional callback_args As Object = null) As OptionParserOption

        Return New OptionParserOption With {
            .[default] = [default],
            .help = help,
            .opt_str = opt_str,
            .type = type,
            .action = action
        }
    End Function
End Module

Public Class OptionParserOption

    Public Property opt_str As String()
    Public Property [default] As Object
    Public Property type As String
    Public Property help As String
    Public Property action As String

End Class