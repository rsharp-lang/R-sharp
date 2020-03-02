# Special Closures in R#

There are sevral closure type in R# language

## ``using`` closure

The ``using`` closure can apply for the automatically operation like write file, send data automatically when finish the operations, example as: 

```R
using file as data.frame() :> auto(table -> table :> write.csv(file = "...")) {

}
```