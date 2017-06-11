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

#### ``if`` statement

```R
if(TRUE) {
    # blablabla
    println("%s is TRUE", TRUE);
}
```

```xml
<?xml version="1.0" encoding="utf-16"?>
<MainOfLanguageTokens xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <program>
    <StatementOfLanguageTokens>
      <t name="Function">
        <Arguments>
          <StatementOfLanguageTokens>
            <t name="Object">TRUE</t>
            <Trace line="1">
if(TRUE)</Trace>
          </StatementOfLanguageTokens>
        </Arguments>
        <Closure>
          <program>
            <StatementOfLanguageTokens>
              <t name="Comment"> blablabla</t>
              <t name="Function">
                <Arguments>
                  <StatementOfLanguageTokens>
                    <t name="String">%s is TRUE</t>
                    <Trace line="2"> {
    # blablabla
    println("%s is TRUE",</Trace>
                  </StatementOfLanguageTokens>
                  <StatementOfLanguageTokens>
                    <t name="Object">TRUE</t>
                    <Trace line="2"> TRUE)</Trace>
                  </StatementOfLanguageTokens>
                </Arguments>println</t>
              <Trace line="2">;</Trace>
            </StatementOfLanguageTokens>
          </program>
        </Closure>if</t>
      <Trace line="3" />
    </StatementOfLanguageTokens>
  </program>
</MainOfLanguageTokens>
```

A more complex ``if`` statement:

```R
if(a == b) {
    println("%s is equals to %s", a, b);
} else if(a <= b) {
    println("%s is less than or equals to %s", a, b);
} else {
    println("Not sure about this.");
}
```

```xml
<?xml version="1.0" encoding="utf-16"?>
<MainOfLanguageTokens xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <program>
    <StatementOfLanguageTokens>
      <t name="Function">
        <Arguments>
          <StatementOfLanguageTokens>
            <t name="Object">a</t>
            <t name="Operator">==</t>
            <t name="Object">b</t>
            <Trace line="1">
if(a == b)</Trace>
          </StatementOfLanguageTokens>
        </Arguments>
        <Closure>
          <program>
            <StatementOfLanguageTokens>
              <t name="Function">
                <Arguments>
                  <StatementOfLanguageTokens>
                    <t name="String">%s is equals to %s</t>
                    <Trace line="2"> {
    println("%s is equals to %s",</Trace>
                  </StatementOfLanguageTokens>
                  <StatementOfLanguageTokens>
                    <t name="Object">a</t>
                    <Trace line="2"> a,</Trace>
                  </StatementOfLanguageTokens>
                  <StatementOfLanguageTokens>
                    <t name="Object">b</t>
                    <Trace line="2"> b)</Trace>
                  </StatementOfLanguageTokens>
                </Arguments>println</t>
              <Trace line="2">;</Trace>
            </StatementOfLanguageTokens>
          </program>
        </Closure>if</t>
      <Trace line="3" />
    </StatementOfLanguageTokens>
    <StatementOfLanguageTokens>
      <t name="Object">else</t>
      <t name="Function">
        <Arguments>
          <StatementOfLanguageTokens>
            <t name="Object">a</t>
            <t name="Operator">&lt;=</t>
            <t name="Object">b</t>
            <Trace line="3"> else if(a &lt;= b)</Trace>
          </StatementOfLanguageTokens>
        </Arguments>
        <Closure>
          <program>
            <StatementOfLanguageTokens>
              <t name="Function">
                <Arguments>
                  <StatementOfLanguageTokens>
                    <t name="String">%s is less than or equals to %s</t>
                    <Trace line="4"> {
    println("%s is less than or equals to %s",</Trace>
                  </StatementOfLanguageTokens>
                  <StatementOfLanguageTokens>
                    <t name="Object">a</t>
                    <Trace line="4"> a,</Trace>
                  </StatementOfLanguageTokens>
                  <StatementOfLanguageTokens>
                    <t name="Object">b</t>
                    <Trace line="4"> b)</Trace>
                  </StatementOfLanguageTokens>
                </Arguments>println</t>
              <Trace line="4">;</Trace>
            </StatementOfLanguageTokens>
          </program>
        </Closure>if</t>
      <Trace line="5" />
    </StatementOfLanguageTokens>
    <StatementOfLanguageTokens>
      <t name="Object">
        <Closure>
          <program>
            <StatementOfLanguageTokens>
              <t name="Function">
                <Arguments>
                  <StatementOfLanguageTokens>
                    <t name="String">Not sure about this.</t>
                    <Trace line="6"> else {
    println("Not sure about this.")</Trace>
                  </StatementOfLanguageTokens>
                </Arguments>println</t>
              <Trace line="6">;</Trace>
            </StatementOfLanguageTokens>
          </program>
        </Closure>else</t>
      <Trace line="7" />
    </StatementOfLanguageTokens>
  </program>
</MainOfLanguageTokens>
```

#### ``function`` declares statement

```R
library(System);

println <- function(s, ...) {
    s <- sprintf(s, ...);
    Console:WriteLine(s);
}
```

Supports of the optional parameter scripting:

```R
test <- function(a, b, c = (a + b)^2) {
	return [a ^ 3, -b * c, c];
}
```

#### ``for`` loop statement

The R language its ``for`` loop is equals to the ``For Each`` Loop in VisualBasic:

```R
for (i in 1:50) {

}

for (n in [1:50, 0.1]) {

}

var x <- [1:50,0.01];

for (d in x) {

}
```

```vbnet
For Each i In (1, 50).Range
Next

For Each n In (1, 50).Range(step:=0.1)
Next

Dim x = (1, 50).Range(0.01)

For Each d In x
Next
```

#### ``do`` loop statement

```R
do(TRUE) {

}

do {

} while(TRUE);
```