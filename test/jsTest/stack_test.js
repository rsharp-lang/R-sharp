function _stop() {
	throw 'error comes from javascript';
}

function stop_caller() {
	_stop();
}