options(strict = FALSE);

y    = 9;
exec = function(x, f) f(x) + y;

(function() {
    const y = 1;
    print(exec(1, function(x) x + y));
})();
(function() {
    const y = 1;
    print(exec(1, x -> x + y));
})();