imports "c" from "d";

let doRequire as function(pkgName as string) {

require(pkgName);

}

let imports_internal_assembly as function() {

	imports "a" from "b";
}