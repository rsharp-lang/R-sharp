imports "package_utils" from "devkit";

setwd(@dir);

package_utils::attach("./pkg1/");
package_utils::attach("./pkg2/");

echo_pkg1();
echo_pkg2();