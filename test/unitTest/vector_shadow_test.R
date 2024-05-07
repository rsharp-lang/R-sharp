

let single_list = list(a = 1, b = 2);

print(data.frame(a = single_list@a, b = single_list@b));

let multiple_list = [

    list(a = 1, b = 2),
    list(a = 2, b = 4),
    list(a = 3, b = 8),
    list(a = 4, b = 16)

];

print(data.frame(a = multiple_list@a, b = multiple_list@b));