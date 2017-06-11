# How the interpreter works?

```
script -> [TokenIcer] -> SourceTree -> ListOf RStatements -> ExecuteMachine
```

## Statement in R#

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