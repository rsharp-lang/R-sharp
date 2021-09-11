imports "package_utils" from "devkit";

const byts = package_utils::serialize(function(x = NULL) {
	print(x);
});
const func = package_utils::read(byts);

func();
func(22);

pause();



