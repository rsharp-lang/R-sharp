let disk <- "./R#save.rda";
let names as string = load(disk);

print("Display loaded data:");

for(name in names) {
    print(`Vector of '${name}':`);
    print(get(name));
}
