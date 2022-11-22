# BlastForth

A Forth-based game development environment for the Sega Genesis/Megadrive platform.  Currently at version 0.  This is still very much a work-in-progress.

Updates will be made to this readme file when I think of something to write and when I have time.  Ask me questions!

Forth is NOT a case-sensitive language.  Uppercase and lowercase letters are treated the same, so "function" and "FUNCTION" are synonymous.

# Dataspace

A standard desktop PC Forth runs exclusively from RAM, and the compiler has a single *dataspace* pointer which indicates where the next cell of data will be appended to the dictionary.  The pointer increments every time a new word is compiled, and it works exactly like an assembler's "program counter" variable.

Since the Genesis is a ROM-based system it's a little bit trickier: we have two dataspaces to keep track of.  The *ROMspace* variable holds the current position within the (up to) 4 megabyte cartridge ROM, and governs the placement of program code and immutable data.  The *RAMspace* variable governs the allocation of mutable data within the system's 64 kilobyte RAM.

Normally you won't access these variable directly -- Forth automatically updates the ROMspace pointer when you define new words, and the RAMspace pointer when you declare user variables.


