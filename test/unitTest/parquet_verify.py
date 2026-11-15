"""使用 pyarrow 校验 R# 脚本（parquet.R）写出的 parquet 文件。

usage:
    python parquet_verify.py <first.parquet> [second.parquet]

第一个文件由 R# 直接写出（期望值在脚本中硬编码），
第二个文件是「读取之后再写出」的结果，要求与第一个文件的内容完全一致。
"""

import datetime
import math
import sys

import pyarrow as pa
import pyarrow.parquet as pq

UTC = datetime.timezone.utc

passed = 0
failed = 0


def report(ok, name, expected=None, actual=None):
    global passed, failed

    if ok:
        passed += 1
        sys.stdout.write("PASS %s\n" % name)
    else:
        failed += 1
        sys.stdout.write("FAIL %s expected=%r actual=%r\n" % (name, expected, actual))


def epoch_millis(value):
    if isinstance(value, datetime.datetime):
        if value.tzinfo is None:
            value = value.replace(tzinfo=UTC)

        return int(round(value.timestamp() * 1000))

    if isinstance(value, datetime.date):
        return int(round(datetime.datetime(value.year, value.month, value.day,
                                          tzinfo=UTC).timestamp() * 1000))

    return None


def value_equals(expected, actual):
    if expected is None:
        return actual is None

    if isinstance(expected, float) and math.isnan(expected):
        return isinstance(actual, float) and math.isnan(actual)

    if isinstance(expected, bool) or isinstance(actual, bool):
        return bool(expected) == bool(actual)

    if isinstance(expected, (int, float)) and isinstance(actual, (int, float)):
        return abs(float(actual) - float(expected)) <= 1e-9 * max(1.0, abs(float(expected)))

    # date32 / timestamp 读取之后统一为日期时间，这里按时间戳比较
    if isinstance(expected, (datetime.date, datetime.datetime)) and \
            isinstance(actual, (datetime.date, datetime.datetime)):
        return epoch_millis(expected) == epoch_millis(actual)

    return expected == actual


EXPECTED = {
    "__index__": ["[1, ]", "[2, ]", "[3, ]", "[4, ]", "[5, ]", "[6, ]"],
    "id": [1, 2, 3, 4, 5, 6],
    # R# 的数值缺失值（NaN）写出为 null 槽位
    "score": [0.5, -1.25, None, 2.0, 0.0, 1e-8],
    "label": ["alpha", None, "", "delta", "中文", "z"],
    "ok": [True, False, True, False, True, False],
}

EXPECTED_TYPES = {
    "__index__": pa.types.is_string,
    # R# 的整数序列使用 32 位或者 64 位整数承载，两者都是合法的
    "id": lambda t: pa.types.is_int32(t) or pa.types.is_int64(t),
    "score": pa.types.is_float64,
    "label": pa.types.is_string,
    "ok": pa.types.is_boolean,
}


def verify_first(path):
    table = pq.read_table(path)

    report(table.num_rows == 6, "rows", 6, table.num_rows)

    for name in EXPECTED:
        if name not in table.column_names:
            report(False, "column %s exists" % name, True, False)
            continue

        report(True, "column %s exists" % name)
        report(EXPECTED_TYPES[name](table.schema.field(name).type),
               "column %s type" % name, name, str(table.schema.field(name).type))

        actual = table.column(name).to_pylist()
        expected = EXPECTED[name]

        if len(actual) != len(expected):
            report(False, "column %s length" % name, len(expected), len(actual))
            continue

        for i in range(len(expected)):
            report(value_equals(expected[i], actual[i]), "column %s[%d]" % (name, i),
                   expected[i], actual[i])


def verify_roundtrip(first, second):
    a = pq.read_table(first).to_pydict()
    b = pq.read_table(second).to_pydict()

    # 目标文件可能额外包含 __index__ 行名列，因此这里只要求原文件的列都还存在
    report(set(a.keys()) <= set(b.keys()), "roundtrip columns",
           sorted(a.keys()), sorted(b.keys()))

    for name in sorted(a.keys()):
        left = a[name]
        right = b.get(name)

        if right is None:
            report(False, "roundtrip column %s" % name, "exists", "missing")
            continue

        if len(left) != len(right):
            report(False, "roundtrip column %s length" % name, len(left), len(right))
            continue

        bad = -1

        for i in range(len(left)):
            if not value_equals(left[i], right[i]):
                bad = i
                break

        if bad < 0:
            report(True, "roundtrip column %s" % name)
        else:
            report(False, "roundtrip column %s[%d]" % (name, bad), left[bad], right[bad])


def main():
    roundtrip_only = "--roundtrip" in sys.argv
    files = [arg for arg in sys.argv[1:] if not arg.startswith("--")]

    if len(files) < 1:
        sys.stderr.write("usage: python parquet_verify.py [--roundtrip] <first.parquet> [second.parquet]\n")
        return 2

    if not roundtrip_only:
        verify_first(files[0])

    if len(files) > 1:
        verify_roundtrip(files[0], files[1])

    sys.stdout.write("SUMMARY pass=%d fail=%d\n" % (passed, failed))

    return 0 if failed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
