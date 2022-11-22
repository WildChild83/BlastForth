( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Forth Cross-Compiler for Motorola 68000 CPU                            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - romfile.fs                                                       )
(           - 68k.fs                                                           )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

{ create state   false , }
{ : compiling? state @ ; }
{ : target,    compiling? not -14 and throw  @ } , { ; }
{ : host-only  compiling? abort" Compiling a host machine word." ; }

( ---------------------------------------------------------------------------- )
(       A7=Return Stack     A6=Data Stack       A5=Thread Pointer              )
(       A0=Next Pointer                         D0=Top of Data Stack           )
( ---------------------------------------------------------------------------- )
{ :   PC    ( -- romaddr ) alignrom romsize ; }
{ :   PC:   ( -- ) PC constant ; }
{ : doprim, ( -- ) PC 4 + } , { ; }

Assembler68k wid { constant asm68k-wid }
{ :     asm        ( -- ) host-only clean alignrom asm68k-wid >order ; }
{ :    code ( "name" -- ) host-only create PC , doprim, asm does> target, ; }
{ : rawcode ( "name" -- ) host-only create PC , asm does> ( -- romaddr ) @ ; }
{ :    anon     ( -- xt ) host-only PC doprim, asm ; }

Assembler68k definitions
{ synonym end } ( -- ) }
{ : next, ( -- ) [np] jump, ; }
{ : next  ( -- ) next, end  ; }

Forth definitions
{ : host/target: ( hostxt targetxt "name" -- ) host-only create , ,
    does> ( .. -- ) compiling? if @ } , { else cell+ @ execute endif ; }

( ---------------------------------------------------------------------------- )
(       ROM Data Areas                                                         )
( ---------------------------------------------------------------------------- )
PC: <dodata&>
{ : codefield, ( romaddr -- ) PC , } , { ; }
{ : data ( "name" -- ) host-only
    create <dodata&> 4 + codefield,
    does> @ compiling? if } , { exit endif 4 + ; }

asm data dodata& ( -- addr ) tos push, a4 tos move, next

( ---------------------------------------------------------------------------- )
(       Literals                                                               )
( ---------------------------------------------------------------------------- )
{ : ?ph ( n -- n flag ) dup  0 65536 within ; }
{ : ?nh ( n -- n flag ) dup -65536 0 within ; }

code lit   ( -- n ) tos push, tos read, next
code plit  ( -- u ) tos push, tos clear, tos h read, next
code nlit  ( -- n ) tos push, -1 # tos move, [a4] tos h move, next
code 2lit  ( -- d ) tos push, -[sp] read, tos read, next
code 2plit ( -- u u )
    tos push, -[sp] h read, -[sp] h clear, tos clear, tos h read, next
code 2nlit ( -- n n )
    tos push, -[sp] h read, -1 # -[sp] h move, 
    -1 # tos move, tos h read, next

{ : literal  ( n -- )
    ?ph if } plit h, { exit endif ?nh if } nlit h, { exit endif } lit , { ; }
{ : (2lit)   ( n n -- ) } 2lit , , { ; }
{ : 2literal ( n n -- )
    ?ph if swap ?ph if } 2plit h, h, { exit endif (2lit) exit endif
    ?nh if swap ?nh if } 2nlit h, h, { exit endif (2lit) exit endif
    swap (2lit) ; }

( ---------------------------------------------------------------------------- )
(       Colon Definitions                                                      )
( ---------------------------------------------------------------------------- )
{ 2variable ]name
: (refill) refill 0= abort" Unexpected end of input." ;
: ]number ]name 2@ s>number? not abort" Unrecognized word." d>s } literal { ;
: ]find   ]name 2@ find-name ?dup if name>interpret execute exit endif ]number ;
: ]parse   parse-name ?dup if ]name 2! ]find exit endif drop (refill) ;
: ] ( -- ) host-only  state on   begin   ]parse   state @ not until ;
: [ ( -- ) state off ;
}
code exit ( -- ) tp rpop, next

Assembler68k definitions
{ : next: ( -- ) tp rpush, } $41FA h, 4 h, { next ] ; }

Forth definitions
asm { : semantics> ( -- addr ) tos push, a4 tos move, next: ; } end

asm data docolon& ( -- ) tp rpush, a4 tp move, next
{ : ; ( -- ) } exit state { off ; }
{ : : ( "name" -- ) host-only
    create docolon& codefield, } ] { does> ( -- ) target, ; }

( ---------------------------------------------------------------------------- )
(       Constants                                                              )
( ---------------------------------------------------------------------------- )
{ : h,rom, ( n -- ) $FFFF and dup , } h, { ; }

asm data doconstp& ( -- u ) tos push, tos clear, [a4] tos h move, next
{ : pconstant ( n "name" -- ) host-only
    create doconstp& codefield,  h,rom,
    does> ( -- n ) compiling? if @ } , { exit endif cell+ @ ; }

asm data doconstn& ( -- n ) tos push, -1 # tos move, [a4] tos h move, next
{ : nconstant ( n "name" -- ) host-only
    create doconstn& codefield,  h,rom,
    does> ( -- n ) compiling? if @ } , { exit endif cell+ @ -65536 or ; }

asm data doconst& ( -- n ) tos push, [a4] tos move, next
{ : constant ( n "name" -- ) host-only
    ?ph if pconstant exit endif   ?nh if nconstant exit endif
    create doconst& codefield, dup , } , {
    does>  compiling? if @ } , { exit endif cell+ @ ; }

asm data do2constp& ( -- d )
    tos push, tos clear, [a4]+ tos h move, 
    [a4]+ -[sp] h move, -[sp] h clear, next
{ : 2pconstant ( d "name" -- ) host-only
    create do2constp& codefield,  h,rom,  h,rom,
    does>  compiling? if @ } , { exit endif cell+ dup @ swap cell+ @ ; }

asm data do2constn& ( -- d )
    tos push, -1 # tos move, [a4]+ tos h move, 
    [a4]+ -[sp] h move, -1 # -[sp] h move, next
{ : 2nconstant ( d "name" -- ) host-only
    create do2constn& codefield,  h,rom,  h,rom,
    does>  compiling? if @ } , { exit endif
           cell+ dup @ -65536 or swap cell+ @ -65536 or ; }

asm data do2const& ( -- d ) tos push, [a4]+ push, [a4] tos move, next
{ : (2const) ( d "name" -- )
    create do2const& codefield, dup , } , { dup , } , {
    does>  compiling? if @ } , { exit endif cell+ dup @ swap cell+ @ ; }
{ : 2constant ( d "name" -- ) host-only
    swap ?ph if swap ?ph if 2pconstant exit endif (2const) exit endif
         ?nh if swap ?nh if 2nconstant exit endif (2const) exit endif
    swap (2const) ; }

( ---------------------------------------------------------------------------- )
(       Memory Buffers                                                         )
( ---------------------------------------------------------------------------- )
{ create ramspace $FF0000 , }
{ : +ramspace> ( n1 -- u1 ) ramspace @ tuck + ramspace ! ; }
{ : alignram ( -- ) ramspace @ 1+ -2 and ramspace ! ; }
{ : <memory>  @ compiling? if } , { exit endif 4 + romh@ $FF0000 + ; }

asm data domem& ( -- addr ) tos push, $FF0000 # tos move, [a4] tos h move, next
{ : buffer: ( u "name" -- ) host-only 
    create domem& codefield, +ramspace> } h, { does> <memory> ; }

{ :  variable ( "name" -- ) host-only alignram 4 buffer: ; }
{ : hvariable ( "name" -- ) host-only alignram 2 buffer: ; }
{ : cvariable ( "name" -- ) host-only          1 buffer: ; }
{ : 2variable ( "name" -- ) host-only alignram 8 buffer: ; }

( ---------------------------------------------------------------------------- )
(       Values                                                                 )
( ---------------------------------------------------------------------------- )
{ :  to ( "name" -- )
    compiling? not -14 and throw
    ' >body dup cell+ @ } , {   @ 4 + romh@ } h, { ; }
{ : +to ( n "name" -- )
    compiling? not -14 and throw
    ' >body dup 2 cells + @ } , {   @ 4 + romh@ } h, { ; }

( ---------------------------------------------------------------------------- )
asm data dovalue& ( -- n )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add, [a1] tos move, next
asm data tovalue& ( n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] move, tos pop, next
asm data +tovalue& ( n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] add, tos pop, next
{ : value ( "name" -- ) host-only 
    create dovalue& codefield, tovalue& , +tovalue& ,
    alignram 4 +ramspace> } h, { does> <memory> ; }

asm data dohvalue& ( -- h )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add,
    tos clear, [a1] tos h move, next
asm data tohvalue& ( h -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] h move, tos pop, next
asm data +tohvalue& ( h -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] h add, tos pop, next
{ : hvalue ( "name" -- ) host-only 
    create dohvalue& codefield, tohvalue& , +tohvalue& ,
    alignram 2 +ramspace> } h, { does> <memory> ; }

asm data docvalue& ( -- c )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add,
    tos clear, [a1] tos c move, next
asm data tocvalue& ( c -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] c move, tos pop, next
asm data +tocvalue& ( c -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] c add, tos pop, next
{ : cvalue ( "name" -- ) host-only 
    create docvalue& codefield, tocvalue& , +tocvalue& ,
    1 +ramspace> } h, { does> <memory> ; }

asm data do2value& ( -- n n )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add,
    tos clear, [a1]+ tos move, [a1]+ push, next
asm data to2value& ( n n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add,
    tos [a1]+ move, [sp]+ [a1] move, tos pop, next
asm data +to2value& ( n n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, [a1]+ d2 move, [a1] d1 move,
    [sp]+ d1 add, tos d2 addx, d1 [a1] move, d2 -[a1] move, tos pop, next
{ : 2value ( "name" -- ) host-only 
    create do2value& codefield, to2value& , +to2value& ,
    alignram 8 +ramspace> } h, { does> <memory> ; }

( ---------------------------------------------------------------------------- )
(       Deferred Words                                                         )
( ---------------------------------------------------------------------------- )
asm data dodefer& ( -- )
    $FF0000 # d1 move, [a4] d1 h move, d1 a4 move, [a4]+ a3 move, [a3] jump, end
{ : defer ( "name" -- ) host-only 
    create dodefer& codefield, 4 +ramspace> } h, { does> <memory> ; }

code defer@ ( xt1 -- xt2 )
    tos a1 move, 4 # a1 add, $FF0000 [#] a2 lea,
    [a1] a2 h add, [a2] tos move, next
{ : action-of ( "name" -- xt )
    compiling? not -14 and throw
    parse-name  find-name  name>interpret >body @ } literal defer@ { ; }

code defer! ( xt1 xt2 -- )
    tos a1 move, 4 # a1 add, $FF0000 [#] a2 lea,
    [a1] a2 h add, [a2] pop, tos pop, next
{ : is ( xt "name" -- )
    compiling? not -14 and throw
    parse-name  find-name  name>interpret >body @ } literal defer! { ; }

( ---------------------------------------------------------------------------- )
(       Miscellaneous other stuff                                              )
( ---------------------------------------------------------------------------- )

asm data next& ( -- ) a4 read, [a4]+ a3 move, [a3] jump, end

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

















