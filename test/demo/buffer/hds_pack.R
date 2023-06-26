require(HDS);

setwd(@dir);

let pack = HDS::openStream("./demo_test.hds", allowCreate = TRUE);

# write small text
pack |> writeText("/small.txt", "abc");
# write large text
pack |> writeText("/large.txt", [
    "asdasdasd","2222222222","3333333333","aaaaaaaaa",
    "qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq",
    "888888888888888888888888888888888888888888888888888888888888888888888888"]);
# write a smaller text file after the large text file
pack |> writeText("/smaller_than_large.txt", "afhakjdgaskdhasjkdhaskjdaskdghasdhaskdhasdhaskdhaskdhasdas");
# write a ultra large text file
pack |> writeText("/large.txt", `number lines at here: ${1:10000}`);
# 1 and 2 text file should be write in the original region of the large.txt
pack |> writeText("/1.txt",">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa<");
pack |> writeText("/2.txt",">bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb<");
pack |> writeText("/larger.txt", `at line: ${1:100}`);

# check file contents
extract_files(pack, fs = "./export_texts/");
# inspect of the file offsets
print(HDS::tree(pack));
