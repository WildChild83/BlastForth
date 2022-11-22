( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Forth Cross-Compiler for Motorola 68000 CPU                            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossaries.fs                                                    )
(           - romfile.fs                                                       )
(           - 68k.fs                                                           )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

{ create state   false , }
{ : compiling? state @ ; }
{ : target, ( romaddr -- ) compiling? not -14 and throw  @ rom, ; }

( ---------------------------------------------------------------------------- )
(       A7=Return Stack     A6=Data Stack       A5=Thread Pointer              )
(       A0=Next Pointer                         D0=Top of Data Stack           )
( ---------------------------------------------------------------------------- )
{ :   PC    ( -- romaddr ) alignrom romsize ; }
{ :   PC:   ( -- ) PC constant ; }
{ : doprim, ( -- ) PC 4 + rom, ; }

Assembler68k wid { constant asm68k-wid }
{ :     asm        ( -- ) clean alignrom asm68k-wid >order ; }
{ :    code ( "name" -- ) create PC , doprim, asm does> ( -- ) target, ; }
{ : rawcode ( "name" -- ) create PC , asm does> ( -- romaddr ) @ ; }
{ :    anon     ( -- xt ) PC doprim, asm ; }

Assembler68k definitions
{ synonym end } ( -- ) }
{ : next, ( -- ) [np] jump, ; }
{ : next  ( -- ) next, end  ; }

Forth definitions
{ : host/target: ( hostxt targetxt "name" -- ) create , ,
    does> ( .. -- ) compiling? if @ rom, else cell+ @ execute endif ; }

( ---------------------------------------------------------------------------- )
(       ROM Data Areas                                                         )
( ---------------------------------------------------------------------------- )
PC: <dodata&>
{ : codefield, ( romaddr -- ) PC , rom, ; }
{ : data ( "name" -- )
    create <dodata&> 4 + codefield,
    does> @ compiling? if rom, exit endif 4 + ; }

asm data dodata& ( -- addr ) tos push, a4 tos move, next

( ---------------------------------------------------------------------------- )
(       Literals                                                               )
( ---------------------------------------------------------------------------- )
{ : ?ph ( n -- n flag ) dup  0 65536 within ; }
{ : ?nh ( n -- n flag ) dup -65536 0 within ; }

code lit,   ( -- n ) tos push, tos read, next
code plit,  ( -- u ) tos push, tos clear, tos h read, next
code nlit,  ( -- n ) tos push, -1 # tos move, [a4] tos h move, next
code 2lit,  ( -- d ) tos push, -[sp] read, tos read, next
code 2plit, ( -- u u )
    tos push, -[sp] h read, -[sp] h clear, tos clear, tos h read, next
code 2nlit, ( -- n n )
    tos push, -[sp] h read, -1 # -[sp] h move, 
    -1 # tos move, tos h read, next

{ : literal ( n -- )
    ?ph if plit, romh, exit endif ?nh if nlit, romh, exit endif lit, rom, ; }
{ : (2lit) ( n n -- ) 2lit, rom, rom, ; }
{ : 2literal ( n n -- )
    ?ph if swap ?ph if 2plit, romh, romh, exit endif (2lit) exit endif
    ?nh if swap ?nh if 2nlit, romh, romh, exit endif (2lit) exit endif
    swap (2lit) ; }

( ---------------------------------------------------------------------------- )
(       Colon Definitions                                                      )
( ---------------------------------------------------------------------------- )
{ 2variable ]name
: (refill) refill 0= abort" Unexpected end of input." ;
: ]number ]name 2@ s>number? not abort" Unrecognized word." d>s } literal { ;
: ]find   ]name 2@ find-name ?dup if name>interpret execute exit endif ]number ;
: ]parse   parse-name ?dup if ]name 2! ]find exit endif drop (refill) ;
: ] ( -- ) state on   begin   ]parse   state @ not until ;
: [ ( -- ) state off ;
}
code exit ( -- ) tp rpop, next

Assembler68k definitions
{ : next: ( -- ) tp rpush, $41FA romh, 4 romh, next ] ; }

Forth definitions
asm { : semantics> ( -- addr ) tos push, a4 tos move, next: ; } end

asm data docolon& ( -- ) tp rpush, a4 tp move, next
{ : ; ( -- ) } exit state { off ; }
{ : : ( "name" -- ) create docolon& codefield, } ] { does> ( -- ) target, ; }

( ---------------------------------------------------------------------------- )
(       Constants                                                              )
( ---------------------------------------------------------------------------- )
{ : h,rom, ( n -- ) $FFFF and dup , romh, ; }

asm data doconstp& ( -- u ) tos push, tos clear, [a4] tos h move, next
{ : pconstant ( n "name" -- )
    create doconstp& codefield,  h,rom,
    does> ( -- n ) compiling? if @ rom, exit endif cell+ @ ; }

asm data doconstn& ( -- n ) tos push, -1 # tos move, [a4] tos h move, next
{ : nconstant ( n "name" -- )
    create doconstn& codefield,  h,rom,
    does> ( -- n ) compiling? if @ rom, exit endif cell+ @ -65536 or ; }

asm data doconst& ( -- n ) tos push, [a4] tos move, next
{ : constant ( n "name" -- )
    ?ph if pconstant exit endif   ?nh if nconstant exit endif
    create doconst& codefield, dup , rom,
    does> ( -- n ) compiling? if @ rom, exit endif cell+ @ ; }

asm data do2constp& ( -- d )
    tos push, tos clear, [a4]+ tos h move, 
    [a4]+ -[sp] h move, -[sp] h clear, next
{ : 2pconstant ( d "name" -- )
    create do2constp& codefield,  h,rom,  h,rom,
    does> ( -- d ) compiling? if @ rom, exit endif cell+ dup @ swap cell+ @ ; }

asm data do2constn& ( -- d )
    tos push, -1 # tos move, [a4]+ tos h move, 
    [a4]+ -[sp] h move, -1 # -[sp] h move, next
{ : 2nconstant ( d "name" -- )
    create do2constn& codefield,  h,rom,  h,rom,
    does> ( -- d ) compiling? if @ rom, exit endif
                   cell+ dup @ -65536 or swap cell+ @ -65536 or ; }

asm data do2const& ( -- d ) tos push, [a4]+ push, [a4] tos move, next
{ : (2const) ( d "name" -- )
    create do2const& codefield, dup , rom, dup , rom,
    does> ( -- d ) compiling? if @ rom, exit endif cell+ dup @ swap cell+ @ ; }
{ : 2constant ( d "name" -- )
    swap ?ph if swap ?ph if 2pconstant exit endif (2const) exit endif
         ?nh if swap ?nh if 2nconstant exit endif (2const) exit endif
    swap (2const) ; }

( ---------------------------------------------------------------------------- )
(       Memory Buffers                                                         )
( ---------------------------------------------------------------------------- )
{ create ramspace $FF0000 , }
{ : +ramspace> ( n1 -- u1 ) ramspace @ tuck + ramspace ! ; }
{ : alignram ( -- ) ramspace @ 1+ -2 and ramspace ! ; }
{ : <memory>  @ compiling? if rom, exit endif 4 + romh@ $FF0000 + ; }

asm data domem& ( -- addr ) tos push, $FF0000 # tos move, [a4] tos h move, next
{ : buffer: ( u "name" -- )
    create domem& codefield, +ramspace> romh, does> <memory> ; }

{ :  variable ( "name" -- ) alignram 4 buffer: ; }
{ : hvariable ( "name" -- ) alignram 2 buffer: ; }
{ : cvariable ( "name" -- )          1 buffer: ; }
{ : 2variable ( "name" -- ) alignram 8 buffer: ; }

( ---------------------------------------------------------------------------- )
(       Values                                                                 )
( ---------------------------------------------------------------------------- )
{ :  to ( "name" -- )
    compiling? not -14 and throw
    ' >body dup cell+ @ rom,   @ 4 + romh@ romh, ; }
{ : +to ( n "name" -- )
    compiling? not -14 and throw
    ' >body dup 2 cells + @ rom,   @ 4 + romh@ romh, ; }

( ---------------------------------------------------------------------------- )
asm data dovalue& ( -- n )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add, [a1] tos move, next
asm data tovalue& ( n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] move, tos pop, next
asm data +tovalue& ( n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] add, tos pop, next
{ : value ( "name" -- )
    create dovalue& codefield, tovalue& , +tovalue& ,
    alignram 4 +ramspace> romh, does> <memory> ; }

asm data dohvalue& ( -- h )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add,
    tos clear, [a1] tos h move, next
asm data tohvalue& ( h -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] h move, tos pop, next
asm data +tohvalue& ( h -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] h add, tos pop, next
{ : hvalue ( "name" -- )
    create dohvalue& codefield, tohvalue& , +tohvalue& ,
    alignram 2 +ramspace> romh, does> <memory> ; }

asm data docvalue& ( -- c )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add,
    tos clear, [a1] tos c move, next
asm data tocvalue& ( c -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] c move, tos pop, next
asm data +tocvalue& ( c -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, tos [a1] c add, tos pop, next
{ : cvalue ( "name" -- )
    create docvalue& codefield, tocvalue& , +tocvalue& ,
    1 +ramspace> romh, does> <memory> ; }

asm data do2value& ( -- n n )
    tos push, $FF0000 [#] a1 lea, [a4] a1 h add,
    tos clear, [a1]+ tos move, [a1]+ push, next
asm data to2value& ( n n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add,
    tos [a1]+ move, [sp]+ [a1] move, tos pop, next
asm data +to2value& ( n n -- )
    $FF0000 [#] a1 lea, [tp]+ a1 h add, [a1]+ d2 move, [a1] d1 move,
    [sp]+ d1 add, tos d2 addx, d1 [a1] move, d2 -[a1] move, tos pop, next
{ : 2value ( "name" -- )
    create do2value& codefield, to2value& , +to2value& ,
    alignram 8 +ramspace> romh, does> <memory> ; }

( ---------------------------------------------------------------------------- )
(       Deferred Words                                                         )
( ---------------------------------------------------------------------------- )
asm data dodefer& ( -- )
    $FF0000 # d1 move, [a4] d1 h move, d1 a4 move, [a4]+ a3 move, [a3] jump, end
{ : defer ( "name" -- )
    create dodefer& codefield, 4 +ramspace> romh, does> <memory> ; }

code defer@ ( xt1 -- xt2 )
    tos a1 move, 4 # a1 add, $FF0000 [#] a2 lea,
    [a1] a2 h add, [a2] tos move, next
{ : action-of ( "name" -- xt )
    parse-name  find-name  name>interpret >body @ } literal defer@ { ; }

code defer! ( xt1 xt2 -- )
    tos a1 move, 4 # a1 add, $FF0000 [#] a2 lea,
    [a1] a2 h add, [a2] pop, tos pop, next
{ : is ( xt "name" -- )
    parse-name  find-name  name>interpret >body @ } literal defer! { ; }

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Core Primitives                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(       State-Smart Words                                                      )
( ---------------------------------------------------------------------------- )
-1 constant true
 0 constant false
 4 constant cell
 2 constant half

{ '  + }    anon          [sp]+ tos add, next   host/target:  +
{ '  - }    anon tos neg, [sp]+ tos add, next   host/target:  -
{ ' 1+ }               anon     tos inc, next   host/target: 1+     aka char+
{ ' 1- }               anon     tos dec, next   host/target: 1-     aka char-
{ ' 2* }               anon 1 # tos lsl, next   host/target: 2*     aka halves
{ ' 2/ }               anon 1 # tos asr, next   host/target: 2/     aka half/
{ :noname 2* 2*    ; } anon 2 # tos lsl, next   host/target: 4*     aka cells
{ :noname 2/ 2/    ; } anon 2 # tos asr, next   host/target: 4/     aka cell/
{ :noname 2* 2* 2* ; } anon 3 # tos lsl, next   host/target: 8*
{ :noname 2/ 2/ 2/ ; } anon 3 # tos asr, next   host/target: 8/
{ :noname 2 + ; }      anon 2 # tos add, next   host/target: 2+     aka half+
{ :noname 2 - ; }      anon 2 # tos sub, next   host/target: 2-     aka half-
{ :noname 4 + ; }      anon 4 # tos add, next   host/target: 4+     aka cell+
{ :noname 4 - ; }      anon 4 # tos sub, next   host/target: 4-     aka cell-
{ :noname 8 + ; }      anon 8 # tos add, next   host/target: 8+
{ :noname 8 - ; }      anon 8 # tos sub, next   host/target: 8-

{ '  abs }      anon tos test, neg if tos neg, endif next   host/target: abs
{ ' lshift }    anon d1 pop, tos d1 lsl, d1 tos move, next  host/target: lshift
{ ' rshift }    anon d1 pop, tos d1 lsr, d1 tos move, next  host/target: rshift
{ ' invert }    anon tos not, next                          host/target: invert
{ ' negate }    anon tos neg, next                          host/target: negate
{ ' and }       anon [sp]+ tos and, next                    host/target: and
{ '  or }       anon [sp]+ tos  or, next                    host/target:  or
{ ' xor }       anon [sp]+ tos xor, next                    host/target: xor

{ ' swap }      anon d1 pop, tos push, d1 tos move, next    host/target: swap
{ ' drop }      anon tos pop, next                          host/target: drop
{ ' dup  }      anon tos push, next                         host/target: dup
{ ' over }      anon d1 peek, tos push, d1 tos move, next   host/target: over
{ ' nip  }      anon cell # sp add, next                    host/target: nip
{ ' tuck }      anon d1 pop, tos push, d1 push, next        host/target: tuck

{ '  rot }      anon d1 pop, d2 pop, d1 push,
                    tos push, d2 tos move, next     host/target:  rot
{ ' -rot }      anon d1 pop, d2 pop, tos push, 
                     d2 push, d1 tos move, next     host/target: -rot
{ ' umin }      anon d1 pop, d1 tos comp,
                    ugt if d1 tos move, endif next  host/target: umin
{ ' umax }      anon d1 pop, d1 tos comp,
                    ult if d1 tos move, endif next  host/target: umax
{ '  min }      anon d1 pop, d1 tos comp, 
                     gt if d1 tos move, endif next  host/target: min
{ '  max }      anon d1 pop, d1 tos comp, 
                     lt if d1 tos move, endif next  host/target: max
{ ' 0= }        anon tos test, tos z= set?,
                    tos w ext, tos ext, next        host/target:  0=  aka not
{ ' 0<> }       anon tos test, tos z<> set?,
                    tos w ext, tos ext, next        host/target:  0<> aka flag

{ :noname ; }   anon next   host/target: noop       aka chars

( ---------------------------------------------------------------------------- )
(       Dumb Words  [aka "compile-only"]                                       )
( ---------------------------------------------------------------------------- )
code   @  tos a1 move, [a1] tos move, next
code  h@  tos a1 move, tos clear, [a1] tos h move, next
code  c@  tos a1 move, tos clear, [a1] tos c move, next
code sh@  tos a1 move, [a1] tos h move, tos ext, next
code sc@  tos a1 move, [a1] tos c move, tos h ext, tos ext, next
code   !  tos a1 move, [a1] pop, tos pop, next
code  h!  tos a1 move, 2 # a1 add, [a1] h pop, tos pop, next
code  c!  tos a1 move, 3 # a1 add, [a1] c pop, tos pop, next
code  2@  tos a1 move, [a1]+ tos move, [a1] push, next
code  2!  tos a1 move, [a1]+ pop, [a1]+ pop, tos pop, next

code 2drop  cell # sp add, tos pop, next
code 2dup   d1 peek, tos push, d1 push, next
code 2over  tos push, [sp 2 cells +] tos move, [sp 3 cells +] push, next
code 2nip   d1 pop, 2 cells # sp add, d1 push, next
code 2tuck  [sp]+ [[ d1 d2 d3 ]] movem, d1 push, tos push,
            [[ d1 d2 d3 ]] -[sp] movem, next
code 2swap  d1 pop, d2 pop, d3 pop, d1 push,
            tos push, d3 push, d2 tos move, next

code dinvert    [sp] not, tos not,  next
code dnegate    [sp] neg, tos negx, next
code dabs       tos test, neg if [sp] neg, tos negx, endif next
code d2*        [sp] lsl, 1 # tos roxl, next
code d2/        1 # tos asr, [sp] roxr, next
code d+         d1 pop, d2 pop, [sp]+ d1 add, d2 tos addx, d1 push, next
code d-         d1 pop, d2 pop, d1 neg, tos neg,
                [sp]+ d1 add, d2 tos addx, d1 push, next

code arshift    d1 pop, tos d1   asr, d1 tos move, next
code lrotate    d1 pop, tos d1   rol, d1 tos move, next
code rrotate    d1 pop, tos d1   ror, d1 tos move, next
code lhrotate   d1 pop, tos d1 h rol, d1 tos move, next
code rhrotate   d1 pop, tos d1 h ror, d1 tos move, next
code lcrotate   d1 pop, tos d1 c rol, d1 tos move, next
code rcrotate   d1 pop, tos d1 c ror, d1 tos move, next

code dlshift
    31 # tos comp, gt if 32 # tos sub, 4 # sp add, d1 peek,
        tos d1 lsl, [sp] clear, d1 tos move, next, endif
    d1 pop, d2 pop, -1 # d3 move, tos d1 lsl, tos d2 rol, tos d3 lsl,
    d3 tos move, tos not, d2 tos and, d1 tos or, d3 d2 and, d2 push, next
code drshift
    31 # tos cmp, gt if 32 # tos sub, d1 pop, 4 # sp add,
        tos d1 lsr, d1 push, tos clear, next, endif
    d1 pop, d2 pop, -1 # d3 move, tos d1 ror, tos d2 lsr, tos d3 lsr,
    d3 tos move, d1 tos and, d3 not, d3 d1 and, d1 d2 or, d2 push, next

code 16rotate   tos swap, next          synonym 32drotate swap

( ---------------------------------------------------------------------------- )
code   mux  tos d1 move, [sp]+ tos and, d1 not, [sp]+ d1 and, d1 tos or, next
code demux  tos d1 move, [sp]  tos and, d1 not, d1 [sp]  and, next

code ms ( n -- )
    tos test, gt if tos begin 708 d1 do loop loop endif tos pop, next

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
















