# ============================================================
# DebugAudit R# script testing toolkit - unit test
#
# 目的：验证 studio/Rsharp_kit/devkit/debugAudit.vb 暴露的全部 ExportAPI：
#       capture / capture_as / expr_text / snapshot / check / test / require /
#       are_equal / check_throws / mark / watch / dump / to_json /
#       save_report / value_text / clear_captures / reset / failure_count /
#       last_failure / records / captures / max_events / throw_on_failure
#
# 运行：
#   cd G:\GCModeller\src\R-sharp\App\net10.0
#   & ".\R#.exe" "G:\GCModeller\src\R-sharp\test\unitTest\environment\frame_test.R" --attach "G:\GCModeller" --debug=none
#
# 前置：dotnet build "G:\GCModeller\src\R-sharp\R_system.NET5.slnx" -c Rsharp_app_release -p:Platform=x64
#       devkit.dll 由该构建输出到 App\net10.0，运行时自动加载
# ============================================================

imports "DebugAudit" from "devkit";

let report = "G:/GCModeller/src/R-sharp/test/unitTest/environment/frame_test_report.txt";
let lines = c("[DebugAudit toolkit test]");

print("[STEP] 1/9 reset and configure");
DebugAudit::reset();
if (DebugAudit::failure_count() != 0) {
    stop("[FAIL] reset should clear failure counter");
}
if (DebugAudit::max_events(32) != 32) {
    stop("[FAIL] max_events setter");
}
if (DebugAudit::throw_on_failure() != FALSE) {
    stop("[FAIL] throw_on_failure should default to FALSE");
}
lines = c(lines, "[PASS] step 1: reset / max_events / throw_on_failure");

print("[STEP] 2/9 capture, capture_as, expr_text, value_text");
let a = c(25, 23, 5, 34);
DebugAudit::capture(a * 2);
DebugAudit::capture_as("answer", 42);
if (DebugAudit::expr_text(a * 2) == "") {
    stop("[FAIL] expr_text should return source text");
}
if (DebugAudit::value_text(42) != "42") {
    stop("[FAIL] value_text(42) should be '42'");
}
lines = c(lines, "[PASS] step 2: capture / capture_as / expr_text / value_text");

print("[STEP] 3/9 captures and clear_captures");
if (length(DebugAudit::captures()) < 2) {
    stop("[FAIL] captures should contain the two recorded values");
}
DebugAudit::mark("checkpoint", 7);
if (length(DebugAudit::captures()) < 3) {
    stop("[FAIL] mark should also write a capture value");
}
DebugAudit::clear_captures();
if (length(DebugAudit::captures()) != 0) {
    stop("[FAIL] clear_captures should empty the snapshot");
}
lines = c(lines, "[PASS] step 3: captures / mark / clear_captures");

print("[STEP] 4/9 check - success and failure");
DebugAudit::check(all(a > 0), "all values positive");
if (DebugAudit::failure_count() != 0) {
    stop("[FAIL] successful check should not record failure");
}
DebugAudit::check(all(a <= 0), "all values non-positive");
if (DebugAudit::failure_count() != 1) {
    stop("[FAIL] failed check should record exactly one failure");
}
lines = c(lines, "[PASS] step 4: check success and failure");

print("[STEP] 5/9 test - boolean return");
if (DebugAudit::test(any(a > 30)) != TRUE) {
    stop("[FAIL] test should return TRUE for satisfied condition");
}
if (DebugAudit::test(any(a > 100)) != FALSE) {
    stop("[FAIL] test should return FALSE for unsatisfied condition");
}
if (DebugAudit::failure_count() != 2) {
    stop("[FAIL] test failure should be recorded");
}
lines = c(lines, "[PASS] step 5: test boolean return");

print("[STEP] 6/9 are_equal - scalar and vector");
if (DebugAudit::are_equal(42, 42) != TRUE) {
    stop("[FAIL] are_equal should accept equal scalars");
}
if (DebugAudit::are_equal(1, 2) != FALSE) {
    stop("[FAIL] are_equal should reject unequal scalars");
}
if (DebugAudit::are_equal(c(1, 2, 3), c(1, 2, 3)) != TRUE) {
    stop("[FAIL] are_equal should accept equal vectors");
}
if (DebugAudit::failure_count() != 3) {
    stop("[FAIL] are_equal failure should be recorded");
}
lines = c(lines, "[PASS] step 6: are_equal scalar and vector");

print("[STEP] 7/9 check_throws and require");
if (DebugAudit::check_throws(stop("boom")) != TRUE) {
    stop("[FAIL] check_throws should detect stop() error");
}
if (DebugAudit::check_throws(1 + 1) != FALSE) {
    stop("[FAIL] check_throws should reject code without error");
}
let reqOk = DebugAudit::require(TRUE);
if (reqOk != TRUE) {
    stop("[FAIL] require should return TRUE for satisfied condition");
}
# require(FALSE) 会抛出 CLR 异常直接终止脚本，属于预期的 fail-fast 行为，
# 因此这里只验证成功路径；失败路径由 check/test/are_equal 覆盖。
if (DebugAudit::failure_count() != 4) {
    stop("[FAIL] expect 4 recorded failures so far");
}
lines = c(lines, "[PASS] step 7: check_throws / require");

print("[STEP] 8/9 watch, snapshot, dump, to_json, save_report");
let sumVal = DebugAudit::watch("sum(a)", sum(a));
if (DebugAudit::value_text(sumVal) != "87") {
    stop("[FAIL] watch should return the evaluated value");
}
DebugAudit::snapshot(list(x = 1, y = 2, z = 3));
if (length(DebugAudit::captures()) != 3) {
    stop("[FAIL] snapshot should replace the whole capture set");
}
if (length(DebugAudit::records()) < 5) {
    stop("[FAIL] records should contain audit events");
}
if (DebugAudit::last_failure() == NULL) {
    stop("[FAIL] last_failure should not be NULL after failures");
}
let textReport = DebugAudit::dump();
if (textReport == "") {
    stop("[FAIL] dump should produce text report");
}
let jsonReport = DebugAudit::to_json();
if (jsonReport == "") {
    stop("[FAIL] to_json should produce JSON report");
}
if (DebugAudit::dump(json = TRUE) != jsonReport) {
    stop("[FAIL] dump(json=TRUE) should match to_json");
}
DebugAudit::save_report("G:/GCModeller/src/R-sharp/test/unitTest/environment/debug_audit_report.json");
lines = c(lines, "[PASS] step 8: watch / snapshot / dump / to_json / save_report");

print("[STEP] 9/9 final verification");
if (DebugAudit::failure_count() != 5) {
    stop("[FAIL] final failure count mismatch");
}
if (DebugAudit::max_events() != 32) {
    stop("[FAIL] max_events getter");
}
if (DebugAudit::throw_on_failure() != FALSE) {
    stop("[FAIL] throw_on_failure getter");
}
lines = c(lines, "[PASS] step 9: final state checks");

writeLines(lines, con = report);
print("[PASS] all DebugAudit toolkit checks passed");
