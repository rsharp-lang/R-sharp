require(graphQL);

imports "mysql" from "graphR";

print(data.frame(a = 1:6, s = c("dsaa","www","qqq","333","4444","5555")) |> .Internal::select( strict = FALSE, a -> "id", "s")   );