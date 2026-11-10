imports "envprobe" from "MLkit";
imports "report" from "base";

report::render(
    envprobe::blocks(), 
    here("server_diagnostics.md")
);
