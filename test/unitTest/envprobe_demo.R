# Production server environment diagnostics
#
# run this script on the target (production) server for collecting all of the
# environment factors which may be limits the cpu utilization rate of the R#
# data science program.
#
#   Rscript envprobe_demo.R

imports "envprobe" from "MLkit";
imports "report" from "base";

# ---------------------------------------------------------------------------
# 1. collect the static environment information only (no benchmark)
# ---------------------------------------------------------------------------

let info = envprobe::report(benchmark = FALSE);

print(info$host);
print(info$os);
print(info$cpu);
print(info$simd);
print(info$gc);

print("--- warnings ---");
print(info$diagnosis$warnings);

# ---------------------------------------------------------------------------
# 2. full diagnosis (with the cpu/memory micro benchmark) and write the
#    report files into both of the markdown and the JSON format
# ---------------------------------------------------------------------------

let files = envprobe::report(
    file = "./envprobe_report.md",
    json = "./envprobe_report.json",
    benchmark = TRUE
);

print(files$report_file);
print(files$json_file);
print(files$benchmark);
print(files$diagnosis$conclusion);

# ---------------------------------------------------------------------------
# 3. render the diagnosis report as a word/pdf/html document
# ---------------------------------------------------------------------------

report::render(
    envprobe::blocks(),
    "./envprobe_report.docx",
    title = "Production Server Environment Diagnostics"
);

# ---------------------------------------------------------------------------
# 4. benchmark only
# ---------------------------------------------------------------------------

let bench = envprobe::benchmark();

print(bench$peak_gflops);
print(bench$threads);
print(bench$thread_gflops);
print(bench$efficiency);
