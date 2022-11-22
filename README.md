# BlastForth

A Forth-based game development environment for the Sega Genesis/Megadrive platform.  Currently at version 0.  This is still very much a work-in-progress.  Updates will be made to this Readme file when I have time and when I think of something to write down.  Ask me questions!

Forth is a very powerful programming language/paradigm that you've probably never heard of.  The best way to learn it is to read the book *Starting Forth* by Leo Brodie.  First published in 1981, it remains today the standard tutorial for the Forth programming language.  There is a PDF copy of this book in the BlastForth folder, or you can read it online here:
https://www.forth.com/starting-forth/

You will need the Gforth compiler in order to build Genesis/Megadrive software with BlastForth.  Gforth is available for all major desktop platforms (Linux, Mac, Windows) and can be downloaded from here:
https://gforth.org/

Linux users can also get Gforth from their distro's repositories, as it is a standard component of the GNU operating system.

See the file *Word-Reference.md* in this folder for a list of all the words provided by BlasForth.

# Style Notes

This section provides some details on the conventions used by this environment.

BlastForth, like most Forths, is NOT case-sensitive.  Uppercase and lowercase letters are treated the same, so "function" and "FUNCTION" are equal to one another.

As a general rule (with some exceptions), words that begin with a **?** (question mark) leave their arguments on the stack.  Words that *end* with a **?** consume their stack arguments.

# Dataspace

A standard desktop PC Forth runs exclusively from RAM, and the compiler has a *dataspace* pointer which indicates where the next cell of data will be appended to the dictionary.  The pointer increments every time a new word is compiled, and it works exactly like an assembler's "program counter" variable.

Since the Genesis is a ROM-based system we have not one but two dataspaces to keep track of.  The *ROMspace* variable holds the current position within the (up to) 4 megabyte cartridge ROM, and governs the placement of program code and immutable data.  The *RAMspace* variable governs the allocation of mutable data within the system's 64 kilobyte RAM.

Normally you won't access these variable directly -- Forth automatically updates the ROMspace pointer when you define new words, and the RAMspace pointer when you declare user variables.


