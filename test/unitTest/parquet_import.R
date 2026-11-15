# 读取 pyarrow 生成的 parquet 文件，并把读取到的 dataframe 再次写出为 parquet。
#
# 该脚本用于验证 R# 的 read.parquet() 可以直接读取外部工具（pyarrow / pandas / spark 等）
# 生成的 parquet 文件；写出的文件由 test/unitTest/parquet_verify.py 比对。
#
# 运行方式：
#   python <parquet>/test/pyarrow/gen_fixture.py <fixture-dir>
#   copy <fixture-dir>/flat_basic.parquet parquet-pyarrow.parquet
#   R#.exe parquet_import.R
#   python parquet_verify.py parquet-pyarrow.parquet parquet-pyarrow-rt.parquet

require(dataframe);

let input = `${@dir}/parquet-pyarrow.parquet`;

let df = read.parquet(input);

print("dataframe that reads from the pyarrow parquet file:");
print(df);

write.parquet(df, file = `${@dir}/parquet-pyarrow-rt.parquet`);

print("the dataframe has been written back to parquet");
