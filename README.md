# BlastForth

A Forth-based software development kit for the Sega Genesis/Megadrive platform.  Currently at version 0.  This is still a work-in-progress.  Updates will be made to this Readme file when I have time and when I think of something to write down.  Ask me questions!

Forth is a very powerful programming language/paradigm that you've probably never heard of.  The best way to learn it is to read the book *Starting Forth* by Leo Brodie.  First published in 1981, it remains today the best tutorial there is for the Forth programming language.  The BlastForth repository contains a PDF copy, or you can read it online here:  
https://www.forth.com/starting-forth/

You will need the Gforth compiler to build Genesis/Megadrive software with BlastForth.  Gforth is available for all major desktop platforms (Linux, Mac, Windows) and can be downloaded from here:  
https://gforth.org/

Linux users can also get Gforth from their distro's repositories, as it is a standard component of the GNU operating system.

See the file *Word-Reference.md* in this folder for a list of all the words provided by BlastForth.

### Contents

[How to Use BlastForth](https://github.com/WildChild83/BlastForth/blob/main/README.md#how-to-use-blastforth)  
[The System.fs File](https://github.com/WildChild83/BlastForth/blob/main/README.md#the-systemfs-file)  
[How Do I...?](https://github.com/WildChild83/BlastForth/blob/main/README.md#how-do-i)  
[Style Notes](https://github.com/WildChild83/BlastForth/blob/main/README.md#style-notes)  

# How to Use BlastForth

### 1) Acquire the BlastForth source code.

Since you're reading this, you've probably already done this step.

### 2) Install the Gforth compiler.

Linux users should be able to install the "gforth" package from their distro's repositories.  Mac and Windows users can dowload the appropriate installer from here:  
https://www.complang.tuwien.ac.at/forth/gforth/Snapshots/current/

Once installed, open a terminal window and enter the command `gforth`.  Once inside the Gforth environment, try typing the following phrase and pressing Enter (making sure to leave a space between each character):

`2 3 + .`

This sequence tells the interpreter to push a "2" then a "3" onto the stack, add them together, and type the result back to you.  If everything comes back "ok" then you have successfully programmed in the Forth language.  Type the word `bye` and press Enter to leave the Gforth environment.

### 3) Read the book *Starting Forth* by Leo Brodie.

Now that you have a working Forth on your computer, you need to learn how to use it!

Use Gforth to work through the code excercises in the book, which can be read online for free:  
https://www.forth.com/starting-forth/  

### 4) Install a Genesis/Megadrive emulator.

If you're ready to start writing your own software for the Sega Genesis, then I'm going to assume you know how to use an emulator.

### 5) Get a decent text editor.

You can use any text editor you want to, but it should at least have syntax highlighting for the Forth programming language.  "Gedit" is a good option, as it is lightweight, simple, and cross-platform:  
https://wiki.gnome.org/Apps/Gedit

### 6) Build a BlastForth project.

Ok then.  Here we go:

- Create a new folder for your project.  We'll call it "Project" for now.
- Copy both the "system.fs" file, and the "system" folder, from the BlastForth folder into the Project folder you just created.
- Create a new text file in your Project folder.  Call it "main.fs" or something.
- Type the following code, exactly as it appears, into your new file:

```
include system.fs
Forth definitions
    
:entry
    ." Hello, world!"
    begin again ;

romfile: test.gen
```

Note the presence of a space between `."` and `Hello`, and between `again` and `;`.  These spaces are mandatory.  Also notice there *isn't* a space within `:entry`.  This is also mandatory and will be explained shortly.

Now open a terminal in your Project folder and enter the following command:

`gforth main.fs`

This will produce the file "test.gen" in your Project folder, which can be launched and tested with any Genesis emulator.  It can also be run on real hardware with an Everdrive or ROM burner.

### 7) Run your program.

Launch the "test.gen" file in your emulator.  If using Gens, for instance:

`gens test.gen`

If the machine says "hello" to you, then you have just Blasted Forth!

### 8) How the "hello world" program works:

`include system.fs`  
This is the first line of executable code in every program you write.  It tells the interpreter to parse the "system.fs" file and initialize the BlastForth development environment.

`Forth definitions`  
A Forth system contains hundreds or thousands of words, and these words are organized into *glossaries*.  The default glossary contains the most commonly-used words and is simply called "Forth."  The phrase `Forth definitions` tells the interpreter where to find the definitions of the words it parses, and the compiler where to put the new definitions it creates.  Other glossaries also exist, such as `Assembler68k` which contains the built-in assembler, and `z80` which contains words related to the sound driver.

It is good programming practice to have each source file indicate which glossary its words belong to.  This indication must come *after* any `include` statements, because other files might change the current glossary to something else.

`:entry`  
Specifies the *entry point* of your application.  When you launch your program, BlastForth first boots the system into a useable state, then it calls `:entry` and your code is in control from there.

`." Hello, world!"`  
This phrase consists of two parts: the `."` is one parsable token, and `Hello, world!"` is the other.  In the Forth language, *all* tokens must be seperated by spaces -- whitespace is the one and only "delimiter".  As a result, quoted strings always have a single space at the beginning, and this space does *not* appear in the output.

If you remove the initial space then the interpreter will treat `."Hello,` as a single word, resulting in an "undefined word" error.

`begin again ;`  
"Begin" marks the beginning of a loop, and "again" causes an unconditional jump back to "begin."  Thus the phrase `begin again` produces an infinite loop.  You will eventually expand this into your "main program loop."  The `;` (semicolon) marks the end of whatever word is currently being defined, which in this case is `:entry`.

If `:entry` returns to its caller, a runtime exception gets thrown and the system hangs.

`romfile: test.gen`  
This statement directs BlastForth to write your compiled program to the specified disk file, free it from memory, and exit the host environment.  It is always the last statement in your code.

# The System.fs File



# How Do I...?

### How do I make an "immediate" word?

### How do I create my own Defining Words?

### How do I manipulate the Dataspace Pointers?

A standard desktop PC Forth runs exclusively from RAM, and the compiler has a *dataspace* pointer which indicates where the next cell of data will be appended to the dictionary.  The pointer increments every time a new word is compiled, and it works exactly like an assembler's "program counter" variable.

Since the Genesis is a ROM-based system we have not one but two dataspaces to keep track of.  The *ROMspace* pointer holds the current position within the (up to) 4 megabyte cartridge ROM, and governs the placement of program code and immutable data.  The *RAMspace* pointer governs the allocation of mutable data within the system's 64 kilobyte RAM.

Normally you won't access these pointers directly; Forth automatically updates the ROMspace pointer when you define new words, and  RAMspace when you declare user variables.

# Style Notes

This section explains the conventions used by the BlastForth environment.

Like most Forths, BlastForth is NOT case-sensitive.  Uppercase and lowercase letters are treated the same, so `negate` and `NEGATE` are equal to one another.

32-bit numbers are called *cells*, 16-bit numbers are *half cells*, and 8-bit numbers are called *chars* (characters).  Words that operate on half-cells or chars have an "h" or "c" prefix.  There is also limited support for 64-bit *double cell* numbers, and those words are prefixed with a "d" or a "2."

Definitions surrounded by `{ }` (curly braces) execute immediately on the host machine at compile time, instead of compiling and executing on the target machine.  `{ }` blocks are the equivalent of compile-time "macros."  The standard word "immediate" is not supported; use `{ }` instead.

"Host" machine refers to the desktop or laptop PC you write your programs with.  The "target" machine is the Sega Genesis/Megadrive.  Since BlastForth is a *cross-compiler* for a ROM-based system, it must distinguish between "compile time" and "run time" like a conventional compiler (native Forths do not typically make this distinction).

The M68000 CPU supports only 16-bit *symmetric* division.  BlastForth's standard division operators (`/`, `/mod`, etc.) use a software fallback routine if the divisor doesn't fit in 16 bits.  You can use the half-cell division operators (`h/`, `h/mod`, etc.) to force the use of hardware division, in which case an exception will be thrown if the divisor is too large.  Your project's settings determine whether the compiler uses "symmetric" or "floored" division, with symmetric being the default.

Words enclosed in parentheses `( )`, such as `(init-video-config)`, are for internal use and should not be directly used in your code.

Words that end with an `&` (ampersand) return a *code field* containing raw machine instructions.  Examples include `docolon&`, `doconst&`, and `next&`.

Words enclosed in `< >` (angle brackets) are the default implementation of a Deferred Word, such as `<emit>`.  They need to be initialized at program start, i.e. `['] <emit> is emit`.

Words defined with `value` or `2value` do not have an initial value.  Like all memory words, they must be explicitly given a value at program start.

Words that begin with a `?` (question mark) generally leave their arguments on the stack.  Words that *end* with a `?` consume their stack arguments.



