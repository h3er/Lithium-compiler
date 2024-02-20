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

```
func <identifier>(<param1>, <param2>...){
    //do something;
    return;
}
```
*note - currently will throw error if there is a return value, however return must be there otherwise program will compile then seg fault.*

*function syntax will change soon when return values are implemented.*

Exit:

`exit(<8 bit exit code)>);`

*note - must be included otherwise program will compile then seg fault.*
