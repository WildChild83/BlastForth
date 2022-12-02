# BlastForth

A Forth-based software development kit for the Sega Genesis/Megadrive platform.  Currently at version 0.  This is still a work-in-progress.  Updates will be made to this Readme file when I have time and when I think of something to write down.  Ask me questions!

Forth is a very powerful programming language/paradigm that you've probably never heard of.  The best way to learn it is to read the book *Starting Forth* by Leo Brodie.  First published in 1981, it remains today the best tutorial there is for the Forth programming language.  The BlastForth repository contains a PDF copy, or you can read it online here:  
https://www.forth.com/starting-forth/

You will need the Gforth compiler to build Genesis/Megadrive software with BlastForth.  Gforth is available for all major desktop platforms (Linux, Mac, Windows) and can be downloaded from here:  
https://gforth.org/

Linux users can also get Gforth from their distro's repositories, as it is a standard component of the GNU operating system.

See the [wiki section](https://github.com/WildChild83/BlastForth/wiki) to learn how to get started.

See the [Word-Reference.md](https://github.com/WildChild83/BlastForth/blob/main/Word-Reference.md) file for a list of all the words provided by BlastForth.

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



