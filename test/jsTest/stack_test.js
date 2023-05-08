function _stop() {
	throw 'error comes from javascript';
}

function stop_caller() {
	_stop();
}

console.log("hello world from javascript")
console.table([1,3,5,7,9])