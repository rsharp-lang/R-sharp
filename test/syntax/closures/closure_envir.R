# The closure expression in R# language is a kind of code block which is wrapped by a pair of bracket {}
# The function in R# language is a kind of closure expression with parameters input
# The closure expression have its own enviroment, so that you can use closure function for simulate a class object or someting, etc in R# language
# here is a demo about create a hit counter in R# language

let counter as function(start = 0) {
    # The closure expression have its own environment
    # so that you can expose the operation of this enviroment through a function closure value
    
    function() {
        start <- start + 1;
    }
}

let hit = counter();

print(hit()); # 1
print(hit()); # 2
print(hit()); # 3
print(hit()); # 4
print(hit()); # 5
print(hit()); # 6
print(hit()); # 7

pause();