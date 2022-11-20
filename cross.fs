( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Forth Cross-Compiler for Motorola 68000 CPU                            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - romfile.fs                                                       )
(           - 68k.fs                                                           )
(                                                                              )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(       A7=Return Stack     A6=Data Stack       A5=Thread Pointer              )
(       A0=Next Pointer                         D0=Top of Data Stack           )
( ---------------------------------------------------------------------------- )
:   PC    ( -- romaddr ) alignrom romsize ;
:   PC:   ( -- ) PC constant ;
: doprim, ( -- ) PC 4 + rom, ;

Assembler68k    
order> constant asm68k-wordlist
:    asm         ( -- ) clean alignrom asm68k-wordlist >order ;
:    code ( "name" -- ) create PC , doprim, asm does> ( -- ) @ rom, ;
: rawcode ( "name" -- ) create PC , asm does> ( -- romaddr ) @ ;

definitions
synonym end previous ( -- )
: next, ( -- ) [np] jump, ;
: next  ( -- ) next, end  ;

end definitions

create state false ,
: compiling? ( -- flag ) state @ ;
: compile? ( n -- n|  )  compiling? if rom, endif ;
: hostcode ( addr -- addr'| ) compiling? if rom, exit endif 4 + ;

( ---------------------------------------------------------------------------- )
(       Code Fields                                                            )
( ---------------------------------------------------------------------------- )
PC: <docreate&>
: create ( "name" -- ) create PC , <docreate&> 4 + rom, does> @ hostcode ;

create docreate& asm ( -- addr ) tos push, a4 tos move, next
create docolon&  asm ( -- )      tp rpush, a4 tp move, next
create domem&    asm ( -- addr )
    tos push, $FF0000 # tos move, [a4] tos h move, next
create dodefer&  asm ( -- )
    $FF0000 # d1 move, [a4] d1 h move, d1 a4 move, [a4]+ a3 move, [a3] jump, end

create doconst&   asm ( -- n ) tos push, [a4] tos move, next
create do2const&  asm ( -- d ) tos push, [a4]+ tos move, [a4] push, next
create doconstp&  asm ( -- u ) tos push, tos clear, [a4] tos h move, next
create doconstn&  asm ( -- n ) tos push, -1 # tos move, [a4] tos h move, next
create do2constp& asm ( -- u u )
    tos push, tos clear, [a4]+ tos h move,
    [a4]+ -[sp] h move, -[sp] h clear, next
create do2constn& asm ( -- n n )
    tos push, -1 # tos move, [a4]+ tos h move,
    [a4]+ -[sp] h move, -1 # -[sp] h move, next

: codefield: ( romaddr -- ) { create } , does> ( -- ) @ alignrom rom, ;
    docreate& codefield: docreate,          docolon& codefield: docolon,
       domem& codefield: domem,             dodefer& codefield: dodefer,
     doconst& codefield: doconst,          do2const& codefield: do2const,
    doconstp& codefield: doconstp,         doconstn& codefield: doconstn,
   do2constp& codefield: do2constp,       do2constn& codefield: do2constn,

( ---------------------------------------------------------------------------- )
(       Literals                                                               )
( ---------------------------------------------------------------------------- )
code lit,   ( -- n ) tos push, tos read, next
code plit,  ( -- u ) tos push, tos clear, tos h read, next
code nlit,  ( -- n ) tos push, -1 # tos move, [a4] tos h move, next
code 2lit,  ( -- d ) tos push, -[sp] read, tos read, next
code 2plit, ( -- u u )
    tos push, -[sp] h read, -[sp] h clear, tos clear, tos h read, next
code 2nlit, ( -- n n )
    tos push, -[sp] h read, -1 # -[sp] h move, 
    -1 # tos move, tos h read, next

: ?plit ( n -- n flag ) dup  0 65536 within ;
: ?nlit ( n -- n flag ) dup -65536 0 within ;
: literal ( n -- )
    ?plit if plit, romh, exit endif ?nlit if nlit, romh, exit endif lit, rom, ;
: (2lit) ( n n -- ) 2lit, rom, rom, ;
: 2literal ( n n -- )
    ?plit if swap ?plit if 2plit, romh, romh, exit endif (2lit) exit endif
    ?nlit if swap ?nlit if 2nlit, romh, romh, exit endif (2lit) exit endif
    swap (2lit) ;

( ---------------------------------------------------------------------------- )
(       Colon Definitions                                                      )
( ---------------------------------------------------------------------------- )
2variable ]name
: ]number ( -- ) ]name 2@ s>number? not abort" Unrecognized word." d>s literal ;
: ]find  ( -- ) ]name 2@ find-name ?dup if name>int execute exit endif ]number ;
: ]refill ( -- ) refill not abort" Unexpected end of input." ;
: ]parse ( -- ) parse-name ?dup if ]name 2! ]find exit endif drop ]refill ;

: ] ( -- ) state on   begin   ]parse   state @ not until ;
: [ ( -- ) state off ;

code exit ( -- ) tp rpop, next

: ; ( -- ) exit state off ;
: : ( "name" -- ) { create } PC , docolon, ] does> ( -- ) @ rom, { ; }

( ---------------------------------------------------------------------------- )
create next& asm ( -- ) a4 read, [a4]+ a3 move, [a3] jump, end

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )


















