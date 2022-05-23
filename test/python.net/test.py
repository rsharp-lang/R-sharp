from clr_loader import get_coreclr
from pythonnet import set_runtime

import sys
import clr

rt = get_coreclr("app.runtime.json")
set_runtime(rt)

sys.path.append(r"/usr/local/bin/")
sys.path.append(r"/etc/r_env/library/ggplot/lib/assembly/")

clr.AddReference(r"REnv.dll")

# test R# engine
from REnv import Class1

calc = Class1()
print(calc.__class__.__name__)
result = calc.HelloWorld()
print(result)