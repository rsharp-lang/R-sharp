let msg = "hello world!";

cat(msg, "\n");
cat(msg, "write this to file", "\n", file = relative_work("aaa.log"));
cat("aabbccdd", "\n", file = relative_work("aaa.log"), append = TRUE);