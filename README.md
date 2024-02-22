<h1 align="center">How to use:</h1>


<h1 align="center">The language:</h1>

Declaring variables:

`<data type> <identifier> = <value>;`

Modifying variables:

`<identifier> = <value>;`

`<identifier>++;`

`<identifier>--;`

Current data types:

`int` 

*help*

Maths operations:

`+`, `-`, `*`, `/`

*note - `/` will return the whole number answer for now, ignoring any remainder (like `//` in python).*

Boolean logic:

`==`, `!=`, `>=`, `<=`, `>`, `<`

*note - can only be used as part of \<condition> until bool type is implemented.*

Comments:

`// <text> ;`

*note - can be multiline, single line or even inline, however the comment will end at the first semicolon(for now)*

Selection:

```
if(<condition>){
    //do something;
}
```
```
elif(<condition>){
    //do something;
}
```
```
else{
    //do something;
}
```

Loops:

```
for(<num>){
    //do something;
}
```

```
while(<condition>){
    //do something;
}
```

Functions:

**NOT CURRENTLY WORKING**

```
<return type> <identifier>(<param1>, <param2>...){
    //do something;
    return;
}
```

Exit:

`exit(<exit code)>);`

<h1 align="center">TODO:</h1>

- Test line numbers in errors
- Add return values for functions
- make changes to remove need for funcOffset
