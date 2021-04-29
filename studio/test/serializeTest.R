imports "package_utils" from "devkit";

print(package_utils::read(package_utils::serialize(parse(`

const target as function(b, a = sprintf("n%s!", [a, "test"])) {
    print(a);
}

`))));

