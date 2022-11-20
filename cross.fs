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
:  hostcode ( addr -- addr'| ) compiling? if   rom, exit endif 4 + ;
: @hostcode ( addr -- addr'| ) compiling? if @ rom, exit endif cell+ @ ;

( ---------------------------------------------------------------------------- )
(       ROM Data Areas                                                         )
( ---------------------------------------------------------------------------- )
PC: <dodata&>
: codefield, ( romaddr -- ) PC , rom, ;
: data ( "name" -- ) create <dodata&> 4 + codefield, does> @ hostcode ;
asm data dodata&  ( -- addr ) tos push, a4 tos move, next



( ---------------------------------------------------------------------------- )


asm data dodefer& ( -- )
    $FF0000 # d1 move, [a4] d1 h move, d1 a4 move, [a4]+ a3 move, [a3] jump, end

( ---------------------------------------------------------------------------- )
(       Constants                                                              )
( ---------------------------------------------------------------------------- )
: ?ph ( n -- n flag ) dup  0 65536 within ;
: ?nh ( n -- n flag ) dup -65536 0 within ;
: h,rom, ( n -- ) $FFFF and dup , romh, ;

asm data doconstp& ( -- u ) tos push, tos clear, [a4] tos h move, next
: pconstant ( n "name" -- )
    create doconstp& codefield,  h,rom,
    does> ( -- n ) compiling? if @ rom, exit endif cell+ @ ;

asm data doconstn& ( -- n ) tos push, -1 # tos move, [a4] tos h move, next
: nconstant ( n "name" -- )
    create doconstn& codefield,  h,rom,
    does> ( -- n ) compiling? if @ rom, exit endif cell+ @ -65536 or ;

asm data doconst& ( -- n ) tos push, [a4] tos move, next
: constant ( n "name" -- )
    ?ph if pconstant exit endif   ?nh if nconstant exit endif
    create doconst& codefield, dup , rom,
    does> ( -- n ) compiling? if @ rom, exit endif cell+ @ ;

asm data do2constp& ( -- u u )
    tos push, tos clear, [a4]+ tos h move, 
    [a4]+ -[sp] h move, -[sp] h clear, next
: 2pconstant ( n n "name" -- )
    create do2constp& codefield,  h,rom,  h,rom,
    does> ( -- n n ) compiling? if @ rom, exit endif cell+ dup @ swap cell+ @ ;

asm data do2constn& ( -- n n )
    tos push, -1 # tos move, [a4]+ tos h move, 
    [a4]+ -[sp] h move, -1 # -[sp] h move, next
: 2nconstant ( n n "name" -- )
    create do2constn& codefield,  h,rom,  h,rom,
    does> ( -- n n ) compiling? if @ rom, exit endif
                     cell+ dup @ -65536 or swap cell+ @ -65536 or ;

asm data do2const& ( -- d ) tos push, [a4]+ push, [a4] tos move, next
: (2const) ( n n "name" -- )
    create do2const& codefield, dup , rom, dup , rom,
    does> ( -- n n ) compiling? if @ rom, exit endif cell+ dup @ swap cell+ @ ;
: 2constant ( n n "name" -- )
    swap ?ph if swap ?ph if 2pconstant exit endif (2const) exit endif
         ?nh if swap ?nh if 2nconstant exit endif (2const) exit endif
    swap (2const) ;

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

: literal ( n -- )
    ?ph if plit, romh, exit endif ?nh if nlit, romh, exit endif lit, rom, ;
: (2lit) ( n n -- ) 2lit, rom, rom, ;
: 2literal ( n n -- )
    ?ph if swap ?ph if 2plit, romh, romh, exit endif (2lit) exit endif
    ?nh if swap ?nh if 2nlit, romh, romh, exit endif (2lit) exit endif
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

asm definitions
: next: ( -- ) tp rpush, $41FA asm, 4 asm, next ] ;
end definitions
asm : semantics> ( -- addr ) tos push, a4 tos move, next: ;   end

asm data docolon& ( -- ) tp rpush, a4 tp move, next
: ; ( -- ) exit state off ;
: : ( "name" -- ) create docolon& codefield, ] does> ( -- ) @ rom, { ; }

( ---------------------------------------------------------------------------- )
asm data next& ( -- ) a4 read, [a4]+ a3 move, [a3] jump, end

( ---------------------------------------------------------------------------- )
(       Memory Spaces                                                          )
( ---------------------------------------------------------------------------- )
{
create ramspace $FF0000 ,
: +ramspace> ( n1 -- u1 ) ramspace @ tuck + ramspace ! ;
: alignram ( -- ) ramspace @ 1+ -2 and ramspace ! ;

asm data domem& ( -- addr ) tos push, $FF0000 # tos move, [a4] tos h move, next
: buffer: ( u "name" -- )
    create domem& codefield, +ramspace> romh,
    does> @ compiling? if rom, exit endif 4 + romh@ $FF0000 + ;

:  variable ( "name" -- ) 4 buffer: ;
: hvariable ( "name" -- ) 2 buffer: ;
: cvariable ( "name" -- ) 1 buffer: ;
: 2variable ( "name" -- ) 8 buffer: ;
}

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


















