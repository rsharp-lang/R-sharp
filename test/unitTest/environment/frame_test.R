imports "DebugAudit" from "devkit";

let a = c(25,23,5,34);

DebugAudit::capture(a * 86.3332);
DebugAudit::check(all(a <= 0));

print(DebugAudit::dump());