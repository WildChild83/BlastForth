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

{ create state   false host, }
{ : compiling? state @ ; }
{ : comp-only  compiling? not -14 and throw ; }
{ : host-only  compiling? abort" Compiling a host-only word." ; }

( ---------------------------------------------------------------------------- )
(       A7=Return Stack     A6=Data Stack       A5=Thread Pointer              )
(       A4=Float Stack      A0=Next Pointer     D0=Top of Data Stack           )
( ---------------------------------------------------------------------------- )
{ :   PC    ( -- romaddr ) alignrom romspace ; }
{ :   PC:   ( -- ) PC make ; }
{ : doprim, ( -- ) PC 4 + , ; }

Assembler wid make asm68k-wid
{ :  asm              host-only clean alignrom asm68k-wid >order ; }
{ : rawcode ( "name") host-only create PC host, asm   does> host-only @ ; }
{ :    anon ( -- xt ) host-only PC doprim, asm ; }
{ :    code ( "name") host-only
    create PC host, doprim, asm does> comp-only @ , ; }

Assembler definitions
{ synonym end } ( -- ) }
{ : next, ( -- ) [np] jump, ; }
{ : next  ( -- ) next, end  ; }

Forth definitions
{ : host/target: ( hostxt targetxt "name" -- ) host-only create host, host,
    does> ( .. -- ) compiling? if @ , else cell+ @ execute endif ; }

{ : ' ( "name" -- xt )   host-only ' >body @ ; }

( ---------------------------------------------------------------------------- )
(       Literals                                                               )
( ---------------------------------------------------------------------------- )
{ : ?zero ( n -- n flag ) dup  0= ; }
{ : ?ph   ( n -- n flag ) dup  0 65536 within ; }
{ : ?nh   ( n -- n flag ) dup -65536 0 within ; }

code lit   ( -- n ) tos push, tos read, next
code zlit  ( -- 0 ) tos push, tos clear, next
code plit  ( -- u ) tos push, tos clear, tos h read, next
code nlit  ( -- n ) tos push, -1 # tos move, tos h read, next
code 2lit  ( -- d ) tos push, [tp]+ -[sp] move, tos read, next
code 2zlit ( -- d ) tos push, tos clear, tos push, next
code 2plit ( -- u u )
    tos push, [tp]+ -[sp] h move, -[sp] h clear, tos clear, tos h read, next
code 2nlit ( -- n n )
    tos push, [tp]+ -[sp] h move, -1 # -[sp] h move, 
    -1 # tos move, tos h read, next

{ : literal  ( n -- ) comp-only
    ?zero if zlit drop exit endif
    ?ph if plit h, exit endif ?nh if nlit h, exit endif lit , ; }
{ : (2lit)   ( n n -- ) comp-only 2lit , , ; }
{ : 2literal ( n n -- ) comp-only
    ?zero if swap ?zero if 2zlit 2drop exit endif  swap  endif
    ?ph   if swap ?ph   if 2plit h, h, exit endif (2lit) exit endif
    ?nh   if swap ?nh   if 2nlit h, h, exit endif (2lit) exit endif
    swap (2lit) ; }

{ : ['] ( "name" -- xt ) comp-only ' >body @ } literal { ; }

( ---------------------------------------------------------------------------- )
(       Created Words                                                          )
( ---------------------------------------------------------------------------- )
{ 0 value lastxt }

PC: <docreated&>
{ : codefield, ( romaddr -- ) PC dup host,  to lastxt   ,  ; }
{ : create ( "name" -- ) host-only
    create <docreated&> 4 + codefield,
    does> @ compiling? if , exit endif 4 + ; }
create docreated&    asm tos push, dfa tos move, next

{ : (newxt) ( romaddr -- ) host-only lastxt rom! ; }
{ : ;code ( -- ) host-only   
    PC postpone literal   postpone (newxt)   postpone ;   asm  ; immediate }

( ---------------------------------------------------------------------------- )
(       RAMspace Pointer                                                       )
( ---------------------------------------------------------------------------- )
{ create ramspace $FF0000 host, }
{ : allot ( n -- ) ramspace +! ; }
{ : here  ( -- u ) ramspace  @ ; }
{ : +ramspace> ( n1 -- u1 ) ramspace @ tuck + ramspace ! ; }
{ : alignram ( -- ) ramspace @ 1+ -2 and ramspace ! ; }
{ : <memory>  @ compiling? if , exit endif 4 + romh@ $FF0000 + ; }

( ---------------------------------------------------------------------------- )
(       Deferred Words                                                         )
( ---------------------------------------------------------------------------- )
create dodefer&   asm
    -1 # d1 move, [dfa] d1 h move, d1 a1 move,
    [a1] dfa move, [dfa]+ a1 move, [a1] jump, end
{ : defer ( "name" -- ) host-only 
    create dodefer& codefield, 4 +ramspace> h, does> <memory> ; }

code defer@ ( xt1 -- xt2 )
    tos a1 move, 4 # a1 add, -1 # d1 move,
    [a1] d1 h move, d1 a1 move, [a1] tos move, next
code defer! ( xt1 xt2 -- )
    tos a1 move, 4 # a1 add, -1 # d1 move,
    [a1] d1 h move, d1 a1 move, [sp]+ [a1] move, tos pull, next

code (action-of) ( -- xt )
    -1 # d1 move, d1 h read, d1 a1 move, tos push, [a1] tos move, next
code (is) ( xt -- )
    -1 # d1 move, d1 h read, d1 a1 move, tos [a1] move, tos pull, next

{ : action-of ( "name" -- xt ) comp-only (action-of) ' >body @ 4 + romh@ h, ; }
{ : is        ( xt "name" -- ) comp-only (is)        ' >body @ 4 + romh@ h, ; }

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

DEBUG [IF]
    defer  exit
    create docolon&     asm $1234 [#] jump, end     PC: (docolon-vector)
[ELSE]
    code   exit         tp rpull, next
    create docolon&     asm tp rpush, dfa tp move, next
[THEN]

{ : ; ( -- ) comp-only } exit { state off ; }
{ : : ( "name" -- ) host-only
    create docolon& codefield, } ] { does> ( -- ) comp-only @ , ; }

{ synonym {:} : }
{ synonym {;} ; immediate }

{ : :noname ( -- xt ) host-only  PC  docolon& codefield, } ] {;}

asm { : (dodoes>) ( -- addr ) host-only
    tos push, dfa tos move, tp rpush, [pc 4 +] tp address, next, } ] {;} end
{ : does> ( -- ) host-only
    PC postpone literal   postpone (newxt)   postpone ; (dodoes>) ; immediate }

code execute ( xt -- ) tos dfa move, tos pull, [dfa]+ a1 move, [a1] jump, end

{ : ]L    ( n -- ) host-only state on }  literal [ ] {;}
{ : ]2L ( n n -- ) host-only state on } 2literal [ ] {;}

( ---------------------------------------------------------------------------- )
(       Constants                                                              )
( ---------------------------------------------------------------------------- )
{ : h,rom, ( n -- ) $FFFF and dup host, h, ; }

create doconstp&   asm tos push, tos clear, [dfa] tos h move, next
{ : pconstant ( n "name" -- ) host-only
    create doconstp& codefield,  h,rom,
    does> ( -- n ) compiling? if @ , exit endif cell+ @ ; }

create doconstn&   asm tos push, -1 # tos move, [dfa] tos h move, next
{ : nconstant ( n "name" -- ) host-only
    create doconstn& codefield,  h,rom,
    does> ( -- n ) compiling? if @ , exit endif cell+ @ -65536 or ; }

create doconst&    asm tos push, [dfa] tos move, next
{ : constant ( n "name" -- ) host-only
    ?ph if pconstant exit endif   ?nh if nconstant exit endif
    create doconst& codefield, dup host, , 
    does>  compiling? if @ , exit endif cell+ @ ; }

create do2constp&   asm
    tos push, tos clear, [dfa]+ tos h move, 
    [dfa]+ -[sp] h move, -[sp] h clear, next
{ : 2pconstant ( d "name" -- ) host-only
    create do2constp& codefield,  h,rom,  h,rom,
    does>  compiling? if @ , exit endif cell+ dup @ swap cell+ @ ; }

create do2constn&   asm
    tos push, -1 # tos move, [dfa]+ tos h move, 
    [dfa]+ -[sp] h move, -1 # -[sp] h move, next
{ : 2nconstant ( d "name" -- ) host-only
    create do2constn& codefield,  h,rom,  h,rom,
    does>  compiling? if @ , exit endif
           cell+ dup @ -65536 or swap cell+ @ -65536 or ; }

create do2const&   asm tos push, [dfa]+ push, [dfa] tos move, next
{ : (2const) ( d "name" -- )
    create do2const& codefield, dup host, , dup host, ,
    does>  compiling? if @ , exit endif cell+ dup @ swap cell+ @ ; }
{ : 2constant ( d "name" -- ) host-only
    swap ?ph if swap ?ph if 2pconstant exit endif (2const) exit endif
         ?nh if swap ?nh if 2nconstant exit endif (2const) exit endif
    swap (2const) ; }

( ---------------------------------------------------------------------------- )
(       Memory Buffers                                                         )
( ---------------------------------------------------------------------------- )
create domem&   asm tos push, -1 # tos move, [dfa] tos h move, next
{ : buffer: ( u "name" -- ) host-only 
    create domem& codefield, +ramspace> h, does> <memory> ; }

{ :  variable ( "name" -- ) host-only alignram 4 buffer: ; }
{ : hvariable ( "name" -- ) host-only alignram 2 buffer: ; }
{ : cvariable ( "name" -- ) host-only          1 buffer: ; }
{ : 2variable ( "name" -- ) host-only alignram 8 buffer: ; }

( ---------------------------------------------------------------------------- )
(       Values                                                                 )
( ---------------------------------------------------------------------------- )
{ :  to ( n "name" -- ) comp-only ' >body dup cell+ @ ,      @ 4 + romh@ h, ; }
{ : +to ( n "name" -- ) comp-only ' >body dup 2 cells + @ ,  @ 4 + romh@ h, ; }

( ---------------------------------------------------------------------------- )
create dovalue&   asm
    tos push, -1 # d1 move, [dfa] d1 h move, d1 a1 move, [a1] tos move, next
create tovalue&   doprim, asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move, tos [a1] move, tos pull, next
create +tovalue&  doprim,  asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move, tos [a1] add, tos pull, next
{ : value ( "name" -- ) host-only 
    create dovalue& codefield, tovalue& host, +tovalue& host,
    alignram 4 +ramspace> h, does> <memory> ; }

create dohvalue&   asm
    tos push, -1 # d1 move, [dfa] d1 h move, d1 a1 move,
    tos clear, [a1] tos h move, next
create tohvalue&   doprim, asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move, tos [a1] h move, tos pull, next
create +tohvalue&  doprim, asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move, tos [a1] h add, tos pull, next
{ : hvalue ( "name" -- ) host-only 
    create dohvalue& codefield, tohvalue& host, +tohvalue& host,
    alignram 2 +ramspace> h, does> <memory> ; }

create docvalue&   asm
    tos push, -1 # d1 move, [dfa] d1 h move, d1 a1 move,
    tos clear, [a1] tos c move, next
create tocvalue&   doprim, asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move, tos [a1] c move, tos pull, next
create +tocvalue&  doprim, asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move, tos [a1] c add, tos pull, next
{ : cvalue ( "name" -- ) host-only 
    create docvalue& codefield, tocvalue& host, +tocvalue& host,
    1 +ramspace> h, does> <memory> ; }

create do2value&   asm
    tos push, -1 # d1 move, [dfa] d1 h move, d1 a1 move,
    tos clear, [a1]+ tos move, [a1]+ -[sp] move, next
create to2value&   doprim, asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move,
    tos [a1]+ move, [sp]+ [a1] move, tos pull, next
create +to2value&  doprim, asm
    -1 # d1 move, [tp]+ d1 h move, d1 a1 move, [a1]+ d2 move, [a1] d1 move,
    [sp]+ d1 add, tos d2 addx, d1 [a1] move, d2 -[a1] move, tos pull, next
{ : 2value ( "name" -- ) host-only 
    create do2value& codefield, to2value& host, +to2value& host,
    alignram 8 +ramspace> h, does> <memory> ; }

( ---------------------------------------------------------------------------- )
(       Data Lists                                                             )
( ---------------------------------------------------------------------------- )
{ variable xt1[    variable xt2[ }
{ : number[ s>number? not abort" Invalid number." d>s 
            xt2[ @ execute not abort" Number out of range." xt1[ @ execute ; }
{ : "exec"  find-name  name>interpret  execute ;
{ : eval[   2dup s" \" str= >r 2dup s" (" str= r> or if "exec" exit endif
            2dup s" ]" str= if 2drop xt1[ off exit endif  number[ ; }
{ : parse[  parse-name ?dup if eval[ exit endif drop (refill) ; }
{ : x[: ( xt1 xt2 ) host-only create host, host, does> ( numbers.. ) host-only
    cellcount xt2[ ! @ xt1[ !   begin parse[  xt1[ @ not until ; }

{ '  , ' ?cell } x[: cells[
{ ' h, ' ?half } x[: halves[
{ ' c, ' ?char } x[: chars[     aka bytes[

( ---------------------------------------------------------------------------- )
(       Miscellaneous other stuff                                              )
( ---------------------------------------------------------------------------- )
code  interrupts ( -- ) $2300 # sr move, next
code -interrupts ( -- ) $2700 # sr move, next

code -interrupts[ ( -- ) ( R: -- int-sys )
     sr d1 h move, d1 h rpush, $2700 # sr move, next

code ]-interrupts ( -- ) ( R: int-sys -- )
    d1 h rpull, d1 sr h move, next

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

















