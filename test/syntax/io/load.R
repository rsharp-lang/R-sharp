setwd(@dir);

# demo dataset contains two variable: x and word
let disk <- "./R#save.rda";
let names as string = load(disk);

print("Display loaded data:");

# x, word
for(name in names) {
    print(`Vector of '${name}':`);
    print(get(name));
}

# 'word' variable is created in global environment
# once the rda load action success
print(`Hello ${word}!!!`);
