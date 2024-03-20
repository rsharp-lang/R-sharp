sink(file = file.path(@dir, "demo.log"));

print("123");

warning("message1");
warning("message1");
warning("message1");
warning("message1");
warning("message1");

try({
    stop("a stop message!");
});

print("end of test");

sink();