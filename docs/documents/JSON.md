# JSON

JSON (JavaScript Object Notation) is a lightweight data-interchange format. 
 It is easy for humans to read and write. It is easy for machines to parse and 
 generate. It is based on a subset of the JavaScript Programming Language 
 Standard ECMA-262 3rd Edition - December 1999. JSON is a text format that 
 is completely language independent but uses conventions of the ``R#`` language. 
 JSON is an ideal data-interchange language.

 JSON Is built On two structures:
 
 + A collection Of name/value pairs. In various languages, this Is realized As 
      an Object, record, struct, dictionary, hash table, keyed list, Or 
      associative array.
 + An ordered list Of values. In most languages, this Is realized As an array, 
      vector, list, Or sequence.
      
 These are universal data structures. Virtually all modern programming languages 
 support them In one form Or another. It makes sense that a data format that 
 Is interchangeable With programming languages also be based On these structures.

+ [json_decode](JSON/json_decode.1) ### Decodes a JSON string
+ [json_encode](JSON/json_encode.1) Returns the JSON representation of a value
+ [parseJSON](JSON/parseJSON.1) parse JSON string into the raw JSON model or R data object
+ [parseBSON](JSON/parseBSON.1) parse the binary JSON data into the raw JSON model or R data object
+ [object](JSON/object.1) Convert the raw json model into R data object
+ [writeBSON](JSON/writeBSON.1) save any R object into BSON stream data
