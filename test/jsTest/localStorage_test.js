var x = [5,6,7,8,34,34,Math.random()];

localStorage.setItem("nums_v", x);
console.log(x);

const zzz = x;

x = null;
console.log(x)
x = localStorage.getItem("nums_v")
console.log(x)

let test = all(zzz = x)

console.log(test)