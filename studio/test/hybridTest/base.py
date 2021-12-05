
def printHello(msg):
    print(" --> printHello")
    xx = msg
    str(xx)
    print(`hello world, result is ${xx}!`)
    xx = "888888"
    print(xx)
    veryDeep()
    
def stopRun():
    
    print(data.frame(ID = ["AA","BB","CC"], live = TRUE))

    def throwEx():
        print(" --> throwEx")
        # print(traceback())
        stop(1111)
        
    print(" --> stopRun")
    throwEx()