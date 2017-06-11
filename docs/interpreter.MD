# How the interpreter works?

```
script -> [TokenIcer] -> SourceTree -> ListOf RStatements -> ExecuteMachine
```

## Statements in R#

#### Variable Declares

```R
# Generic type variable
var x <- 123;
```

```xml
<?xml version="1.0" encoding="utf-16"?>
<MainOfLanguageTokens xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <program>
    <StatementOfLanguageTokens>
      <t name="Comment"> Generic type variable</t>
      <t name="Object">var</t>
      <t name="Object">x</t>
      <t name="LeftAssign">&lt;-</t>
      <t name="Object">123</t>
      <Trace line="1">
# Generic type variable
var x &lt;- 123;</Trace>
    </StatementOfLanguageTokens>
  </program>
</MainOfLanguageTokens>
```

By default the R variable is generic type, but the user can constraint the variable to a specific primitive type. This type constraint feature only works on the R primitive type, which their type codes can be found in this declare:

```vbnet
''' <summary>
''' The R# types
''' </summary>
Public Enum TypeCodes As Byte

    ''' <summary>
    ''' Object type in R#
    ''' </summary>
    [generic] = 0

    ''' <summary>
    ''' Class type in R#
    ''' </summary>
    [list] = 10

    ''' <summary>
    ''' <see cref="Integer"/> vector
    ''' </summary>
    [integer] = 100
    ''' <summary>
    ''' <see cref="ULong"/> vector
    ''' </summary>
    [uinteger]
    ''' <summary>
    ''' <see cref="Double"/> numeric vector
    ''' </summary>
    [double]
    ''' <summary>
    ''' <see cref="String"/> vector
    ''' </summary>
    [string]
    ''' <summary>
    ''' <see cref="Char"/> vector
    ''' </summary>
    [char]
    ''' <summary>
    ''' <see cref="Boolean"/> vector
    ''' </summary>
    [boolean]

End Enum
```

Example:

```R
# Type constraint variable
var x as integer <- {1, 2, 3};
```

```xml
<?xml version="1.0" encoding="utf-16"?>
<MainOfLanguageTokens xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <program>
    <StatementOfLanguageTokens>
      <t name="Comment"> Type constraint variable</t>
      <t name="Object">var</t>
      <t name="Object">x</t>
      <t name="Object">as</t>
      <t name="Object">integer</t>
      <t name="LeftAssign">
        <Closure>
          <program>
            <StatementOfLanguageTokens>
              <t name="Object">1</t>
              <Trace line="1">
# Type constraint variable
var x as integer &lt;- {1,</Trace>
            </StatementOfLanguageTokens>
            <StatementOfLanguageTokens>
              <t name="Object">2</t>
              <Trace line="1"> 2,</Trace>
            </StatementOfLanguageTokens>
            <StatementOfLanguageTokens>
              <t name="Object">3</t>
              <Trace line="1"> 3}</Trace>
            </StatementOfLanguageTokens>
          </program>
        </Closure>&lt;-</t>
      <Trace line="1" />
    </StatementOfLanguageTokens>
  </program>
</MainOfLanguageTokens>
```