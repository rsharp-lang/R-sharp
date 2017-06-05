# 所有的语句强制使用分号进行结尾
# for/if 强制使用大括号
# 变量赋值强制使用 <-， 变量在使用之前必须经过var定义
# 参数赋值使用等号
# 

var x <- {1, 2, 3, 4, 5};
var m <- mean(x);
var n <- x + m;

if (m <= 10) {
	println("test message %s", m);
} else {
	println("test2 %d", m);
}

## 数据类型是默认为向量类型的
## integer则为integer类型向量，double则为double类型的向量
## 函数必须要使用return进行返回，return不再是一个函数调用，但是也可以使用return (a/b);的形式
## 如果没有定义return，则默认返回null
## 函数也是一种变量类型，但是函数变量的初始化不需要var关键词
test <- function(a as integer, b as double) {

	var n <- a/b;
	
    ## 可以直接引用全局变量，起冲突的时候使用global变量
	return n + global$n;
}

var c <- test(11, b = 44);

## 常量？
imports s <- "123456";

## 列表参数类型
## 函数无返回值，默认返回null
test.list <- function(...) {

    ## 这个表达式表示从...列表之中取出变量a和变量b，当变量不存在的时候，初始化为null.
    var a <- ...;
	var b <- ...;
	
	if (a is null) {
	    println("var a is missing");
	} else if (b is null) {
	    println("var b is missing");
	}
	
	println(...);
}

test.list(a = 3, b = {1,2,3}, c = "yes");

## 逻辑值只有TRUE/FALSE
var true <- TRUE;
var false <- FALSE;