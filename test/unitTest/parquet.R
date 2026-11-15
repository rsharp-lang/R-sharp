# parquet file format read/write api test
#
# 该脚本会把 dataframe 写成 parquet 文件，然后再读取回来，
# 并由 test/unitTest/parquet_verify.py 使用 pyarrow 对写出的文件做逐值校验。
#
# 运行方式：
#   R#.exe parquet.R
#   python parquet_verify.py parquet-rscript.parquet parquet-rscript-2.parquet

require(dataframe);

let file1 = `${@dir}/parquet-rscript.parquet`;
let file2 = `${@dir}/parquet-rscript-2.parquet`;

# 数值缺失值使用 NaN（0/0）表示，写出时会被表示为 parquet 的 null 槽位；
# 字符列的缺失值（NA）同样被写出为 null 槽位。
let df = data.frame(
    id = 1:6,
    score = [0.5, -1.25, 0/0, 2.0, 0.0, 1e-8],
    label = ["alpha", NA, "", "delta", "中文", "z"],
    ok = [TRUE, FALSE, TRUE, FALSE, TRUE, FALSE]
);

print("source dataframe:");
print(df);

let ok = write.parquet(df, file = file1);

if (!ok) {
    stop("write.parquet() failed!");
}

print(`parquet file is written: ${file1}`);

let df2 = read.parquet(file1);

print("dataframe that reads from the parquet file:");
print(df2);

# 读回来的 dataframe 再次写出，用于验证两次写出的文件内容一致
write.parquet(df2, file = file2);

print(`parquet file is written: ${file2}`);
print("parquet test done!");
