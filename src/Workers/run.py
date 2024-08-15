import os, filecmp ,sys

codes = {200:'success',404:'file not found',400:'error',408:'timeout'}

def compile(file,lang):

    if(lang =='python' or lang == 'javascript' or lang == 'csharp' or lang == 'typescript'):
        return 200

    if (os.path.isfile(file)):
        if lang=='c':
            os.system('gcc ' + file)
        elif lang=='cpp':
            os.system('g++ ' + file)
        elif lang=='java':
             # Get the class name from the file
            class_name = get_java_class_name_from_file(file)
            expected_file_name = class_name + '.java'
            # If the file name doesn't match the class name, rename the file
            if file != expected_file_name:
                os.rename(file, expected_file_name)
                file = expected_file_name
            os.system('javac ' + file)
        if (os.path.isfile('a.out')) or (os.path.isfile(get_java_class_name(os.getcwd()))):
            return 200
        else:
            return 400
    else:
        return 404

def run(file,input,timeout,lang):
    cmd='' 
    if lang == 'java':
        class_name = get_java_class_name(os.getcwd())
        cmd += 'java ' + class_name.split('.')[0]
    elif lang=='c' or lang=='cpp':
        cmd += './a.out'
    elif lang=='python':
        cmd += 'python3 '+ file
    elif lang == 'javascript':
        cmd += 'node '+ file
    elif lang == 'csharp':
        cmd += 'dotnet run ' + file

    r = os.system('timeout '+timeout+' '+cmd+' < '+input + ' > '+testout)

    if r==0:
        return 200
    elif r==31744:
        return 408
    else:
        return 400

def match(output):
    if os.path.isfile('out.txt') and os.path.isfile(output):
        b = filecmp.cmp('out.txt',output)
        os.remove('out.txt')
        return b
    else:
        return 404

def get_java_class_name(path):
    os.chdir(path)
    class_name = ''
    for filename in os.listdir():
        if filename.endswith(".class"):
            class_name = filename
    return class_name   

def get_java_class_name_from_file(file):
    with open(file, 'r') as f:
        for line in f:
            if line.strip().startswith('public class'):
                return line.split()[2]
    return None              

params=sys.argv
print(params)
file = params[1].split('/')[2]
print(file)
path = os.getcwd()
folder = params[1].split('/')[1]
print(folder)
path = 'temp/' +folder +'/'

os.chdir(path)
lang = params[2]
timeout = str(min(15,int(params[3])))


testin =  "input.txt"
testout =  "output.txt"

status=compile(file,lang)
if status ==200:
    status=run(file,testin,timeout,lang)
print(codes[status])